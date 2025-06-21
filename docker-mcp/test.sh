#!/bin/bash

# Test script for Docker MCP Server
# Validates that the container is working correctly

set -e

echo "🧪 Testing Docker MCP Server..."

# Configuration
CONTAINER_NAME="hellomcp-docker-test"
PORT="5090"
BASE_URL="http://localhost:$PORT"

# Cleanup function
cleanup() {
    echo "🧹 Cleaning up test container..."
    docker stop "$CONTAINER_NAME" 2>/dev/null || true
    docker rm "$CONTAINER_NAME" 2>/dev/null || true
}

# Cleanup on exit
trap cleanup EXIT

# Start container
echo "🚀 Starting test container..."
docker run -d --name "$CONTAINER_NAME" -p "$PORT:80" hellomcp-docker:latest

# Wait for container to be ready
echo "⏳ Waiting for container to be ready..."
sleep 10

# Function to test endpoint
test_endpoint() {
    local endpoint="$1"
    local method="$2"
    local data="$3"
    local description="$4"
    
    echo "🔍 Testing $description..."
    
    if [ -n "$data" ]; then
        response=$(curl -s -w "\n%{http_code}" -X "$method" \
            -H "Content-Type: application/json" \
            -d "$data" \
            "$BASE_URL$endpoint")
    else
        response=$(curl -s -w "\n%{http_code}" -X "$method" "$BASE_URL$endpoint")
    fi
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | head -n -1)
    
    if [ "$http_code" -eq 200 ]; then
        echo "✅ $description - HTTP $http_code"
        return 0
    else
        echo "❌ $description - HTTP $http_code"
        echo "   Response: $body"
        return 1
    fi
}

# Test basic endpoints
echo ""
echo "🧪 Running MCP protocol tests..."

# Test 1: Health check
test_endpoint "/health" "GET" "" "Health check"

# Test 2: MCP Initialize
test_endpoint "/v1/initialize" "POST" '{}' "MCP initialize"

# Test 3: OAuth discovery
test_endpoint "/.well-known/oauth-authorization-server" "GET" "" "OAuth discovery"

# Test 4: Session creation
session_data='{"id": "docker-test", "attributes": {"access_token": "test"}}'
if test_endpoint "/v1/session" "POST" "$session_data" "Session creation"; then
    # Extract session ID for further tests
    session_id=$(echo "$body" | grep -o '"session_id":"[^"]*"' | cut -d'"' -f4)
    
    if [ -n "$session_id" ]; then
        echo "🔑 Session ID: $session_id"
        
        # Test 5: Text completion
        completion_data="{\"id\": \"docker-completion-test\", \"session_id\": \"$session_id\", \"inputs\": {\"prompt\": \"Hello Docker\"}}"
        test_endpoint "/v1/text/completions" "POST" "$completion_data" "Text completion"
        
        # Test 6: Close session
        test_endpoint "/v1/session/$session_id" "DELETE" "" "Session closure"
    fi
fi

echo ""
echo "🎉 Docker MCP Server test completed!"
echo "📊 Container logs:"
docker logs "$CONTAINER_NAME" --tail 10
