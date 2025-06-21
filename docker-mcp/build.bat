@echo off

REM Build script for Docker MCP Server (Windows)
REM This script copies the .NET MCP server source to docker-mcp build context

echo 🐳 Setting up Docker MCP Server build context...

REM Get script directory
set "SCRIPT_DIR=%~dp0"
set "DOCKER_DIR=%SCRIPT_DIR%"
set "DOTNET_DIR=%SCRIPT_DIR%..\dotnet-mcp"

REM Check if dotnet-mcp directory exists
if not exist "%DOTNET_DIR%" (
    echo ❌ Error: dotnet-mcp directory not found at %DOTNET_DIR%
    exit /b 1
)

REM Copy source files to build context
echo 📁 Copying source files...
if exist "%DOCKER_DIR%McpServer" rmdir /s /q "%DOCKER_DIR%McpServer"
if exist "%DOCKER_DIR%McpServer.Tests" rmdir /s /q "%DOCKER_DIR%McpServer.Tests"

xcopy "%DOTNET_DIR%\McpServer" "%DOCKER_DIR%McpServer\" /e /i /h /y
xcopy "%DOTNET_DIR%\McpServer.Tests" "%DOCKER_DIR%McpServer.Tests\" /e /i /h /y

REM Create .dockerignore if it doesn't exist
if not exist "%DOCKER_DIR%.dockerignore" (
    echo 📝 Creating .dockerignore...
    (
        echo # Build artifacts
        echo **/bin/
        echo **/obj/
        echo **/out/
        echo.
        echo # IDE files
        echo .vs/
        echo .vscode/
        echo *.user
        echo *.suo
        echo.
        echo # OS files
        echo .DS_Store
        echo Thumbs.db
        echo.
        echo # Logs
        echo *.log
        echo.
        echo # Temporary files
        echo *.tmp
        echo *.temp
    ) > "%DOCKER_DIR%.dockerignore"
)

echo 🔨 Building Docker image...
docker build -t hellomcp-docker:latest .

if %ERRORLEVEL% neq 0 (
    echo ❌ Docker build failed
    exit /b 1
)

echo ✅ Docker MCP Server build complete!
echo.
echo 🚀 Quick start commands:
echo    docker run -p 5090:80 hellomcp-docker:latest
echo    or
echo    docker-compose up -d
echo.
echo 🔗 Test the server:
echo    curl http://localhost:5090/v1/initialize
