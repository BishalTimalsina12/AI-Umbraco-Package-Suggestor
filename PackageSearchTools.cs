using System.ComponentModel;
using ModelContextProtocol.Server;
using UmbracoPackageSuggest.Services;

namespace UmbracoPackageSuggest.Tools;

[McpServerToolType]
public static class PackageSearchTools
{
    [McpServerTool, Description("Searches for NuGet packages matching a query string. Useful for finding specific packages or packages related to a feature.")]
    public static async Task<string> SearchNuGetPackages(
        NuGetApiClient nuGetClient,
        [Description("The search query (e.g., 'umbraco forms', 'umbraco commerce')")] string query,
        [Description("Maximum number of results to return (default: 20)")] int maxResults = 20)
    {
        try
        {
            var packages = await nuGetClient.SearchPackagesAsync(query, take: maxResults);

            var response = new
            {
                Query = query,
                Results = packages.Select(p => new
                {
                    p.Id,
                    p.Version,
                    p.Description,
                    p.TotalDownloads,
                    p.Tags,
                    p.ProjectUrl
                })
            };

            return System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"Failed to search NuGet packages: {ex.Message}\"}}";
        }
    }

    [McpServerTool, Description("Searches the Umbraco Marketplace for packages and templates matching a query.")]
    public static async Task<string> SearchUmbracoMarketplace(
        UmbracoMarketplaceClient marketplaceClient,
        [Description("The search query")] string query,
        [Description("Maximum number of results to return (default: 20)")] int maxResults = 20)
    {
        try
        {
            var packages = await marketplaceClient.SearchPackagesAsync(query, pageSize: maxResults);

            var response = new
            {
                Query = query,
                Results = packages.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.PackageType,
                    p.Downloads,
                    p.Tags,
                    p.Compatibility,
                    p.Version,
                    p.PackageUrl
                })
            };

            return System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"Failed to search Umbraco Marketplace: {ex.Message}\"}}";
        }
    }

    [McpServerTool, Description("Gets available versions for a specific NuGet package.")]
    public static async Task<string> GetPackageVersions(
        NuGetApiClient nuGetClient,
        [Description("The NuGet package ID")] string packageId)
    {
        try
        {
            var versions = await nuGetClient.GetPackageVersionsAsync(packageId);

            var response = new
            {
                PackageId = packageId,
                Versions = versions.OrderByDescending(v => new Version(v.Split('-')[0])).ToList()
            };

            return System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"Failed to get package versions: {ex.Message}\"}}";
        }
    }

    [McpServerTool, Description("Gets popular Umbraco marketplace templates.")]
    public static async Task<string> GetUmbracoTemplates(
        UmbracoMarketplaceClient marketplaceClient,
        [Description("Maximum number of templates to return (default: 20)")] int maxResults = 20)
    {
        try
        {
            var templates = await marketplaceClient.GetTemplatesAsync(pageSize: maxResults);

            var response = new
            {
                Templates = templates.Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.Description,
                    t.Downloads,
                    t.Tags,
                    t.Compatibility,
                    t.Version,
                    t.PackageUrl
                })
            };

            return System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"Failed to get Umbraco templates: {ex.Message}\"}}";
        }
    }
}