# MCP SDK .NET Server

A production-ready Model Context Protocol (MCP) server implementation using the official Microsoft MCP SDK. This server demonstrates advanced capabilities with comprehensive tooling, monitoring, and Docker support.

## Overview

This is an **official SDK-based** implementation that leverages Microsoft's ModelContextProtocol NuGet packages. It provides a robust, fully-featured MCP server with attribute-based tool registration, comprehensive diagnostics, and production-ready deployment capabilities.

## Features

### Core MCP Capabilities
- ✅ **Official MCP SDK**: Built with Microsoft's ModelContextProtocol NuGet packages
- ✅ **Stdio Transport**: Full JSON-RPC over stdio protocol support
- ✅ **Attribute-based Tools**: Clean, declarative tool registration with `[McpServerTool]`
- ✅ **Dependency Injection**: Full ASP.NET Core DI container support
- ✅ **Enhanced Logging**: Structured logging with configurable levels

### Tool Categories

#### Text & Analysis Tools (`TextCompletionTools`)
- **CompleteText**: Text completion with statistical analysis support
- **AnalyzeData**: Comprehensive data analysis with JSON processing
- **Echo**: Simple echo tool for testing connectivity
- **GetServerInfo**: Server capabilities and version information

#### Advanced Processing (`AdvancedTools`)
- **AnalyzeWebContent**: Download and analyze web content with multiple analysis types
- **GenerateSystemReport**: Detailed system and performance reporting
- **ProcessStructuredData**: Advanced JSON data processing with aggregation, filtering, and transformation

#### System Monitoring (`SystemTools`)
- **GetSystemInfo**: Comprehensive system information (OS, runtime, hardware)
- **MonitorResources**: Real-time resource monitoring with alerts
- **ForceGarbageCollection**: Memory management and optimization

#### Web Utilities (`WebTools`)
- **FetchAndAnalyze**: HTTP request analysis with content, headers, and status
- **HealthCheck**: Multi-URL health monitoring
- **DownloadAndSave**: Content downloading with metadata analysis
- **NetworkDiagnostics**: DNS resolution and connectivity testing

#### Diagnostics & Health (`DiagnosticTools`)
- **GetServerDiagnostics**: Complete server health and configuration analysis
- **ListAvailableTools**: Dynamic tool discovery and documentation
- **PerformHealthCheck**: Comprehensive health checks including external dependencies
- **ValidateProtocolCompliance**: MCP protocol compliance validation

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Docker (optional, for containerized deployment)

### Running Locally

1. **Clone and navigate to the SDK server directory:**
```bash
cd mcp-sdk-dotnet
```

2. **Build the project:**
```bash
dotnet build
```

3. **Run the server:**
```bash
dotnet run
```

The server will start and listen on stdio for MCP protocol messages.

### Docker Deployment

#### Build Docker Image
```bash
docker build -t mcp-sdk-server:latest .
```

#### Run with Docker
```bash
docker run -it --rm mcp-sdk-server:latest
```

#### Docker Compose (Production)
```bash
docker-compose up
```

#### Docker Compose (Development with hot reload)
```bash
docker-compose -f docker-compose.dev.yml up
```

## Configuration

### Application Settings

The server supports comprehensive configuration through `appsettings.json`:

```json
{
  "McpServer": {
    "Name": "MCP SDK .NET Server",
    "Version": "1.0.0",
    "Description": "Production-ready MCP server using official Microsoft SDK",
    "MaxConcurrentRequests": 100,
    "RequestTimeoutSeconds": 30,
    "EnableMetrics": true,
    "EnableHealthChecks": true
  },
  "HttpClient": {
    "TimeoutSeconds": 30,
    "MaxConcurrentConnections": 50,
    "UserAgent": "MCP-SDK-DotNet/1.0.0"
  }
}
```

### Environment Variables

- `ASPNETCORE_ENVIRONMENT`: Environment (Development/Production)
- `DOTNET_RUNNING_IN_CONTAINER`: Container detection flag

## Architecture

