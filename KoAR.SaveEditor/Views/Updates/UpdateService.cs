using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using KoAR.SaveEditor.Constructs;

namespace KoAR.SaveEditor.Views.Updates
{
    public sealed class UpdateService
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonSnakeCaseNamingPolicy.Instance,
            DictionaryKeyPolicy = JsonSnakeCaseNamingPolicy.Instance
        };

        private Release? _update;

        public UpdateService()
            : this(interval: 250)
        {
        }

        public UpdateService(int interval)
        {
            this.Interval = interval;
        }

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

        private sealed class JsonSnakeCaseNamingPolicy : JsonNamingPolicy
        {
            public static readonly JsonNamingPolicy Instance = new JsonSnakeCaseNamingPolicy();

            public override string ConvertName(string name)
            {
                if (string.IsNullOrEmpty(name))
                {
                    return name;
                }
                StringBuilder builder = new StringBuilder(name.Length + Math.Max(2, name.Length / 5));
                UnicodeCategory? previousCategory = null;
                for (int index = 0; index < name.Length; index++)
                {
                    char current = name[index];
                    if (current == '_')
                    {
                        builder.Append('_');
                        previousCategory = null;
                        continue;
                    }
                    UnicodeCategory currentCategory = char.GetUnicodeCategory(current);
                    switch (currentCategory)
                    {
                        case UnicodeCategory.UppercaseLetter:
                        case UnicodeCategory.TitlecaseLetter:
                            if (previousCategory == UnicodeCategory.SpaceSeparator ||
                                previousCategory == UnicodeCategory.LowercaseLetter ||
                                previousCategory != UnicodeCategory.DecimalDigitNumber &&
                                index > 0 &&
                                index + 1 < name.Length &&
                                char.IsLower(name, index + 1))
                            {
                                builder.Append('_');
                            }
                            current = char.ToLowerInvariant(current);
                            break;
                        case UnicodeCategory.LowercaseLetter:
                        case UnicodeCategory.DecimalDigitNumber:
                            if (previousCategory == UnicodeCategory.SpaceSeparator)
                            {
                                builder.Append('_');
                            }
                            break;
                        case UnicodeCategory.Surrogate:
                            break;
                        default:
                            if (previousCategory != null)
                            {
                                previousCategory = UnicodeCategory.SpaceSeparator;
                            }
                            continue;
                    }
                    builder.Append(current);
                    previousCategory = currentCategory;
                }
                return builder.ToString();
            }
        }
    }
}
