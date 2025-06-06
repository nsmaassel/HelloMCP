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

public class SessionFunctionsTests
{
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ILogger<SessionFunctions>> _loggerMock;
    private readonly Mock<ISessionService> _sessionServiceMock;

    public SessionFunctionsTests()
    {
        _loggerMock = new Mock<ILogger<SessionFunctions>>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
        _sessionServiceMock = new Mock<ISessionService>();
    }

    [Fact]
    public async Task CreateSession_ReturnsValidResponse()
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
        
        var memoryStream = new MemoryStream();
        var requestBody = new SessionCreateRequest
        {
            Attributes = new SessionCreateAttributes
            {
                AccessToken = "test-token"
            }
        };
        var requestJson = JsonSerializer.Serialize(requestBody);
        var requestBytes = System.Text.Encoding.UTF8.GetBytes(requestJson);
        var requestStream = new MemoryStream(requestBytes);
        requestStream.Position = 0;
        
        httpRequestMock.Setup(r => r.Body).Returns(requestStream);
        httpRequestMock.Setup(r => r.ReadFromJsonAsync<SessionCreateRequest>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestBody);
        
        httpResponseMock.Setup(r => r.Body).Returns(memoryStream);
        
        var sessionFunctions = new SessionFunctions(_loggerFactoryMock.Object, _sessionServiceMock.Object);

        // Act
        var result = await sessionFunctions.CreateSession(httpRequestMock.Object);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        _sessionServiceMock.Verify(x => x.CreateSession(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CloseSession_ExistingSession_ReturnsOk()
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
        
        var memoryStream = new MemoryStream();
        httpResponseMock.Setup(r => r.Body).Returns(memoryStream);
        
        var sessionFunctions = new SessionFunctions(_loggerFactoryMock.Object, _sessionServiceMock.Object);

        // Act
        var result = await sessionFunctions.CloseSession(httpRequestMock.Object, testSessionId);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        _sessionServiceMock.Verify(x => x.CloseSession(testSessionId), Times.Once);
    }
}