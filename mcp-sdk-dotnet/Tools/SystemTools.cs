using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace McpSdkServer.Tools;

/// <summary>
/// System monitoring and resource management tools
/// </summary>
[McpServerToolType]
public static class SystemTools
{
    [McpServerTool]
    [Description("Gets comprehensive system information including hardware and runtime details")]
    public static async Task<string> GetSystemInfo(
        [Description("Include detailed hardware information")] bool includeHardware = true,
        [Description("Include memory and performance metrics")] bool includeMetrics = true)
    {
        await Task.Delay(10); // Simulate brief processing
        
        var info = new
        {
            System = new
            {
                OperatingSystem = Environment.OSVersion.ToString(),
                Platform = RuntimeInformation.OSDescription,
                Architecture = RuntimeInformation.OSArchitecture.ToString(),
                ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
                MachineName = Environment.MachineName,
                UserName = Environment.UserName,
                Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
                Is64BitProcess = Environment.Is64BitProcess
            },
            Runtime = new
            {
                DotNetVersion = Environment.Version.ToString(),
                FrameworkDescription = RuntimeInformation.FrameworkDescription,
                RuntimeIdentifier = RuntimeInformation.RuntimeIdentifier,
                ProcessorCount = Environment.ProcessorCount,
                CurrentDirectory = Environment.CurrentDirectory,
                CommandLine = Environment.CommandLine,
                WorkingSet = Environment.WorkingSet,
                HasShutdownStarted = Environment.HasShutdownStarted
            },
            Hardware = includeHardware ? new
            {
                ProcessorCount = Environment.ProcessorCount,
                SystemPageSize = Environment.SystemPageSize,
                TickCount = Environment.TickCount64
            } : null,
            Performance = includeMetrics ? new
            {
                WorkingSetMB = Math.Round(Environment.WorkingSet / 1024.0 / 1024.0, 2),
                GCTotalMemoryMB = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 2),
                GCGen0Collections = GC.CollectionCount(0),
                GCGen1Collections = GC.CollectionCount(1),
                GCGen2Collections = GC.CollectionCount(2)
            } : null,
            Timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Monitors system resources and provides alerts for resource usage")]
    public static async Task<string> MonitorResources(
        [Description("Memory threshold percentage (0-100) for alerts")] double memoryThresholdPercent = 80.0,
        [Description("Include CPU usage monitoring")] bool includeCpu = false)
    {
        await Task.Delay(50); // Simulate monitoring
        
        var currentMemoryMB = Math.Round(Environment.WorkingSet / 1024.0 / 1024.0, 2);
        var gcMemoryMB = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 2);
        
        var monitoring = new
        {
            Timestamp = DateTime.UtcNow,
            Memory = new
            {
                WorkingSetMB = currentMemoryMB,
                GCMemoryMB = gcMemoryMB,
                ThresholdPercent = memoryThresholdPercent,
                Alert = currentMemoryMB > (memoryThresholdPercent / 100.0 * 1000) // Simple threshold check
            },
            GarbageCollection = new
            {
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2),
                TotalMemoryMB = gcMemoryMB,
                LastCollectionTime = DateTime.UtcNow
            },
            Process = new
            {
                ProcessorCount = Environment.ProcessorCount,
                TickCount = Environment.TickCount64,
                HasShutdownStarted = Environment.HasShutdownStarted
            },
            Recommendations = GetResourceRecommendations(currentMemoryMB, gcMemoryMB)
        };

        return JsonSerializer.Serialize(monitoring, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Forces garbage collection and reports memory statistics")]
    public static async Task<string> ForceGarbageCollection(
        [Description("Generation to collect (0, 1, 2, or -1 for all)")] int generation = -1,
        [Description("Include detailed memory analysis")] bool includeAnalysis = true)
    {
        var beforeMemory = GC.GetTotalMemory(false);
        var beforeWorkingSet = Environment.WorkingSet;
        
        await Task.Delay(10);
        
        // Force garbage collection
        if (generation == -1)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        else
        {
            GC.Collect(generation);
        }
        
        var afterMemory = GC.GetTotalMemory(false);
        var afterWorkingSet = Environment.WorkingSet;
        
        var result = new
        {
            Timestamp = DateTime.UtcNow,
            Generation = generation == -1 ? "All" : generation.ToString(),
            BeforeCollection = new
            {
                GCMemoryBytes = beforeMemory,
                GCMemoryMB = Math.Round(beforeMemory / 1024.0 / 1024.0, 2),
                WorkingSetBytes = beforeWorkingSet,
                WorkingSetMB = Math.Round(beforeWorkingSet / 1024.0 / 1024.0, 2)
            },
            AfterCollection = new
            {
                GCMemoryBytes = afterMemory,
                GCMemoryMB = Math.Round(afterMemory / 1024.0 / 1024.0, 2),
                WorkingSetBytes = afterWorkingSet,
                WorkingSetMB = Math.Round(afterWorkingSet / 1024.0 / 1024.0, 2)
            },
            Freed = new
            {
                GCMemoryBytes = beforeMemory - afterMemory,
                GCMemoryMB = Math.Round((beforeMemory - afterMemory) / 1024.0 / 1024.0, 2),
                WorkingSetBytes = beforeWorkingSet - afterWorkingSet,
                WorkingSetMB = Math.Round((beforeWorkingSet - afterWorkingSet) / 1024.0 / 1024.0, 2)
            },
            Collections = new
            {
                Gen0 = GC.CollectionCount(0),
                Gen1 = GC.CollectionCount(1),
                Gen2 = GC.CollectionCount(2)
            },
            Analysis = includeAnalysis ? AnalyzeMemoryUsage(afterMemory, afterWorkingSet) : null
        };

        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string[] GetResourceRecommendations(double workingSetMB, double gcMemoryMB)
    {
        var recommendations = new List<string>();
        
        if (workingSetMB > 500)
        {
            recommendations.Add("High working set detected - consider memory optimization");
        }
        
        if (gcMemoryMB > 100)
        {
            recommendations.Add("High GC memory usage - consider forcing garbage collection");
        }
        
        if (recommendations.Count == 0)
        {
            recommendations.Add("Memory usage within normal parameters");
        }
        
        return recommendations.ToArray();
    }

    private static object AnalyzeMemoryUsage(long gcMemory, long workingSet)
    {
        return new
        {
            MemoryEfficiency = gcMemory == 0 ? 100.0 : Math.Round((double)gcMemory / workingSet * 100, 2),
            MemoryCategory = gcMemory < 50 * 1024 * 1024 ? "Low" : gcMemory < 200 * 1024 * 1024 ? "Moderate" : "High",
            Recommendations = gcMemory > 100 * 1024 * 1024 ? new[] { "Consider memory optimization", "Monitor for memory leaks" } : new[] { "Memory usage normal" }
        };
    }
}
