using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace McpSdkServer.Tools;

/// <summary>
/// Text completion and analysis tools for MCP server
/// </summary>
[McpServerToolType]
public static class TextCompletionTools
{
    [McpServerTool]
    [Description("Generates text completions with optional statistical analysis")]
    public static async Task<string> CompleteText(
        [Description("The input text prompt to complete")] string prompt,
        [Description("Optional data for statistical analysis (JSON array)")] string? data = null,
        [Description("Analysis type: 'text' for completion or 'stats' for analysis")] string analysisType = "text")
    {
        if (analysisType.ToLower() == "stats" && !string.IsNullOrEmpty(data))
        {
            return await PerformStatisticalAnalysis(prompt, data);
        }
        
        return await PerformTextCompletion(prompt);
    }

    [McpServerTool]
    [Description("Performs statistical analysis on provided data")]
    public static async Task<string> AnalyzeData(
        [Description("The data to analyze (JSON array)")] string data,
        [Description("Type of analysis to perform")] string analysisType = "basic")
    {
        return await PerformStatisticalAnalysis("Statistical Analysis", data);
    }

    [McpServerTool]
    [Description("Simple echo tool for testing")]
    public static string Echo(
        [Description("The message to echo back")] string message)
    {
        return $"Echo: {message}";
    }

    [McpServerTool]
    [Description("Gets server information and capabilities")]
    public static object GetServerInfo()
    {
        return new
        {
            Name = "MCP SDK .NET Server",
            Version = "1.0.0",
            Description = "Official SDK-based MCP server with text completion and statistical analysis",
            Capabilities = new[] { "text-completions", "statistical-analysis", "echo", "server-info" },
            Transport = "Stdio/JSON-RPC",
            Implementation = "Official Microsoft MCP SDK"
        };
    }

    private static async Task<string> PerformTextCompletion(string prompt)
    {
        // Simulate async processing
        await Task.Delay(50);
        
        return $"{prompt} [SDK COMPLETION]";
    }

    private static async Task<string> PerformStatisticalAnalysis(string prompt, string data)
    {
        // Simulate async processing
        await Task.Delay(100);
          try
        {
            var jsonData = JsonSerializer.Deserialize<JsonElement[]>(data);
            if (jsonData == null)
            {
                return "Error: Failed to deserialize JSON data";
            }
            
            var stats = new
            {
                ItemCount = jsonData.Length,
                Analysis = "Statistical analysis completed using MCP SDK",
                ProcessedAt = DateTime.UtcNow,
                SampleData = jsonData.Take(3).ToArray()
            };
            
            return JsonSerializer.Serialize(stats, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }
        catch (JsonException)
        {
            return "Error: Invalid JSON data provided for statistical analysis";
        }
    }
}
