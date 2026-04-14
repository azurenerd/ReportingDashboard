using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ReportingDashboard.Models;
using ReportingDashboard.Services;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly Mock<ILogger<DashboardDataService>> _loggerMock;

    private static readonly string ValidJson = """
    {
      "title": "Test Project",
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

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.ContentRootPath).Returns(_tempDir);

        _loggerMock = new Mock<ILogger<DashboardDataService>>();
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private DashboardDataService CreateService() =>
        new(_envMock.Object, _loggerMock.Object);

    private void WriteDataFile(string fileName, string content) =>
        File.WriteAllText(Path.Combine(_tempDir, fileName), content);

    [Fact]
    public void GetData_WithValidDefaultJson_ReturnsDeserializedData()
    {
        WriteDataFile("data.json", ValidJson);
        using var svc = CreateService();

        var data = svc.GetData(null);

        Assert.Equal("Test Project", data.Title);
        Assert.Equal("Test Subtitle", data.Subtitle);
        Assert.Equal(6, data.Months.Count);
        Assert.Equal(3, data.CurrentMonthIndex);
        Assert.Single(data.Milestones);
        Assert.Equal("M1", data.Milestones[0].Id);
        Assert.Equal("#0078D4", data.Milestones[0].Color);
        Assert.Single(data.Milestones[0].Markers);
        Assert.Equal("checkpoint", data.Milestones[0].Markers[0].Type);
        Assert.Equal(4, data.Categories.Count);
    }

    [Fact]
    public void GetData_WithMissingTitle_ThrowsDashboardDataException()
    {
        var json = ValidJson.Replace("\"title\": \"Test Project\"", "\"title\": \"\"");
        WriteDataFile("data.json", json);
        using var svc = CreateService();

        var ex = Assert.Throws<DashboardDataException>(() => svc.GetData(null));
        Assert.Contains("'title' is required", ex.Message);
    }

    [Fact]
    public void GetData_WithInvalidProjectNameChars_ThrowsDashboardDataException()
    {
        WriteDataFile("data.json", ValidJson);
        using var svc = CreateService();

        var ex = Assert.Throws<DashboardDataException>(() => svc.GetData("../hack"));
        Assert.Contains("Invalid project name", ex.Message);
        Assert.Contains("Only alphanumeric characters, hyphens, and underscores", ex.Message);
    }

    [Fact]
    public void GetData_WithInvalidCategoryKey_ThrowsDashboardDataException()
    {
        var json = ValidJson.Replace("\"key\": \"shipped\"", "\"key\": \"completed\"");
        WriteDataFile("data.json", json);
        using var svc = CreateService();

        var ex = Assert.Throws<DashboardDataException>(() => svc.GetData(null));
        Assert.Contains("Category key 'completed' is not recognized", ex.Message);
    }

    [Fact]
    public void Initialize_WithNoDataJson_DoesNotThrow()
    {
        using var svc = CreateService();

        var exception = Record.Exception(() => svc.Initialize());

        Assert.Null(exception);
    }
}