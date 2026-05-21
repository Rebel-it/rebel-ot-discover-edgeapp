using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Rebelit.OT.InstallationWizard;

public class GitHubReleaseDownloader(string repo, string accessToken)
{
    private const string GitHubApiBase = "https://api.github.com";

    private HttpClient CreateHttpClient()
    {
        var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        http.DefaultRequestHeaders.UserAgent.ParseAdd("OT-InstallationWizard/1.0");
        return http;
    }

    public async Task<string> DownloadAndExtractLatestReleaseAsync(string extractDir)
    {
        using var http = CreateHttpClient();

        var (downloadUrl, assetName) = await FindLatestZipAssetAsync(http);

        Console.WriteLine($"✓ Found release asset: {assetName}");
        Console.WriteLine("  Downloading...");

        var zipPath = await DownloadAssetAsync(http, downloadUrl, assetName);
        Console.WriteLine($"✓ Downloaded to {zipPath}");

        ExtractZip(zipPath, extractDir);
        Console.WriteLine($"✓ Extracted to {extractDir}");

        return extractDir;
    }

    private async Task<(string downloadUrl, string assetName)> FindLatestZipAssetAsync(HttpClient http)
    {
        var apiUrl = $"{GitHubApiBase}/repos/{repo}/releases/latest";
        var json = await http.GetStringAsync(apiUrl);
        using var doc = JsonDocument.Parse(json);

        var assets = doc.RootElement.GetProperty("assets");
        foreach (var asset in assets.EnumerateArray())
        {
            var name = asset.GetProperty("name").GetString() ?? "";
            if (!name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                continue;

            var assetId = asset.GetProperty("id").GetInt64();
            var downloadUrl = $"{GitHubApiBase}/repos/{repo}/releases/assets/{assetId}";
            return (downloadUrl, name);
        }

        throw new InvalidOperationException("No .zip asset found in the latest release.");
    }

    private static async Task<string> DownloadAssetAsync(HttpClient http, string downloadUrl, string assetName)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
        request.Headers.Accept.ParseAdd("application/octet-stream");

        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var zipPath = Path.Combine(Path.GetTempPath(), assetName);
        var zipBytes = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync(zipPath, zipBytes);

        return zipPath;
    }

    private static void ExtractZip(string zipPath, string extractDir)
    {
        if (Directory.Exists(extractDir))
            Directory.Delete(extractDir, recursive: true);

        ZipFile.ExtractToDirectory(zipPath, extractDir);
    }
}
