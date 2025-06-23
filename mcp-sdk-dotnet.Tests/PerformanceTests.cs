using System.Diagnostics;
using Xunit;

namespace McpSdkServer.Tests;

/// <summary>
/// Basic performance tests for MCP SDK server
/// These provide a foundation for the future performance monitoring dashboard
/// </summary>
public class PerformanceTests
{
    [Fact]
    public void BasicOperation_CompletesWithinTimeLimit()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        var testData = new List<string>();

        // Act - Simulate basic operations that might be common in MCP servers
        for (int i = 0; i < 1000; i++)
        {
            testData.Add($"test-data-{i}");
        }
        
        var result = testData.Count;
        stopwatch.Stop();

        // Assert - Basic operation should complete quickly
        Assert.Equal(1000, result);
        Assert.True(stopwatch.ElapsedMilliseconds < 100, 
            $"Basic operation took {stopwatch.ElapsedMilliseconds}ms, which exceeds 100ms limit");
    }

    [Fact]
    public void MemoryUsage_RemainsReasonable()
    {
        // Arrange
        var initialMemory = GC.GetTotalMemory(false);

        // Act - Allocate some memory like an MCP server might
        var testObjects = new List<object>();
        for (int i = 0; i < 10000; i++)
        {
            testObjects.Add(new { Id = i, Data = $"test-{i}" });
        }

        var peakMemory = GC.GetTotalMemory(false);
        
        // Cleanup
        testObjects.Clear();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        
        var finalMemory = GC.GetTotalMemory(true);

        // Assert - Memory usage should be reasonable
        var memoryUsed = peakMemory - initialMemory;
        Assert.True(memoryUsed < 10_000_000, // Less than 10MB for this simple test
            $"Memory usage was {memoryUsed / 1024 / 1024}MB, which exceeds 10MB limit");
        
        // Memory should be mostly cleaned up
        var memoryLeaked = finalMemory - initialMemory;
        Assert.True(memoryLeaked < 1_000_000, // Less than 1MB leaked
            $"Memory leak detected: {memoryLeaked / 1024}KB not cleaned up");
    }
}