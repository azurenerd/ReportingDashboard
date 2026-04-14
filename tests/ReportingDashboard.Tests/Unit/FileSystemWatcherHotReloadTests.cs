using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Xunit;
using ReportingDashboard.Services;

namespace ReportingDashboard.Tests.Unit;

/// <summary>
/// Captures log entries so tests can assert on log level and message content.
/// </summary>
internal class CapturingLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, string Message, Exception? Exception)> Entries { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Entries.Add((logLevel, formatter(state, exception), exception));
    }
}

internal class HotReloadStubWebHostEnvironment : IWebHostEnvironment
{
    public string ContentRootPath { get; set; } = string.Empty;
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
    public string WebRootPath { get; set; } = string.Empty;
    public IFileProvider WebRootFileProvider { get; set; } = null!;
    public string ApplicationName { get; set; } = "ReportingDashboard";
    public string EnvironmentName { get; set; } = "Development";
}

[Trait("Category", "Unit")]
public class FileSystemWatcherHotReloadTests : IDisposable
{
    private readonly string _tempDir;
    private readonly IWebHostEnvironment _env;
    private readonly CapturingLogger<DashboardDataService> _logger;

    private static readonly string ValidJson = """
    {
      "title": "Original Title",
      "subtitle": "Test Subtitle",
      "backlogUrl": "https://example.com",
      "currentDate": "2026-04-14",
      "months": ["Jan","Feb","Mar","Apr","May","Jun"],
      "currentMonthIndex": 3,
      "timelineStart": "2026-01-01",
      "timelineEnd": "2026-06-30",
      "milestones": [
        {
          "id": "M1",
          "label": "M1",
          "description": "Milestone One",
          "color": "#0078D4",
          "markers": [
            { "date": "2026-02-15", "type": "checkpoint", "label": "Feb Check" }
          ]
        }
      ],
      "categories": [
        { "name": "Shipped", "key": "shipped", "items": { "Jan": ["Item A"] } },
        { "name": "In Progress", "key": "inProgress", "items": {} },
        { "name": "Carryover", "key": "carryover", "items": {} },
        { "name": "Blockers", "key": "blockers", "items": {} }
      ]
    }
    """;

    private static readonly string UpdatedJson = """
    {
      "title": "Updated Title",
      "subtitle": "Test Subtitle",
      "backlogUrl": "https://example.com",
      "currentDate": "2026-04-14",
      "months": ["Jan","Feb","Mar","Apr","May","Jun"],
      "currentMonthIndex": 3,
      "timelineStart": "2026-01-01",
      "timelineEnd": "2026-06-30",
      "milestones": [
        {
          "id": "M1",
          "label": "M1",
          "description": "Milestone One",
          "color": "#0078D4",
          "markers": [
            { "date": "2026-02-15", "type": "checkpoint", "label": "Feb Check" }
          ]
        }
      ],
      "categories": [
        { "name": "Shipped", "key": "shipped", "items": { "Jan": ["Item B"] } },
        { "name": "In Progress", "key": "inProgress", "items": {} },
        { "name": "Carryover", "key": "carryover", "items": {} },
        { "name": "Blockers", "key": "blockers", "items": {} }
      ]
    }
    """;

    public FileSystemWatcherHotReloadTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"HotReloadTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        _env = new HotReloadStubWebHostEnvironment { ContentRootPath = _tempDir };
        _logger = new CapturingLogger<DashboardDataService>();
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private DashboardDataService CreateService() => new(_env, _logger);

    private void WriteDataFile(string fileName, string content) =>
        File.WriteAllText(Path.Combine(_tempDir, fileName), content);

    [Fact]
    public void ExecuteReload_ClearsCacheAndRaisesEvent_WhenFileIsValid()
    {
        // Arrange: populate cache with original data
        WriteDataFile("data.json", ValidJson);
        using var svc = CreateService();
        var originalData = svc.GetData();
        Assert.Equal("Original Title", originalData.Title);

        // Write updated valid JSON
        WriteDataFile("data.json", UpdatedJson);

        var eventFired = false;
        svc.OnDataChanged += () => eventFired = true;

        // Act: call ExecuteReload directly (internal, visible via InternalsVisibleTo)
        svc.ExecuteReload();

        // Assert: event was raised and cache returns new data
        Assert.True(eventFired, "OnDataChanged should have been raised");
        var newData = svc.GetData();
        Assert.Equal("Updated Title", newData.Title);

        // Verify info log was written
        Assert.Contains(_logger.Entries,
            e => e.Level == LogLevel.Information &&
                 e.Message.Contains("Dashboard data reloaded successfully"));
    }

