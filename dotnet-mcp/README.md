# .NET Model Context Protocol (MCP) Server

This folder contains a .NET implementation of the Model Context Protocol (MCP) server, which enables applications to communicate with AI models using a standardized protocol.

## Features

- Supports Model Context Protocol specification
- Provides session management
- Implements both modern Streamable HTTP and legacy HTTP+SSE transport options
- Includes OAuth discovery and authentication
- Error handling and logging
- Full API documentation with Swagger UI
- Includes automated tests

## Getting Started

### Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) or later

### Building the Project

1. Clone the repository:
   ```bash
   git clone https://github.com/nsmaassel/HelloMCP.git
   cd HelloMCP/dotnet-mcp
   ```

2. Build the solution:
   ```bash
   dotnet build
   ```

3. Run the tests:
   ```bash
   dotnet test
   ```

### Running the MCP Server

```bash
cd McpServer
dotnet run
```

By default, the server will be available at:
- HTTP: http://localhost:5086
- HTTPS: https://localhost:7240
- Swagger UI: https://localhost:7240/swagger

## Usage Guide

### API Endpoints

The MCP server implements the following key endpoints:

1. **OAuth Discovery**
   - `GET /.well-known/oauth-authorization-server` - Returns OAuth server metadata

2. **Session Management**
   - `POST /v1/session` - Creates a new session
   - `DELETE /v1/session/{sessionId}` - Closes an existing session


3. **Text Completion & Analysis**
   - `POST /v1/text/completions` - Main endpoint for both text completions and deterministic/statistical analysis
   - `POST /v1/text/completions/stream` - Streaming endpoint (SSE) for both modes

## Example Request Flows

### 1. Create a Session
```json
POST /v1/session
{
  "id": "request-123",
  "version": "0.1",
  "type": "session-create-request",
  "attributes": {
    "access_token": "your_token_here"
  }
}
```

### 2a. Text Completion Request (Prompt)
```json
POST /v1/text/completions
{
  "id": "request-456",
  "version": "0.1",
  "type": "text-completion-request",
  "session_id": "session_id_from_step_1",
  "inputs": {
    "prompt": "The capital of France is",
    "temperature": 0.7,
    "max_tokens": 100
  }
}
```

### 2b. Deterministic Stat Analysis Request (Tennis Example)
```json
POST /v1/text/completions
{
  "id": "request-789",
  "version": "0.1",
  "type": "text-completion-request",
  "session_id": "session_id_from_step_1",
  "inputs": {
    "stats": {
      "player1": { "aces": 5, "points": 20 },
      "player2": { "aces": 3, "points": 25 }
    }
  }
}
```

### 2c. Streaming (SSE) Request
Use the same request body as above, but POST to `/v1/text/completions/stream`.

### 3. Close the Session
```json
DELETE /v1/session/{session_id_from_step_1}
```


## Deployment

### Docker

```bash
cd dotnet-mcp
docker build -t mcp-server .
docker run -p 5000:80 mcp-server
```

### Azure App Service

You can deploy the MCP server to Azure App Service using the Azure CLI:

```bash
az webapp up --sku F1 --name your-mcp-server --resource-group your-resource-group
```

### Environment Variables

- `ASPNETCORE_ENVIRONMENT` - Set to `Development` for detailed errors and Swagger UI
- `ASPNETCORE_URLS` - Override default HTTP/HTTPS ports
- `MCP_OAUTH_ISSUER` - Custom OAuth issuer URL (optional)

## Architecture

The server is built using ASP.NET Core and follows a modular architecture:

- **Controllers** - Handle HTTP endpoints and protocol implementation
- **Models** - Define request/response models for MCP
- **Services** - Provide session management and business logic

## License

See the root `README.md` for license information.
