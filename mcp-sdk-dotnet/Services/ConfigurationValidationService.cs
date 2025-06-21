using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using McpSdkServer.Configuration;

namespace McpSdkServer.Services;

/// <summary>
/// Service for validating server configuration at startup
/// </summary>
public class ConfigurationValidationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationValidationService> _logger;

    public ConfigurationValidationService(
        IConfiguration configuration,
        ILogger<ConfigurationValidationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Validates the server configuration and logs any issues
    /// </summary>
    /// <returns>True if configuration is valid, false otherwise</returns>
    public bool ValidateConfiguration()
    {
        var isValid = true;
        var errors = new List<string>();

        try
        {
            // Validate MCP Server configuration
            var mcpConfig = _configuration.GetSection("McpServer");
            if (!mcpConfig.Exists())
            {
                errors.Add("McpServer configuration section is missing");
                isValid = false;
            }
            else
            {
                ValidateMcpServerConfig(mcpConfig, errors);
            }

            // Validate HTTP Client configuration
            var httpConfig = _configuration.GetSection("HttpClient");
            if (httpConfig.Exists())
            {
                ValidateHttpClientConfig(httpConfig, errors);
            }

            // Log validation results
            if (isValid)
            {
                _logger.LogInformation("✅ Configuration validation completed successfully");
            }
            else
            {
                _logger.LogError("❌ Configuration validation failed:");
                foreach (var error in errors)
                {
                    _logger.LogError("  - {Error}", error);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Configuration validation failed with exception");
            isValid = false;
        }

        return isValid;
    }

    private void ValidateMcpServerConfig(IConfigurationSection mcpConfig, List<string> errors)
    {
        // Validate required string fields
        ValidateStringField(mcpConfig, "Name", errors, required: true);
        ValidateStringField(mcpConfig, "Version", errors, required: true);
        ValidateStringField(mcpConfig, "Description", errors, required: false);

        // Validate numeric fields
        ValidateIntegerField(mcpConfig, "MaxConcurrentRequests", errors, min: 1, max: 1000);
        ValidateIntegerField(mcpConfig, "RequestTimeoutSeconds", errors, min: 1, max: 300);

        // Validate boolean fields
        ValidateBooleanField(mcpConfig, "EnableMetrics", errors);
        ValidateBooleanField(mcpConfig, "EnableHealthChecks", errors);
    }

    private void ValidateHttpClientConfig(IConfigurationSection httpConfig, List<string> errors)
    {
        ValidateIntegerField(httpConfig, "TimeoutSeconds", errors, min: 1, max: 300);
        ValidateIntegerField(httpConfig, "MaxConcurrentConnections", errors, min: 1, max: 1000);
        ValidateStringField(httpConfig, "UserAgent", errors, required: false);
    }

    private void ValidateStringField(IConfigurationSection section, string key, List<string> errors, bool required = true)
    {
        var value = section[key];
        if (required && string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"Required field '{section.Path}:{key}' is missing or empty");
        }
        else if (!required && value != null && string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"Optional field '{section.Path}:{key}' should not be empty if specified");
        }
    }

    private void ValidateIntegerField(IConfigurationSection section, string key, List<string> errors, int? min = null, int? max = null)
    {
        var valueStr = section[key];
        if (string.IsNullOrWhiteSpace(valueStr))
        {
            return; // Optional field
        }

        if (!int.TryParse(valueStr, out var value))
        {
            errors.Add($"Field '{section.Path}:{key}' must be a valid integer, got '{valueStr}'");
            return;
        }

        if (min.HasValue && value < min.Value)
        {
            errors.Add($"Field '{section.Path}:{key}' must be at least {min.Value}, got {value}");
        }

        if (max.HasValue && value > max.Value)
        {
            errors.Add($"Field '{section.Path}:{key}' must be at most {max.Value}, got {value}");
        }
    }

    private void ValidateBooleanField(IConfigurationSection section, string key, List<string> errors)
    {
        var valueStr = section[key];
        if (string.IsNullOrWhiteSpace(valueStr))
        {
            return; // Optional field
        }

        if (!bool.TryParse(valueStr, out _))
        {
            errors.Add($"Field '{section.Path}:{key}' must be a valid boolean (true/false), got '{valueStr}'");
        }
    }

    /// <summary>
    /// Gets a summary of the current configuration for logging
    /// </summary>
    public string GetConfigurationSummary()
    {
        try
        {
            var summary = new
            {
                McpServer = new
                {
                    Name = _configuration["McpServer:Name"] ?? "Not configured",
                    Version = _configuration["McpServer:Version"] ?? "Not configured",
                    MaxConcurrentRequests = _configuration.GetValue("McpServer:MaxConcurrentRequests", 100),
                    RequestTimeoutSeconds = _configuration.GetValue("McpServer:RequestTimeoutSeconds", 30),
                    EnableMetrics = _configuration.GetValue("McpServer:EnableMetrics", true),
                    EnableHealthChecks = _configuration.GetValue("McpServer:EnableHealthChecks", true)
                },
                HttpClient = new
                {
                    TimeoutSeconds = _configuration.GetValue("HttpClient:TimeoutSeconds", 30),
                    MaxConcurrentConnections = _configuration.GetValue("HttpClient:MaxConcurrentConnections", 50),
                    UserAgent = _configuration["HttpClient:UserAgent"] ?? "MCP-SDK-DotNet/1.0.0"
                },
                Environment = new
                {
                    AspNetCoreEnvironment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Not set",
                    DotNetRunningInContainer = _configuration.GetValue("DOTNET_RUNNING_IN_CONTAINER", false)
                }
            };

            return System.Text.Json.JsonSerializer.Serialize(summary, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return $"Error generating configuration summary: {ex.Message}";
        }
    }
}
