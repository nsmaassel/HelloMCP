/**
 * MCP Controller - Handles MCP protocol endpoints
 */
const { v4: uuidv4 } = require('uuid');
const sessionService = require('../services/SessionService');
const logger = require('../services/LoggerService');
const { 
  SessionCreateResponse, 
  TextCompletionStreamResponse, 
  TextCompletionDelta,
  TextCompletionUsage,
  SessionCloseResponse,
  ErrorResponse 
} = require('../models/McpModels');

/**
 * Create a new session
 */
function createSession(req, res) {
  try {
    const request = req.body;
    logger.info(`Session creation requested with ID: ${request.id}`);
    
    // Generate a unique session ID
    const sessionId = uuidv4();
    
    // Create the session
    const accessToken = request.attributes?.access_token;
    sessionService.createSession(sessionId, accessToken);
    
    // Create and send the response
    const response = new SessionCreateResponse(request.id, sessionId);
    
    logger.info(`Session created successfully: ${sessionId}`);
    return res.json(response);
  } catch (error) {
    logger.error(`Error creating session: ${error.message}`);
    const errorResponse = new ErrorResponse(
      req.body.id || uuidv4(),
      'internal_error',
      'An unexpected error occurred'
    );
    return res.status(500).json(errorResponse);
  }
}

/**
 * Text completion endpoint - Streamable HTTP
 */
function getTextCompletions(req, res) {
  try {
    const request = req.body;
    logger.info(`Text completion requested for session ${request.session_id}`);
    
    // Validate session
    const sessionInfo = sessionService.getSession(request.session_id);
    if (!sessionInfo) {
      logger.warn(`Invalid session ID: ${request.session_id}`);
      const errorResponse = new ErrorResponse(
        request.id,
        'invalid_session',
        'Invalid or expired session ID'
      );
      return res.status(400).json(errorResponse);
    }
    
    // Set up response for streaming
    res.setHeader('Content-Type', 'application/x-ndjson');
    
    // Generate demo responses
    const responses = generateDemoResponses(request);
    
    // Stream responses with small delays to simulate generation
    let index = 0;
    const interval = setInterval(() => {
      if (index < responses.length) {
        const json = JSON.stringify(responses[index]) + '\n';
        res.write(json);
        index++;
      } else {
        clearInterval(interval);
        res.end();
      }
    }, 100);
    
    // Handle client disconnect
    req.on('close', () => {
      clearInterval(interval);
      logger.info(`Client disconnected from text completion stream for session ${request.session_id}`);
    });
  } catch (error) {
    logger.error(`Error processing text completion: ${error.message}`);
    const errorResponse = new ErrorResponse(
      req.body.id || uuidv4(),
      'internal_error',
      'An unexpected error occurred'
    );
    return res.status(500).json(errorResponse);
  }
}

/**
 * Legacy HTTP+SSE streaming endpoint
 */
function streamTextCompletions(req, res) {
  try {
    const request = req.body;
    logger.info(`SSE text completion requested for session ${request.session_id}`);
    
    // Validate session
    const sessionInfo = sessionService.getSession(request.session_id);
    if (!sessionInfo) {
      logger.warn(`Invalid session ID: ${request.session_id}`);
      const errorResponse = new ErrorResponse(
        request.id,
        'invalid_session',
        'Invalid or expired session ID'
      );
      return res.status(400).json(errorResponse);
    }
    
    // Set up SSE headers
    res.setHeader('Content-Type', 'text/event-stream');
    res.setHeader('Cache-Control', 'no-cache');
    res.setHeader('Connection', 'keep-alive');
    
    // Generate demo responses
    const responses = generateDemoResponses(request);
    
    // Stream responses with small delays to simulate generation
    let index = 0;
    const interval = setInterval(() => {
      if (index < responses.length) {
        const json = JSON.stringify(responses[index]);
        res.write(`data: ${json}\n\n`);
        index++;
      } else {
        res.write('data: [DONE]\n\n');
        clearInterval(interval);
        res.end();
      }
    }, 100);
    
    // Handle client disconnect
    req.on('close', () => {
      clearInterval(interval);
      logger.info(`Client disconnected from SSE stream for session ${request.session_id}`);
    });
  } catch (error) {
    logger.error(`Error processing SSE text completion: ${error.message}`);
    const errorResponse = new ErrorResponse(
      req.body.id || uuidv4(),
      'internal_error',
      'An unexpected error occurred'
    );
    return res.status(500).json(errorResponse);
  }
}

/**
 * Close session endpoint
 */
function closeSession(req, res) {
  try {
    const sessionId = req.params.sessionId;
    logger.info(`Session close requested: ${sessionId}`);
    
    // Try to close the session
    if (sessionService.closeSession(sessionId)) {
      const response = new SessionCloseResponse(uuidv4());
      
      logger.info(`Session closed successfully: ${sessionId}`);
      return res.json(response);
    } else {
      logger.warn(`Failed to close nonexistent session: ${sessionId}`);
      const errorResponse = new ErrorResponse(
        uuidv4(),
        'invalid_session',
        'Session not found'
      );
      return res.status(404).json(errorResponse);
    }
  } catch (error) {
    logger.error(`Error closing session: ${error.message}`);
    const errorResponse = new ErrorResponse(
      uuidv4(),
      'internal_error',
      'An unexpected error occurred'
    );
    return res.status(500).json(errorResponse);
  }
}

/**
 * Generate demo responses for text completion
 */
function generateDemoResponses(request) {
  // Generate simulated responses for demonstration purposes
  const lorem = "This is a demonstration of the Model Context Protocol server implemented in Node.js. " +
               "The server supports both modern Streamable HTTP and legacy HTTP+SSE transport options. " +
               "In a real implementation, this would connect to an AI model to generate responses based on the provided prompt.";
  
  const words = lorem.split(' ');
  const responses = [];
  
  // Initial chunks with content
  for (let i = 0; i < words.length; i++) {
    responses.push(new TextCompletionStreamResponse(
      request.id,
      new TextCompletionDelta(words[i] + ' ')
    ));
  }
  
  // Final chunk with finish reason and usage
  responses.push(new TextCompletionStreamResponse(
    request.id,
    new TextCompletionDelta('', 'stop'),
    new TextCompletionUsage(
      Math.ceil(request.inputs.prompt.length / 4), // Very rough approximation
      Math.ceil(lorem.length / 4),
      Math.ceil((request.inputs.prompt.length + lorem.length) / 4)
    )
  ));
  
  return responses;
}

module.exports = {
  createSession,
  getTextCompletions,
  streamTextCompletions,
  closeSession
};