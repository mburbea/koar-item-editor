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

namespace KoAR.SaveEditor.Updates
{
    public static class UpdateMethods
    {
        private static readonly Lazy<string?> _credentials = new Lazy<string?>(UpdateMethods.LoadCredentials);
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonSnakeCaseNamingPolicy.Instance };

        /// <summary>
        /// Given the path to a zip file containing an update, executes the update process.
        /// </summary>
        /// <param name="zipFilePath">Zip file path.</param>
        public static async void ExecuteUpdate(string zipFilePath)
        {
            string scriptFileName = await UpdateMethods.ExtractPowershellScript().ConfigureAwait(false);
            Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = Path.GetTempPath(),
                UseShellExecute = false,
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{Path.GetFileName(scriptFileName)}\" {Process.GetCurrentProcess().Id} \"{Path.GetFileName(zipFilePath)}\"",
            }).WaitForExit();
        }

        /// <summary>
        /// Fetches the latest 2.x release.
        /// </summary>
        /// <param name="cancellationToken">Optionally used to propagate cancellation requests.</param>
        /// <returns>Information related to a release. Returns <see langword="null"/> if not found or an error occurs.</returns>
        public static async Task<IReleaseInfo?> FetchLatest2xReleaseAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (Tag tag in await UpdateMethods.FetchTagsAsync(cancellationToken).ConfigureAwait(false))
                {
                    if (tag.Version.Major != 2)
                    {
                        continue;
                    }
                    Release? release = await UpdateMethods.FetchReleaseAsync(tag.Name, cancellationToken).ConfigureAwait(false);
                    if (release != null && release.HasUpdateAsset)
                    {
                        return release;
                    }
                }
            }
            catch
            {
            }
            return default;
        }

        /// <summary>
        /// Fetches an array of the interim update releases for the current major version of the application to the latest.
        /// Only checks up to <paramref name="maxReleases"/> number of tags/releases.
        /// </summary>
        /// <param name="cancellationToken">Optionally used to propagate cancellation requests.</param>
        /// <returns>Information related to interim releases. Returns <see langword="null"/> if no update is found or an error occurs.</returns>
        public static async Task<IReleaseInfo[]?> FetchUpdateReleasesAsync(int maxReleases = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                List<string> tagNames = (await UpdateMethods.FetchTagsAsync(cancellationToken).ConfigureAwait(false))
                    .Where(tag => tag.Version.Major == App.Version.Major && tag.Version > App.Version)
                    .Take(maxReleases)
                    .Select(tag => tag.Name)
                    .ToList();
                Release[] array = (await Task.WhenAll(tagNames.Select(tag => UpdateMethods.FetchReleaseAsync(tag, cancellationToken))).ConfigureAwait(false))
                    .OfType<Release>()
                    .Where(release => release.HasUpdateAsset)
                    .ToArray();
                if (array.Length != 0)
                {
                    return array;
                }
            }
            catch
            {
            }
            return default;
        }

        private static async Task<string> ExtractPowershellScript()
        {
            string fileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.ps1");
            using FileStream fileStream = File.Create(fileName);
            await UpdateMethods.GetResourceFileStream("update.ps1").CopyToAsync(fileStream).ConfigureAwait(false);
            return fileName;
        }

        private static async Task<T?> FetchDataAsync<T>(string suffix, CancellationToken cancellationToken)
            where T : class
        {
            try
            {
                HttpWebRequest request = WebRequest.CreateHttp($"https://api.github.com/repos/mburbea/koar-item-editor/{suffix}");
                request.UserAgent = request.Accept = "application/vnd.github.v3+json";
                if (UpdateMethods._credentials.Value is string credentials)
                {
                    request.Headers.Add(HttpRequestHeader.Authorization, credentials);
                }
                using WebResponse response = await request.GetResponseAsync().ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                using Stream stream = response.GetResponseStream();
                cancellationToken.ThrowIfCancellationRequested();
                return await JsonSerializer.DeserializeAsync<T>(stream, UpdateMethods._jsonOptions).ConfigureAwait(false);
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
        /// Fetches a release by tag asynchronously.
        /// </summary>
        /// <param name="tag">Name of the tag associated with the release.</param>
        /// <param name="cancellationToken">Optionally used to propagate cancellation requests.</param>
        /// <returns>A model representing the GitHub release.  Returns <see langword="null"/> if the release was deleted or an error occurs.</returns>
        private static Task<Release?> FetchReleaseAsync(string tag, CancellationToken cancellationToken) => UpdateMethods.FetchDataAsync<Release>($"releases/tags/{tag}", cancellationToken);

        private static async Task<Tag[]> FetchTagsAsync(CancellationToken cancellationToken) => (await UpdateMethods.FetchDataAsync<Tag[]>("tags", cancellationToken).ConfigureAwait(false))!;

        private static Stream GetResourceFileStream(string name) => Application.GetResourceStream(new Uri($"/Updates/{name}", UriKind.Relative)).Stream;

        private static string? LoadCredentials()
        {
            using StreamReader reader = new StreamReader(UpdateMethods.GetResourceFileStream("github.credentials"));
            return reader.EndOfStream ? default : $"Basic {Convert.ToBase64String(Encoding.ASCII.GetBytes(reader.ReadToEnd()))}";
        }

        private sealed class Release : IReleaseInfo
        {
            private ReleaseAsset? _zipFileAsset;

            public ReleaseAsset[] Assets { get; set; } = Array.Empty<ReleaseAsset>();

            public string Body { get; set; } = string.Empty;

            public bool HasUpdateAsset => this.ZipFileAsset != null;

            public string Name { get; set; } = string.Empty;

            public DateTime PublishedAt { get; set; }

            public string TagName { get; set; } = string.Empty;

            public string Version => this.TagName.Length == 0 ? string.Empty : this.TagName[1..];

            public ReleaseAsset? ZipFileAsset => this._zipFileAsset ??= this.Assets.FirstOrDefault(asset => asset.IsZipFile);

            public int ZipFileSize => this.ZipFileAsset?.Size ?? 0;

            public string ZipFileUri => this.ZipFileAsset?.BrowserDownloadUrl ?? string.Empty;
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
            private Version? _version;

            private static readonly Regex _regex = new Regex(@"^v(?<version>\d+\.\d+\.\d+)$", RegexOptions.ExplicitCapture);

            public string Name { get; set; } = string.Empty;

            public Version Version => this._version ??= new Version(Tag._regex.IsMatch(this.Name) ? this.Name[1..] : "0.0.0");
        }
    }
}
