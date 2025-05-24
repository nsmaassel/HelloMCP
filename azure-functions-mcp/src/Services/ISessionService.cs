namespace AzureFunctionsMcp.Services;

public interface ISessionService
{
    string CreateSession(string? accessToken = null);
    bool ValidateSession(string sessionId);
    bool CloseSession(string sessionId);
}