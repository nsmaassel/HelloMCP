using System.Net;
using System.Text.Json;
using AzureFunctionsMcp.Functions;
using AzureFunctionsMcp.Models;
using AzureFunctionsMcp.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureFunctionsMcp.Tests;

public class TextCompletionFunctionsTests
{
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ILogger<TextCompletionFunctions>> _loggerMock;
    private readonly Mock<SessionService> _sessionServiceMock;

    public TextCompletionFunctionsTests()
    {
        _loggerMock = new Mock<ILogger<TextCompletionFunctions>>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
        _sessionServiceMock = new Mock<SessionService>(new Mock<ILogger<SessionService>>().Object);
    }

    [Fact]
    public async Task GetTextCompletions_ValidSession_ReturnsOk()
    {
        // Arrange
        var testSessionId = "test-session-id";
        _sessionServiceMock.Setup(x => x.ValidateSession(testSessionId)).Returns(true);

        var httpRequestMock = new Mock<HttpRequestData>(TestHelpers.CreateFunctionContext());
        var httpResponseMock = new Mock<HttpResponseData>(TestHelpers.CreateFunctionContext());
        httpRequestMock.Setup(r => r.CreateResponse()).Returns(httpResponseMock.Object);
        httpResponseMock.SetupProperty(r => r.StatusCode);
        httpResponseMock.SetupProperty(r => r.Headers, new HttpHeadersCollection());
        
        var memoryStream = new MemoryStream();
        httpResponseMock.Setup(r => r.Body).Returns(memoryStream);
        
        var requestBody = new TextCompletionRequest
        {
            SessionId = testSessionId,
            Inputs = new TextCompletionInputs
            {
                Prompt = "Hello, world!",
                Temperature = 0.7f,
                MaxTokens = 100
            }
        };
        
        httpRequestMock.Setup(r => r.ReadFromJsonAsync<TextCompletionRequest>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestBody);
        
        var textCompletionFunctions = new TextCompletionFunctions(_loggerFactoryMock.Object, _sessionServiceMock.Object);

        // Act
        var result = await textCompletionFunctions.GetTextCompletions(httpRequestMock.Object);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        _sessionServiceMock.Verify(x => x.ValidateSession(testSessionId), Times.Once);
    }

    [Fact]
    public async Task GetTextCompletions_InvalidSession_ReturnsBadRequest()
    {
        // Arrange
        var testSessionId = "invalid-session-id";
        _sessionServiceMock.Setup(x => x.ValidateSession(testSessionId)).Returns(false);

        var httpRequestMock = new Mock<HttpRequestData>(TestHelpers.CreateFunctionContext());
        var httpResponseMock = new Mock<HttpResponseData>(TestHelpers.CreateFunctionContext());
        httpRequestMock.Setup(r => r.CreateResponse()).Returns(httpResponseMock.Object);
        httpResponseMock.SetupProperty(r => r.StatusCode);
        httpResponseMock.SetupProperty(r => r.Headers, new HttpHeadersCollection());
        
        var memoryStream = new MemoryStream();
        httpResponseMock.Setup(r => r.Body).Returns(memoryStream);
        
        var requestBody = new TextCompletionRequest
        {
            SessionId = testSessionId,
            Inputs = new TextCompletionInputs
            {
                Prompt = "Hello, world!",
                Temperature = 0.7f,
                MaxTokens = 100
            }
        };
        
        httpRequestMock.Setup(r => r.ReadFromJsonAsync<TextCompletionRequest>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestBody);
        
        var textCompletionFunctions = new TextCompletionFunctions(_loggerFactoryMock.Object, _sessionServiceMock.Object);

        // Act
        var result = await textCompletionFunctions.GetTextCompletions(httpRequestMock.Object);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        _sessionServiceMock.Verify(x => x.ValidateSession(testSessionId), Times.Once);
    }
}