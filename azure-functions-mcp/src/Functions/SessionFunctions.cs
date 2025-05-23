using System.Net;
using AzureFunctionsMcp.Models;
using AzureFunctionsMcp.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsMcp.Functions;

public class SessionFunctions
{
    private readonly ILogger _logger;
    private readonly SessionService _sessionService;

    public SessionFunctions(ILoggerFactory loggerFactory, SessionService sessionService)
    {
        _logger = loggerFactory.CreateLogger<SessionFunctions>();
        _sessionService = sessionService;
    }

    [Function("CreateSession")]
    public async Task<HttpResponseData> CreateSession(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/session")] HttpRequestData req)
    {
        _logger.LogInformation("Processing session creation request");

        try
        {
            var request = await req.ReadFromJsonAsync<SessionCreateRequest>();
            if (request == null)
            {
                return CreateErrorResponse(req, "invalid_request", "Missing or invalid session creation request");
            }

            var sessionId = _sessionService.CreateSession(request.Attributes?.AccessToken);
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            
            var responseData = new SessionCreateResponse 
            {
                Id = request.Id,
                SessionId = sessionId
            };
            
            await response.WriteAsJsonAsync(responseData);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing session creation request");
            return CreateErrorResponse(req, "internal_error", "An unexpected error occurred");
        }
    }

    [Function("CloseSession")]
    public async Task<HttpResponseData> CloseSession(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/session/{sessionId}")] HttpRequestData req,
        string sessionId)
    {
        _logger.LogInformation("Processing session close request for session {SessionId}", sessionId);

        try
        {
            var success = _sessionService.CloseSession(sessionId);
            if (!success)
            {
                return CreateErrorResponse(req, "invalid_session", "Session not found or already closed");
            }
            
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");
            
            var responseData = new SessionCloseResponse
            {
                Id = Guid.NewGuid().ToString()
            };
            
            await response.WriteAsJsonAsync(responseData);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing session close request");
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
}