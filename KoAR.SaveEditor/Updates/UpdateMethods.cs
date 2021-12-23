using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using KoAR.Core;

namespace KoAR.SaveEditor.Updates;

public static class UpdateMethods
{
    private static readonly HttpClient _client = UpdateMethods.InitializeClient();
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNamingPolicy = JsonSnakeCaseNamingPolicy.Instance };

    /// <summary>
    /// Given the path to a zip file containing an update, executes the update process.
    /// </summary>
    /// <param name="zipFilePath">Zip file path.</param>
    public static async void ExecuteUpdate(string zipFilePath)
    {
        string scriptFileName = await UpdateMethods.ExtractPowershellScript().ConfigureAwait(false);
        using Process process = Process.Start(startInfo: new()
        {
            WorkingDirectory = Path.GetTempPath(),
            UseShellExecute = false,
            FileName = "powershell.exe",
            Arguments = $"-ExecutionPolicy Bypass -File \"{Path.GetFileName(scriptFileName)}\" {Environment.ProcessId} \"{Path.GetFileName(zipFilePath)}\"",
        })!;
        await process.WaitForExitAsync().ConfigureAwait(false);
        process.WaitForExit();
    }

    /// <summary>
    /// Fetches the latest release with the specified <paramref name="majorVersion"/>.
    /// </summary>
    /// <param name="majorVersion">The major version of the release.</param>
    /// <param name="cancellationToken">Optionally used to propagate cancellation requests.</param>
    /// <returns>Information related to a release. Returns <see langword="null"/> if not found or an error occurs.</returns>
    public static async Task<IReleaseInfo?> FetchLatest2xReleaseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            const string tagName = "v2.1.189";
            return await UpdateMethods.FetchReleaseAsync(tagName, cancellationToken).ConfigureAwait(false);
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
            Release[] array = (await UpdateMethods.FetchReleasesAsync(cancellationToken).ConfigureAwait(false))
                .Where(release => release.Version.Major == App.Version.Major && release.Version > App.Version)
                .Take(maxReleases)
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
        using FileStream fileStream = File.OpenWrite(fileName);
        await UpdateMethods.GetResourceFileStream("update.ps1").CopyToAsync(fileStream).ConfigureAwait(false);
        return fileName;
    }

    private static async Task<T?> FetchDataAsync<T>(string suffix, CancellationToken cancellationToken)
    {
        try
        {
            string uri = $"https://api.github.com/repos/mburbea/koar-item-editor/{suffix}";
            return await UpdateMethods._client.GetFromJsonAsync<T>(uri, UpdateMethods._jsonOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }
    }

    /// <summary>
    /// Fetches a release by tag asynchronously.
    /// </summary>
    /// <param name="tag">Name of the tag associated with the release.</param>
    /// <param name="cancellationToken">Optionally used to propagate cancellation requests.</param>
    /// <returns>A model representing the GitHub release.  Returns <see langword="null"/> if the release was deleted or an error occurs.</returns>
    private static Task<Release?> FetchReleaseAsync(string tag, CancellationToken cancellationToken) => UpdateMethods.FetchDataAsync<Release>($"releases/tags/{tag}", cancellationToken);

    private static Task<Release[]> FetchReleasesAsync(CancellationToken cancellationToken) => UpdateMethods.FetchDataAsync<Release[]>("releases", cancellationToken)!;

    private static Stream GetResourceFileStream(string name) => Application.GetResourceStream(new($"/Updates/{name}", UriKind.Relative)).Stream;

    private static HttpClient InitializeClient()
    {
        HttpClient client = new(new SocketsHttpHandler { AutomaticDecompression = DecompressionMethods.All });
        client.DefaultRequestHeaders.Accept.TryParseAdd("application/vnd.github.v3+json");
        client.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("gzip, deflate, br");
        client.DefaultRequestHeaders.UserAgent.TryParseAdd("application/vnd.github.v3+json");
        using StreamReader reader = new(UpdateMethods.GetResourceFileStream("github.credentials"));
        client.DefaultRequestHeaders.Authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes(reader.ReadToEnd())) is { Length: > 0 } credentials
            ? new("Basic", credentials)
            : null;
        return client;
    }

    private sealed class Release : IReleaseInfo
    {
        private static readonly Regex _regex = new(@"^v(?<version>\d+\.\d+\.\d+)$", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        private Version? _version;
        private ReleaseAsset? _zipFileAsset;

        public ReleaseAsset[] Assets { get; set; } = Array.Empty<ReleaseAsset>();

        public string Body { get; set; } = string.Empty;

        public bool HasUpdateAsset => this.ZipFileAsset != null;

        public string Name { get; set; } = string.Empty;

        public DateTime PublishedAt { get; set; }

        public string TagName { get; set; } = string.Empty;

        public Version Version => this._version ??= new(this.TagName.Length != 0 && Release._regex.IsMatch(this.TagName) ? this.TagName[1..] : "0.0.0");

        public ReleaseAsset? ZipFileAsset => this._zipFileAsset ??= this.Assets.FirstOrDefault(asset => asset.ContentType == "application/zip");

        public int ZipFileSize => this.ZipFileAsset?.Size ?? 0;

        public string ZipFileUri => this.ZipFileAsset?.BrowserDownloadUrl ?? string.Empty;
    }

    private sealed class ReleaseAsset
    {
        public string BrowserDownloadUrl { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public int Size { get; set; }
    }
}
