using System.Net;
using System.Text.Json;
using AzureFunctionsMcp.Models;
using AzureFunctionsMcp.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsMcp.Functions;

public class TextCompletionFunctions
{
    private readonly ILogger _logger;
    private readonly SessionService _sessionService;

    public TextCompletionFunctions(ILoggerFactory loggerFactory, SessionService sessionService)
    {
        _logger = loggerFactory.CreateLogger<TextCompletionFunctions>();
        _sessionService = sessionService;
    }

    [Function("GetTextCompletions")]
    public async Task<HttpResponseData> GetTextCompletions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/text/completions")] HttpRequestData req)
    {
        _logger.LogInformation("Processing text completion request");

        try
        {
            var request = await req.ReadFromJsonAsync<TextCompletionRequest>();
            if (request == null)
            {
                return CreateErrorResponse(req, "invalid_request", "Missing or invalid text completion request");
            }

            if (!_sessionService.ValidateSession(request.SessionId))
            {
                return CreateErrorResponse(req, "invalid_session", "Session not found or expired");
            }

            // In a real implementation, this would call an LLM API
            // For demo purposes, we'll generate a simple response based on the prompt
            var demoResponses = GenerateDemoResponses(request);
            var lastResponse = demoResponses.Last();

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            await response.WriteAsJsonAsync(lastResponse);
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing text completion request");
            return CreateErrorResponse(req, "internal_error", "An unexpected error occurred");
        }
    }

    [Function("StreamTextCompletions")]
    public async Task<HttpResponseData> StreamTextCompletions(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/text/completions/stream")] HttpRequestData req)
    {
        _logger.LogInformation("Processing streaming text completion request");

        try
        {
            var request = await req.ReadFromJsonAsync<TextCompletionRequest>();
            if (request == null)
            {
                return CreateErrorResponse(req, "invalid_request", "Missing or invalid text completion request");
            }

            if (!_sessionService.ValidateSession(request.SessionId))
            {
                return CreateErrorResponse(req, "invalid_session", "Session not found or expired");
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/event-stream");
            response.Headers.Add("Cache-Control", "no-cache");
            response.Headers.Add("Connection", "keep-alive");

            // Generate demo responses
            var exampleResponses = GenerateDemoResponses(request);
            
            // Start writing to the response stream
            var writer = response.Body;
            foreach (var responsePart in exampleResponses)
            {
                var json = JsonSerializer.Serialize(responsePart);
                var data = $"data: {json}\n\n";
                var bytes = System.Text.Encoding.UTF8.GetBytes(data);
                await writer.WriteAsync(bytes);
                await writer.FlushAsync();
                
                // Add a small delay to simulate streaming
                await Task.Delay(100);
            }
            
            // Signal end of stream
            var doneBytes = System.Text.Encoding.UTF8.GetBytes("data: [DONE]\n\n");
            await writer.WriteAsync(doneBytes);
            await writer.FlushAsync();
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing streaming text completion request");
            return CreateErrorResponse(req, "internal_error", "An unexpected error occurred");
        }
    }

    private HttpResponseData CreateErrorResponse(HttpRequestData req, string code, string message)
    {
        var response = req.CreateResponse(HttpStatusCode.BadRequest);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        
        var errorResponse = new ErrorResponse
        {
            Id = Guid.NewGuid().ToString(),
            Error = new McpError
            {
                Code = code,
                Message = message
            }
        };
        
        response.WriteAsJsonAsync(errorResponse);
        return response;
    }

    private static IEnumerable<TextCompletionStreamResponse> GenerateDemoResponses(TextCompletionRequest request)
    {
        // This is a simplified demo implementation
        // In a real scenario, this would call an LLM API
        
        var prompt = request.Inputs.Prompt.Trim().ToLowerInvariant();
        var maxTokens = request.Inputs.MaxTokens ?? 100;
        string responseText;
        
        if (prompt.Contains("hello") || prompt.Contains("hi"))
        {
            responseText = "Hello there! How can I assist you today?";
        }
        else if (prompt.Contains("weather"))
        {
            responseText = "I don't have real-time weather data, but I can help you find a weather service!";
        }
        else
        {
            responseText = "Thank you for your message. This is a demo MCP server, so I don't have real AI capabilities. In a production environment, this would connect to an actual language model.";
        }

        // Split the response into chunks to simulate streaming
        var words = responseText.Split(' ');
        var chunks = new List<string>();
        var currentChunk = "";
        
        foreach (var word in words)
        {
            currentChunk += word + " ";
            if (currentChunk.Length > 10)
            {
                chunks.Add(currentChunk.Trim());
                currentChunk = "";
            }
        }
        
        if (!string.IsNullOrWhiteSpace(currentChunk))
        {
            chunks.Add(currentChunk.Trim());
        }

        // Generate a response for each chunk
        var responses = new List<TextCompletionStreamResponse>();
        var totalTokens = 0;
        
        foreach (var chunk in chunks)
        {
            var tokenCount = chunk.Split(' ').Length;
            totalTokens += tokenCount;
            
            if (totalTokens > maxTokens)
            {
                break;
            }
            
            responses.Add(new TextCompletionStreamResponse
            {
                Id = request.Id,
                Delta = new TextCompletionDelta
                {
                    Text = chunk
                }
            });
        }
        
        // Add usage stats to the last response
        if (responses.Count > 0)
        {
            var promptTokens = prompt.Split(' ').Length;
            var completionTokens = totalTokens;
            
            responses[responses.Count - 1].Usage = new TextCompletionUsage
            {
                PromptTokens = promptTokens,
                CompletionTokens = completionTokens,
                TotalTokens = promptTokens + completionTokens
            };
        }
        
        return responses;
    }
}