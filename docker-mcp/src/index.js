/**
 * MCP Server - Main application file
 */
const express = require('express');
const cors = require('cors');
const oauthController = require('./controllers/OAuthController');
const mcpController = require('./controllers/McpController');
const logger = require('./services/LoggerService');

// Initialize Express app
const app = express();
const PORT = process.env.PORT || 3000;

// Middleware
app.use(cors());
app.use(express.json());

// Request logging middleware
app.use((req, res, next) => {
  logger.info(`${req.method} ${req.url}`);
  next();
});

// OAuth discovery endpoint
app.get('/.well-known/oauth-authorization-server', oauthController.getOAuthServerMetadata);

// OAuth endpoints (placeholders)
app.get('/oauth/authorize', oauthController.authorize);
app.post('/oauth/token', oauthController.token);

// MCP endpoints
app.post('/v1/session', mcpController.createSession);
app.delete('/v1/session/:sessionId', mcpController.closeSession);
app.post('/v1/text/completions', mcpController.getTextCompletions);
app.post('/v1/text/completions/stream', mcpController.streamTextCompletions);

// Health check endpoint
app.get('/health', (req, res) => {
  res.json({ status: 'ok' });
});

// Root endpoint with basic info
app.get('/', (req, res) => {
  res.json({
    name: 'Docker MCP Server',
    version: '1.0.0',
    description: 'Model Context Protocol server implemented in Node.js',
    endpoints: {
      oauth_discovery: '/.well-known/oauth-authorization-server',
      session_create: '/v1/session',
      session_close: '/v1/session/:sessionId',
      text_completions: '/v1/text/completions',
      text_completions_sse: '/v1/text/completions/stream'
    }
  });
});

// Error handling middleware
app.use((err, req, res, next) => {
  logger.error(`Unhandled error: ${err.message}`);
  res.status(500).json({
    error: {
      code: 'internal_error',
      message: 'An unexpected error occurred'
    }
  });
});

// Start the server
app.listen(PORT, () => {
  logger.info(`MCP Server running on port ${PORT}`);
});