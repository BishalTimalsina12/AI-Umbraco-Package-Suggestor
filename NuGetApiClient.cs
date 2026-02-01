using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UmbracoPackageSuggest.Services;

public class NuGetPackageInfo
{
    public string Id { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long TotalDownloads { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? ProjectUrl { get; set; }
}

public class NuGetSearchResult
{
    [JsonPropertyName("data")]
    public List<NuGetPackageInfo>? Data { get; set; }
}

public class NuGetApiClient
{
    private readonly HttpClient _httpClient;
    private const string SearchBaseUrl = "https://azuresearch-usnc.nuget.org/query";
    private const string PackageBaseUrl = "https://api.nuget.org/v3-flatcontainer";

    public NuGetApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "UmbracoPackageSuggest-MCP/1.0");
    }

    public async Task<List<NuGetPackageInfo>> SearchPackagesAsync(string query, int take = 20, int skip = 0)
    {
        try
        {
            var url = $"{SearchBaseUrl}?q={Uri.EscapeDataString(query)}&take={take}&skip={skip}&prerelease=false";
            var response = await _httpClient.GetFromJsonAsync<NuGetSearchResult>(url);
            return response?.Data ?? new List<NuGetPackageInfo>();
        }
        catch
        {
            return new List<NuGetPackageInfo>();
        }
    }

    public async Task<List<string>> GetPackageVersionsAsync(string packageId)
    {
        try
        {
            var url = $"{PackageBaseUrl}/{packageId.ToLowerInvariant()}/index.json";
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(url);
            
            if (response.TryGetProperty("versions", out var versions))
            {
                return versions.EnumerateArray()
                    .Select(v => v.GetString() ?? string.Empty)
                    .Where(v => !string.IsNullOrEmpty(v))
                    .ToList();
            }
        }
        catch
        {
            // Ignore errors
        }

        return new List<string>();
    }

    public async Task<NuGetPackageInfo?> GetPackageInfoAsync(string packageId)
    {
        try
        {
            var results = await SearchPackagesAsync($"packageid:{packageId}", take: 1);
            return results.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }
}
