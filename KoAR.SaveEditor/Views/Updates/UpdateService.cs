using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using KoAR.Core;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views.Updates
{
    public sealed class UpdateService
    {
        public static readonly string CurrentVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonSnakeCaseNamingPolicy.Instance };

        private UpdateInfo? _update;

        public UpdateService()
            : this(interval: 250)
        {
        }

        public UpdateService(int interval) => this.Interval = interval;

        public event EventHandler<EventArgs<DownloadProgress>>? DownloadProgress;

        public event EventHandler? UpdateChanged;

        public int Interval { get; }

        public UpdateInfo? Update
        {
            get => this._update;
            private set
            {
                this._update = value;
                this.UpdateChanged?.Invoke(this, EventArgs.Empty);
            }
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
            int bytesTransferred = 0, bytesPerInterval = 0;
            using Timer timer = new Timer(OnTick, null, 0, this.Interval);
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
                DownloadProgress progress = new DownloadProgress(bytesTransferred, bytesPerInterval * 1000d / this.Interval);
                bytesPerInterval = 0;
                this.OnDownloadProgress(progress);
            }
        }

        private static async Task<Release[]> GetReleasesAsync(CancellationToken cancellationToken)
        {
            const string endpoint = "https://api.github.com/repos/mburbea/koar-item-editor/releases";
            const int maxReleases = 15;
            HttpWebRequest request = WebRequest.CreateHttp($"{endpoint}?per_page={maxReleases}");
            request.UserAgent = request.Accept = "application/vnd.github.v3+json";
            using WebResponse response = await request.GetResponseAsync().ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            using Stream stream = response.GetResponseStream();
            cancellationToken.ThrowIfCancellationRequested();
            return await JsonSerializer.DeserializeAsync<Release[]>(stream, UpdateService._jsonOptions).ConfigureAwait(false);
        }

        private async static Task<UpdateInfo?> GetUpdateInfoAsync(CancellationToken cancellationToken)
        {
            try
            {
                Release[] releases = await UpdateService.GetReleasesAsync(cancellationToken).ConfigureAwait(false);
                Release? latest = releases.FirstOrDefault();
                ReleaseAsset? asset;
                if (latest != null && UpdateService.CurrentVersion != latest.Version && (asset = latest.GetZipFileAsset()) != null)
                {
                    return new UpdateInfo(
                        latest.Version,
                        asset.BrowserDownloadUrl,
                        asset.Size,
                        releases.TakeWhile(release => release.Version != UpdateService.CurrentVersion).ToArray()
                    );
                }
            }
            catch
            {
            }
            return null;
        }

        private void OnDownloadProgress(DownloadProgress data) => this.DownloadProgress?.Invoke(this, new EventArgs<DownloadProgress>(data));

        private sealed class Release : IReleaseInfo
        {
            public ReleaseAsset[] Assets { get; set; } = Array.Empty<ReleaseAsset>();

            public string Body { get; set; } = string.Empty;

            public string Name { get; set; } = string.Empty;

            public DateTime PublishedAt { get; set; }

            public string TagName { get; set; } = string.Empty;

            public string Version => this.TagName.Length == 0 ? string.Empty : this.TagName.Substring(1);

            public ReleaseAsset? GetZipFileAsset() => this.Assets.FirstOrDefault(asset => asset.IsZipFile);
        }

        private sealed class ReleaseAsset
        {
            public string BrowserDownloadUrl { get; set; } = string.Empty;

            public string ContentType { get; set; } = string.Empty;

            public bool IsZipFile => this.ContentType == "application/zip";

            public int Size { get; set; }
        }
    }
}
