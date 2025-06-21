namespace McpSdkServer.Configuration;

/// <summary>
/// Configuration options for the MCP server
/// </summary>
public class McpServerOptions
{
    public const string SectionName = "McpServer";
    
    public string Name { get; set; } = "MCP SDK .NET Server";
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = "Production-ready MCP server using official Microsoft SDK";
    public int MaxConcurrentRequests { get; set; } = 100;
    public int RequestTimeoutSeconds { get; set; } = 30;
    public bool EnableMetrics { get; set; } = true;
    public bool EnableHealthChecks { get; set; } = true;
}

/// <summary>
/// Configuration options for HTTP client
/// </summary>
public class HttpClientOptions
{
    public const string SectionName = "HttpClient";
    
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxConcurrentConnections { get; set; } = 50;
    public string UserAgent { get; set; } = "MCP-SDK-DotNet/1.0.0";
}
