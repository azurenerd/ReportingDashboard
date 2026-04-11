using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class DashboardDataServiceConcurrencyTests : IDisposable
{
    private readonly Mock<ILogger<DashboardDataService>> _loggerMock;
    private readonly string _tempDir;

    public DashboardDataServiceConcurrencyTests()
    {
        _loggerMock = new Mock<ILogger<DashboardDataService>>();
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardConcurrency_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteTempJson(string json)
    {
        var filePath = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(filePath, json);
        return filePath;
    }

    private static string ValidJson => """
    {
        "title": "Test",
        "subtitle": "Sub",
        "backlogLink": "https://test.com",
        "currentMonth": "Apr",
        "months": ["Jan"],
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
        },
        "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
    }
    """;

    [Fact]
    public async Task MultipleLoadCalls_LastCallWins()
    {
        var service = new DashboardDataService(_loggerMock.Object);
        var filePath = WriteTempJson(ValidJson);

        await service.LoadAsync(filePath);
        await service.LoadAsync(filePath);
        await service.LoadAsync(filePath);

        service.IsError.Should().BeFalse();
        service.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadAsync_SuccessThenFailure_ClearsData()
    {
        var service = new DashboardDataService(_loggerMock.Object);
        var filePath = WriteTempJson(ValidJson);

        await service.LoadAsync(filePath);
        service.Data.Should().NotBeNull();

        await service.LoadAsync(Path.Combine(_tempDir, "does_not_exist.json"));

        service.IsError.Should().BeTrue();
        service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_FailureThenSuccess_RestoresData()
    {
        var service = new DashboardDataService(_loggerMock.Object);

        await service.LoadAsync(Path.Combine(_tempDir, "missing.json"));
        service.IsError.Should().BeTrue();

        var filePath = WriteTempJson(ValidJson);
        await service.LoadAsync(filePath);

        service.IsError.Should().BeFalse();
        service.Data.Should().NotBeNull();
        service.Data!.Title.Should().Be("Test");
    }
}