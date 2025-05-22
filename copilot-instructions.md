# Copilot Coding Agent Setup Instructions for HelloMCP

This document provides best practices and workflow guidelines for using GitHub Copilot Coding Agent with the HelloMCP project. It is based on:
- [GitHub Copilot Coding Agent Environment Customization](https://docs.github.com/en/copilot/customizing-copilot/customizing-the-development-environment-for-copilot-coding-agent)
- [How to MCP: The Complete Guide](https://simplescraper.io/blog/how-to-mcp)

## Best Practices

- Use a `.github/workflows/copilot-setup-steps.yml` workflow to preinstall all required tools and dependencies for .NET, Node.js, and Python projects.
- Ensure the workflow job is named `copilot-setup-steps`.
- Keep permissions minimal (usually `contents: read`).
- Use `runs-on: ubuntu-latest` or a larger runner if needed.
- Install .NET SDK, Node.js, and Python as required for MCP server implementations.
- Use `actions/checkout`, `actions/setup-dotnet`, `actions/setup-node`, and `actions/setup-python` as needed.

## Workflow Example

See `.github/workflows/copilot-setup-steps.yml` for a multi-language setup example.

## MCP Server Implementation Guidance

- Follow the [How to MCP](https://simplescraper.io/blog/how-to-mcp) guide for protocol compliance, session management, authentication (OAuth 2.1), and error handling.
- Support both modern (Streamable HTTP) and legacy (HTTP+SSE) MCP transports if possible.
- Implement endpoints for OAuth discovery and token exchange if building a remote MCP server.
- Use environment variables for secrets and API keys; never commit them to source control.
- Add tests and linters for each server type.

## Acceptance Criteria

- Copilot agent can build, test, and run each MCP server type in CI.
- All dependencies are preinstalled or installable via the setup workflow.
- Each server folder contains a README with build and usage instructions.
- The project root README links to this file and the workflow.

## References
- [GitHub Copilot Coding Agent Docs](https://docs.github.com/en/copilot/customizing-copilot/customizing-the-development-environment-for-copilot-coding-agent)
- [How to MCP: The Complete Guide](https://simplescraper.io/blog/how-to-mcp)
- [Model Context Protocol Spec](https://modelcontextprotocol.io/specification/2025-03-26)
