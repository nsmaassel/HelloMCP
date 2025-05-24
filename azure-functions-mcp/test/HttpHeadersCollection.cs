using System.Collections;
using Microsoft.Azure.Functions.Worker.Http;

namespace AzureFunctionsMcp.Tests;

// Interface for HTTP headers in tests
public abstract class HttpHeaders
{
    public abstract void Add(string key, string value);
    public abstract IEnumerable<(string Key, IEnumerable<string> Value)> GetHeaders();
}

// Simple implementation for HttpHeadersCollection
public class HttpHeadersCollection : HttpHeaders
{
    private readonly Dictionary<string, List<string>> _headers = new();

    public override void Add(string key, string value)
    {
        if (_headers.TryGetValue(key, out var values))
        {
            values.Add(value);
        }
        else
        {
            _headers[key] = new List<string> { value };
        }
    }

    public override IEnumerable<(string Key, IEnumerable<string> Value)> GetHeaders()
    {
        return _headers.Select(kvp => (kvp.Key, (IEnumerable<string>)kvp.Value));
    }
}