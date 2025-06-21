using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace McpSdkServer.Tools;

/// <summary>
/// Advanced tools showcasing MCP SDK capabilities
/// </summary>
[McpServerToolType]
public static class AdvancedTools
{
    [McpServerTool]
    [Description("Downloads content from a URL and provides analysis")]
    public static async Task<string> AnalyzeWebContent(
        IMcpServer server,
        HttpClient httpClient,
        [Description("The URL to download and analyze")] string url,
        [Description("Type of analysis: 'summary', 'length', 'structure'")] string analysisType = "summary",
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Download content
            var content = await httpClient.GetStringAsync(url, cancellationToken);
            
            return analysisType.ToLower() switch
            {
                "length" => $"Content length analysis:\n- Characters: {content.Length}\n- Lines: {content.Split('\n').Length}\n- Words (approx): {content.Split(' ').Length}",
                "structure" => AnalyzeContentStructure(content),
                "summary" or _ => $"Content summary for {url}:\n- Length: {content.Length} characters\n- Type: Web content\n- Analysis: SDK-powered content analysis"
            };
        }
        catch (HttpRequestException ex)
        {
            return $"Error downloading content from {url}: {ex.Message}";
        }
    }

    [McpServerTool]
    [Description("Generates a detailed system report")]
    public static async Task<string> GenerateSystemReport(
        [Description("Include performance metrics")] bool includePerformance = true,
        [Description("Include environment details")] bool includeEnvironment = true)
    {
        await Task.Delay(50); // Simulate processing

        var report = new
        {
            Timestamp = DateTime.UtcNow,
            Server = new
            {
                Name = "MCP SDK Server",
                Version = "1.0.0",
                Runtime = Environment.Version.ToString(),
                Platform = Environment.OSVersion.ToString()
            },
            Performance = includePerformance ? new
            {
                WorkingSet = Environment.WorkingSet,
                ProcessorCount = Environment.ProcessorCount,
                TickCount = Environment.TickCount64
            } : null,
            Environment = includeEnvironment ? new
            {
                MachineName = Environment.MachineName,
                UserName = Environment.UserName,
                CurrentDirectory = Environment.CurrentDirectory,
                CommandLine = Environment.CommandLine
            } : null
        };

        return JsonSerializer.Serialize(report, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }

    [McpServerTool]
    [Description("Processes structured data with advanced analytics")]
    public static async Task<string> ProcessStructuredData(
        [Description("JSON data to process")] string jsonData,
        [Description("Processing operation: 'aggregate', 'filter', 'transform'")] string operation = "aggregate",
        [Description("Optional filter criteria (JSON object)")] string? filterCriteria = null)
    {
        await Task.Delay(75); // Simulate processing
          try
        {
            var data = JsonSerializer.Deserialize<JsonElement[]>(jsonData);
            if (data == null)
            {
                return "Error: Failed to deserialize JSON data";
            }
            
            return operation.ToLower() switch
            {
                "filter" => ProcessFilter(data, filterCriteria),
                "transform" => ProcessTransform(data),
                "aggregate" or _ => ProcessAggregate(data)
            };
        }
        catch (JsonException ex)
        {
            return $"Error processing JSON data: {ex.Message}";
        }
    }

    private static string AnalyzeContentStructure(string content)
    {
        var lines = content.Split('\n');
        var paragraphs = content.Split("\n\n");
        var sentences = content.Split('.', '!', '?');
        
        return $"Content structure analysis:\n" +
               $"- Lines: {lines.Length}\n" +
               $"- Paragraphs: {paragraphs.Length}\n" +
               $"- Sentences: {sentences.Length}\n" +
               $"- Average line length: {(content.Length / (double)lines.Length):F1} characters";
    }

    private static string ProcessAggregate(JsonElement[] data)
    {
        var result = new
        {
            TotalItems = data.Length,
            ProcessedAt = DateTime.UtcNow,
            AggregateType = "Basic statistics",
            Summary = $"Processed {data.Length} items using MCP SDK aggregation"
        };
        
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string ProcessFilter(JsonElement[] data, string? filterCriteria)
    {
        var filteredCount = data.Length; // Simple implementation
        
        var result = new
        {
            OriginalItems = data.Length,
            FilteredItems = filteredCount,
            FilterCriteria = filterCriteria ?? "No filter applied",
            ProcessedAt = DateTime.UtcNow
        };
        
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string ProcessTransform(JsonElement[] data)
    {
        var result = new
        {
            OriginalItems = data.Length,
            TransformedItems = data.Length,
            TransformationType = "Identity transformation",
            ProcessedAt = DateTime.UtcNow,
            Note = "Advanced transformations can be implemented based on requirements"
        };
        
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }
}
