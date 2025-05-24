/**
 * OAuth Controller - Handles OAuth discovery and endpoints
 */
const logger = require('../services/LoggerService');

/**
 * Get OAuth server metadata
 */
function getOAuthServerMetadata(req, res) {
  const baseUrl = `${req.protocol}://${req.get('host')}`;
  
  logger.info('OAuth server metadata requested');
  
  const metadata = {
    issuer: baseUrl,
    authorization_endpoint: `${baseUrl}/oauth/authorize`,
    token_endpoint: `${baseUrl}/oauth/token`,
    token_endpoint_auth_methods_supported: ['client_secret_basic', 'client_secret_post'],
    revocation_endpoint: `${baseUrl}/oauth/revoke`,
    revocation_endpoint_auth_methods_supported: ['client_secret_basic', 'client_secret_post'],
    grant_types_supported: ['authorization_code', 'refresh_token', 'client_credentials'],
    response_types_supported: ['code'],
    scopes_supported: ['text.completions', 'session']
  };
  
  return res.json(metadata);
}

/**
 * OAuth authorization endpoint (placeholder)
 */
function authorize(req, res) {
  logger.info('OAuth authorization requested');
  
  return res.json({
    message: 'OAuth Authorization Endpoint'
  });
}

/**
 * OAuth token endpoint (placeholder)
 */
function token(req, res) {
  logger.info('OAuth token requested');
  
  const token = {
    access_token: 'demo_access_token',
    token_type: 'Bearer',
    expires_in: 3600,
    scope: 'text.completions session'
  };
  
  return res.json(token);
}

module.exports = {
  getOAuthServerMetadata,
  authorize,
  token
};