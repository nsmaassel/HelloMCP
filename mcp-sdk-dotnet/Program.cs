using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Server;
using McpSdkServer.Tools;
using McpSdkServer.Services;
using System.Net.Http;
using McpSdkServer;

// Check for validation mode
if (args.Length > 0 && args[0].Equals("--validate", StringComparison.OrdinalIgnoreCase))
{
    return await ValidationTool.ValidateServerAsync();
}

var builder = Host.CreateApplicationBuilder(args);

// Enhanced logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Add configuration for better settings management
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// Configure HTTP client with timeout and user agent
builder.Services.AddHttpClient("default", (serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var timeoutSeconds = config.GetValue("HttpClient:TimeoutSeconds", 30);
    var userAgent = config.GetValue("HttpClient:UserAgent", "MCP-SDK-DotNet/1.0.0");
    
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
    client.DefaultRequestHeaders.Add("User-Agent", userAgent);
});

// Add MCP server with tools and enhanced configuration
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

// Add additional services for production readiness
builder.Services.AddMemoryCache();
builder.Services.AddLogging();

// Add custom services
builder.Services.AddSingleton<ConfigurationValidationService>();
builder.Services.AddSingleton<MetricsCollectionService>();

// Register cancellation token for graceful shutdown
builder.Services.AddSingleton<CancellationTokenSource>();

var app = builder.Build();

// Initialize monitoring tools with service provider
MonitoringTools.Initialize(app.Services);

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var config = app.Services.GetRequiredService<IConfiguration>();
var configValidator = app.Services.GetRequiredService<ConfigurationValidationService>();

logger.LogInformation("=== MCP SDK Server Starting ===");
logger.LogInformation("Server Name: {ServerName}", config["McpServer:Name"]);
logger.LogInformation("Server Version: {ServerVersion}", config["McpServer:Version"]);
logger.LogInformation("Environment: {Environment}", builder.Environment.EnvironmentName);
logger.LogInformation("Working Directory: {WorkingDirectory}", Environment.CurrentDirectory);
logger.LogInformation("Runtime: {Runtime}", System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);

// Validate configuration at startup
logger.LogInformation("🔧 Validating server configuration...");
var configValid = configValidator.ValidateConfiguration();
if (!configValid)
{
    logger.LogError("❌ Configuration validation failed. Server may not function correctly.");
}

// Log configuration summary in development
if (builder.Environment.IsDevelopment())
{
    logger.LogDebug("📋 Configuration Summary:");
    var configSummary = configValidator.GetConfigurationSummary();
    logger.LogDebug("{ConfigSummary}", configSummary);
}

// Setup graceful shutdown handling
var cancellationTokenSource = app.Services.GetRequiredService<CancellationTokenSource>();
AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
{
    logger.LogInformation("🛑 Process exit requested, initiating graceful shutdown...");
    cancellationTokenSource.Cancel();
};

Console.CancelKeyPress += (sender, e) =>
{
    logger.LogInformation("🛑 Ctrl+C received, initiating graceful shutdown...");
    e.Cancel = true;
    cancellationTokenSource.Cancel();
};

try
{
    logger.LogInformation("🚀 Starting MCP SDK Server...");
    await app.RunAsync(cancellationTokenSource.Token);
    return 0;
}
catch (OperationCanceledException)
{
    logger.LogInformation("✅ Server shutdown completed gracefully");
    return 0;
}
catch (Exception ex)
{
    logger.LogCritical(ex, "💥 Application terminated unexpectedly");
    return 1;
}
finally
{
    // Dispose services
    var metricsService = app.Services.GetService<MetricsCollectionService>();
    metricsService?.Dispose();
    
    logger.LogInformation("=== MCP SDK Server Stopped ===");
}
