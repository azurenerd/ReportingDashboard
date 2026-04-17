using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _webRootPath;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardTests_{Guid.NewGuid():N}");
        _webRootPath = Path.Combine(_tempDir, "wwwroot");
        Directory.CreateDirectory(_webRootPath);

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_webRootPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void LoadDashboard_MissingFile_ReturnsNullDataWithError()
    {
        var service = new DashboardDataService(_envMock.Object);

        var (data, error) = service.LoadDashboard();

        data.Should().BeNull();
        error.Should().StartWith("data.json not found.");
        error.Should().Contain("wwwroot/");
        error.Should().Contain("data.sample.json");
    }

    [Fact]
    public void LoadDashboard_InvalidJson_ReturnsNullDataWithParseError()
    {
        File.WriteAllText(Path.Combine(_webRootPath, "data.json"), "{ invalid json }");
        var service = new DashboardDataService(_envMock.Object);

        var (data, error) = service.LoadDashboard();

        data.Should().BeNull();
        error.Should().StartWith("Unable to load dashboard data.");
        error.Should().Contain("syntax errors");
    }

    [Fact]
    public void LoadDashboard_ValidJson_ReturnsDeserializedData()
    {
        var json = """
        {
            "title": "My Project",
            "subtitle": "Q1 2026",
            "backlogUrl": "https://dev.azure.com/test",
            "currentDate": "2026-02-15",
            "timelineStartMonth": "2025-11",
            "timelineEndMonth": "2026-04",
            "milestones": [
                { "id": "M1", "label": "Alpha", "color": "#0078D4", "events": [] }
            ],
            "heatmap": {
                "months": ["Jan", "Feb", "Mar"],
                "currentMonthIndex": 1,
                "rows": []
            }
        }
        """;
        File.WriteAllText(Path.Combine(_webRootPath, "data.json"), json);
        var service = new DashboardDataService(_envMock.Object);

        var (data, error) = service.LoadDashboard();

        error.Should().BeNull();
        data.Should().NotBeNull();
        data!.Title.Should().Be("My Project");
        data.Subtitle.Should().Be("Q1 2026");
        data.Milestones.Should().HaveCount(1);
        data.Milestones[0].Id.Should().Be("M1");
        data.Heatmap.Months.Should().HaveCount(3);
        data.Heatmap.CurrentMonthIndex.Should().Be(1);
    }

    [Fact]
    public void LoadDashboard_CaseInsensitiveDeserialization_Works()
    {
        var json = """
        {
            "Title": "Upper Case",
            "SUBTITLE": "ALL CAPS",
            "milestones": [],
            "heatmap": { "months": [], "currentMonthIndex": 0, "rows": [] }
        }
        """;
        File.WriteAllText(Path.Combine(_webRootPath, "data.json"), json);
        var service = new DashboardDataService(_envMock.Object);

        var (data, error) = service.LoadDashboard();

        error.Should().BeNull();
        data.Should().NotBeNull();
        data!.Title.Should().Be("Upper Case");
    }

    [Fact]
    public void LoadDashboard_EmptyJsonObject_ReturnsDataWithDefaults()
    {
        File.WriteAllText(Path.Combine(_webRootPath, "data.json"), "{}");
        var service = new DashboardDataService(_envMock.Object);

        var (data, error) = service.LoadDashboard();

        error.Should().BeNull();
        data.Should().NotBeNull();
        data!.Title.Should().Be("");
        data.Milestones.Should().BeEmpty();
    }
}