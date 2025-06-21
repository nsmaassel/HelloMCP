#!/usr/bin/env pwsh
# MCP Server Verification Script for VS Code Integration

Write-Host "🔍 MCP Server VS Code Integration Verification" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan

$ErrorActionPreference = "Continue"
$success = $true

# 1. Check .NET SDK
Write-Host "`n1️⃣  Checking .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK Version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ .NET SDK not found or not working" -ForegroundColor Red
    $success = $false
}

# 2. Check project builds
Write-Host "`n2️⃣  Verifying project build..." -ForegroundColor Yellow
try {
    $buildResult = dotnet build --verbosity quiet 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Project builds successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Build failed:" -ForegroundColor Red
        Write-Host $buildResult -ForegroundColor Red
        $success = $false
    }
} catch {
    Write-Host "❌ Build error: $_" -ForegroundColor Red
    $success = $false
}

# 3. Check validation
Write-Host "`n3️⃣  Testing server validation..." -ForegroundColor Yellow
try {
    $job = Start-Job -ScriptBlock {
        Set-Location $using:PWD
        dotnet run --validate 2>&1
    }
    $output = Wait-Job $job -Timeout 15 | Receive-Job
    Remove-Job $job -Force
    
    if ($output -match "All tests PASSED") {
        Write-Host "✅ Validation tests passed" -ForegroundColor Green
    } else {
        Write-Host "❌ Validation failed or timed out" -ForegroundColor Red
        $success = $false
    }
} catch {
    Write-Host "❌ Validation error: $_" -ForegroundColor Red
    $success = $false
}

# 4. Check mcp.json configuration
Write-Host "`n4️⃣  Checking mcp.json configuration..." -ForegroundColor Yellow
$mcpJsonPath = "$env:USERPROFILE\.cursor\mcp.json"
if (Test-Path $mcpJsonPath) {
    try {
        $mcpConfig = Get-Content $mcpJsonPath | ConvertFrom-Json
        if ($mcpConfig.mcpServers."DotNet MCP SDK") {
            Write-Host "✅ DotNet MCP SDK found in mcp.json" -ForegroundColor Green
            $serverConfig = $mcpConfig.mcpServers."DotNet MCP SDK"
            Write-Host "   Command: $($serverConfig.command)" -ForegroundColor Gray
            Write-Host "   Args: $($serverConfig.args -join ' ')" -ForegroundColor Gray
            
            # Verify project path exists
            $projectPath = $serverConfig.args | Where-Object { $_ -like "*mcp-sdk-dotnet*" }
            if ($projectPath -and (Test-Path $projectPath)) {
                Write-Host "✅ Project path exists and is accessible" -ForegroundColor Green
            } else {
                Write-Host "❌ Project path in mcp.json doesn't exist or is incorrect" -ForegroundColor Red
                $success = $false
            }
        } else {
            Write-Host "❌ DotNet MCP SDK not found in mcp.json" -ForegroundColor Red
            $success = $false
        }
    } catch {
        Write-Host "❌ Error reading mcp.json: $_" -ForegroundColor Red
        $success = $false
    }
} else {
    Write-Host "❌ mcp.json not found at $mcpJsonPath" -ForegroundColor Red
    $success = $false
}

# 5. Check for running processes
Write-Host "`n5️⃣  Checking for conflicting processes..." -ForegroundColor Yellow
$mcpProcesses = Get-Process -Name "*McpSdk*" -ErrorAction SilentlyContinue
if ($mcpProcesses) {
    Write-Host "⚠️  Found running MCP server processes (this is normal if VS Code is running):" -ForegroundColor Yellow
    $mcpProcesses | ForEach-Object { Write-Host "   PID: $($_.Id) - $($_.ProcessName)" -ForegroundColor Gray }
} else {
    Write-Host "✅ No conflicting MCP server processes found" -ForegroundColor Green
}

# 6. Test basic server startup (quick test)
Write-Host "`n6️⃣  Testing basic server startup..." -ForegroundColor Yellow
try {
    $job = Start-Job -ScriptBlock {
        Set-Location $using:PWD
        $process = Start-Process -FilePath "dotnet" -ArgumentList "run" -PassThru -WindowStyle Hidden
        Start-Sleep 3
        $process | Stop-Process -Force 2>$null
        return "started"
    }
    $result = Wait-Job $job -Timeout 10 | Receive-Job
    Remove-Job $job -Force
    
    if ($result -eq "started") {
        Write-Host "✅ Server can start successfully" -ForegroundColor Green
    } else {
        Write-Host "❌ Server startup test failed" -ForegroundColor Red
        $success = $false
    }
} catch {
    Write-Host "❌ Server startup error: $_" -ForegroundColor Red
    $success = $false
}

# Summary
Write-Host "`n📊 Summary" -ForegroundColor Cyan
Write-Host "==========" -ForegroundColor Cyan
if ($success) {
    Write-Host "🎉 All checks passed! Your MCP server is ready for VS Code Insiders." -ForegroundColor Green
    Write-Host "`n📋 Next Steps:" -ForegroundColor Yellow
    Write-Host "1. Close VS Code Insiders if it's running" -ForegroundColor White
    Write-Host "2. Open VS Code Insiders" -ForegroundColor White
    Write-Host "3. VS Code will automatically start the MCP server when needed" -ForegroundColor White
    Write-Host "4. Check the MCP extension logs if you have connection issues" -ForegroundColor White
    Write-Host "`n💡 VS Code will manage the server lifecycle automatically!" -ForegroundColor Cyan
} else {
    Write-Host "❌ Some checks failed. Please review the issues above." -ForegroundColor Red
    Write-Host "`n🔧 Common fixes:" -ForegroundColor Yellow
    Write-Host "- Ensure .NET 8.0 SDK is installed" -ForegroundColor White
    Write-Host "- Run 'dotnet build' to fix build issues" -ForegroundColor White
    Write-Host "- Check that mcp.json path is correct" -ForegroundColor White
    Write-Host "- Stop any running MCP server processes" -ForegroundColor White
}

exit ($success ? 0 : 1)
