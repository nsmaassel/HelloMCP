using System.Net;
using System.Text.Json;
using AzureFunctionsMcp.Models;
using AzureFunctionsMcp.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureFunctionsMcp.Tests;

/// <summary>
/// Integration tests to validate MCP protocol compliance
/// </summary>
public class McpProtocolComplianceTests
{
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ISessionService> _sessionServiceMock;

    public McpProtocolComplianceTests()
    {
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(Mock.Of<ILogger>());
        _sessionServiceMock = new Mock<ISessionService>();
    }

    [Fact]
    public async Task CreateSession_ShouldReturnValidMcpResponse()
    {
        // Arrange
        var testSessionId = "test-session-id";
        _sessionServiceMock.Setup(x => x.CreateSession(It.IsAny<string>())).Returns(testSessionId);

        var httpRequestMock = new Mock<HttpRequestData>(Mock.Of<FunctionContext>());
        var httpResponseMock = new Mock<HttpResponseData>(Mock.Of<FunctionContext>());
        httpRequestMock.Setup(r => r.CreateResponse()).Returns(httpResponseMock.Object);
        httpResponseMock.SetupProperty<HttpStatusCode>(r => r.StatusCode);
        
        var headersMock = new Mock<HttpHeadersCollection>();
        httpResponseMock.Setup(r => r.Headers).Returns(headersMock.Object);
        
        var responseMemoryStream = new MemoryStream();
        httpResponseMock.Setup(r => r.Body).Returns(responseMemoryStream);
        
        // Create a request directly rather than mocking ReadFromJsonAsync
        var requestBody = new SessionCreateRequest
        {
            Id = "test-request-id",
            Attributes = new SessionCreateAttributes
            {
                AccessToken = "test-token"
            }
        };
        
        // Setup the function handler
        var functions = new AzureFunctionsMcp.Functions.SessionFunctions(_loggerFactoryMock.Object, _sessionServiceMock.Object);

        // Attach a handler to capture the response body
        var responseJson = "";
        httpResponseMock.Setup(r => r.WriteAsJsonAsync(It.IsAny<SessionCreateResponse>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((obj, _) => 
            {
                responseJson = JsonSerializer.Serialize(obj);
            })
            .Returns(new ValueTask());

        // Act
        await functions.CreateSession(httpRequestMock.Object);

        // Assert
        // Verify the response JSON has the correct MCP format
        Assert.Contains("test-request-id", responseJson); // Verify it returns the same ID
        Assert.Contains(testSessionId, responseJson); // Verify it includes the session ID
        headersMock.Verify(h => h.Add("Content-Type", "application/json; charset=utf-8"), Times.Once);
    }
    
    [Fact]
    public async Task CloseSession_ShouldReturnValidMcpResponse()
    {
        // Arrange
        var testSessionId = "test-session-id";
        _sessionServiceMock.Setup(x => x.CloseSession(testSessionId)).Returns(true);

        var httpRequestMock = new Mock<HttpRequestData>(Mock.Of<FunctionContext>());
        var httpResponseMock = new Mock<HttpResponseData>(Mock.Of<FunctionContext>());
        httpRequestMock.Setup(r => r.CreateResponse()).Returns(httpResponseMock.Object);
        httpResponseMock.SetupProperty<HttpStatusCode>(r => r.StatusCode);
        
        var headersMock = new Mock<HttpHeadersCollection>();
        httpResponseMock.Setup(r => r.Headers).Returns(headersMock.Object);
        
        var responseMemoryStream = new MemoryStream();
        httpResponseMock.Setup(r => r.Body).Returns(responseMemoryStream);
        
        // Setup the function handler
        var functions = new AzureFunctionsMcp.Functions.SessionFunctions(_loggerFactoryMock.Object, _sessionServiceMock.Object);

        // Attach a handler to capture the response body
        var responseJson = "";
        httpResponseMock.Setup(r => r.WriteAsJsonAsync(It.IsAny<SessionCloseResponse>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((obj, _) => 
            {
                responseJson = JsonSerializer.Serialize(obj);
            })
            .Returns(new ValueTask());

        // Act
        await functions.CloseSession(httpRequestMock.Object, testSessionId);

        // Assert
        // Verify the response JSON has the correct MCP format
        Assert.Contains("id", responseJson.ToLower()); // Verify it includes an ID
        Assert.Equal(HttpStatusCode.OK, httpResponseMock.Object.StatusCode);
        headersMock.Verify(h => h.Add("Content-Type", "application/json; charset=utf-8"), Times.Once);
    }
    
    [Fact]
    public async Task GetTextCompletions_ShouldReturnValidMcpResponse()
    {
        // Arrange
        var testSessionId = "test-session-id";
        _sessionServiceMock.Setup(x => x.ValidateSession(testSessionId)).Returns(true);

        var httpRequestMock = new Mock<HttpRequestData>(Mock.Of<FunctionContext>());
        var httpResponseMock = new Mock<HttpResponseData>(Mock.Of<FunctionContext>());
        httpRequestMock.Setup(r => r.CreateResponse()).Returns(httpResponseMock.Object);
        httpResponseMock.SetupProperty<HttpStatusCode>(r => r.StatusCode);
        
        var headersMock = new Mock<HttpHeadersCollection>();
        httpResponseMock.Setup(r => r.Headers).Returns(headersMock.Object);
        
        var responseMemoryStream = new MemoryStream();
        httpResponseMock.Setup(r => r.Body).Returns(responseMemoryStream);
        
        // Create a request directly rather than mocking ReadFromJsonAsync
        var requestBody = new TextCompletionRequest
        {
            Id = "test-request-id",
            SessionId = testSessionId,
            Inputs = new TextCompletionInputs
            {
                Prompt = "Hello, world!",
                Temperature = 0.7f,
                MaxTokens = 100
            }
        };
        
        // Setup the function handler
        var functions = new AzureFunctionsMcp.Functions.TextCompletionFunctions(_loggerFactoryMock.Object, _sessionServiceMock.Object);

        // Attach a handler to capture the response body
        var responseJson = "";
        httpResponseMock.Setup(r => r.WriteAsJsonAsync(It.IsAny<TextCompletionStreamResponse>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((obj, _) => 
            {
                responseJson = JsonSerializer.Serialize(obj);
            })
            .Returns(new ValueTask());
            
        // Setup request body
        httpRequestMock.Setup(r => r.ReadFromJsonAsync<TextCompletionRequest>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestBody);

        // Act
        await functions.GetTextCompletions(httpRequestMock.Object);

        // Assert
        // Verify the response has the correct MCP format
        Assert.Equal(HttpStatusCode.OK, httpResponseMock.Object.StatusCode);
        headersMock.Verify(h => h.Add("Content-Type", "application/json; charset=utf-8"), Times.Once);
    }
    
    [Fact]
    public async Task StreamTextCompletions_ShouldUseCorrectEventStreamFormat()
    {
        // Arrange
        var testSessionId = "test-session-id";
        _sessionServiceMock.Setup(x => x.ValidateSession(testSessionId)).Returns(true);

        var httpRequestMock = new Mock<HttpRequestData>(Mock.Of<FunctionContext>());
        var httpResponseMock = new Mock<HttpResponseData>(Mock.Of<FunctionContext>());
        httpRequestMock.Setup(r => r.CreateResponse()).Returns(httpResponseMock.Object);
        httpResponseMock.SetupProperty<HttpStatusCode>(r => r.StatusCode);
        
        var headersMock = new Mock<HttpHeadersCollection>();
        httpResponseMock.Setup(r => r.Headers).Returns(headersMock.Object);
        
        var responseMemoryStream = new MemoryStream();
        httpResponseMock.Setup(r => r.Body).Returns(responseMemoryStream);
        
        // Create a request directly rather than mocking ReadFromJsonAsync
        var requestBody = new TextCompletionRequest
        {
            Id = "test-request-id",
            SessionId = testSessionId,
            Inputs = new TextCompletionInputs
            {
                Prompt = "Hello, world!",
                Temperature = 0.7f,
                MaxTokens = 100
            }
        };
        
        // Setup the function handler
        var functions = new AzureFunctionsMcp.Functions.TextCompletionFunctions(_loggerFactoryMock.Object, _sessionServiceMock.Object);
            
        // Setup request body
        httpRequestMock.Setup(r => r.ReadFromJsonAsync<TextCompletionRequest>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestBody);

        // Act
        await functions.StreamTextCompletions(httpRequestMock.Object);

        // Assert
        // Verify the response has the correct SSE format headers
        Assert.Equal(HttpStatusCode.OK, httpResponseMock.Object.StatusCode);
        headersMock.Verify(h => h.Add("Content-Type", "text/event-stream"), Times.Once);
        headersMock.Verify(h => h.Add("Cache-Control", "no-cache"), Times.Once);
        headersMock.Verify(h => h.Add("Connection", "keep-alive"), Times.Once);
    }
    
    [Fact]
    public async Task InvalidSession_ShouldReturnBadRequest()
    {
        // Arrange
        var testSessionId = "invalid-session-id";
        _sessionServiceMock.Setup(x => x.ValidateSession(testSessionId)).Returns(false);

        var httpRequestMock = new Mock<HttpRequestData>(Mock.Of<FunctionContext>());
        var httpResponseMock = new Mock<HttpResponseData>(Mock.Of<FunctionContext>());
        httpRequestMock.Setup(r => r.CreateResponse()).Returns(httpResponseMock.Object);
        httpResponseMock.SetupProperty<HttpStatusCode>(r => r.StatusCode);
        
        var headersMock = new Mock<HttpHeadersCollection>();
        httpResponseMock.Setup(r => r.Headers).Returns(headersMock.Object);
        
        var responseMemoryStream = new MemoryStream();
        httpResponseMock.Setup(r => r.Body).Returns(responseMemoryStream);
        
        // Create a request directly rather than mocking ReadFromJsonAsync
        var requestBody = new TextCompletionRequest
        {
            Id = "test-request-id",
            SessionId = testSessionId,
            Inputs = new TextCompletionInputs
            {
                Prompt = "Hello, world!",
                Temperature = 0.7f,
                MaxTokens = 100
            }
        };
        
        // Setup the function handler
        var functions = new AzureFunctionsMcp.Functions.TextCompletionFunctions(_loggerFactoryMock.Object, _sessionServiceMock.Object);
            
        // Setup request body
        httpRequestMock.Setup(r => r.ReadFromJsonAsync<TextCompletionRequest>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestBody);

        // Attach a handler to capture the response body
        var responseJson = "";
        httpResponseMock.Setup(r => r.WriteAsJsonAsync(It.IsAny<ErrorResponse>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((obj, _) => 
            {
                responseJson = JsonSerializer.Serialize(obj);
            })
            .Returns(new ValueTask());

        // Act
        await functions.GetTextCompletions(httpRequestMock.Object);

        // Assert
        // Verify the error response has the correct MCP format
        Assert.Equal(HttpStatusCode.BadRequest, httpResponseMock.Object.StatusCode);
        Assert.Contains("invalid_session", responseJson.ToLower());
        Assert.Contains("error", responseJson.ToLower());
        Assert.Contains("code", responseJson.ToLower());
        Assert.Contains("message", responseJson.ToLower());
    }
}