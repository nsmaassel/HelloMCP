# MCP Server Integration Guide

## VS Code Insiders Setup

Your `mcp.json` is configured with the .NET MCP SDK server. Here are both deployment options:

### Option 1: Direct .NET Execution (Recommended for Development)

```json
{
  "mcpServers": {
    "DotNet MCP SDK": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "c:\\Workspace\\AI\\MCPs\\HelloMCP\\mcp-sdk-dotnet"
      ],
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

**Pros:**
- ✅ Fast startup (~2-3 seconds)
- ✅ Direct system access for monitoring tools
- ✅ Easy debugging and development
- ✅ Lower resource usage

**Requirements:**
- .NET 8.0 SDK installed
- Project built (`dotnet build`)

### Option 2: Docker Execution (Production-Ready)

```json
{
  "mcpServers": {
    "DotNet MCP SDK (Docker)": {
      "command": "docker",
      "args": [
        "run",
        "-i",
        "--rm",
        "--name",
        "mcp-sdk-server",
        "mcp-sdk-server:latest"
      ]
    }
  }
}
```

**Pros:**
- ✅ Complete isolation
- ✅ No host dependencies
- ✅ Consistent environment
- ✅ Production deployment ready

**Requirements:**
- Docker installed
- Image built (`docker build -t mcp-sdk-server .`)

## Testing the Integration

### Automated Verification (Recommended)

Run the comprehensive verification script:

```powershell
.\verify-mcp-integration.ps1
```

This script checks:
- ✅ .NET SDK installation
- ✅ Project build status  
- ✅ Server validation tests
- ✅ mcp.json configuration
- ✅ Process conflicts
- ✅ Server startup capability

### Manual Verification Steps

1. **Ensure VS Code Insiders is closed**
2. **Verify the server builds:**

   ```bash
   cd c:\Workspace\AI\MCPs\HelloMCP\mcp-sdk-dotnet
   dotnet build
   ```

3. **Test server startup:**

   ```bash
   dotnet run --validate
   ```

4. **Open VS Code Insiders**
5. **Check MCP extension logs** for connection status

### Important: How VS Code Manages MCP Servers

**VS Code will automatically:**
- ✅ Start the MCP server when it needs to use tools
- ✅ Stop the server when it's no longer needed  
- ✅ Restart the server if it crashes
- ✅ Handle all stdio communication

**You don't need to:**
- ❌ Manually start the server
- ❌ Keep the server running in background
- ❌ Manage server processes yourself

## Available Tools

Once connected, you'll have access to 20+ tools across 6 categories:

### 🔧 System Tools
- `GetSystemInfo` - System information and specs
- `MonitorResources` - Real-time resource monitoring
- `ForceGarbageCollection` - Memory optimization

### 📝 Text & Analysis
- `Echo` - Simple echo for testing
- `CompleteText` - Text completion with analysis
- `GetServerInfo` - Server capabilities and version

### 🌐 Web Utilities
- `FetchAndAnalyze` - HTTP request analysis
- `HealthCheck` - Multi-URL health monitoring
- `NetworkDiagnostics` - DNS and connectivity testing

### 🔍 Diagnostics
- `GetServerDiagnostics` - Complete server health
- `ListAvailableTools` - Dynamic tool discovery
- `PerformHealthCheck` - Comprehensive health checks

### 📊 Monitoring
- `GetPerformanceMetrics` - Server performance stats
- `GetConfigurationStatus` - Configuration validation
- `OptimizeMemory` - Memory optimization with stats

### ⚡ Advanced Processing
- `ProcessStructuredData` - JSON data processing
- `AnalyzeWebContent` - Web content analysis
- `GenerateSystemReport` - Detailed system reports

## Troubleshooting

### Server Won't Start
```bash
# Check for running instances
Get-Process -Name "McpSdkServer" | Stop-Process -Force

# Rebuild
dotnet clean && dotnet build

# Test validation
dotnet run --validate
```

### VS Code Can't Connect

1. **Run the verification script:** `.\verify-mcp-integration.ps1`
2. **Check MCP extension is enabled** in VS Code Insiders
3. **Verify `mcp.json` path** is correct: `%USERPROFILE%\.cursor\mcp.json`
4. **Check VS Code Insiders logs** (Help → Toggle Developer Tools → Console)
5. **Try restarting VS Code Insiders**
6. **Ensure no antivirus is blocking** the dotnet process
7. **Check Windows permissions** for the project directory

### How to Check if MCP Server is Working in VS Code

1. **Open Command Palette** (`Ctrl+Shift+P`)
2. **Look for MCP-related commands** or try asking Copilot to use tools
3. **Check Output panel** for MCP extension logs
4. **Use Copilot Chat** and ask it to "use the GetSystemInfo tool"
5. **Monitor Task Manager** for `McpSdkServer.exe` process when tools are used

### Performance Issues
- Use `dotnet run` instead of Docker for development
- Ensure ASPNETCORE_ENVIRONMENT=Development for detailed logging
- Check system resources with monitoring tools

---

**Current Status:** ✅ Ready for VS Code Insiders integration!
