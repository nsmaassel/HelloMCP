using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using McpServer.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace McpServer.Tests;

/// <summary>
/// End-to-end integration tests for the MCP server
/// </summary>
public class EndToEndIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EndToEndIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CompleteEndToEndFlow_VerifySuccessfulFlow()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Step 1: Create a session
        var sessionCreateRequest = new SessionCreateRequest
        {
            Attributes = new SessionCreateAttributes
            {
                AccessToken = "test_token"
            }
        };

        // Act - Create Session
        var createSessionResponse = await client.PostAsJsonAsync("/v1/session", sessionCreateRequest);

        // Assert - Create Session
        Assert.Equal(HttpStatusCode.OK, createSessionResponse.StatusCode);
        var sessionContent = await createSessionResponse.Content.ReadAsStringAsync();
        var sessionResponse = JsonSerializer.Deserialize<SessionCreateResponse>(sessionContent);
        Assert.NotNull(sessionResponse);
        Assert.False(string.IsNullOrEmpty(sessionResponse!.SessionId));

        // Step 2: Send a text completion request
        var textCompletionRequest = new TextCompletionRequest
        {
            SessionId = sessionResponse.SessionId,
            Inputs = new TextCompletionInputs
            {
                Prompt = "Hello, world!",
                Temperature = 0.7f,
                MaxTokens = 100
            }
        };

        // Act - Text Completion
        var textCompletionResponse = await client.PostAsJsonAsync("/v1/text/completions", textCompletionRequest);

        // Assert - Text Completion
        Assert.Equal(HttpStatusCode.OK, textCompletionResponse.StatusCode);
        // Content type can be application/json or application/x-ndjson depending on implementation
        Assert.Contains(textCompletionResponse.Content.Headers.ContentType?.MediaType, 
            new[] { "application/json", "application/x-ndjson" });
        
        var completionContent = await textCompletionResponse.Content.ReadAsStringAsync();
        
        // Instead of parsing, just verify it contains expected fields
        Assert.Contains("id", completionContent.ToLower());

        // Step 3: Close the session
        // Act - Close Session
        var closeSessionResponse = await client.DeleteAsync($"/v1/session/{sessionResponse.SessionId}");

        // Assert - Close Session
        Assert.Equal(HttpStatusCode.OK, closeSessionResponse.StatusCode);
    }

    [Fact]
    public async Task StreamCompletions_ReturnsServerSentEvents()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Create a session first
        var sessionCreateRequest = new SessionCreateRequest();
        var createSessionResponse = await client.PostAsJsonAsync("/v1/session", sessionCreateRequest);
        var sessionContent = await createSessionResponse.Content.ReadAsStringAsync();
        var sessionResponse = JsonSerializer.Deserialize<SessionCreateResponse>(sessionContent);
        Assert.NotNull(sessionResponse);

        // Create a streaming completion request
        var streamCompletionRequest = new TextCompletionRequest
        {
            SessionId = sessionResponse!.SessionId,
            Inputs = new TextCompletionInputs
            {
                Prompt = "Hello, world!",
                Temperature = 0.7f,
                MaxTokens = 100
            }
        };

        // Set up HttpRequestMessage to specify we want to receive the raw stream
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/v1/text/completions/stream");
        requestMessage.Content = JsonContent.Create(streamCompletionRequest);

        // Act - Start streaming request
        var streamResponse = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

        // Assert - Check headers for SSE format
        Assert.Equal(HttpStatusCode.OK, streamResponse.StatusCode);
        Assert.Equal("text/event-stream", streamResponse.Content.Headers.ContentType?.MediaType);

        // Read the stream content to verify it follows SSE format (data: prefix)
        var stream = await streamResponse.Content.ReadAsStreamAsync();
        var buffer = new byte[1024];
        var read = await stream.ReadAsync(buffer, 0, buffer.Length);
        var content = Encoding.UTF8.GetString(buffer, 0, read);

        // Check for SSE format
        Assert.StartsWith("data:", content);
    }

    [Fact]
    public async Task InvalidSession_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var invalidSessionId = "non-existent-session-id";

        // Create a completion request with invalid session ID
        var completionRequest = new TextCompletionRequest
        {
            SessionId = invalidSessionId,
            Inputs = new TextCompletionInputs
            {
                Prompt = "This should fail",
                Temperature = 0.7f,
                MaxTokens = 100
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/v1/text/completions", completionRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        
        // Verify error structure by checking for keywords
        Assert.Contains("error", content.ToLower());
        Assert.Contains("code", content.ToLower());
        Assert.Contains("message", content.ToLower());
    }
}