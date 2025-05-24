using System.Collections;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker.Http;

namespace AzureFunctionsMcp.Tests;

// Mock implementation of HttpHeadersCollection for testing
public class MockHttpHeadersCollection : IEnumerable<KeyValuePair<string, IEnumerable<string>>>
{
    private readonly Dictionary<string, List<string>> _headers = new();

    public void Add(string key, string value)
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

    public IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator()
    {
        foreach (var header in _headers)
        {
            yield return new KeyValuePair<string, IEnumerable<string>>(header.Key, header.Value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}