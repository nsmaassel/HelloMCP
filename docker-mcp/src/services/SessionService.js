/**
 * Session Service - Manages MCP server sessions
 */

class SessionService {
  constructor() {
    this.sessions = new Map();
  }

  /**
   * Create a new session
   * @param {string} sessionId - Session ID
   * @param {string} accessToken - Optional access token
   * @returns {Object} Session info object
   */
  createSession(sessionId, accessToken = null) {
    const sessionInfo = {
      id: sessionId,
      accessToken,
      createdAt: new Date()
    };

    this.sessions.set(sessionId, sessionInfo);
    return sessionInfo;
  }

  /**
   * Get a session by ID
   * @param {string} sessionId - Session ID
   * @returns {Object|null} Session info or null if not found
   */
  getSession(sessionId) {
    return this.sessions.get(sessionId) || null;
  }

  /**
   * Close a session
   * @param {string} sessionId - Session ID
   * @returns {boolean} True if session was found and closed, false otherwise
   */
  closeSession(sessionId) {
    return this.sessions.delete(sessionId);
  }
}

module.exports = new SessionService();