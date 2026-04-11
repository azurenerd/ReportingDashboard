using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Services;

/// <summary>
/// Integration tests verifying DashboardDataService logging behavior
/// for success, error, and validation scenarios.
/// </summary>
[Trait("Category", "Integration")]
public class DashboardDataServiceLoggingIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly Mock<ILogger<DashboardDataService>> _mockLogger;

    public DashboardDataServiceLoggingIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardLog_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _mockLogger = new Mock<ILogger<DashboardDataService>>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteJson(string json)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        return path;
    }

    private static string GetValidJson() => """
    {
        "title": "Log Test",
        "subtitle": "Sub",
        "backlogLink": "https://test.com",
        "currentMonth": "Jan",
        "months": ["Jan"],
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [{ "name": "T", "label": "L", "milestones": [] }]
        },
        "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
    }
    """;

    [Fact]
    public async Task LoadAsync_Success_LogsInformationWithTitleAndCounts()
    {
        var service = new DashboardDataService(_mockLogger.Object);
        var path = WriteJson(GetValidJson());

        await service.LoadAsync(path);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Successfully loaded") &&
                    v.ToString()!.Contains("Log Test")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_LogsError()
    {
        var service = new DashboardDataService(_mockLogger.Object);

        await service.LoadAsync(Path.Combine(_tempDir, "missing.json"));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("not found")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_LogsErrorWithParseDetails()
    {
        var service = new DashboardDataService(_mockLogger.Object);
        var path = WriteJson("{ broken json }");

        await service.LoadAsync(path);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("Failed to parse")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_ValidationFailure_LogsErrorWithValidationDetails()
    {
        var json = """
        {
            "title": "",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Jan",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "T", "label": "L", "milestones": [] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var service = new DashboardDataService(_mockLogger.Object);
        var path = WriteJson(json);

        await service.LoadAsync(path);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("title")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_NullDeserialization_LogsError()
    {
        var service = new DashboardDataService(_mockLogger.Object);
        var path = WriteJson("null");

        await service.LoadAsync(path);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("deserialization returned null")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_SuccessDoesNotLogError()
    {
        var service = new DashboardDataService(_mockLogger.Object);
        var path = WriteJson(GetValidJson());

        await service.LoadAsync(path);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}