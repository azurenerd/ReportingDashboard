using Microsoft.Extensions.Logging;

namespace ReportingDashboard.Tests.Helpers;

internal class TestLogger<T> : ILogger<T>
{
    private readonly object _lock = new();

    public List<LogEntry> Entries { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        lock (_lock)
        {
            Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
        }
    }

    public bool HasWarning(string substring)
    {
        lock (_lock)
        {
            return Entries.Any(e =>
                e.Level == LogLevel.Warning &&
                e.Message.Contains(substring, StringComparison.OrdinalIgnoreCase));
        }
    }

    public bool HasLogLevel(LogLevel level)
    {
        lock (_lock)
        {
            return Entries.Any(e => e.Level == level);
        }
    }
}

internal record LogEntry(LogLevel Level, string Message, Exception? Exception);