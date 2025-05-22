using System.Text.Json.Serialization;

namespace McpServer.Models;

// Base class for all MCP requests
public abstract class McpRequest
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("version")]
    public string Version { get; set; } = "0.1";
    
    [JsonPropertyName("type")]
    public abstract string Type { get; }
}

// Base class for all MCP responses
public abstract class McpResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public abstract string Type { get; }
}

// Session creation request
public class SessionCreateRequest : McpRequest
{
    [JsonPropertyName("type")]
    public override string Type => "session-create-request";
    
    [JsonPropertyName("attributes")]
    public SessionCreateAttributes Attributes { get; set; } = new();
}

public class SessionCreateAttributes
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
}

// Session creation response
public class SessionCreateResponse : McpResponse
{
    [JsonPropertyName("type")]
    public override string Type => "session-create-response";
    
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
}

// Text Completion request
public class TextCompletionRequest : McpRequest
{
    [JsonPropertyName("type")]
    public override string Type => "text-completion-request";
    
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;
    
    [JsonPropertyName("inputs")]
    public TextCompletionInputs Inputs { get; set; } = new();
}

public class TextCompletionInputs
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;
    
    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; } = 0.7;
    
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; } = 1000;
}

// Text Completion response
public class TextCompletionStreamResponse : McpResponse
{
    [JsonPropertyName("type")]
    public override string Type => "text-completion-stream-response";
    
    [JsonPropertyName("delta")]
    public TextCompletionDelta Delta { get; set; } = new();
    
    [JsonPropertyName("usage")]
    public TextCompletionUsage? Usage { get; set; }
}

public class TextCompletionDelta
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
    
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

public class TextCompletionUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }
    
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }
    
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

// Session close request
public class SessionCloseRequest : McpRequest
{
    [JsonPropertyName("type")]
    public override string Type => "session-close-request";
    
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;
}

// Session close response
public class SessionCloseResponse : McpResponse
{
    [JsonPropertyName("type")]
    public override string Type => "session-close-response";
}

// Error response
public class ErrorResponse : McpResponse
{
    [JsonPropertyName("type")]
    public override string Type => "error";
    
    [JsonPropertyName("error")]
    public McpError Error { get; set; } = new();
}

public class McpError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}