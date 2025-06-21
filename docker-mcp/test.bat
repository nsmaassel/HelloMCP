@echo off

REM Test script for Docker MCP Server (Windows)
REM Validates that the container is working correctly

echo 🧪 Testing Docker MCP Server...

REM Configuration
set "CONTAINER_NAME=hellomcp-docker-test"
set "PORT=5090"
set "BASE_URL=http://localhost:%PORT%"

REM Cleanup function
:cleanup
echo 🧹 Cleaning up test container...
docker stop "%CONTAINER_NAME%" 2>nul
docker rm "%CONTAINER_NAME%" 2>nul
goto :eof

REM Start container
echo 🚀 Starting test container...
docker run -d --name "%CONTAINER_NAME%" -p "%PORT%:80" hellomcp-docker:latest

if %ERRORLEVEL% neq 0 (
    echo ❌ Failed to start container
    exit /b 1
)

REM Wait for container to be ready
echo ⏳ Waiting for container to be ready...
timeout /t 10 /nobreak >nul

echo.
echo 🧪 Running MCP protocol tests...

REM Test basic endpoints
echo 🔍 Testing Health check...
curl -s "%BASE_URL%/health" >nul
if %ERRORLEVEL% equ 0 (
    echo ✅ Health check - OK
) else (
    echo ❌ Health check - Failed
)

echo 🔍 Testing MCP initialize...
curl -s -X POST -H "Content-Type: application/json" -d "{}" "%BASE_URL%/v1/initialize" >nul
if %ERRORLEVEL% equ 0 (
    echo ✅ MCP initialize - OK
) else (
    echo ❌ MCP initialize - Failed
)

echo 🔍 Testing OAuth discovery...
curl -s "%BASE_URL%/.well-known/oauth-authorization-server" >nul
if %ERRORLEVEL% equ 0 (
    echo ✅ OAuth discovery - OK
) else (
    echo ❌ OAuth discovery - Failed
)

echo 🔍 Testing Session creation...
curl -s -X POST -H "Content-Type: application/json" -d "{\"id\": \"docker-test\", \"attributes\": {\"access_token\": \"test\"}}" "%BASE_URL%/v1/session" >nul
if %ERRORLEVEL% equ 0 (
    echo ✅ Session creation - OK
) else (
    echo ❌ Session creation - Failed
)

echo.
echo 🎉 Docker MCP Server test completed!
echo 📊 Container logs:
docker logs "%CONTAINER_NAME%" --tail 10

REM Cleanup
call :cleanup
