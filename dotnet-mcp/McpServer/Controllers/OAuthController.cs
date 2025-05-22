using Microsoft.AspNetCore.Mvc;

namespace McpServer.Controllers;

[ApiController]
public class OAuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    
    public OAuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    [HttpGet(".well-known/oauth-authorization-server")]
    public IActionResult GetOAuthServerMetadata()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        
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
        
        return Ok(metadata);
    }
    
    // In a real implementation, these endpoints would handle OAuth functionality
    [HttpGet("oauth/authorize")]
    public IActionResult Authorize()
    {
        // This is a placeholder for a real OAuth authorization endpoint
        return Ok("OAuth Authorization Endpoint");
    }
    
    [HttpPost("oauth/token")]
    public IActionResult Token()
    {
        // This is a placeholder for a real OAuth token endpoint
        var token = new
        {
            access_token = "demo_access_token",
            token_type = "Bearer",
            expires_in = 3600,
            scope = "text.completions session"
        };
        
        return Ok(token);
    }
}