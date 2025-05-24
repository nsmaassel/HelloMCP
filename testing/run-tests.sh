#!/bin/bash

# Run all MCP server tests
# This script runs tests for all MCP server implementations

set -e  # Exit immediately if a command exits with a non-zero status

# Print header
print_header() {
    echo "===================================================="
    echo "  $1"
    echo "===================================================="
}

# Run test for a specific MCP server implementation
run_test() {
    local dir=$1
    local name=$2
    
    print_header "Testing $name"
    
    if [ ! -d "$dir" ]; then
        echo "Directory $dir not found. Skipping tests for $name."
        return
    fi
    
    cd $dir
    echo "Running tests in $(pwd)..."
    
    if [ -f "package.json" ]; then
        # JavaScript implementation
        npm test
    elif [ -f "Cargo.toml" ]; then
        # Rust implementation
        cargo test
    elif [ -f "go.mod" ]; then
        # Go implementation
        go test ./...
    elif [ -f "pom.xml" ]; then
        # Java implementation
        mvn test
    elif ls *.csproj >/dev/null 2>&1; then
        # .NET implementation
        dotnet test
    elif [ -f "Dockerfile" ]; then
        # Docker implementation - build and run tests in container
        docker build -t mcp-server-test .
        docker run --rm mcp-server-test test
    else
        echo "Unknown project type in $dir. Skipping tests."
    fi
    
    cd - > /dev/null  # Return to the original directory
    echo ""
}

# Main execution
main() {
    # Get the repository root directory
    REPO_ROOT=$(git rev-parse --show-toplevel 2>/dev/null || echo ".")
    cd $REPO_ROOT
    
    print_header "MCP Server Tests"
    echo "Repository: $REPO_ROOT"
    echo "Started at: $(date)"
    echo ""
    
    # Run tests for each implementation
    run_test "dotnet-mcp" ".NET SDK MCP Server"
    run_test "azure-functions-mcp" "Azure Functions MCP Server"
    run_test "docker-mcp" "Docker MCP Server"
    
    print_header "Test Summary"
    echo "Completed at: $(date)"
    
    # Return to the original directory
    cd - > /dev/null
}

# Run the main function
main "$@"