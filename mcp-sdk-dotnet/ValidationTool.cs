using System.Diagnostics;
using System.Text.Json;
using McpSdkServer.Tools;

namespace McpSdkServer;

/// <summary>
/// Simple validation tool to verify the MCP SDK server functionality
/// </summary>
public static class ValidationTool
{
    public static async Task<int> ValidateServerAsync()
    {
        Console.WriteLine("🔍 MCP SDK Server Validation Tool");
        Console.WriteLine("=====================================");
        
        var testCount = 0;
        var passedCount = 0;

        // Test 1: System Info
        Console.WriteLine("\n📊 Testing SystemTools.GetSystemInfo...");
        testCount++;
        try
        {
            var systemInfo = await SystemTools.GetSystemInfo();
            if (!string.IsNullOrEmpty(systemInfo))
            {
                var json = JsonDocument.Parse(systemInfo);
                if (json.RootElement.TryGetProperty("System", out _))
                {
                    Console.WriteLine("✅ SystemTools.GetSystemInfo - PASSED");
                    passedCount++;
                }
                else
                {
                    Console.WriteLine("❌ SystemTools.GetSystemInfo - FAILED");
                }
            }
            else
            {
                Console.WriteLine("❌ SystemTools.GetSystemInfo - FAILED (empty result)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ SystemTools.GetSystemInfo - FAILED ({ex.Message})");
        }        // Test 2: Echo
        Console.WriteLine("\n📢 Testing TextCompletionTools.Echo...");
        testCount++;
        try
        {
            var testMessage = "Validation Test Message";
            var echoResult = TextCompletionTools.Echo(testMessage);
            if (!string.IsNullOrEmpty(echoResult) && echoResult.Contains(testMessage))
            {
                Console.WriteLine("✅ TextCompletionTools.Echo - PASSED");
                passedCount++;
            }
            else
            {
                Console.WriteLine("❌ TextCompletionTools.Echo - FAILED");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ TextCompletionTools.Echo - FAILED ({ex.Message})");
        }        // Test 3: Server Info
        Console.WriteLine("\n🖥️ Testing TextCompletionTools.GetServerInfo...");
        testCount++;
        try
        {
            var serverInfo = TextCompletionTools.GetServerInfo();
            if (serverInfo != null)
            {
                var json = JsonSerializer.Serialize(serverInfo);
                var jsonDoc = JsonDocument.Parse(json);
                if (jsonDoc.RootElement.TryGetProperty("Name", out _) && 
                    jsonDoc.RootElement.TryGetProperty("Version", out _))
                {
                    Console.WriteLine("✅ TextCompletionTools.GetServerInfo - PASSED");
                    passedCount++;
                }
                else
                {
                    Console.WriteLine("❌ TextCompletionTools.GetServerInfo - FAILED (missing properties)");
                }
            }
            else
            {
                Console.WriteLine("❌ TextCompletionTools.GetServerInfo - FAILED (null result)");
            }
        }        catch (Exception ex)
        {
            Console.WriteLine($"❌ TextCompletionTools.GetServerInfo - FAILED ({ex.Message})");
        }

        // Test 4: Monitoring Tools (new)
        Console.WriteLine("\n📊 Testing MonitoringTools.GetPerformanceMetrics...");
        testCount++;
        try
        {
            // Note: This will show metrics service not available since we're in validation mode
            var performanceMetrics = MonitoringTools.GetPerformanceMetrics();
            if (!string.IsNullOrEmpty(performanceMetrics))
            {
                var json = JsonDocument.Parse(performanceMetrics);
                if (json.RootElement.TryGetProperty("Status", out _))
                {
                    Console.WriteLine("✅ MonitoringTools.GetPerformanceMetrics - PASSED");
                    passedCount++;
                }
                else
                {
                    Console.WriteLine("❌ MonitoringTools.GetPerformanceMetrics - FAILED (no status)");
                }
            }
            else
            {
                Console.WriteLine("❌ MonitoringTools.GetPerformanceMetrics - FAILED (empty result)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ MonitoringTools.GetPerformanceMetrics - FAILED ({ex.Message})");
        }

        // Summary
        Console.WriteLine("\n📈 Validation Summary");
        Console.WriteLine("====================");
        Console.WriteLine($"Tests Run: {testCount}");
        Console.WriteLine($"Tests Passed: {passedCount}");
        Console.WriteLine($"Tests Failed: {testCount - passedCount}");
        Console.WriteLine($"Success Rate: {(passedCount / (double)testCount * 100):F1}%");
        
        if (passedCount == testCount)
        {
            Console.WriteLine("\n🎉 All tests PASSED! The MCP SDK Server is working correctly.");
            return 0;
        }
        else
        {
            Console.WriteLine("\n❌ Some tests FAILED. Please check the implementation.");
            return 1;
        }
    }
}