### Project Structure
```
mcp-sdk-dotnet/
├── Program.cs                      # Application entry point with enhanced DI
├── McpSdkServer.csproj            # Project file with SDK dependencies
├── appsettings.json               # Production configuration
├── appsettings.Development.json   # Development configuration
├── Dockerfile                     # Multi-stage Docker build
├── docker-compose.yml             # Production Docker Compose
├── docker-compose.dev.yml         # Development Docker Compose
├── Configuration/
│   └── McpServerOptions.cs        # Strongly-typed configuration
└── Tools/
    ├── AdvancedTools.cs           # Web content and data processing
    ├── DiagnosticTools.cs         # Server diagnostics and health
    ├── SystemTools.cs             # System monitoring and resources
    ├── TextCompletionTools.cs     # Text processing and analysis
    └── WebTools.cs                # HTTP utilities and networking
```

### Key Components

1. **SDK Integration**: Uses official `ModelContextProtocol` and `ModelContextProtocol.AspNetCore` packages
2. **Tool Registration**: Automatic discovery of tools marked with `[McpServerToolType]` and `[McpServerTool]`
3. **Dependency Injection**: Full ASP.NET Core DI with scoped services
4. **Configuration Management**: Strongly-typed options pattern
5. **Logging**: Structured logging with stderr output for MCP compatibility

## Development

### Adding New Tools

1. Create a new static class in the `Tools` directory
2. Decorate with `[McpServerToolType]`
3. Add static methods with `[McpServerTool]` and `[Description]` attributes:

```csharp
[McpServerToolType]
public static class MyTools
{
    [McpServerTool]
    [Description("Description of what this tool does")]
    public static async Task<string> MyTool(
        [Description("Parameter description")] string parameter)
    {
        // Tool implementation
        return "Result";
    }
}
```

### Building and Testing

```bash
# Build
dotnet build

# Run tests (if added)
dotnet test

# Run with different configuration
dotnet run --environment Development
```

## Comparison with Custom Implementation

| Feature | Custom Version (dotnet-mcp) | SDK Version (mcp-sdk-dotnet) |
|---------|---------------------------|-------------------------------|
| **Protocol Implementation** | Manual JSON-RPC handling | Official Microsoft SDK |
| **Tool Registration** | Manual endpoint registration | Attribute-based auto-discovery |
| **Transport** | HTTP/REST with manual MCP mapping | Native stdio/JSON-RPC |
| **Configuration** | Basic appsettings | Strongly-typed options pattern |
| **Dependency Injection** | ASP.NET Core controllers | MCP SDK with full DI |
| **Diagnostics** | Basic health checks | Comprehensive monitoring suite |
| **Docker Support** | Basic containerization | Multi-stage optimized builds |
| **Logging** | Standard ASP.NET logging | MCP-compatible stderr logging |
| **Production Readiness** | Development/demo focused | Production-ready with monitoring |

## Monitoring and Health

### Built-in Health Checks
- Memory usage monitoring
- External dependency validation
- MCP protocol compliance validation
- Configuration validation

### Diagnostic Endpoints (via Tools)
- `GetServerDiagnostics`: Complete server status
- `ListAvailableTools`: Dynamic tool inventory
- `PerformHealthCheck`: Multi-component health validation
- `ValidateProtocolCompliance`: Protocol compliance report

## Deployment

### Docker Production Deployment

The included Dockerfile uses multi-stage builds for optimized production images:

1. **Build Stage**: Full SDK for compilation
2. **Runtime Stage**: Minimal ASP.NET runtime
3. **Security**: Non-root user execution
4. **Health Checks**: Built-in container health monitoring

### Kubernetes Ready

The server is designed for cloud-native deployment with:
- Health check endpoints
- Graceful shutdown handling
- Resource monitoring
- Configurable logging levels

## Contributing

1. Follow the existing tool pattern with attributes
2. Include comprehensive descriptions for all parameters
3. Implement proper error handling and validation
4. Add appropriate logging for debugging
5. Update documentation for new capabilities

## License

This project follows the same license as the parent HelloMCP project.

---

**Note**: This SDK version demonstrates production-ready MCP server implementation using Microsoft's official SDK, complementing the custom implementation for learning and comparison purposes.
