using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KoAR.SaveEditor.Updates
{
    public sealed class UpdateService
    {
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonSnakeCaseNamingPolicy.Instance,
            DictionaryKeyPolicy = JsonSnakeCaseNamingPolicy.Instance
        };

        public UpdateService()
        {
        }

        public string CurrentVersion { get; } = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        public async Task<ReleaseInfo> GetLatestRelease()
        {
            HttpWebRequest request = WebRequest.CreateHttp("https://api.github.com/repos/mburbea/koar-item-editor/releases/latest");
            request.UserAgent = nameof(UpdateService);
            request.Accept = "application/vnd.github.v3+json";
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception();
            }
            using Stream stream = response.GetResponseStream();
            return await JsonSerializer.DeserializeAsync<ReleaseInfo>(stream, this._options);
        }

        public sealed class ReleaseAsset
        {
            public string BrowserDownloadUrl { get; set; } = string.Empty;

            public string ContentType { get; set; } = string.Empty;

            public bool IsZipFile => this.ContentType == "application/zip";

            public long Size { get; set; }
        }

        public sealed class ReleaseInfo
        {
            public ReleaseAsset[] Assets { get; set; } = Array.Empty<ReleaseAsset>();

            public string Body { get; set; } = string.Empty;

            public string Name { get; set; } = string.Empty;

            public DateTime PublishedAt { get; set; }

            public string TagName { get; set; } = string.Empty;

            public string Version => this.TagName.Length == 0 ? string.Empty : this.TagName.Substring(1);

            public ReleaseAsset? GetZipFileAsset() => this.Assets.FirstOrDefault(asset => asset.IsZipFile);
        }
    }

    internal sealed class JsonSnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public static readonly JsonNamingPolicy Instance = new JsonSnakeCaseNamingPolicy();

        private JsonSnakeCaseNamingPolicy()
        {
        }

        public override string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            // Allocates a string builder with the guessed result length,
            // where 5 is the average word length in English, and
            // max(2, length / 5) is the number of underscores.
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
