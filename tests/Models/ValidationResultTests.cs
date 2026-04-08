using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Tests.Models;

public class ValidationResultTests
{
    [Fact]
    public void ValidationResult_Constructor_DefaultIsValid()
    {
        var result = new ValidationResult();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidationResult_Constructor_WithIsValid()
    {
        var result = new ValidationResult(true);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidationResult_Constructor_WithIsValidFalse()
    {
        var result = new ValidationResult(false);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidationResult_Constructor_WithErrors()
    {
        var errors = new List<ValidationError>
        {
            new ValidationError("CODE1", "Message1"),
            new ValidationError("CODE2", "Message2")
        };
        var result = new ValidationResult(false, errors);

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void ValidationResult_AddError_WithValidationError()
    {
        var result = new ValidationResult();
        var error = new ValidationError("CODE", "Message", "Field");

        result.AddError(error);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal(error, result.Errors[0]);
    }

    [Fact]
    public void ValidationResult_AddError_WithParameters()
    {
        var result = new ValidationResult();

        result.AddError("CODE", "Message", "Field");

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("CODE", result.Errors[0].ErrorCode);
        Assert.Equal("Message", result.Errors[0].Message);
        Assert.Equal("Field", result.Errors[0].FieldName);
    }

    [Fact]
    public void ValidationResult_AddError_WithoutFieldName()
    {
        var result = new ValidationResult();

        result.AddError("CODE", "Message");

        Assert.False(result.IsValid);
        Assert.Null(result.Errors[0].FieldName);
    }

    [Fact]
    public void ValidationResult_AddError_SetsIsValidToFalse()
    {
        var result = new ValidationResult { IsValid = true };

        result.AddError("CODE", "Message");

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidationResult_AddMultipleErrors()
    {
        var result = new ValidationResult();

        result.AddError("CODE1", "Message1");
        result.AddError("CODE2", "Message2");
        result.AddError("CODE3", "Message3");

        Assert.Equal(3, result.Errors.Count);
    }

    [Fact]
    public void ValidationResult_GetErrorSummary_WhenValid()
    {
        var result = new ValidationResult { IsValid = true };

        var summary = result.GetErrorSummary();

        Assert.Contains("passed", summary.ToLower());
    }

    [Fact]
    public void ValidationResult_GetErrorSummary_WithFieldErrors()
    {
        var result = new ValidationResult();
        result.AddError("CODE1", "Message1", "Name");
        result.AddError("CODE2", "Message2", "Email");

        var summary = result.GetErrorSummary();

        Assert.Contains("failed", summary.ToLower());
        Assert.Contains("Name", summary);
        Assert.Contains("Email", summary);
    }

    [Fact]
    public void ValidationResult_GetErrorSummary_WithoutFieldNames()
    {
        var result = new ValidationResult();
        result.AddError("CODE1", "Message1");

        var summary = result.GetErrorSummary();

        Assert.Contains("failed", summary.ToLower());
    }

    [Fact]
    public void ValidationResult_GetErrorSummary_UniqueFields()
    {
        var result = new ValidationResult();
        result.AddError("CODE1", "Message1", "Name");
        result.AddError("CODE2", "Message2", "Name");

        var summary = result.GetErrorSummary();

        var nameCount = summary.Split("Name").Length - 1;
        Assert.Equal(1, nameCount);
    }
}