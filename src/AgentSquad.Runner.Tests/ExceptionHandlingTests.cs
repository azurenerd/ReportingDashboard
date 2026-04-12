#nullable enable

using AgentSquad.Runner.Exceptions;
using Xunit;

namespace AgentSquad.Runner.Tests;

public class ExceptionHandlingTests
{
    [Fact]
    public void DashboardExceptionCanBeInstantiatedWithMessage()
    {
        var message = "Test error message";
        var exception = new DashboardException(message);

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void DashboardExceptionCanBeInstantiatedWithInnerException()
    {
        var innerException = new ArgumentNullException("test");
        var message = "Test error with inner exception";
        var exception = new DashboardException(message, innerException);

        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void InvalidDataExceptionInheritsFromDashboardException()
    {
        var exception = new InvalidDataException("Test invalid data");
        Assert.IsAssignableFrom<DashboardException>(exception);
    }

    [Fact]
    public void InvalidDataExceptionPreservesMessage()
    {
        var message = "Invalid JSON format";
        var exception = new InvalidDataException(message);

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void InvalidDataExceptionCanHaveInnerException()
    {
        var inner = new JsonException("Parse error");
        var exception = new InvalidDataException("Failed to parse", inner);

        Assert.NotNull(exception.InnerException);
        Assert.IsType<JsonException>(exception.InnerException);
    }
}