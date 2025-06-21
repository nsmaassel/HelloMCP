#!/bin/bash

# Build script for Docker MCP Server
# This script copies the .NET MCP server source to docker-mcp build context

set -e

echo "🐳 Setting up Docker MCP Server build context..."

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DOCKER_DIR="$SCRIPT_DIR"
DOTNET_DIR="$(dirname "$SCRIPT_DIR")/dotnet-mcp"

# Ensure we're in the right place
if [ ! -d "$DOTNET_DIR" ]; then
    echo "❌ Error: dotnet-mcp directory not found at $DOTNET_DIR"
    exit 1
fi

# Copy source files to build context
echo "📁 Copying source files..."
cp -r "$DOTNET_DIR/McpServer" "$DOCKER_DIR/"
cp -r "$DOTNET_DIR/McpServer.Tests" "$DOCKER_DIR/"

# Create .dockerignore if it doesn't exist
if [ ! -f "$DOCKER_DIR/.dockerignore" ]; then
    echo "📝 Creating .dockerignore..."
    cat > "$DOCKER_DIR/.dockerignore" << EOF
# Build artifacts
**/bin/
**/obj/
**/out/

# IDE files
.vs/
.vscode/
*.user
*.suo

# OS files
.DS_Store
Thumbs.db

# Logs
*.log

# Temporary files
*.tmp
*.temp
EOF
fi

echo "🔨 Building Docker image..."
docker build -t hellomcp-docker:latest .

echo "✅ Docker MCP Server build complete!"
echo ""
echo "🚀 Quick start commands:"
echo "   docker run -p 5090:80 hellomcp-docker:latest"
echo "   or"
echo "   docker-compose up -d"
echo ""
echo "🔗 Test the server:"
echo "   curl http://localhost:5090/v1/initialize"
