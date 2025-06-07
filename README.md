# HelloMCP Project Structure

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/nsmaassel/HelloMCP)
[![Open in VS Code](https://img.shields.io/badge/Open%20in-VS%20Code-007ACC?style=flat&logo=visualstudiocode)](https://vscode.dev/github/nsmaassel/HelloMCP)
[![Open in Visual Studio](https://img.shields.io/badge/Open%20in-Visual%20Studio-5C2D91?style=flat&logo=visualstudio)](https://visualstudio.microsoft.com/downloads/)

> **References:**
>
> - [Customizing the Development Environment for Copilot Coding Agent (GitHub Docs)](https://docs.github.com/en/copilot/customizing-copilot/customizing-the-development-environment-for-copilot-coding-agent)
> - [How to MCP: Everything I Learned Building a Remote MCP Server (SimpleScraper Blog)](https://simplescraper.io/blog/how-to-mcp)

This project demonstrates multiple ways to create and deploy Model Context Protocol (MCP) servers:

- `azure-functions-mcp/` — MCP server using Azure Functions (C# or Python)
- `docker-mcp/` — MCP server using Docker (containerized, portable)
- `dotnet-mcp/` — MCP server using .NET SDK and Semantic Kernel

Each folder will contain a sample implementation and deployment instructions.

## Development Environment Options

Choose your preferred development environment:

- **🚀 GitHub Codespaces** - Full cloud development environment with .NET 8, Azure CLI, and Docker pre-installed
- **💻 VS Code (Web)** - Lightweight browser-based editor for quick edits and reviews  
- **🎯 Visual Studio** - Full IDE experience (requires local installation of Visual Studio 2022)

All environments support the complete .NET MCP server development workflow. Codespaces provides the most seamless experience with zero setup required.

## Project Roadmap

For a comprehensive development plan, milestones, and learning objectives, see the [Project Roadmap](ROADMAP.md) document.
