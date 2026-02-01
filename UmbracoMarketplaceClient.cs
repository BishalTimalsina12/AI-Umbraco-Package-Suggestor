using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace UmbracoPackageSuggest.Services;

public class UmbracoPackage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("packageType")]
    public string? PackageType { get; set; }

    [JsonPropertyName("downloads")]
    public long Downloads { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("compatibility")]
    public List<string> Compatibility { get; set; } = new();

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("packageUrl")]
    public string? PackageUrl { get; set; }
}

public class UmbracoMarketplaceResponse
{
    [JsonPropertyName("items")]
    public List<UmbracoPackage>? Items { get; set; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }
}

public class UmbracoMarketplaceClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.marketplace.umbraco.com/api/v1.0";

    public UmbracoMarketplaceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "UmbracoPackageSuggest-MCP/1.0");
    }

    public async Task<List<UmbracoPackage>> GetPackagesAsync(
        string? packageType = null,
        string? orderBy = "MostDownloads",
        int pageSize = 50,
        int pageNumber = 1)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"orderBy={orderBy}",
                $"pageSize={pageSize}",
                $"pageNumber={pageNumber}",
                "fields=id,name,description,packageType,downloads,tags,compatibility,version,packageUrl"
            };

            if (!string.IsNullOrEmpty(packageType))
            {
                queryParams.Add($"packageType={Uri.EscapeDataString(packageType)}");
            }

            var url = $"{BaseUrl}/packages?{string.Join("&", queryParams)}";
            var response = await _httpClient.GetFromJsonAsync<UmbracoMarketplaceResponse>(url);
            return response?.Items ?? new List<UmbracoPackage>();
        }
        catch
        {
            return new List<UmbracoPackage>();
        }
    }

    public async Task<List<UmbracoPackage>> SearchPackagesAsync(string query, int pageSize = 50)
    {
        try
        {
            var url = $"{BaseUrl}/packages?search={Uri.EscapeDataString(query)}&pageSize={pageSize}&fields=id,name,description,packageType,downloads,tags,compatibility,version,packageUrl";
            var response = await _httpClient.GetFromJsonAsync<UmbracoMarketplaceResponse>(url);
            return response?.Items ?? new List<UmbracoPackage>();
        }
        catch
        {
            return new List<UmbracoPackage>();
        }
    }

    public async Task<List<UmbracoPackage>> GetTemplatesAsync(int pageSize = 50)
    {
        return await GetPackagesAsync(packageType: "Template", pageSize: pageSize);
    }

    public async Task<UmbracoPackage?> GetPackageAsync(string packageId)
    {
        try
        {
            var results = await SearchPackagesAsync($"id:{packageId}", pageSize: 1);
            return results.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }
}
