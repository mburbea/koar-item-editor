using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Updates
{
    public sealed class UpdateService
    {
        private static readonly Lazy<string?> _credentials = new Lazy<string?>(UpdateService.LoadCredentials);
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonSnakeCaseNamingPolicy.Instance };

        private UpdateInfo? _update;

        public event EventHandler<EventArgs<DownloadProgress>>? DownloadProgress;

        public event EventHandler? UpdateChanged;

        public UpdateInfo? Update
        {
            get => this._update;
            private set
            {
                this._update = value;
                this.UpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public static void ExecuteUpdate(string scriptFileName, string zipFileName)
        {
            Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = Path.GetTempPath(),
                UseShellExecute = false,
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{Path.GetFileName(scriptFileName)}\" {Process.GetCurrentProcess().Id} \"{Path.GetFileName(zipFileName)}\"",
            }).WaitForExit();
        }

        public static async Task<string> ExtractPowershellScript()
        {
            string fileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.ps1");
            using FileStream fileStream = File.Create(fileName);
            await UpdateService.GetResourceFileStream("update.ps1").CopyToAsync(fileStream).ConfigureAwait(false);
            return fileName;
        }

        public async Task CheckForUpdatesAsync(CancellationToken cancellationToken = default)
        {
            this.Update = await UpdateService.GetUpdateInfoAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DownloadUpdateAsync(string targetFileName, CancellationToken cancellationToken = default)
        {
            if (this.Update == null)
            {
                return;
            }
            const int interval = 250;
            int bytesTransferred = 0, bytesPerInterval = 0;
            using Timer timer = new Timer(OnTick, null, 0, interval);
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp(this.Update.Uri);
                using WebResponse response = await request.GetResponseAsync().ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                using Stream stream = response.GetResponseStream();
                using FileStream fileStream = File.Create(targetFileName);
                byte[] buffer = new byte[8192];
                while (bytesTransferred < this.Update.Size)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    int count = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                    await fileStream.WriteAsync(buffer, 0, count).ConfigureAwait(false);
                    bytesTransferred += count;
                    bytesPerInterval += count;
                }
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                this.OnDownloadProgress(new DownloadProgress(bytesTransferred, default));
            }
            catch (Exception e)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                this.OnDownloadProgress(e);
                throw;
            }

            void OnTick(object _)
            {
                DownloadProgress progress = new DownloadProgress(bytesTransferred, bytesPerInterval * 1000d / interval);
                bytesPerInterval = 0;
                this.OnDownloadProgress(progress);
            }
        }

        private static async Task<T?> FetchAsync<T>(string suffix, CancellationToken cancellationToken)
            where T : class
        {
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp($"https://api.github.com/repos/mburbea/koar-item-editor/{suffix}");
                request.UserAgent = request.Accept = "application/vnd.github.v3+json";
                if (UpdateService._credentials.Value is string credentials)
                {
                    request.Headers.Add(HttpRequestHeader.Authorization, credentials);
                }
                using WebResponse response = await request.GetResponseAsync().ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                using Stream stream = response.GetResponseStream();
                cancellationToken.ThrowIfCancellationRequested();
                return await JsonSerializer.DeserializeAsync<T>(stream, UpdateService._jsonOptions).ConfigureAwait(false);
            }
            catch (WebException e)
            {
                if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound)
                {
                    return default;
                }
                throw;
            }
        }

        /// <summary>
        /// Gets a release by tag asynchronously.
        /// It's possible for this task to resolve to <see langword="null"/> as a release may have been deleted.
        /// </summary>
        private static Task<Release?> GetRelease(string tag, CancellationToken cancellationToken) => UpdateService.FetchAsync<Release>($"releases/tags/{tag}", cancellationToken);

        private static Stream GetResourceFileStream(string name) => Application.GetResourceStream(new Uri($"/Updates/{name}", UriKind.Relative)).Stream;

        private static async Task<Tag[]> GetTagsAsync(CancellationToken cancellationToken) => (await UpdateService.FetchAsync<Tag[]>("tags", cancellationToken).ConfigureAwait(false))!;

        private async static Task<UpdateInfo?> GetUpdateInfoAsync(CancellationToken cancellationToken)
        {
            try
            {
                const int maxReleases = 10;
                List<string> tagNames = (await UpdateService.GetTagsAsync(cancellationToken).ConfigureAwait(false))
                    .Where(tag => tag.Version.Value.Major == App.Version.Major)
                    .TakeWhile(tag => tag.Version.Value > App.Version)
                    .Take(maxReleases)
                    .Select(tag => tag.Name)
                    .ToList();
                if (tagNames.Count == 0)
                {
                    return null;
                }
                List<Release> releases = (await Task.WhenAll(tagNames.Select(tag => UpdateService.GetRelease(tag, cancellationToken))).ConfigureAwait(false))
                    .OfType<Release>()
                    .Where(release => release.GetZipFileAsset() != null)
                    .ToList();
                if (releases.Count != 0)
                {
                    return new UpdateInfo(releases);
                }
            }
            catch
            {
            }
            return null;
        }

        private static string? LoadCredentials()
        {
            using StreamReader reader = new StreamReader(UpdateService.GetResourceFileStream("github.credentials"));
            return reader.EndOfStream ? default : $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes(reader.ReadToEnd()))}";
        }

        private void OnDownloadProgress(DownloadProgress data) => this.DownloadProgress?.Invoke(this, new EventArgs<DownloadProgress>(data));

        private sealed class Release : IReleaseInfo
        {
            private ReleaseAsset? _zipFileAsset;

            public ReleaseAsset[] Assets { get; set; } = Array.Empty<ReleaseAsset>();

            public string Body { get; set; } = string.Empty;

            public string Name { get; set; } = string.Empty;

            public DateTime PublishedAt { get; set; }

            public string TagName { get; set; } = string.Empty;

            public string Version => this.TagName.Length == 0 ? string.Empty : this.TagName.Substring(1);

            public int ZipFileSize => this.GetZipFileAsset()?.Size ?? 0;

            public string ZipFileUri => this.GetZipFileAsset()?.BrowserDownloadUrl ?? string.Empty;

            public ReleaseAsset? GetZipFileAsset() => this._zipFileAsset ??= this.Assets.FirstOrDefault(asset => asset.IsZipFile);
        }

        private sealed class ReleaseAsset
        {
            public string BrowserDownloadUrl { get; set; } = string.Empty;

            public string ContentType { get; set; } = string.Empty;

            public bool IsZipFile => this.ContentType == "application/zip";

            public int Size { get; set; }
        }

        private sealed class Tag
        {
            private static readonly Regex _regex = new Regex(@"^v(?<version>\d+\.\d+\.\d+)$", RegexOptions.ExplicitCapture);

            public string Name { get; set; } = string.Empty;

            public Lazy<Version> Version => new Lazy<Version>(
                () => new Version(Tag._regex.Match(this.Name) is { Success: true } match ? match.Groups["version"].Value : "0.0.0")
            );
        }
    }
}