    [Fact]
    public void ExecuteReload_RetainsCacheAndLogsWarning_WhenJsonIsMalformed()
    {
        // Arrange: populate cache with valid data
        WriteDataFile("data.json", ValidJson);
        using var svc = CreateService();
        var originalData = svc.GetData();
        Assert.Equal("Original Title", originalData.Title);

        // Overwrite with malformed JSON
        WriteDataFile("data.json", "{broken json content");

        var eventFired = false;
        svc.OnDataChanged += () => eventFired = true;

        // Act
        svc.ExecuteReload();

        // Assert: event was NOT raised, cache retains original data
        Assert.False(eventFired, "OnDataChanged should NOT fire on malformed JSON");
        var cachedData = svc.GetData();
        Assert.Equal("Original Title", cachedData.Title);

        // Verify warning was logged
        Assert.Contains(_logger.Entries,
            e => e.Level == LogLevel.Warning &&
                 e.Message.Contains("Failed to reload data after file change"));
    }

    [Fact]
    public void ExecuteReload_RetainsCacheAndLogsWarning_WhenFileIsLocked()
    {
        // Arrange: populate cache
        WriteDataFile("data.json", ValidJson);
        using var svc = CreateService();
        var originalData = svc.GetData();

        var eventFired = false;
        svc.OnDataChanged += () => eventFired = true;

        // Lock the file exclusively so PreValidateDataFiles throws IOException
        var filePath = Path.Combine(_tempDir, "data.json");
        using (var lockStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            // Act: ExecuteReload should catch IOException, retry, then fail gracefully
            svc.ExecuteReload();
        }

        // Assert: cache retained, event not fired
        Assert.False(eventFired, "OnDataChanged should NOT fire when file is locked");
        var cachedData = svc.GetData();
        Assert.Equal("Original Title", cachedData.Title);

        // Verify the IOException retry path was followed
        Assert.Contains(_logger.Entries,
            e => e.Level == LogLevel.Debug &&
                 e.Message.Contains("IOException on first reload attempt"));
        Assert.Contains(_logger.Entries,
            e => e.Level == LogLevel.Warning &&
                 e.Message.Contains("retry failed"));
    }

    [Fact]
    public void Dispose_IsIdempotent_NoExceptionOnDoubleDispose()
    {
        WriteDataFile("data.json", ValidJson);
        var svc = CreateService();
        svc.Initialize();

        // Act: dispose twice
        var ex = Record.Exception(() =>
        {
            svc.Dispose();
            svc.Dispose();
        });

        // Assert
        Assert.Null(ex);
    }

    [Fact]
    public void ExecuteReload_RetainsCacheWhenValidationFails_WithEmptyTitle()
    {
        // Arrange: populate cache with valid data
        WriteDataFile("data.json", ValidJson);
        using var svc = CreateService();
        var originalData = svc.GetData();
        Assert.Equal("Original Title", originalData.Title);

        // Replace with JSON that has empty title (validation failure, not parse failure)
        var invalidJson = ValidJson.Replace("\"title\": \"Original Title\"", "\"title\": \"\"");
        WriteDataFile("data.json", invalidJson);

        var eventFired = false;
        svc.OnDataChanged += () => eventFired = true;

        // Act
        svc.ExecuteReload();

        // Assert: validation fails in PreValidateDataFiles, cache retained
        Assert.False(eventFired, "OnDataChanged should NOT fire on validation failure");
        var cachedData = svc.GetData();
        Assert.Equal("Original Title", cachedData.Title);

        Assert.Contains(_logger.Entries,
            e => e.Level == LogLevel.Warning &&
                 e.Message.Contains("Failed to reload data after file change"));
    }
}