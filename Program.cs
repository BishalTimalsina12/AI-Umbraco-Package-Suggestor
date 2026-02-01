using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UmbracoPackageSuggest.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Configure services and MCP server
McpServerSetup.ConfigureServices(builder.Services);
McpServerSetup.ConfigureMcpServer(builder.Services);

await builder.Build().RunAsync();
