using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Tests.Models;

public class ValidationErrorTests
{
    [Fact]
    public void ValidationError_Constructor_WithoutFieldName()
    {
        var error = new ValidationError("TEST_CODE", "Test message");

        Assert.Equal("TEST_CODE", error.ErrorCode);
        Assert.Equal("Test message", error.Message);
        Assert.Null(error.FieldName);
        Assert.NotEqual(default, error.Timestamp);
    }

    [Fact]
    public void ValidationError_Constructor_WithFieldName()
    {
        var error = new ValidationError("TEST_CODE", "Test message", "FieldName");

        Assert.Equal("TEST_CODE", error.ErrorCode);
        Assert.Equal("Test message", error.Message);
        Assert.Equal("FieldName", error.FieldName);
    }

    [Fact]
    public void ValidationError_DefaultConstructor_CreatesInstance()
    {
        var error = new ValidationError();

        Assert.Equal(string.Empty, error.ErrorCode);
        Assert.Equal(string.Empty, error.Message);
        Assert.Null(error.FieldName);
    }

    [Fact]
    public void ValidationError_Timestamp_IsSetToUtcNow()
    {
        var before = DateTime.UtcNow;
        var error = new ValidationError("CODE", "Message");
        var after = DateTime.UtcNow;

        Assert.True(error.Timestamp >= before);
        Assert.True(error.Timestamp <= after);
    }

    [Fact]
    public void ValidationError_ToString_WithFieldName()
    {
        var error = new ValidationError("CODE", "Message", "Field");

        var result = error.ToString();

        Assert.Contains("CODE", result);
        Assert.Contains("Message", result);
        Assert.Contains("Field", result);
    }

    [Fact]
    public void ValidationError_ToString_WithoutFieldName()
    {
        var error = new ValidationError("CODE", "Message");

        var result = error.ToString();

        Assert.Contains("CODE", result);
        Assert.Contains("Message", result);
        Assert.DoesNotContain("Field:", result);
    }
}