using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Services;

public class ErrorLogger : IErrorHandler
{
    private readonly ILogger<ErrorLogger> _logger;
    private readonly IWebHostEnvironment _environment;

    public ErrorLogger(ILogger<ErrorLogger> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public Task CaptureErrorAsync(Exception exception, ErrorContext context)
    {
        var timestamp = DateTime.UtcNow.ToString("O");
        var exceptionType = exception.GetType().Name;
        var message = exception.Message;
        var stackTrace = exception.StackTrace ?? "N/A";

        var logMessage = $"[{timestamp}] {context} - {exceptionType}: {message}\nStackTrace: {stackTrace}";

        _logger.LogError(logMessage);

        if (!_environment.IsDevelopment())
        {
            LogToFile(logMessage).GetAwaiter().GetResult();
        }

        return Task.CompletedTask;
    }

    public string GetUserMessage(ErrorContext context, Exception exception)
    {
        if (_environment.IsDevelopment())
        {
            return $"{context}: {exception.Message}";
        }

        return context switch
        {
            ErrorContext.FileNotFound =>
                "Configuration file missing. Please ensure data.json exists in the application directory.",
            ErrorContext.InvalidJson =>
                "Invalid JSON format in configuration file. Please check data.json syntax.",
            ErrorContext.ValidationFailed =>
                "Configuration validation failed. Required fields are missing.",
            ErrorContext.NullProject =>
                "Project data is invalid. Please verify the data.json file structure.",
            ErrorContext.MissingMilestones =>
                "No milestones found. At least one milestone is required.",
            ErrorContext.NullMetrics =>
                "Project metrics are missing. Please ensure all required metrics are present.",
            ErrorContext.NullWorkItems =>
                "Work items data is invalid. Please verify the data.json file structure.",
            ErrorContext.InvalidCompletionPercentage =>
                "Invalid completion percentage. Value must be between 0 and 100.",
            ErrorContext.InvalidHealthStatus =>
                "Invalid health status. Please check the data.json file.",
            _ => "An unexpected error occurred. Please try again or contact support."
        };
    }

    private static async Task LogToFile(string message)
    {
        try
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logPath);

            var logFile = Path.Combine(logPath, $"error-{DateTime.UtcNow:yyyy-MM-dd}.log");
            await File.AppendAllTextAsync(logFile, message + Environment.NewLine);
        }
        catch
        {
            // Silently fail if file logging is not available
        }
    }
}