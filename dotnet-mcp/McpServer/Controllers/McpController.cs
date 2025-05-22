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
    public async Task<IActionResult> GetTextCompletions([FromBody] TextCompletionRequest request)
    {
        try
        {
            _logger.LogInformation("Text completion requested for session {SessionId}", request.SessionId);
            
            if (!_sessionService.TryGetSession(request.SessionId, out var sessionInfo))
            {
                _logger.LogWarning("Invalid session ID: {SessionId}", request.SessionId);
                return BadRequest(CreateErrorResponse(request.Id, "invalid_session", "Invalid or expired session ID"));
            }
            
            // Set the response content type
            Response.ContentType = "application/x-ndjson";
            
            // Send the response in chunks
            var exampleResponses = GenerateDemoResponses(request);
            
            foreach (var responsePart in exampleResponses)
            {
                var json = JsonSerializer.Serialize(responsePart) + "\n";
                var bytes = Encoding.UTF8.GetBytes(json);
                await Response.Body.WriteAsync(bytes, 0, bytes.Length);
                await Response.Body.FlushAsync();
                
                // Add a small delay to simulate streaming
                await Task.Delay(100);
            }
            
            return new EmptyResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing text completion");
            return StatusCode(500, CreateErrorResponse(request.Id, "internal_error", "An unexpected error occurred"));
        }
    }
    
    // Legacy HTTP+SSE streaming endpoint
    [HttpPost("text/completions/stream")]
    public IActionResult StreamTextCompletions([FromBody] TextCompletionRequest request)
    {
        try
        {
            _logger.LogInformation("SSE text completion requested for session {SessionId}", request.SessionId);
            
            if (!_sessionService.TryGetSession(request.SessionId, out var sessionInfo))
            {
                _logger.LogWarning("Invalid session ID: {SessionId}", request.SessionId);
                return BadRequest(CreateErrorResponse(request.Id, "invalid_session", "Invalid or expired session ID"));
            }
            
            // Set up SSE headers
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";
            
            // Return StreamingResponse
            return new StreamingResponse(async stream =>
            {
                var writer = new StreamWriter(stream);
                var exampleResponses = GenerateDemoResponses(request);
                
                foreach (var responsePart in exampleResponses)
                {
                    var json = JsonSerializer.Serialize(responsePart);
                    await writer.WriteAsync($"data: {json}\n\n");
                    await writer.FlushAsync();
                    
                    // Add a small delay to simulate streaming
                    await Task.Delay(100);
                }
                
                await writer.WriteAsync("data: [DONE]\n\n");
                await writer.FlushAsync();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SSE text completion");
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
    
    private static IEnumerable<TextCompletionStreamResponse> GenerateDemoResponses(TextCompletionRequest request)
    {
        // Generate some simulated responses for demonstration purposes
        var lorem = "This is a demonstration of the Model Context Protocol server implemented in .NET. " +
                   "The server supports both modern Streamable HTTP and legacy HTTP+SSE transport options. " +
                   "In a real implementation, this would connect to an AI model to generate responses based on the provided prompt.";
        
        var words = lorem.Split(' ');
        var responses = new List<TextCompletionStreamResponse>();
        
        // Initial chunks with content
        for (var i = 0; i < words.Length; i++)
        {
            responses.Add(new TextCompletionStreamResponse
            {
                Id = request.Id,
                Delta = new TextCompletionDelta
                {
                    Text = words[i] + " "
                }
            });
        }
        
        // Final chunk with finish reason and usage
        responses.Add(new TextCompletionStreamResponse
        {
            Id = request.Id,
            Delta = new TextCompletionDelta
            {
                Text = "",
                FinishReason = "stop"
            },
            Usage = new TextCompletionUsage
            {
                PromptTokens = request.Inputs.Prompt.Length / 4, // Very rough approximation
                CompletionTokens = lorem.Length / 4,
                TotalTokens = (request.Inputs.Prompt.Length + lorem.Length) / 4
            }
        });
        
        return responses;
    }
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