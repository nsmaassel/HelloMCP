# Docker Model Context Protocol (MCP) Server

This folder contains a Docker-containerized implementation of the Model Context Protocol (MCP) server, based on the proven .NET implementation in `../dotnet-mcp/`. This approach provides a portable, consistent deployment option for any environment that supports Docker.

## Features

- **Containerized .NET MCP Server**: Same proven functionality as the standalone .NET version
- **Production-ready**: Optimized multi-stage Docker build
- **Easy deployment**: Single command deployment with Docker Compose
- **Environment flexibility**: Works on Windows, Linux, macOS with Docker
- **VS Code integration**: Pre-configured for GitHub Copilot integration
- **Health monitoring**: Built-in health checks and monitoring endpoints

## Architecture

```
┌─────────────────────────────────────┐
│             Docker Host             │
│  ┌─────────────────────────────────┐│
│  │        MCP Container            ││
│  │  ┌─────────────────────────────┐││
│  │  │     .NET 8 Runtime         │││
│  │  │  ┌─────────────────────────┐│││
│  │  │  │    MCP Server App       ││││
│  │  │  │  • OAuth Discovery      ││││
│  │  │  │  • Session Management   ││││
│  │  │  │  • Text Completions     ││││
│  │  │  │  • Statistical Analysis ││││
│  │  │  └─────────────────────────┘│││
│  │  └─────────────────────────────┘││
│  └─────────────────────────────────┘│
│              Port 5090               │
└─────────────────────────────────────┘
```

