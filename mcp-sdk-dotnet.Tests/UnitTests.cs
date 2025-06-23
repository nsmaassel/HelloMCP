using Xunit;

namespace McpSdkServer.Tests;

public class BasicTests
{
    [Fact]
    public void Sample_Test_Always_Passes()
    {
        // This is a basic test to ensure the test project works
        Assert.True(true);
    }

    [Fact]
    public void String_Concatenation_Works()
    {
        // Arrange
        var str1 = "Hello";
        var str2 = "World";

        // Act
        var result = str1 + " " + str2;

        // Assert
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void DateTime_Now_Is_Valid()
    {
        // Arrange & Act
        var now = DateTime.Now;

        // Assert
        Assert.True(now > DateTime.MinValue);
        Assert.True(now < DateTime.MaxValue);
    }
}