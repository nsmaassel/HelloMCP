# Multi-stage build for optimized production container
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files from the parent dotnet-mcp directory
COPY ../dotnet-mcp/McpServer/McpServer.csproj McpServer/
COPY ../dotnet-mcp/McpServer.Tests/McpServer.Tests.csproj McpServer.Tests/

# Restore dependencies first (for better layer caching)
RUN dotnet restore "McpServer/McpServer.csproj"

# Copy all source code
COPY ../dotnet-mcp/McpServer/ McpServer/
COPY ../dotnet-mcp/McpServer.Tests/ McpServer.Tests/

# Build the application
WORKDIR /src/McpServer
RUN dotnet build "McpServer.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "McpServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser

# Set environment variables for containerized deployment
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

# Expose port
EXPOSE 80

# Start the application
ENTRYPOINT ["dotnet", "McpServer.dll"]
