using System.Net;
using AzureFunctionsMcp.Functions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureFunctionsMcp.Tests;

public class OAuthFunctionsTests
{
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Mock<ILogger<OAuthFunctions>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;

    public OAuthFunctionsTests()
    {
        _loggerMock = new Mock<ILogger<OAuthFunctions>>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_loggerMock.Object);
        _configurationMock = new Mock<IConfiguration>();
    }

    [Fact]
    public void GetOAuthServerMetadata_ReturnsValidResponse()
    {
        // Arrange
        var testIssuer = "https://test-issuer.example.com";
        _configurationMock.Setup(x => x[It.Is<string>(s => s == "MCP_OAUTH_ISSUER")]).Returns(testIssuer);

        var httpRequestMock = new Mock<HttpRequestData>(Mock.Of<FunctionContext>());
        var httpResponseMock = new Mock<HttpResponseData>(Mock.Of<FunctionContext>());
        
        // Mock URL
        var url = new Uri("https://localhost:7071");
        httpRequestMock.Setup(r => r.Url).Returns(url);
        
        httpRequestMock.Setup(r => r.CreateResponse()).Returns(httpResponseMock.Object);
        httpResponseMock.SetupProperty(r => r.StatusCode);
        httpResponseMock.SetupProperty(r => r.Headers, new HttpHeadersCollection());
        
        var memoryStream = new MemoryStream();
        httpResponseMock.Setup(r => r.Body).Returns(memoryStream);
        
        var oauthFunctions = new OAuthFunctions(_loggerFactoryMock.Object, _configurationMock.Object);

        // Act
        var result = oauthFunctions.GetOAuthServerMetadata(httpRequestMock.Object);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Contains(result.Headers.GetHeaders(), h => h.Key == "Content-Type" && h.Value.Contains("application/json"));
    }

    [Fact]
    public void OAuthToken_ReturnsValidToken()
    {
        // Arrange
        var httpRequestMock = new Mock<HttpRequestData>(Mock.Of<FunctionContext>());
        var httpResponseMock = new Mock<HttpResponseData>(Mock.Of<FunctionContext>());
        
        httpRequestMock.Setup(r => r.CreateResponse()).Returns(httpResponseMock.Object);
        httpResponseMock.SetupProperty(r => r.StatusCode);
        httpResponseMock.SetupProperty(r => r.Headers, new HttpHeadersCollection());
        
        var memoryStream = new MemoryStream();
        httpResponseMock.Setup(r => r.Body).Returns(memoryStream);
        
        var oauthFunctions = new OAuthFunctions(_loggerFactoryMock.Object, _configurationMock.Object);

        // Act
        var result = oauthFunctions.Token(httpRequestMock.Object);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Contains(result.Headers.GetHeaders(), h => h.Key == "Content-Type" && h.Value.Contains("application/json"));
    }
}