# Docker Model Context Protocol (MCP) Server

This folder contains a Node.js implementation of the Model Context Protocol (MCP) server designed to run in Docker containers. It enables applications to communicate with AI models using a standardized protocol.

## Features

- Supports Model Context Protocol specification
- Provides session management
- Implements both modern Streamable HTTP and legacy HTTP+SSE transport options
- Includes OAuth discovery and authentication
- Error handling and logging
- Containerized for easy deployment
- Includes automated tests

## Getting Started

### Prerequisites

- [Node.js](https://nodejs.org/) (v16 or later)
- [Docker](https://www.docker.com/get-started)

### Running Locally (Development)

1. Clone the repository:
   ```bash
   git clone https://github.com/nsmaassel/HelloMCP.git
   cd HelloMCP/docker-mcp
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the server:
   ```bash
   npm start
   ```

4. For development with auto-restart:
   ```bash
   npm run dev
   ```

5. Run tests:
   ```bash
   npm test
   ```

By default, the server will be available at http://localhost:3000

## Deployment

### Docker

1. Build the Docker image:
   ```bash
   cd docker-mcp
   docker build -t mcp-server .
   ```

2. Run the container:
   ```bash
   docker run -p 3000:3000 mcp-server
   ```

3. Access the server at http://localhost:3000

### Docker Compose

You can use Docker Compose to run the server. Create a `docker-compose.yml` file with:

```yaml
version: '3'
services:
  mcp-server:
    build: .
    ports:
      - "3000:3000"
    environment:
      - NODE_ENV=production
      - PORT=3000
```

Then run:
```bash
docker-compose up
```

### Container Registry

1. Tag your image:
   ```bash
   docker tag mcp-server:latest your-registry/mcp-server:latest
   ```

2. Push to a registry:
   ```bash
   docker push your-registry/mcp-server:latest
   ```

## Usage Guide

### API Endpoints

The MCP server implements the following key endpoints:

1. **OAuth Discovery**
   - `GET /.well-known/oauth-authorization-server` - Returns OAuth server metadata

2. **Session Management**
   - `POST /v1/session` - Creates a new session
   - `DELETE /v1/session/{sessionId}` - Closes an existing session

3. **Text Completion**
   - `POST /v1/text/completions` - Modern Streamable HTTP endpoint
   - `POST /v1/text/completions/stream` - Legacy HTTP+SSE endpoint

### Example Request Flow

1. Create a session:
   ```bash
   curl -X POST http://localhost:3000/v1/session \
     -H "Content-Type: application/json" \
     -d '{
       "id": "req-123",
       "version": "0.1",
       "type": "session-create-request",
       "attributes": {}
     }'
   ```

2. Use the session for text completion:
   ```bash
   curl -X POST http://localhost:3000/v1/text/completions \
     -H "Content-Type: application/json" \
     -d '{
       "id": "req-456",
       "version": "0.1",
       "type": "text-completion-request",
       "session_id": "SESSION_ID_FROM_STEP_1",
       "inputs": {
         "prompt": "Write a hello world program",
         "temperature": 0.7,
         "max_tokens": 500
       }
     }'
   ```

3. Close the session:
   ```bash
   curl -X DELETE http://localhost:3000/v1/session/SESSION_ID_FROM_STEP_1
   ```

## Environment Variables

- `PORT` - Server port (default: 3000)
- `NODE_ENV` - Environment (development/production)
- `LOG_LEVEL` - Logging level (default: info)

## Architecture

The server is built using Express.js and follows a modular architecture:

- **Controllers** - Handle HTTP endpoints and protocol implementation
- **Models** - Define request/response models for MCP
- **Services** - Provide session management and business logic

## License

See the root `README.md` for license information.
