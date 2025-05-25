# MCP Server Testing Strategy

This document outlines the testing strategy for Model Context Protocol (MCP) servers in the HelloMCP project.

## Test Categories

### 1. Unit Tests
These tests verify individual components in isolation:
- Function handlers
- Services (e.g., SessionService)
- Model implementations

### 2. Integration Tests
These tests verify how components work together:
- Protocol compliance tests
- Request/response format validation
- Transport options (HTTP, SSE)
- Authentication flows

### 3. End-to-End Tests
These tests simulate real client-server interactions:
- Complete session lifecycle (create, use, close)
- Error handling and edge cases
- Content streaming

### 4. Health Monitoring
These tests ensure the server is functioning properly in production:
- Server availability checks
- Response time monitoring
- Error rate tracking

## Test Coverage by Server Type

### .NET SDK MCP Server
- Unit tests for controllers and services
- End-to-end tests for client-server interactions
- Health monitoring tests

### Azure Functions MCP Server
- Unit tests for functions and services
- Protocol compliance tests
- Authentication tests

**Current Limitations**: The Azure Functions MCP Server tests have some mock setup issues with extension methods like `HttpRequestDataExtensions.ReadFromJsonAsync` and `HttpResponseDataExtensions.WriteAsJsonAsync` which cannot be directly mocked using Moq. This is a known issue that will be addressed in a future update by:
1. Creating wrapper interfaces for these extension methods
2. Using custom test helpers instead of directly mocking extension methods
3. Using actual request/response objects where possible instead of mocks

### Docker MCP Server (Future)
- Integration tests for container functionality
- Load testing

## Running Tests

### .NET SDK MCP Tests
```bash
cd dotnet-mcp
dotnet test
```

### Azure Functions MCP Tests
```bash
cd azure-functions-mcp
dotnet test
```

## Test Conventions

1. **Test Naming**: `[MethodName]_[Scenario]_[ExpectedResult]`
   - Example: `CreateSession_ValidRequest_ReturnsSessionId`

2. **Test Structure**: Each test follows the Arrange-Act-Assert pattern
   - Arrange: Set up the test environment and inputs
   - Act: Execute the method under test
   - Assert: Verify the results

3. **Mocking**: Use Moq for mocking dependencies

### Mock Implementation Rationale

Mock implementations are used in specific scenarios for the following reasons:

1. **Isolation of Components**: Mocks allow testing of individual components without dependencies on external services or infrastructure.
   - Example: The `ISessionService` interface enables testing controllers/functions without requiring a real session backend.

2. **Testing Edge Cases**: Mocks allow simulation of error conditions and edge cases that would be difficult to reproduce with real implementations.
   - Example: Testing session validation failures or authentication errors.

3. **Faster Test Execution**: Tests with mocks run faster as they don't require external resources or services.

**Alternatives Considered**:
- **In-memory implementations**: For some components like `SessionService`, we could use in-memory implementations instead of mocks. However, mocks provide more control over behavior during tests.
- **Integration tests with real dependencies**: We use these where appropriate (see the EndToEndIntegrationTests), but they're not suitable for all test scenarios.
- **Test containers**: For future improvements, we could use Docker test containers for more realistic integration tests.

## Continuous Integration

Tests are automatically run as part of the CI/CD pipeline on GitHub Actions. The workflow is defined in `.github/workflows/run-tests.yml` and runs:
1. On pull requests to main branch
2. On pushes to main branch
3. Captures and uploads test results as artifacts

## Future Improvements

1. Add performance/load testing
2. Implement security scanning tests
3. Add more comprehensive edge case testing
4. Add cross-implementation compatibility tests