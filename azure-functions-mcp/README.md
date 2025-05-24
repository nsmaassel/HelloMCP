# Azure Functions Model Context Protocol (MCP) Server

This folder contains an implementation of a Model Context Protocol (MCP) server using Azure Functions. The server provides endpoints for OAuth discovery, session management, and text completions following the MCP specification.

## Features

- Fully MCP compliant server implementation
- OAuth discovery endpoints and authentication
- Session management (creation and closing)
- Text Completion endpoints supporting both:
  - Modern Streamable HTTP
  - Legacy HTTP+SSE transport
- Demo response generation
- Comprehensive test coverage
- Easy deployment to Azure

## Getting Started

### Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) or later
- [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) (for deployment)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/) (optional)

### Local Development

1. Clone the repository:
   ```bash
   git clone https://github.com/nsmaassel/HelloMCP.git
   cd HelloMCP/azure-functions-mcp
   ```

2. Build the solution:
   ```bash
   dotnet build
   ```

3. Run the tests:
   ```bash
   dotnet test
   ```

4. Create a `local.settings.json` file in the src directory:
   ```bash
   cp src/local.settings.json.example src/local.settings.json
   ```

5. Start the Azure Functions host:
   ```bash
   cd src
   func start
   ```

By default, the server will be available at http://localhost:7071.

## API Endpoints

The MCP server implements the following endpoints:

### OAuth Discovery and Authentication

- `GET /.well-known/oauth-authorization-server` - Returns OAuth server metadata
- `GET /oauth/authorize` - OAuth authorization endpoint (placeholder)
- `POST /oauth/token` - OAuth token endpoint (returns demo token)

### Session Management

- `POST /v1/session` - Creates a new MCP session
- `DELETE /v1/session/{sessionId}` - Closes an existing session

### Text Completion

- `POST /v1/text/completions` - Modern Streamable HTTP endpoint
- `POST /v1/text/completions/stream` - Legacy HTTP+SSE endpoint for streaming responses

## Example Request Flow

1. Discover OAuth endpoints:
   ```bash
   curl -X GET http://localhost:7071/.well-known/oauth-authorization-server
   ```

2. Get an access token (demo implementation):
   ```bash
   curl -X POST http://localhost:7071/oauth/token
   ```

3. Create a session:
   ```bash
   curl -X POST http://localhost:7071/v1/session \
     -H "Content-Type: application/json" \
     -d '{"id":"1234","version":"0.1","type":"session-create-request","attributes":{"access_token":"demo_access_token"}}'
   ```

4. Make a text completion request:
   ```bash
   curl -X POST http://localhost:7071/v1/text/completions \
     -H "Content-Type: application/json" \
     -d '{"id":"5678","version":"0.1","type":"text-completion-request","session_id":"SESSION_ID_FROM_STEP_3","inputs":{"prompt":"Hello, world!","temperature":0.7,"max_tokens":100}}'
   ```

5. Close the session:
   ```bash
   curl -X DELETE http://localhost:7071/v1/session/SESSION_ID_FROM_STEP_3
   ```

## Deployment

### Azure Portal Deployment

1. Create a new Function App in the Azure Portal
2. Configure the following application settings:
   - `MCP_OAUTH_ISSUER`: Your function app URL
   - `MCP_AUTH_ENABLED`: Set to "true" in production

3. Deploy the application:
   ```bash
   cd src
   func azure functionapp publish YOUR_FUNCTION_APP_NAME
   ```

### Azure CLI Deployment

```bash
# Login to Azure
az login

# Create a resource group
az group create --name myResourceGroup --location eastus

# Create a storage account
az storage account create --name mystorageaccount --location eastus --resource-group myResourceGroup --sku Standard_LRS

# Create a function app
az functionapp create --resource-group myResourceGroup --consumption-plan-location eastus --runtime dotnet-isolated --functions-version 4 --name YOUR_FUNCTION_APP_NAME --storage-account mystorageaccount

# Deploy the function
cd src
func azure functionapp publish YOUR_FUNCTION_APP_NAME
```

## Environment Variables

- `MCP_OAUTH_ISSUER` - Base URL for OAuth endpoints
- `MCP_AUTH_ENABLED` - Enable or disable authentication (default: false for development)
- `AzureWebJobsStorage` - Azure Storage connection string

## Architecture

The server follows a modular architecture:

- **Functions/** - Azure Functions that handle HTTP endpoints
- **Models/** - Request/response models for MCP protocol
- **Services/** - Business logic and session management

## License

See the root project README for license information.
