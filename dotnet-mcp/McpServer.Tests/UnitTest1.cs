using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using McpServer.Models;

namespace McpServer.Tests;

public class McpControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public McpControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SessionCreate_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new SessionCreateRequest
        {
            Attributes = new SessionCreateAttributes
            {
                AccessToken = "test_token"
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/v1/session", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var sessionResponse = JsonSerializer.Deserialize<SessionCreateResponse>(content);
        
        Assert.NotNull(sessionResponse);
        Assert.Equal(request.Id, sessionResponse!.Id);
        Assert.False(string.IsNullOrEmpty(sessionResponse.SessionId));
    }

    [Fact]
    public async Task SessionClose_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // First create a session
        var createRequest = new SessionCreateRequest();
        var createResponse = await client.PostAsJsonAsync("/v1/session", createRequest);
        var content = await createResponse.Content.ReadAsStringAsync();
        var sessionResponse = JsonSerializer.Deserialize<SessionCreateResponse>(content);
        
        Assert.NotNull(sessionResponse);
        
        // Now close it
        var sessionId = sessionResponse!.SessionId;

        // Act
        var closeHttpResponse = await client.DeleteAsync($"/v1/session/{sessionId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, closeHttpResponse.StatusCode);
    }
}