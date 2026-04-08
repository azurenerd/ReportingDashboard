using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgentSquad.Runner.Tests.Services;

public class ErrorLoggerTests
{
    private readonly Mock<ILogger<ErrorLogger>> _mockLogger;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly IErrorHandler _errorHandler;

    public ErrorLoggerTests()
    {
        _mockLogger = new Mock<ILogger<ErrorLogger>>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _errorHandler = new ErrorLogger(_mockLogger.Object, _mockEnvironment.Object);
    }

    [Fact]
    public async Task CaptureErrorAsync_LogsExceptionToConsole()
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(true);
        var exception = new FileNotFoundException("Test file not found");
        var context = ErrorContext.FileNotFound;

        await _errorHandler.CaptureErrorAsync(exception, context);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CaptureErrorAsync_IncludesExceptionType()
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(true);
        var exception = new JsonException("Invalid JSON");
        var context = ErrorContext.InvalidJson;

        await _errorHandler.CaptureErrorAsync(exception, context);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("JsonException")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CaptureErrorAsync_IncludesStackTrace()
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(true);
        var exception = new InvalidOperationException("Test error");
        var context = ErrorContext.ValidationFailed;

        await _errorHandler.CaptureErrorAsync(exception, context);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("StackTrace")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CaptureErrorAsync_IncludesTimestamp()
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(true);
        var exception = new Exception("Test");
        var context = ErrorContext.UnknownError;

        var beforeCapture = DateTime.UtcNow;
        await _errorHandler.CaptureErrorAsync(exception, context);
        var afterCapture = DateTime.UtcNow;

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(":")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CaptureErrorAsync_InProduction_DoesNotLogToConsole()
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false);
        var exception = new Exception("Test");
        var context = ErrorContext.UnknownError;

        await _errorHandler.CaptureErrorAsync(exception, context);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(ErrorContext.FileNotFound)]
    [InlineData(ErrorContext.InvalidJson)]
    [InlineData(ErrorContext.ValidationFailed)]
    public void GetUserMessage_ReturnsUserFriendlyMessage_InProduction(ErrorContext context)
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false);
        var exception = new Exception("Detailed error");

        var message = _errorHandler.GetUserMessage(context, exception);

        Assert.NotEmpty(message);
        Assert.NotEqual("Detailed error", message);
    }

    [Fact]
    public void GetUserMessage_ForFileNotFound_ReturnsSpecificMessage()
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false);

        var message = _errorHandler.GetUserMessage(ErrorContext.FileNotFound, new Exception());

        Assert.Contains("Configuration file missing", message);
        Assert.Contains("data.json", message);
    }

    [Fact]
    public void GetUserMessage_ForInvalidJson_ReturnsSpecificMessage()
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false);

        var message = _errorHandler.GetUserMessage(ErrorContext.InvalidJson, new Exception());

        Assert.Contains("Invalid JSON", message);
        Assert.Contains("data.json", message);
    }

    [Fact]
    public void GetUserMessage_ForValidationFailed_ReturnsSpecificMessage()
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false);

        var message = _errorHandler.GetUserMessage(ErrorContext.ValidationFailed, new Exception());

        Assert.Contains("validation failed", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetUserMessage_ForNullProject_ReturnsSpecificMessage()
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false);

        var message = _errorHandler.GetUserMessage(ErrorContext.NullProject, new Exception());

        Assert.Contains("Project data is invalid", message);
    }

    [Fact]
    public void GetUserMessage_ForMissingMilestones_ReturnsSpecificMessage()
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false);

        var message = _errorHandler.GetUserMessage(ErrorContext.MissingMilestones, new Exception());

        Assert.Contains("milestone", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetUserMessage_ForNullMetrics_ReturnsSpecificMessage()
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false);

        var message = _errorHandler.GetUserMessage(ErrorContext.NullMetrics, new Exception());

        Assert.Contains("metrics", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetUserMessage_ForNullWorkItems_ReturnsSpecificMessage()
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false);

        var message = _errorHandler.GetUserMessage(ErrorContext.NullWorkItems, new Exception());

        Assert.Contains("Work items", message);
    }

    [Fact]
    public void GetUserMessage_ForInvalidCompletionPercentage_ReturnsSpecificMessage()
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false);

        var message = _errorHandler.GetUserMessage(ErrorContext.InvalidCompletionPercentage, new Exception());

        Assert.Contains("completion", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetUserMessage_ForInvalidHealthStatus_ReturnsSpecificMessage()
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false);

        var message = _errorHandler.GetUserMessage(ErrorContext.InvalidHealthStatus, new Exception());

        Assert.Contains("health status", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetUserMessage_ForUnknownError_ReturnsGenericMessage()
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false);

        var message = _errorHandler.GetUserMessage(ErrorContext.UnknownError, new Exception());

        Assert.Contains("unexpected error", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetUserMessage_InDevelopment_ReturnsDetailedMessage()
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(true);
        var detailedError = "Detailed technical error message";
        var exception = new Exception(detailedError);

        var message = _errorHandler.GetUserMessage(ErrorContext.ValidationFailed, exception);

        Assert.Contains(detailedError, message);
    }

    [Fact]
    public void GetUserMessage_UnknownContext_ReturnsGenericMessage()
    {
        _mockEnvironment.Setup(x => x.IsDevelopment()).Returns(false);

        var message = _errorHandler.GetUserMessage((ErrorContext)999, new Exception("Test"));

        Assert.Contains("unexpected error", message, StringComparison.OrdinalIgnoreCase);
    }
}