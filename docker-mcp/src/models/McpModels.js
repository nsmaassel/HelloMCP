/**
 * MCP Models - Defines request and response models for the Model Context Protocol
 */
const { v4: uuidv4 } = require('uuid');

/**
 * Base class for all MCP requests
 */
class McpRequest {
  constructor(type) {
    this.id = uuidv4();
    this.version = '0.1';
    this.type = type;
  }
}

/**
 * Base class for all MCP responses
 */
class McpResponse {
  constructor(requestId, type) {
    this.id = requestId;
    this.type = type;
  }
}

/**
 * Session creation request
 */
class SessionCreateRequest extends McpRequest {
  constructor(attributes = {}) {
    super('session-create-request');
    this.attributes = attributes;
  }
}

/**
 * Session creation attributes
 */
class SessionCreateAttributes {
  constructor(accessToken = null) {
    this.access_token = accessToken;
  }
}

/**
 * Session creation response
 */
class SessionCreateResponse extends McpResponse {
  constructor(requestId, sessionId) {
    super(requestId, 'session-create-response');
    this.session_id = sessionId;
  }
}

/**
 * Text completion request
 */
class TextCompletionRequest extends McpRequest {
  constructor(sessionId, inputs = {}) {
    super('text-completion-request');
    this.session_id = sessionId;
    this.inputs = inputs;
  }
}

/**
 * Text completion inputs
 */
class TextCompletionInputs {
  constructor(prompt = '', temperature = 0.7, maxTokens = 1000) {
    this.prompt = prompt;
    this.temperature = temperature;
    this.max_tokens = maxTokens;
  }
}

/**
 * Text completion stream response
 */
class TextCompletionStreamResponse extends McpResponse {
  constructor(requestId, delta = {}, usage = null) {
    super(requestId, 'text-completion-response');
    this.delta = delta;
    this.usage = usage;
  }
}

/**
 * Text completion delta
 */
class TextCompletionDelta {
  constructor(text = '', finishReason = null) {
    this.text = text;
    this.finish_reason = finishReason;
  }
}

/**
 * Text completion usage
 */
class TextCompletionUsage {
  constructor(promptTokens = 0, completionTokens = 0, totalTokens = 0) {
    this.prompt_tokens = promptTokens;
    this.completion_tokens = completionTokens;
    this.total_tokens = totalTokens;
  }
}

/**
 * Session close request
 */
class SessionCloseRequest extends McpRequest {
  constructor(sessionId) {
    super('session-close-request');
    this.session_id = sessionId;
  }
}

/**
 * Session close response
 */
class SessionCloseResponse extends McpResponse {
  constructor(requestId) {
    super(requestId, 'session-close-response');
  }
}

/**
 * Error response
 */
class ErrorResponse extends McpResponse {
  constructor(requestId, code, message) {
    super(requestId, 'error');
    this.error = {
      code,
      message
    };
  }
}

module.exports = {
  McpRequest,
  McpResponse,
  SessionCreateRequest,
  SessionCreateAttributes,
  SessionCreateResponse,
  TextCompletionRequest,
  TextCompletionInputs,
  TextCompletionStreamResponse,
  TextCompletionDelta,
  TextCompletionUsage,
  SessionCloseRequest,
  SessionCloseResponse,
  ErrorResponse
};