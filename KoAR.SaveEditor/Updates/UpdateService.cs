using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace KoAR.SaveEditor.Updates
{
    public sealed class UpdateService
    {
        private readonly string _currentVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonSnakeCaseNamingPolicy.Instance,
            DictionaryKeyPolicy = JsonSnakeCaseNamingPolicy.Instance
        };
        private UpdateInfo _updateInfo;

        public event EventHandler? UpdateChanged;

        public UpdateInfo Update
        {
            get => this._updateInfo;
            private set
            {
                if (this._updateInfo == value)
                {
                    return;
                }
                this._updateInfo = value;
                this.UpdateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task CheckForUpdatesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                Release info = await this.GetLatestReleaseAsync(cancellationToken).ConfigureAwait(false);
                ReleaseAsset? asset;
                if (info.Version != this._currentVersion && (asset = info.GetZipFileAsset()) != null)
                {
                    this.Update = new UpdateInfo(info.Version, info.Body, asset.BrowserDownloadUrl, asset.Size);
                }
            }
            catch
            {
            }
        }

        private async Task<Release> GetLatestReleaseAsync(CancellationToken cancellationToken)
        {
            HttpWebRequest request = WebRequest.CreateHttp("https://api.github.com/repos/mburbea/koar-item-editor/releases/latest");
            request.UserAgent = request.Accept = "application/vnd.github.v3+json";
            using WebResponse response = await request.GetResponseAsync().ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            using Stream stream = response.GetResponseStream();
            cancellationToken.ThrowIfCancellationRequested();
            return await JsonSerializer.DeserializeAsync<Release>(stream, this._options).ConfigureAwait(false);
        }

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

        private sealed class Release
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

            public long Size { get; set; }
        }
    }
}
