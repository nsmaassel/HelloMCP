using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsMcp.Functions;

public class OAuthFunctions
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public OAuthFunctions(ILoggerFactory loggerFactory, IConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger<OAuthFunctions>();
        _configuration = configuration;
    }

    [Function("GetOAuthServerMetadata")]
    public HttpResponseData GetOAuthServerMetadata(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ".well-known/oauth-authorization-server")] HttpRequestData req)
    {
        _logger.LogInformation("Processing OAuth server metadata request");
        
        var baseUrl = _configuration["MCP_OAUTH_ISSUER"] ?? $"{GetBaseUrl(req)}";
        
        var metadata = new
        {
            issuer = baseUrl,
            authorization_endpoint = $"{baseUrl}/oauth/authorize",
            token_endpoint = $"{baseUrl}/oauth/token",
            token_endpoint_auth_methods_supported = new[] { "client_secret_basic", "client_secret_post" },
            revocation_endpoint = $"{baseUrl}/oauth/revoke",
            revocation_endpoint_auth_methods_supported = new[] { "client_secret_basic", "client_secret_post" },
            grant_types_supported = new[] { "authorization_code", "refresh_token", "client_credentials" },
            response_types_supported = new[] { "code" },
            scopes_supported = new[] { "text.completions", "session" }
        };

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        response.WriteAsJsonAsync(metadata);
        
        return response;
    }

    [Function("OAuthAuthorize")]
    public HttpResponseData Authorize(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "oauth/authorize")] HttpRequestData req)
    {
        _logger.LogInformation("Processing OAuth authorization request");
        
        // This is a placeholder for a real OAuth authorization endpoint
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        response.WriteString("OAuth Authorization Endpoint");
        
        return response;
    }

    [Function("OAuthToken")]
    public HttpResponseData Token(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "oauth/token")] HttpRequestData req)
    {
        _logger.LogInformation("Processing OAuth token request");
        
        // This is a placeholder for a real OAuth token endpoint
        var token = new
        {
            access_token = "demo_access_token",
            token_type = "Bearer",
            expires_in = 3600,
            scope = "text.completions session"
        };
        
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        response.WriteAsJsonAsync(token);
        
        return response;
    }

    private string GetBaseUrl(HttpRequestData req)
    {
        return $"{req.Url.Scheme}://{req.Url.Host}{(req.Url.Port != 80 && req.Url.Port != 443 ? $":{req.Url.Port}" : "")}";
    }
}