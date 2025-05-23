using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace AzureFunctionsMcp.Tests;

public static class TestHelpers
{
    public static FunctionContext CreateFunctionContext()
    {
        // Create a mock service provider
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Create a mock function context
        var functionContextMock = new Mock<FunctionContext>();
        functionContextMock.Setup(c => c.InstanceServices).Returns(serviceProvider);
        
        return functionContextMock.Object;
    }
}