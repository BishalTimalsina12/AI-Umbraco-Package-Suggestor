using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using UmbracoPackageSuggest.Services.Analysis;

namespace UmbracoPackageSuggest.Services;

public static class McpServerSetup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Register HTTP client
        services.AddHttpClient();

        // Register core services
        services.AddSingleton<ProjectAnalyzer>(sp =>
        {
            return new ProjectAnalyzer();
        });

        services.AddSingleton<NuGetApiClient>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new NuGetApiClient(httpClientFactory.CreateClient());
        });

        services.AddSingleton<UmbracoMarketplaceClient>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new UmbracoMarketplaceClient(httpClientFactory.CreateClient());
        });

        services.AddSingleton<RecommendationEngine>(sp =>
        {
            var nuGetClient = sp.GetRequiredService<NuGetApiClient>();
            var marketplaceClient = sp.GetRequiredService<UmbracoMarketplaceClient>();
            return new RecommendationEngine(nuGetClient, marketplaceClient);
        });
    }

    public static void ConfigureMcpServer(IServiceCollection services)
    {
        services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();
    }
}