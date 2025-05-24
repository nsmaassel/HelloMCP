/**
 * Basic MCP Server tests
 */
const request = require('supertest');
const express = require('express');
const cors = require('cors');
const oauthController = require('../src/controllers/OAuthController');
const mcpController = require('../src/controllers/McpController');

// Create a test app
const app = express();
app.use(cors());
app.use(express.json());

// Register routes
app.get('/.well-known/oauth-authorization-server', oauthController.getOAuthServerMetadata);
app.post('/v1/session', mcpController.createSession);
app.delete('/v1/session/:sessionId', mcpController.closeSession);
app.post('/v1/text/completions', mcpController.getTextCompletions);
app.post('/v1/text/completions/stream', mcpController.streamTextCompletions);

describe('MCP Server API', () => {
  // OAuth tests
  describe('OAuth Endpoints', () => {
    test('GET /.well-known/oauth-authorization-server returns OAuth metadata', async () => {
      const response = await request(app).get('/.well-known/oauth-authorization-server');
      expect(response.status).toBe(200);
      expect(response.body).toHaveProperty('issuer');
      expect(response.body).toHaveProperty('authorization_endpoint');
      expect(response.body).toHaveProperty('token_endpoint');
    });
  });

  // Session management tests
  describe('Session Management', () => {
    test('POST /v1/session creates a new session', async () => {
      const response = await request(app)
        .post('/v1/session')
        .send({
          id: '123',
          version: '0.1',
          type: 'session-create-request',
          attributes: {}
        });
      
      expect(response.status).toBe(200);
      expect(response.body).toHaveProperty('id', '123');
      expect(response.body).toHaveProperty('type', 'session-create-response');
      expect(response.body).toHaveProperty('session_id');
    });

    test('DELETE /v1/session/:sessionId returns 404 for non-existent session', async () => {
      const response = await request(app)
        .delete('/v1/session/non-existent-session');
      
      expect(response.status).toBe(404);
      expect(response.body).toHaveProperty('error');
      expect(response.body.error).toHaveProperty('code', 'invalid_session');
    });
  });

  // Integration test for full flow
  describe('Session Flow', () => {
    test('Create session, use it for text completion, then close it', async () => {
      // Create session
      const sessionResponse = await request(app)
        .post('/v1/session')
        .send({
          id: '456',
          version: '0.1',
          type: 'session-create-request',
          attributes: {}
        });
      
      expect(sessionResponse.status).toBe(200);
      const sessionId = sessionResponse.body.session_id;
      
      // Request text completion (not checking full response since it's streamed)
      const completionResponse = await request(app)
        .post('/v1/text/completions')
        .send({
          id: '789',
          version: '0.1',
          type: 'text-completion-request',
          session_id: sessionId,
          inputs: {
            prompt: 'Hello world',
            temperature: 0.7,
            max_tokens: 100
          }
        });
      
      expect(completionResponse.status).toBe(200);
      
      // Close session
      const closeResponse = await request(app)
        .delete(`/v1/session/${sessionId}`);
      
      expect(closeResponse.status).toBe(200);
      expect(closeResponse.body).toHaveProperty('type', 'session-close-response');
    });
  });
});