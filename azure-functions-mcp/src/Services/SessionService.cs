using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace AzureFunctionsMcp.Services;

public class SessionService : ISessionService
{
    private readonly ConcurrentDictionary<string, SessionInfo> _sessions = new();
    private readonly ILogger<SessionService>? _logger;

    public SessionService(ILogger<SessionService>? logger = null)
    {
        _logger = logger;
    }

    public string CreateSession(string? accessToken = null)
    {
        var sessionId = Guid.NewGuid().ToString();
        var sessionInfo = new SessionInfo
        {
            Id = sessionId,
            CreatedAt = DateTimeOffset.UtcNow,
            AccessToken = accessToken
        };

        _sessions.TryAdd(sessionId, sessionInfo);
        _logger?.LogInformation("Created session {SessionId}", sessionId);
        return sessionId;
    }

    public bool ValidateSession(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            // Check if session is expired (30 minutes)
            if (DateTimeOffset.UtcNow - session.CreatedAt > TimeSpan.FromMinutes(30))
            {
                _logger?.LogInformation("Session {SessionId} has expired", sessionId);
                _sessions.TryRemove(sessionId, out _);
                return false;
            }
            
            // Update last accessed time
            session.LastAccessedAt = DateTimeOffset.UtcNow;
            return true;
        }
        
        return false;
    }

    public bool CloseSession(string sessionId)
    {
        var result = _sessions.TryRemove(sessionId, out _);
        if (result)
        {
            _logger?.LogInformation("Closed session {SessionId}", sessionId);
        }
        return result;
    }
}

public class SessionInfo
{
    public string Id { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastAccessedAt { get; set; }
    public string? AccessToken { get; set; }
}