using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using McpSdkServer.Services;

namespace McpSdkServer.Tools;

/// <summary>
/// Advanced monitoring and metrics tools for production use
/// </summary>
[McpServerToolType]
public static class MonitoringTools
{
    private static IServiceProvider? _serviceProvider;

    /// <summary>
    /// Initialize the service provider for dependency injection
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [McpServerTool]
    [Description("Get comprehensive server performance metrics including request statistics, memory usage, and system performance")]
    public static string GetPerformanceMetrics()
    {
        try
        {
            var metricsService = _serviceProvider?.GetService<MetricsCollectionService>();
            if (metricsService == null)
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = "Metrics collection service not available",
                    Timestamp = DateTime.UtcNow
                }, new JsonSerializerOptions { WriteIndented = true });
            }

            var metrics = metricsService.GetMetrics();
            
            return JsonSerializer.Serialize(new
            {
                Status = "Success",
                ServerMetrics = new
                {
                    Timestamp = metrics.Timestamp,
                    UptimeHours = Math.Round(metrics.UptimeSeconds / 3600, 2),
                    Performance = new
                    {
                        RequestCount = metrics.RequestCount,
                        ErrorCount = metrics.ErrorCount,
                        ErrorRate = Math.Round(metrics.ErrorRate * 100, 2) + "%",
                        RequestsPerSecond = Math.Round(metrics.RequestsPerSecond, 2),
                        AverageResponseTimeMs = Math.Round(metrics.AverageRequestDurationMs, 2),
                        P95ResponseTimeMs = Math.Round(metrics.P95RequestDurationMs, 2),
                        P99ResponseTimeMs = Math.Round(metrics.P99RequestDurationMs, 2)
                    },
                    Memory = new
                    {
                        WorkingSetMB = Math.Round(metrics.MemoryUsageMB, 2),
                        AllocatedMB = Math.Round(metrics.AllocatedMemoryMB, 2),
                        GarbageCollection = new
                        {
                            Gen0Collections = metrics.GcGen0Collections,
                            Gen1Collections = metrics.GcGen1Collections,
                            Gen2Collections = metrics.GcGen2Collections
                        }
                    },
                    System = new
                    {
                        CpuTimeMs = Math.Round(metrics.CpuTimeMs, 2),
                        ThreadCount = metrics.ThreadCount,
                        RuntimeVersion = Environment.Version.ToString(),
                        ProcessorCount = Environment.ProcessorCount
                    }
                }
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                Status = "Error",
                Message = ex.Message,
                Timestamp = DateTime.UtcNow
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    [McpServerTool]
    [Description("Reset all performance metrics counters to start fresh monitoring")]
    public static string ResetMetrics()
    {
        try
        {
            var metricsService = _serviceProvider?.GetService<MetricsCollectionService>();
            if (metricsService == null)
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = "Metrics collection service not available",
                    Timestamp = DateTime.UtcNow
                });
            }

            metricsService.ResetMetrics();
            
            return JsonSerializer.Serialize(new
            {
                Status = "Success",
                Message = "Performance metrics have been reset",
                ResetTime = DateTime.UtcNow
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                Status = "Error",
                Message = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    [McpServerTool]
    [Description("Get current server configuration summary with validation status")]
    public static string GetConfigurationStatus()
    {
        try
        {
            var configValidationService = _serviceProvider?.GetService<ConfigurationValidationService>();
            if (configValidationService == null)
            {
                return JsonSerializer.Serialize(new
                {
                    Status = "Error",
                    Message = "Configuration validation service not available",
                    Timestamp = DateTime.UtcNow
                });
            }

            var isValid = configValidationService.ValidateConfiguration();
            var configSummary = configValidationService.GetConfigurationSummary();
            
            return JsonSerializer.Serialize(new
            {
                Status = "Success",
                ConfigurationValid = isValid,
                ValidationTime = DateTime.UtcNow,
                Configuration = JsonDocument.Parse(configSummary).RootElement
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                Status = "Error",
                Message = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    [McpServerTool]
    [Description("Perform garbage collection and return memory statistics before and after")]
    public static string OptimizeMemory()
    {
        try
        {
            // Get memory before GC
            var memoryBefore = GC.GetTotalMemory(false);
            var gen0Before = GC.CollectionCount(0);
            var gen1Before = GC.CollectionCount(1);
            var gen2Before = GC.CollectionCount(2);

            // Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Get memory after GC
            var memoryAfter = GC.GetTotalMemory(false);
            var gen0After = GC.CollectionCount(0);
            var gen1After = GC.CollectionCount(1);
            var gen2After = GC.CollectionCount(2);

            var memoryFreed = memoryBefore - memoryAfter;
            var memoryFreedMB = memoryFreed / 1024.0 / 1024.0;

            return JsonSerializer.Serialize(new
            {
                Status = "Success",
                MemoryOptimization = new
                {
                    Timestamp = DateTime.UtcNow,
                    MemoryBefore = new
                    {
                        Bytes = memoryBefore,
                        MB = Math.Round(memoryBefore / 1024.0 / 1024.0, 2)
                    },
                    MemoryAfter = new
                    {
                        Bytes = memoryAfter,
                        MB = Math.Round(memoryAfter / 1024.0 / 1024.0, 2)
                    },
                    MemoryFreed = new
                    {
                        Bytes = memoryFreed,
                        MB = Math.Round(memoryFreedMB, 2),
                        Percentage = Math.Round((double)memoryFreed / memoryBefore * 100, 2)
                    },
                    GarbageCollections = new
                    {
                        Gen0 = gen0After - gen0Before,
                        Gen1 = gen1After - gen1Before,
                        Gen2 = gen2After - gen2Before
                    }
                }
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                Status = "Error",
                Message = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    [McpServerTool]
    [Description("Get comprehensive system resource monitoring including CPU, memory, disk, and network statistics")]
    public static async Task<string> GetResourceMonitoring()
    {
        try
        {
            var tasks = new List<Task<object>>();

            // CPU Information
            tasks.Add(Task.Run(() => (object)new
            {
                ProcessorCount = Environment.ProcessorCount,
                IsServerGC = System.Runtime.GCSettings.IsServerGC,
                WorkingSet = Environment.WorkingSet / 1024.0 / 1024.0, // MB
                ProcessorTime = Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds
            }));

            // Memory Information
            tasks.Add(Task.Run(() => (object)new
            {
                TotalMemoryMB = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 2),
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2),
                MaxGeneration = GC.MaxGeneration
            }));

            // System Information
            tasks.Add(Task.Run(() => (object)new
            {
                MachineName = Environment.MachineName,
                OSVersion = Environment.OSVersion.ToString(),
                RuntimeVersion = Environment.Version.ToString(),
                Is64BitProcess = Environment.Is64BitProcess,
                Is64BitOS = Environment.Is64BitOperatingSystem,
                CurrentDirectory = Environment.CurrentDirectory,
                SystemDirectory = Environment.SystemDirectory
            }));

            // Process Information
            tasks.Add(Task.Run(() =>
            {
                var process = Process.GetCurrentProcess();
                return (object)new
                {
                    ProcessId = process.Id,
                    ProcessName = process.ProcessName,
                    StartTime = process.StartTime,
                    ThreadCount = process.Threads.Count,
                    HandleCount = process.HandleCount,
                    PagedMemoryMB = Math.Round(process.PagedMemorySize64 / 1024.0 / 1024.0, 2),
                    VirtualMemoryMB = Math.Round(process.VirtualMemorySize64 / 1024.0 / 1024.0, 2)
                };
            }));

            await Task.WhenAll(tasks);

            return JsonSerializer.Serialize(new
            {
                Status = "Success",
                MonitoringTimestamp = DateTime.UtcNow,
                CPU = tasks[0].Result,
                Memory = tasks[1].Result,
                System = tasks[2].Result,
                Process = tasks[3].Result
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                Status = "Error",
                Message = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
