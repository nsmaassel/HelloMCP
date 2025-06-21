using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace McpSdkServer.Tools;

/// <summary>
/// Web scraping and HTTP utilities for MCP server
/// </summary>
[McpServerToolType]
public static class WebTools
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool]
    [Description("Fetches content from a URL and analyzes it")]
    public static async Task<string> FetchAndAnalyze(
        HttpClient httpClient,
        [Description("The URL to fetch")] string url,
        [Description("Analysis type: 'content', 'headers', 'status', 'full'")] string analysisType = "content",
        [Description("Maximum content length to analyze")] int maxContentLength = 10000,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync(url, cancellationToken);
            
            var analysis = new
            {
                Url = url,
                Timestamp = DateTime.UtcNow,
                Status = new
                {
                    Code = (int)response.StatusCode,
                    Reason = response.ReasonPhrase,
                    IsSuccess = response.IsSuccessStatusCode
                },
                Headers = analysisType is "headers" or "full" ? 
                    response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value)) : null,
                Content = analysisType is "content" or "full" && response.IsSuccessStatusCode ? 
                    await AnalyzeContent(response, maxContentLength, cancellationToken) : null
            };

            return JsonSerializer.Serialize(analysis, JsonOptions);
        }
        catch (HttpRequestException ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                Error = "HTTP request failed", 
                Message = ex.Message, 
                Url = url,
                Timestamp = DateTime.UtcNow
            }, JsonOptions);
        }
    }

    [McpServerTool]
    [Description("Performs health checks on multiple URLs")]
    public static async Task<string> HealthCheck(
        HttpClient httpClient,
        [Description("Comma-separated list of URLs to check")] string urls,
        [Description("Timeout in seconds for each request")] int timeoutSeconds = 10,
        CancellationToken cancellationToken = default)
    {
        var urlList = urls.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(u => u.Trim())
                         .ToArray();

        var results = new List<object>();
        
        httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

        foreach (var url in urlList)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                using var response = await httpClient.GetAsync(url, cancellationToken);
                var endTime = DateTime.UtcNow;
                
                results.Add(new
                {
                    Url = url,
                    Status = "Success",
                    StatusCode = (int)response.StatusCode,
                    ResponseTime = (endTime - startTime).TotalMilliseconds,
                    IsHealthy = response.IsSuccessStatusCode,
                    Timestamp = startTime
                });
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                results.Add(new
                {
                    Url = url,
                    Status = "Failed",
                    Error = ex.Message,
                    ResponseTime = (endTime - startTime).TotalMilliseconds,
                    IsHealthy = false,
                    Timestamp = startTime
                });
            }
        }

        var summary = new
        {
            TotalUrls = urlList.Length,
            HealthyUrls = results.Count(r => ((dynamic)r).IsHealthy),
            UnhealthyUrls = results.Count(r => !((dynamic)r).IsHealthy),
            AverageResponseTime = results.Where(r => ((dynamic)r).Status == "Success")
                                         .Average(r => (double)((dynamic)r).ResponseTime),
            CheckTimestamp = DateTime.UtcNow,
            Results = results
        };

        return JsonSerializer.Serialize(summary, JsonOptions);
    }

    [McpServerTool]
    [Description("Downloads content from a URL and saves analysis results")]
    public static async Task<string> DownloadAndSave(
        HttpClient httpClient,
        [Description("The URL to download from")] string url,
        [Description("Analysis to perform: 'metadata', 'content', 'both'")] string analysisType = "both",
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return JsonSerializer.Serialize(new 
                { 
                    Error = "Download failed", 
                    StatusCode = (int)response.StatusCode,
                    Url = url,
                    Timestamp = DateTime.UtcNow
                }, JsonOptions);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = new
            {
                Url = url,
                DownloadTimestamp = DateTime.UtcNow,
                Metadata = analysisType is "metadata" or "both" ? new
                {
                    ContentType = response.Content.Headers.ContentType?.ToString(),
                    ContentLength = response.Content.Headers.ContentLength,
                    LastModified = response.Content.Headers.LastModified?.ToString(),
                    ETag = response.Headers.ETag?.ToString(),
                    Server = response.Headers.Server?.ToString()
                } : null,
                ContentAnalysis = analysisType is "content" or "both" ? new
                {
                    Length = content.Length,
                    LineCount = content.Split('\n').Length,
                    WordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                    CharacterEncoding = Encoding.UTF8.WebName,
                    Preview = content.Length > 200 ? content[..200] + "..." : content
                } : null,
                Status = "Success"
            };

            return JsonSerializer.Serialize(result, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                Error = "Download failed", 
                Message = ex.Message, 
                Url = url,
                Timestamp = DateTime.UtcNow
            }, JsonOptions);
        }
    }

    [McpServerTool]
    [Description("Tests network connectivity and DNS resolution")]
    public static async Task<string> NetworkDiagnostics(
        HttpClient httpClient,
        [Description("Host to test (domain name or IP)")] string host,
        [Description("Port to test (default: 80 for HTTP, 443 for HTTPS)")] int? port = null,
        CancellationToken cancellationToken = default)
    {
        var results = new
        {
            Host = host,
            Port = port,
            TestTimestamp = DateTime.UtcNow,
            DnsResolution = await TestDnsResolution(host),
            HttpConnectivity = await TestHttpConnectivity(httpClient, host, port, cancellationToken),
            NetworkInfo = new
            {
                LocalMachineName = Environment.MachineName,
                OSDescription = System.Runtime.InteropServices.RuntimeInformation.OSDescription
            }
        };

        return JsonSerializer.Serialize(results, JsonOptions);
    }

    private static async Task<object> AnalyzeContent(HttpResponseMessage response, int maxLength, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (content.Length > maxLength)
        {
            content = content[..maxLength];
        }

        return new
        {
            Length = content.Length,
            Truncated = content.Length == maxLength,
            LineCount = content.Split('\n').Length,
            WordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
            ContentType = response.Content.Headers.ContentType?.ToString(),
            Preview = content.Length > 500 ? content[..500] + "..." : content
        };
    }

    private static async Task<object> TestDnsResolution(string host)
    {
        try
        {
            var addresses = await System.Net.Dns.GetHostAddressesAsync(host);
            return new
            {
                Success = true,
                ResolvedAddresses = addresses.Select(a => a.ToString()).ToArray(),
                AddressCount = addresses.Length
            };
        }
        catch (Exception ex)
        {
            return new
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    private static async Task<object> TestHttpConnectivity(HttpClient httpClient, string host, int? port, CancellationToken cancellationToken)
    {
        var testUrl = port.HasValue ? 
            $"http://{host}:{port}" : 
            (host.StartsWith("http") ? host : $"http://{host}");

        try
        {
            var startTime = DateTime.UtcNow;
            using var response = await httpClient.GetAsync(testUrl, cancellationToken);
            var endTime = DateTime.UtcNow;

            return new
            {
                Success = true,
                StatusCode = (int)response.StatusCode,
                ResponseTime = (endTime - startTime).TotalMilliseconds,
                IsSuccessStatusCode = response.IsSuccessStatusCode
            };
        }
        catch (Exception ex)
        {
            return new
            {
                Success = false,
                Error = ex.Message,
                TestUrl = testUrl
            };
        }
    }
}
