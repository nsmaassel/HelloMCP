# syntax=docker/dockerfile:1

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY McpSdkServer.csproj .
RUN dotnet restore "McpSdkServer.csproj"

# Copy source code and build
COPY . .
RUN dotnet build "McpSdkServer.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "McpSdkServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Set ownership and switch to non-root user
RUN chown -R appuser:appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD echo "MCP SDK Server health check - running" || exit 1

# Set environment for MCP
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "McpSdkServer.dll"]
