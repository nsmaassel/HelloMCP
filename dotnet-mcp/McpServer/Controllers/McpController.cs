using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using McpServer.Models;
using McpServer.Services;

namespace McpServer.Controllers;

[ApiController]
[Route("v1")]
public class McpController : ControllerBase
{
    private readonly SessionService _sessionService;
    private readonly ILogger<McpController> _logger;
    
    public McpController(SessionService sessionService, ILogger<McpController> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    // MCP initialize endpoint for protocol compatibility
    [HttpPost("initialize")]
    public IActionResult Initialize([FromBody] object request)
    {
        // Return a minimal MCP-compliant response
        var response = new
        {
            id = Guid.NewGuid().ToString(),
            object_type = "mcp-initialize-response",
            server = new {
                name = ".NET MCP Example Server",
                version = "1.0.0",
                description = "A .NET MCP server supporting stat analysis and text completions."
            },
            capabilities = new[] { "text-completions", "stat-analysis", "streaming" }
        };
        return Ok(response);
    }
    
    // Session creation endpoint
    [HttpPost("session")]
    public IActionResult CreateSession([FromBody] SessionCreateRequest request)
    {
        try
        {
            _logger.LogInformation("Session creation requested with ID: {Id}", request.Id);
            
            var sessionId = Guid.NewGuid().ToString();
            
            if (_sessionService.TryCreateSession(sessionId, request.Attributes.AccessToken, out var sessionInfo))
            {
                var response = new SessionCreateResponse
                {
                    Id = request.Id,
                    SessionId = sessionId
                };
                
                _logger.LogInformation("Session created successfully: {SessionId}", sessionId);
                return Ok(response);
            }
            else
            {
                return StatusCode(500, CreateErrorResponse(request.Id, "internal_error", "Failed to create session"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session");
            return StatusCode(500, CreateErrorResponse(request.Id, "internal_error", "An unexpected error occurred"));
        }
    }
    
    // Text completion endpoint - Streamable HTTP
    [HttpPost("text/completions")]
    public IActionResult GetTextCompletions([FromBody] TextCompletionRequest request)
    {
        try
        {
            _logger.LogInformation("Text completion/stat analysis requested for session {SessionId}", request.SessionId);

            if (!_sessionService.TryGetSession(request.SessionId, out var sessionInfo))
            {
                _logger.LogWarning("Invalid session ID: {SessionId}", request.SessionId);
                return BadRequest(CreateErrorResponse(request.Id, "invalid_session", "Invalid or expired session ID"));
            }

            // If stats are present, do deterministic stat analysis
            if (request.Inputs?.Stats != null)
            {
                var result = new Dictionary<string, string>();
                var p1 = request.Inputs.Stats.Player1;
                var p2 = request.Inputs.Stats.Player2;
                foreach (var stat in p1.Keys)
                {
                    if (p2.ContainsKey(stat))
                    {
                        if (p1[stat] > p2[stat]) result[stat] = "player1";
                        else if (p1[stat] < p2[stat]) result[stat] = "player2";
                        else result[stat] = "tie";
                    }
                }
                // Save to session history
                sessionInfo.AnalysisHistory.Add(new Dictionary<string, string>(result));
                var response = new
                {
                    id = request.Id,
                    type = "stat-analysis-response",
                    result,
                    history = sessionInfo.AnalysisHistory
                };
                return Ok(response);
            }
            // If prompt is present, do text completion (demo/dummy response)
            else if (!string.IsNullOrWhiteSpace(request.Inputs?.Prompt))
            {
                // Demo: echo the prompt with a canned completion
                var completion = $"{request.Inputs.Prompt} [COMPLETION]";
                var response = new
                {
                    id = request.Id,
                    type = "text-completion-response",
                    result = new { text = completion },
                    usage = new { prompt_tokens = request.Inputs.Prompt.Length, completion_tokens = 1, total_tokens = request.Inputs.Prompt.Length + 1 }
                };
                return Ok(response);
            }
            else
            {
                return BadRequest(CreateErrorResponse(request.Id, "invalid_request", "Missing stats or prompt in request."));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing text completion/stat analysis");
            return StatusCode(500, CreateErrorResponse(request.Id, "internal_error", "An unexpected error occurred"));
        }
    }
    
    // Legacy HTTP+SSE streaming endpoint
    [HttpPost("text/completions/stream")]
    public IActionResult StreamTextCompletions([FromBody] TextCompletionRequest request)
    {
        try
        {
            _logger.LogInformation("SSE text completion/stat analysis requested for session {SessionId}", request.SessionId);

            if (!_sessionService.TryGetSession(request.SessionId, out var sessionInfo))
            {
                _logger.LogWarning("Invalid session ID: {SessionId}", request.SessionId);
                return BadRequest(CreateErrorResponse(request.Id, "invalid_session", "Invalid or expired session ID"));
            }

            // Set up SSE headers
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            // If stats are present, stream stat analysis
            if (request.Inputs?.Stats != null)
            {
                var result = new Dictionary<string, string>();
                var p1 = request.Inputs.Stats.Player1;
                var p2 = request.Inputs.Stats.Player2;
                foreach (var stat in p1.Keys)
                {
                    if (p2.ContainsKey(stat))
                    {
                        if (p1[stat] > p2[stat]) result[stat] = "player1";
                        else if (p1[stat] < p2[stat]) result[stat] = "player2";
                        else result[stat] = "tie";
                    }
                }
                // Save to session history
                sessionInfo.AnalysisHistory.Add(new Dictionary<string, string>(result));
                return new StreamingResponse(async stream =>
                {
                    var writer = new StreamWriter(stream);
                    // Stream each stat as a separate SSE event
                    foreach (var stat in result.Keys)
                    {
                        var chunk = new
                        {
                            id = request.Id,
                            type = "stat-analysis-stream-response",
                            stat,
                            winner = result[stat]
                        };
                        var json = JsonSerializer.Serialize(chunk);
                        await writer.WriteAsync($"data: {json}\n\n");
                        await writer.FlushAsync();
                        await Task.Delay(100); // Simulate streaming
                    }
                    // Final event with full history
                    var final = new
                    {
                        id = request.Id,
                        type = "stat-analysis-stream-history",
                        history = sessionInfo.AnalysisHistory
                    };
                    var finalJson = JsonSerializer.Serialize(final);
                    await writer.WriteAsync($"data: {finalJson}\n\n");
                    await writer.WriteAsync("data: [DONE]\n\n");
                    await writer.FlushAsync();
                });
            }
            // If prompt is present, stream a text completion (demo)
            else if (!string.IsNullOrWhiteSpace(request.Inputs?.Prompt))
            {
                var prompt = request.Inputs.Prompt;
                return new StreamingResponse(async stream =>
                {
                    var writer = new StreamWriter(stream);
                    // Simulate streaming the completion one word at a time
                    var words = (prompt + " [COMPLETION]").Split(' ');
                    foreach (var word in words)
                    {
                        var chunk = new
                        {
                            id = request.Id,
                            type = "text-completion-stream-response",
                            text = word
                        };
                        var json = JsonSerializer.Serialize(chunk);
                        await writer.WriteAsync($"data: {json}\n\n");
                        await writer.FlushAsync();
                        await Task.Delay(100); // Simulate streaming
                    }
                    await writer.WriteAsync("data: [DONE]\n\n");
                    await writer.FlushAsync();
                });
            }
            else
            {
                return BadRequest(CreateErrorResponse(request.Id, "invalid_request", "Missing stats or prompt in request."));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SSE text completion/stat analysis");
            return StatusCode(500, CreateErrorResponse(request.Id, "internal_error", "An unexpected error occurred"));
        }
    }
    
    // Close session endpoint
    [HttpDelete("session/{sessionId}")]
    public IActionResult CloseSession(string sessionId)
    {
        try
        {
            _logger.LogInformation("Session close requested: {SessionId}", sessionId);
            
            if (_sessionService.TryCloseSession(sessionId))
            {
                var response = new SessionCloseResponse
                {
                    Id = Guid.NewGuid().ToString()
                };
                
                _logger.LogInformation("Session closed successfully: {SessionId}", sessionId);
                return Ok(response);
            }
            else
            {
                _logger.LogWarning("Failed to close nonexistent session: {SessionId}", sessionId);
                return NotFound(CreateErrorResponse(Guid.NewGuid().ToString(), "invalid_session", "Session not found"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing session");
            return StatusCode(500, CreateErrorResponse(Guid.NewGuid().ToString(), "internal_error", "An unexpected error occurred"));
        }
    }
    
    // Helper methods
    private static ErrorResponse CreateErrorResponse(string requestId, string code, string message)
    {
        return new ErrorResponse
        {
            Id = requestId,
            Error = new McpError
            {
                Code = code,
                Message = message
            }
        };
    }
    
    // The old GenerateDemoResponses method is now obsolete and not used. It is safe to remove it.
}

// Custom result type for SSE streaming
public class StreamingResponse : IActionResult
{
    private readonly Func<Stream, Task> _callback;
    
    public StreamingResponse(Func<Stream, Task> callback)
    {
        _callback = callback;
    }
    
    public async Task ExecuteResultAsync(ActionContext context)
    {
        await _callback(context.HttpContext.Response.Body);
    }
}