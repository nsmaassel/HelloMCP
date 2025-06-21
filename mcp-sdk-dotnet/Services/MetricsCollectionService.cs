using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime;

namespace McpSdkServer.Services;

/// <summary>
/// Service for collecting and monitoring performance metrics
/// </summary>
public class MetricsCollectionService
{
    private readonly ILogger<MetricsCollectionService> _logger;
    private readonly Timer _metricsTimer;
    private readonly object _lock = new();
    
    // Metrics storage
    private long _requestCount;
    private long _errorCount;
    private readonly List<double> _requestDurations = new();
    private DateTime _lastResetTime = DateTime.UtcNow;

    public MetricsCollectionService(ILogger<MetricsCollectionService> logger)
    {
        _logger = logger;
        
        // Collect metrics every 30 seconds
        _metricsTimer = new Timer(CollectAndLogMetrics, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// Record a successful request with its duration
    /// </summary>
    public void RecordRequest(TimeSpan duration)
    {
        lock (_lock)
        {
            _requestCount++;
            _requestDurations.Add(duration.TotalMilliseconds);
            
            // Keep only last 1000 durations to prevent memory issues
            if (_requestDurations.Count > 1000)
            {
                _requestDurations.RemoveRange(0, _requestDurations.Count - 1000);
            }
        }
    }

    /// <summary>
    /// Record an error
    /// </summary>
    public void RecordError()
    {
        lock (_lock)
        {
            _errorCount++;
        }
    }

    /// <summary>
    /// Get current metrics snapshot
    /// </summary>
    public MetricsSnapshot GetMetrics()
    {
        lock (_lock)
        {
            var uptime = DateTime.UtcNow - _lastResetTime;
            var process = Process.GetCurrentProcess();
            
            // Calculate request duration statistics
            var avgDuration = _requestDurations.Count > 0 ? _requestDurations.Average() : 0;
            var p95Duration = CalculatePercentile(_requestDurations, 0.95);
            var p99Duration = CalculatePercentile(_requestDurations, 0.99);

            return new MetricsSnapshot
            {
                Timestamp = DateTime.UtcNow,
                UptimeSeconds = uptime.TotalSeconds,
                RequestCount = _requestCount,
                ErrorCount = _errorCount,
                ErrorRate = _requestCount > 0 ? (double)_errorCount / _requestCount : 0,
                RequestsPerSecond = _requestCount / uptime.TotalSeconds,
                AverageRequestDurationMs = avgDuration,
                P95RequestDurationMs = p95Duration,
                P99RequestDurationMs = p99Duration,
                MemoryUsageMB = process.WorkingSet64 / 1024.0 / 1024.0,
                CpuTimeMs = process.TotalProcessorTime.TotalMilliseconds,
                ThreadCount = process.Threads.Count,
                GcGen0Collections = GC.CollectionCount(0),
                GcGen1Collections = GC.CollectionCount(1),
                GcGen2Collections = GC.CollectionCount(2),
                AllocatedMemoryMB = GC.GetTotalMemory(false) / 1024.0 / 1024.0
            };
        }
    }

    /// <summary>
    /// Reset metrics counters
    /// </summary>
    public void ResetMetrics()
    {
        lock (_lock)
        {
            _requestCount = 0;
            _errorCount = 0;
            _requestDurations.Clear();
            _lastResetTime = DateTime.UtcNow;
            _logger.LogInformation("📊 Metrics counters have been reset");
        }
    }

    private void CollectAndLogMetrics(object? state)
    {
        try
        {
            var metrics = GetMetrics();
            
            _logger.LogInformation("📊 Performance Metrics:");
            _logger.LogInformation("  Uptime: {Uptime:F1}s", metrics.UptimeSeconds);
            _logger.LogInformation("  Requests: {RequestCount} ({RequestsPerSecond:F2}/sec)", 
                metrics.RequestCount, metrics.RequestsPerSecond);
            _logger.LogInformation("  Errors: {ErrorCount} ({ErrorRate:P2})", 
                metrics.ErrorCount, metrics.ErrorRate);
            _logger.LogInformation("  Avg Response: {AvgDuration:F1}ms (P95: {P95Duration:F1}ms, P99: {P99Duration:F1}ms)", 
                metrics.AverageRequestDurationMs, metrics.P95RequestDurationMs, metrics.P99RequestDurationMs);
            _logger.LogInformation("  Memory: {MemoryUsage:F1}MB (Allocated: {AllocatedMemory:F1}MB)", 
                metrics.MemoryUsageMB, metrics.AllocatedMemoryMB);
            _logger.LogInformation("  GC: Gen0={Gen0}, Gen1={Gen1}, Gen2={Gen2}", 
                metrics.GcGen0Collections, metrics.GcGen1Collections, metrics.GcGen2Collections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error collecting metrics");
        }
    }

    private double CalculatePercentile(List<double> values, double percentile)
    {
        if (values.Count == 0) return 0;
        
        var sorted = values.OrderBy(x => x).ToList();
        var index = (int)Math.Ceiling(sorted.Count * percentile) - 1;
        return sorted[Math.Max(0, Math.Min(index, sorted.Count - 1))];
    }

    public void Dispose()
    {
        _metricsTimer?.Dispose();
    }
}

/// <summary>
/// Snapshot of current performance metrics
/// </summary>
public class MetricsSnapshot
{
    public DateTime Timestamp { get; set; }
    public double UptimeSeconds { get; set; }
    public long RequestCount { get; set; }
    public long ErrorCount { get; set; }
    public double ErrorRate { get; set; }
    public double RequestsPerSecond { get; set; }
    public double AverageRequestDurationMs { get; set; }
    public double P95RequestDurationMs { get; set; }
    public double P99RequestDurationMs { get; set; }
    public double MemoryUsageMB { get; set; }
    public double CpuTimeMs { get; set; }
    public int ThreadCount { get; set; }
    public int GcGen0Collections { get; set; }
    public int GcGen1Collections { get; set; }
    public int GcGen2Collections { get; set; }
    public double AllocatedMemoryMB { get; set; }
}
