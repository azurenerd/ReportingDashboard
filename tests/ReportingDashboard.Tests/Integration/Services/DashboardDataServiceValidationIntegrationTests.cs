using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Services;

[Trait("Category", "Integration")]
public class DashboardDataServiceValidationIntegrationTests : IDisposable
{
    private readonly DashboardDataService _service;
    private readonly string _tempDir;

    public DashboardDataServiceValidationIntegrationTests()
    {
        var logger = new Mock<ILogger<DashboardDataService>>();
        _service = new DashboardDataService(logger.Object);
        _tempDir = Path.Combine(Path.GetTempPath(), $"Validation_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteJson(string json)
    {
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, json);
        return path;
    }

    [Fact]
    public async Task Validation_WhitespaceTitle_IsRejected()
    {
        var json = """
        {
            "title": "   ",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Apr",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01", "endDate": "2026-06-30", "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("title is required");
    }

    [Fact]
    public async Task Validation_MissingTimelineStartDate_IsRejected()
    {
        var json = """
        {
            "title": "T", "subtitle": "S", "backlogLink": "https://test.com",
            "currentMonth": "Apr", "months": ["Jan"],
            "timeline": {
                "endDate": "2026-06-30", "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("timeline.startDate is required");
    }

    [Fact]
    public async Task Validation_MissingTimelineEndDate_IsRejected()
    {
        var json = """
        {
            "title": "T", "subtitle": "S", "backlogLink": "https://test.com",
            "currentMonth": "Apr", "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01", "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("timeline.endDate is required");
    }

    [Fact]
    public async Task Validation_MissingTimelineNowDate_IsRejected()
    {
        var json = """
        {
            "title": "T", "subtitle": "S", "backlogLink": "https://test.com",
            "currentMonth": "Apr", "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01", "endDate": "2026-06-30",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain("timeline.nowDate is required");
    }

    [Fact]
    public async Task Validation_AllFieldsMissing_ReportsMultipleErrors()
    {
        var json = """
        {
            "currentMonth": "Apr",
            "months": [],
            "timeline": { "tracks": [] }
        }
        """;
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();

        var msg = _service.ErrorMessage!;
        msg.Should().Contain("title is required");
        msg.Should().Contain("subtitle is required");
        msg.Should().Contain("backlogLink is required");
        msg.Should().Contain("months is required");
        msg.Should().Contain("timeline.startDate is required");
        msg.Should().Contain("timeline.endDate is required");
        msg.Should().Contain("timeline.nowDate is required");
        msg.Should().Contain("timeline.tracks is required");
    }

    [Fact]
    public async Task Validation_AllFieldsPresent_Succeeds()
    {
        var json = """
        {
            "title": "Valid",
            "subtitle": "Valid Sub",
            "backlogLink": "https://valid.com",
            "currentMonth": "Apr",
            "months": ["Jan"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
            }
        }
        """;
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeFalse();
        _service.Data.Should().NotBeNull();
        _service.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task Validation_ErrorMessages_ContainSemicolonSeparators()
    {
        var json = """{ "months": [], "timeline": { "tracks": [] } }""";
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.IsError.Should().BeTrue();
        _service.ErrorMessage.Should().Contain(";");
    }

    [Fact]
    public async Task Validation_ErrorMessage_StartsWithPrefix()
    {
        var json = """{ "months": [], "timeline": { "tracks": [] } }""";
        var path = WriteJson(json);

        await _service.LoadAsync(path);

        _service.ErrorMessage.Should().StartWith("data.json validation:");
    }
}