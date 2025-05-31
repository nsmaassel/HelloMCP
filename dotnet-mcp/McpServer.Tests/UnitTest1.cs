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
    [Fact]
    public async Task TextCompletion_WithPrompt_ReturnsCompletion()
    {
        var client = _factory.CreateClient();
        // Create session
        var sessionResp = await client.PostAsJsonAsync("/v1/session", new SessionCreateRequest());
        var sessionContent = await sessionResp.Content.ReadAsStringAsync();
        var session = JsonSerializer.Deserialize<SessionCreateResponse>(sessionContent);
        Assert.NotNull(session);

        // Text completion request
        var req = new TextCompletionRequest
        {
            SessionId = session!.SessionId,
            Inputs = new TextCompletionInputs { Prompt = "The capital of France is" }
        };
        var resp = await client.PostAsJsonAsync("/v1/text/completions", req);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("COMPLETION", body);
    }

    [Fact]
    public async Task TextCompletion_WithStats_ReturnsStatAnalysis()
    {
        var client = _factory.CreateClient();
        // Create session
        var sessionResp = await client.PostAsJsonAsync("/v1/session", new SessionCreateRequest());
        var sessionContent = await sessionResp.Content.ReadAsStringAsync();
        var session = JsonSerializer.Deserialize<SessionCreateResponse>(sessionContent);
        Assert.NotNull(session);

        // Stat analysis request
        var req = new TextCompletionRequest
        {
            SessionId = session!.SessionId,
            Inputs = new TextCompletionInputs
            {
                Stats = new MatchStats
                {
                    Player1 = new() { ["aces"] = 5, ["points"] = 20 },
                    Player2 = new() { ["aces"] = 3, ["points"] = 25 }
                }
            }
        };
        var resp = await client.PostAsJsonAsync("/v1/text/completions", req);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("stat-analysis-response", body);
        Assert.Contains("player1", body);
        Assert.Contains("player2", body);
    }

    [Fact]
    public async Task TextCompletion_MissingInputs_ReturnsError()
    {
        var client = _factory.CreateClient();
        // Create session
        var sessionResp = await client.PostAsJsonAsync("/v1/session", new SessionCreateRequest());
        var sessionContent = await sessionResp.Content.ReadAsStringAsync();
        var session = JsonSerializer.Deserialize<SessionCreateResponse>(sessionContent);
        Assert.NotNull(session);

        // Missing both prompt and stats
        var req = new TextCompletionRequest { SessionId = session!.SessionId, Inputs = new TextCompletionInputs() };
        var resp = await client.PostAsJsonAsync("/v1/text/completions", req);
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("invalid_request", body);
    }

    [Fact]
    public async Task TextCompletion_Stream_WithPrompt_Works()
    {
        var client = _factory.CreateClient();
        // Create session
        var sessionResp = await client.PostAsJsonAsync("/v1/session", new SessionCreateRequest());
        var sessionContent = await sessionResp.Content.ReadAsStringAsync();
        var session = JsonSerializer.Deserialize<SessionCreateResponse>(sessionContent);
        Assert.NotNull(session);

        // Streaming endpoint with prompt
        var req = new TextCompletionRequest
        {
            SessionId = session!.SessionId,
            Inputs = new TextCompletionInputs { Prompt = "Stream this!" }
        };
        var resp = await client.PostAsJsonAsync("/v1/text/completions/stream", req);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("data:", body);
    }

    [Fact]
    public async Task TextCompletion_Stream_WithStats_Works()
    {
        var client = _factory.CreateClient();
        // Create session
        var sessionResp = await client.PostAsJsonAsync("/v1/session", new SessionCreateRequest());
        var sessionContent = await sessionResp.Content.ReadAsStringAsync();
        var session = JsonSerializer.Deserialize<SessionCreateResponse>(sessionContent);
        Assert.NotNull(session);

        // Streaming endpoint with stats
        var req = new TextCompletionRequest
        {
            SessionId = session!.SessionId,
            Inputs = new TextCompletionInputs
            {
                Stats = new MatchStats
                {
                    Player1 = new() { ["aces"] = 2 },
                    Player2 = new() { ["aces"] = 3 }
                }
            }
        };
        var resp = await client.PostAsJsonAsync("/v1/text/completions/stream", req);
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        Assert.Contains("data:", body);
        Assert.Contains("stat-analysis-stream-response", body);
    }
}