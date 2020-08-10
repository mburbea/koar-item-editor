using System;
using System.IO;
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
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonSnakeCaseNamingPolicy.Instance
        };

        private Release? _update;

        public UpdateService()
            : this(interval: 250)
        {
        }

        public UpdateService(int interval) => this.Interval = interval;

        public event EventHandler<EventArgs<DownloadProgress>>? DownloadProgress;

        public event EventHandler? UpdateChanged;

        public string CurrentVersion { get; } = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        public int Interval { get; }

        public Release? Update
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
            try
            {
                Release release = await UpdateService.GetLatestReleaseAsync(cancellationToken).ConfigureAwait(false);
                if (this.CurrentVersion != release.Version && release.Assets.Length != 0 && release.Assets[0].Equals(release.GetZipFileAsset()))
                {
                    this.Update = release;
                }
            }
            catch
            {
            }
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
                ReleaseAsset asset = this.Update.Assets[0];
                HttpWebRequest request = WebRequest.CreateHttp(asset.BrowserDownloadUrl);
                using WebResponse response = await request.GetResponseAsync().ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                using Stream stream = response.GetResponseStream();
                using FileStream fileStream = File.Create(targetFileName);
                byte[] buffer = new byte[8192];
                while (bytesTransferred < asset.Size)
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

        private static async Task<Release> GetLatestReleaseAsync(CancellationToken cancellationToken)
        {
            HttpWebRequest request = WebRequest.CreateHttp("https://api.github.com/repos/mburbea/koar-item-editor/releases/latest");
            request.UserAgent = request.Accept = "application/vnd.github.v3+json";
            using WebResponse response = await request.GetResponseAsync().ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            using Stream stream = response.GetResponseStream();
            cancellationToken.ThrowIfCancellationRequested();
            return await JsonSerializer.DeserializeAsync<Release>(stream, UpdateService._options).ConfigureAwait(false);
        }

        private void OnDownloadProgress(DownloadProgress data) => this.DownloadProgress?.Invoke(this, new EventArgs<DownloadProgress>(data));

    }
}
