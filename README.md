# HelloMCP Project Structure

> **References:**
> - [Customizing the Development Environment for Copilot Coding Agent (GitHub Docs)](https://docs.github.com/en/copilot/customizing-copilot/customizing-the-development-environment-for-copilot-coding-agent)
> - [How to MCP: Everything I Learned Building a Remote MCP Server (SimpleScraper Blog)](https://simplescraper.io/blog/how-to-mcp)

This project demonstrates multiple ways to create and deploy Model Context Protocol (MCP) servers:

- `azure-functions-mcp/` — MCP server using Azure Functions (C# or Python)
- `docker-mcp/` — **✅ COMPLETE** - MCP server using Docker (containerized, portable)
- `dotnet-mcp/` — **✅ COMPLETE** - MCP server using .NET SDK and Semantic Kernel

Each folder contains a complete implementation with deployment instructions and VS Code integration.

## Implementation Status

| Implementation | Status | Features | Deployment |
|---------------|--------|----------|------------|
| **Docker MCP** | ✅ Complete | Container-based, portable, production-ready | `docker-compose up` |
| **.NET MCP** | ✅ Complete | Native .NET, fastest performance, full-featured | `dotnet run` |
| **Azure Functions MCP** | 🚧 In Progress | Serverless, auto-scaling, cloud-native | Azure deployment |

## Quick Start

### Docker MCP Server (Recommended for Production)
```bash
# From project root
docker-compose -f docker-mcp/docker-compose.yml up -d
```

### .NET MCP Server (Recommended for Development)
```bash
cd dotnet-mcp/McpServer
dotnet run
```

Both servers provide identical functionality and are accessible at `http://localhost:5090/v1`.

## Project Roadmap

For a comprehensive development plan, milestones, and learning objectives, see the [Project Roadmap](ROADMAP.md) document.