## Quick Start

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) 20.10 or later
- [Docker Compose](https://docs.docker.com/compose/install/) (usually included with Docker Desktop)

> **🚀 Zero-Script Setup**: No build scripts, batch files, or shell scripts are needed! Docker Compose handles everything - building, testing, and running the MCP server. The setup is fully platform-agnostic and works identically on Windows, macOS, and Linux.

### Using Docker Compose (Recommended)

```bash
# Navigate to the HelloMCP project root
cd HelloMCP

# Start the MCP server using Docker Compose
docker-compose -f docker-mcp/docker-compose.yml up -d

# View logs
docker-compose -f docker-mcp/docker-compose.yml logs -f mcp-server

# Stop the server
docker-compose -f docker-mcp/docker-compose.yml down
```

### Alternative: Direct Docker Commands

```bash
# Navigate to the HelloMCP project root
cd HelloMCP

# Build the image (context is the project root)
docker build -f docker-mcp/Dockerfile -t hellomcp-docker .

# Run the container
docker run -d -p 5090:80 --name mcp-server hellomcp-docker

# View logs
docker logs -f mcp-server

# Stop and remove
docker stop mcp-server && docker rm mcp-server
```

The server will be available at:
- **API Base URL**: `http://localhost:5090/v1`
- **Swagger UI**: `http://localhost:5090/swagger`
- **Health Check**: `http://localhost:5090/health`

> **✅ VERIFIED:** All endpoints tested and working. See [TESTING_RESULTS.md](./TESTING_RESULTS.md) for detailed test results including Docker-specific performance metrics.

## VS Code Integration

### Using with GitHub Copilot

The Docker MCP server is pre-configured for VS Code integration. The `.vscode/mcp.json` configuration includes:

```json
{
  "servers": {
    "mcp-server-docker": {
      "command": "docker",
      "args": [
        "run", "-i", "--rm", 
        "-p", "5090:80",
        "hellomcp-docker:latest"
      ]
    }
  }
}
```

### Integration Steps

1. **Ensure the Docker image is built**:
   ```bash
   cd docker-mcp
   docker build -t hellomcp-docker:latest .
   ```

2. **Verify VS Code can connect**:
   ```bash
   # Test the Docker MCP server
   curl http://localhost:5090/v1/initialize
   ```

3. **Use in Copilot Chat**: The server provides the same capabilities as the .NET version:
   - Text completions and analysis
   - Statistical analysis of player data
   - Session-based interactions

## API Documentation

The Docker MCP server provides the same API as the .NET version. Key endpoints:

### Core MCP Endpoints
- `POST /v1/initialize` - MCP protocol initialization
- `POST /v1/session` - Create new session
- `DELETE /v1/session/{sessionId}` - Close session

### Text & Analysis
- `POST /v1/text/completions` - Text completions and statistical analysis
- `POST /v1/text/completions/stream` - Streaming responses (SSE)

### Discovery & Health
- `GET /.well-known/oauth-authorization-server` - OAuth discovery
- `GET /health` - Container health status
- `GET /swagger` - API documentation

For complete API documentation, see the [.NET MCP Server README](../dotnet-mcp/README.md) or visit the Swagger UI when the container is running.

## Configuration

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Runtime environment |
| `ASPNETCORE_URLS` | `http://+:80` | Server binding URLs |
| `MCP_OAUTH_ISSUER` | Auto-detected | OAuth issuer URL |
| `MCP_AUTH_ENABLED` | `false` | Enable authentication |

### Docker Compose Configuration

Customize `docker-compose.yml` for your environment:

```yaml
version: '3.8'
services:
  mcp-server:
    build: .
    ports:
      - "5090:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - MCP_AUTH_ENABLED=false
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3
```

## Development

### Local Development with Docker

```bash
# Development with live reload (requires volume mounting)
docker-compose -f docker-compose.dev.yml up

# Run tests in container
docker-compose exec mcp-server dotnet test

# Access container shell
docker-compose exec mcp-server /bin/bash
```

### Building for Different Architectures

```bash
# Build for multiple platforms
docker buildx build --platform linux/amd64,linux/arm64 -t hellomcp-docker:latest .

# Build for ARM64 (Apple Silicon)
docker buildx build --platform linux/arm64 -t hellomcp-docker:arm64 .
```

## Deployment Options

### Local Development
- Use Docker Compose for easy local testing
- Volume mount source code for development

### Production Deployment
- Use Docker Swarm or Kubernetes for orchestration
- Configure proper SSL/TLS termination
- Set up monitoring and logging
- Use environment-specific configurations

### Cloud Platforms
- **Azure Container Instances**: Direct container deployment
- **AWS ECS/Fargate**: Managed container service
- **Google Cloud Run**: Serverless container platform
- **DigitalOcean App Platform**: Simple container hosting

## Comparison with Other Implementations

| Feature | Docker MCP | .NET MCP | Azure Functions MCP |
|---------|------------|----------|-------------------|
| **Deployment** | Container-based | Direct executable | Serverless |
| **Portability** | ✅ High | ⚠️ .NET required | ⚠️ Azure-specific |
| **Scaling** | Manual/Orchestrator | Manual | Automatic |
| **Cold Start** | Fast | Fastest | Slow |
| **Resource Usage** | Medium | Low | Variable |
| **Cost** | Predictable | Lowest | Pay-per-use |

## Troubleshooting

### Common Issues

1. **Port Already in Use**
   ```bash
   # Change port in docker-compose.yml
   ports:
     - "5091:80"  # Use different external port
   ```

2. **Container Won't Start**
   ```bash
   # Check logs
   docker-compose logs mcp-server
   
   # Verify image built correctly
   docker images | grep hellomcp-docker
   ```

3. **VS Code Integration Issues**
   ```bash
   # Ensure image is tagged correctly
   docker tag hellomcp-docker:latest hellomcp-docker:latest
   
   # Test manual container run
   docker run -p 5090:80 hellomcp-docker:latest
   ```

### Performance Tuning

- **Memory**: Adjust Docker memory limits in compose file
- **CPU**: Configure CPU limits for consistent performance
- **Networking**: Use bridge networks for multiple containers
- **Storage**: Use volumes for persistent data if needed

## Security Considerations

- Container runs as non-root user by default
- Only essential ports are exposed
- Environment variables for sensitive configuration
- Regular base image updates for security patches

## Contributing

This Docker implementation follows the same patterns as the .NET MCP server. For contributing:

1. Maintain compatibility with the base .NET implementation
2. Follow Docker best practices for builds and deployment
3. Update both Dockerfile and docker-compose.yml as needed
4. Test against the same MCP protocol compliance tests

## License

See the root project README for license information.

---

**Next Steps**: 
- Try the [Azure Functions MCP](../azure-functions-mcp/) for serverless deployment
- Review the [.NET MCP](../dotnet-mcp/) for direct deployment
- Check the [project roadmap](../ROADMAP.md) for additional implementations
