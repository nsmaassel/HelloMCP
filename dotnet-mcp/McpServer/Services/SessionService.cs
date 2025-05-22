using System.Collections.Concurrent;

namespace McpServer.Services;

public class SessionService
{
    private readonly ConcurrentDictionary<string, SessionInfo> _sessions = new();
    
    public bool TryCreateSession(string sessionId, string? accessToken, out SessionInfo sessionInfo)
    {
        sessionInfo = new SessionInfo
        {
            Id = sessionId,
            AccessToken = accessToken,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        return _sessions.TryAdd(sessionId, sessionInfo);
    }
    
    public bool TryGetSession(string sessionId, out SessionInfo sessionInfo)
    {
        return _sessions.TryGetValue(sessionId, out sessionInfo!);
    }
    
    public bool TryCloseSession(string sessionId)
    {
        return _sessions.TryRemove(sessionId, out _);
    }
}

public class SessionInfo
{
    public string Id { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}