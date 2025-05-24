using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using McpServer.Models;

namespace McpServer.Tests;

/// <summary>
/// Health monitoring tests for MCP servers
/// These tests can be used to monitor servers in production
/// </summary>
public class HealthMonitoringTests
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;

    // In a real scenario, this would be configured from environment variables or settings
    public HealthMonitoringTests()
    {
        _client = new HttpClient();
        _baseUrl = "http://localhost:5000"; // Would be configurable in production
    }

    [Fact(Skip = "This test is meant to be run against a live server")]
    public async Task ServerAvailability_ReturnsSuccess()
    {
        // Act
        try
        {
            var response = await _client.GetAsync($"{_baseUrl}/health");
            
            // Assert
            Assert.True(response.IsSuccessStatusCode, 
                $"Server health check failed with status code: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Assert.Fail($"Server is not available: {ex.Message}");
        }
    }

    [Fact(Skip = "This test is meant to be run against a live server")]
    public async Task ServerResponseTime_WithinAcceptableLimits()
    {
        // Arrange
        var stopwatch = new Stopwatch();
        var timeoutThreshold = TimeSpan.FromSeconds(2); // Configurable threshold

        // Act
        stopwatch.Start();
        var response = await _client.GetAsync($"{_baseUrl}/health");
        stopwatch.Stop();

        // Assert
        Assert.True(response.IsSuccessStatusCode, "Health check failed");
        Assert.True(stopwatch.Elapsed < timeoutThreshold, 
            $"Response time exceeds threshold. Expected < {timeoutThreshold}, Actual: {stopwatch.Elapsed}");
    }

    [Fact(Skip = "This test is meant to be run against a live server")]
    public async Task SessionCreateAndClose_Succeeds()
    {
        // Arrange
        var createRequest = new SessionCreateRequest
        {
            Attributes = new SessionCreateAttributes
            {
                AccessToken = "monitoring_test_token"
            }
        };

        // Act - Create session
        var createResponse = await _client.PostAsJsonAsync($"{_baseUrl}/v1/session", createRequest);
        
        // Verify create
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var sessionResponse = await createResponse.Content.ReadFromJsonAsync<SessionCreateResponse>();
        Assert.NotNull(sessionResponse);
        
        // Act - Close session
        var closeResponse = await _client.DeleteAsync($"{_baseUrl}/v1/session/{sessionResponse!.SessionId}");
        
        // Verify close
        Assert.Equal(HttpStatusCode.OK, closeResponse.StatusCode);
    }

    [Fact(Skip = "This test is meant to be run against a live server")]
    public async Task ErrorRateMonitoring_BelowThreshold()
    {
        // Arrange
        int totalRequests = 10;
        int successfulRequests = 0;
        double errorRateThreshold = 0.1; // 10% error rate threshold
        
        // Act
        for (int i = 0; i < totalRequests; i++)
        {
            try
            {
                var response = await _client.GetAsync($"{_baseUrl}/health");
                if (response.IsSuccessStatusCode)
                {
                    successfulRequests++;
                }
            }
            catch
            {
                // Count as an error
            }
        }
        
        // Calculate error rate
        double errorRate = (double)(totalRequests - successfulRequests) / totalRequests;
        
        // Assert
        Assert.True(errorRate <= errorRateThreshold, 
            $"Error rate ({errorRate:P}) exceeds threshold ({errorRateThreshold:P})");
    }
}