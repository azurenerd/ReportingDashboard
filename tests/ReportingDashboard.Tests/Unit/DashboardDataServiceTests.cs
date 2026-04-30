using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly Mock<ILogger<DashboardDataService>> _loggerMock;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(Path.Combine(_tempDir, "Data"));
        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.ContentRootPath).Returns(_tempDir);
        _loggerMock = new Mock<ILogger<DashboardDataService>>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void ValidJson_LoadsDataSuccessfully()
    {
        var json = """
        {
            "title": "Test Dashboard",
            "subtitle": "Test Subtitle",
            "backlogUrl": "https://example.com",
            "currentDate": "2026-04-15",
            "timelineStartDate": "2026-01-01",
            "timelineEndDate": "2026-06-30",
            "milestoneStreams": [],
            "heatmapMonths": ["Jan", "Feb"],
            "currentMonth": "Feb",
            "heatmapRows": []
        }
        """;
        File.WriteAllText(Path.Combine(_tempDir, "Data", "dashboard-data.json"), json);

        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        service.HasError.Should().BeFalse();
        service.ErrorMessage.Should().BeNull();
        service.Data.Should().NotBeNull();
        service.Data!.Title.Should().Be("Test Dashboard");
        service.Data.HeatmapMonths.Should().HaveCount(2);
    }

    [Fact]
    public void MissingFile_SetsHasErrorAndErrorMessage()
    {
        // Don't create the file - it should be missing
        File.Delete(Path.Combine(_tempDir, "Data", "dashboard-data.json"));

        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        service.HasError.Should().BeTrue();
        service.Data.Should().BeNull();
        service.ErrorMessage.Should().Contain("Configuration file not found");
        service.ErrorMessage.Should().Contain("dashboard-data.json");
    }

    [Fact]
    public void InvalidJson_SetsHasErrorWithParseMessage()
    {
        var invalidJson = "{ \"title\": \"Test\", INVALID }";
        File.WriteAllText(Path.Combine(_tempDir, "Data", "dashboard-data.json"), invalidJson);

        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        service.HasError.Should().BeTrue();
        service.Data.Should().BeNull();
        service.ErrorMessage.Should().Contain("JSON parse error");
        service.ErrorMessage.Should().Contain("dashboard-data.json");
    }

    [Fact]
    public void JsonWithComments_LoadsSuccessfully()
    {
        var json = """
        {
            // This is a comment
            "title": "Commented Dashboard",
            "subtitle": "Sub",
            "backlogUrl": "",
            "currentDate": "2026-04-01",
            "timelineStartDate": "2026-01-01",
            "timelineEndDate": "2026-06-30",
            "milestoneStreams": [],
            "heatmapMonths": [],
            "currentMonth": "",
            "heatmapRows": []
        }
        """;
        File.WriteAllText(Path.Combine(_tempDir, "Data", "dashboard-data.json"), json);

        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        service.HasError.Should().BeFalse();
        service.Data!.Title.Should().Be("Commented Dashboard");
    }

    [Fact]
    public void CaseInsensitiveDeserialization_WorksCorrectly()
    {
        var json = """
        {
            "Title": "Upper Case",
            "Subtitle": "Test",
            "BacklogUrl": "http://test.com",
            "CurrentDate": "2026-04-15",
            "TimelineStartDate": "2026-01-01",
            "TimelineEndDate": "2026-06-30",
            "MilestoneStreams": [{ "Id": "M1", "Label": "Stream 1", "Color": "#0078D4", "Milestones": [] }],
            "HeatmapMonths": ["Apr"],
            "CurrentMonth": "Apr",
            "HeatmapRows": [{ "Category": "shipped", "Items": { "Apr": ["Item 1"] } }]
        }
        """;
        File.WriteAllText(Path.Combine(_tempDir, "Data", "dashboard-data.json"), json);

        var service = new DashboardDataService(_envMock.Object, _loggerMock.Object);

        service.HasError.Should().BeFalse();
        service.Data!.Title.Should().Be("Upper Case");
        service.Data.MilestoneStreams.Should().HaveCount(1);
        service.Data.HeatmapRows[0].Items["Apr"].Should().Contain("Item 1");
    }
}