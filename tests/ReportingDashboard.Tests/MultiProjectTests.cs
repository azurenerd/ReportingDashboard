using Xunit;
using ReportingDashboard.Services;

namespace ReportingDashboard.Tests;

public class MultiProjectTests : IDisposable
{
    private readonly string _tempDir;
    private readonly DashboardDataService _service;

    public MultiProjectTests()
    {
        _tempDir = TestHelper.CreateTempDir(new Dictionary<string, string>
        {
            ["data.json"] = TestHelper.MinimalValidJson,
            ["data.phoenix.json"] = TestHelper.PhoenixValidJson
        });
        _service = TestHelper.CreateService(_tempDir);
    }

    // --- Project name sanitization tests ---

    [Fact]
    public void GetData_InvalidProjectName_Dot_ThrowsException()
    {
        var ex = Assert.Throws<DashboardDataException>(() => _service.GetData("../traversal"));
        Assert.Contains("Invalid project name", ex.Message);
    }

    [Fact]
    public void GetData_InvalidProjectName_Slash_ThrowsException()
    {
        var ex = Assert.Throws<DashboardDataException>(() => _service.GetData("path/traversal"));
        Assert.Contains("Invalid project name", ex.Message);
    }

    [Fact]
    public void GetData_InvalidProjectName_Backslash_ThrowsException()
    {
        var ex = Assert.Throws<DashboardDataException>(() => _service.GetData("path\\traversal"));
        Assert.Contains("Invalid project name", ex.Message);
    }

    [Fact]
    public void GetData_InvalidProjectName_Space_ThrowsException()
    {
        var ex = Assert.Throws<DashboardDataException>(() => _service.GetData("has space"));
        Assert.Contains("Invalid project name", ex.Message);
    }

    [Fact]
    public void GetData_InvalidProjectName_DotOnly_ThrowsException()
    {
        var ex = Assert.Throws<DashboardDataException>(() => _service.GetData("has.dots"));
        Assert.Contains("Invalid project name", ex.Message);
    }

    [Fact]
    public void GetData_PathTraversal_ThrowsInvalidNotFileError()
    {
        var ex = Assert.Throws<DashboardDataException>(() => _service.GetData("../../etc/passwd"));
        Assert.Contains("Invalid project name", ex.Message);
        Assert.DoesNotContain("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetData_ValidProjectName_MissingFile_ThrowsNotFound()
    {
        var ex = Assert.Throws<DashboardDataException>(() => _service.GetData("my-project_v2"));
        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Invalid project name", ex.Message);
    }

    // --- Default project resolution ---

    [Fact]
    public void GetData_DefaultProject_ReturnsDefaultData()
    {
        var data = _service.GetData();
        Assert.Equal("Test Dashboard", data.Title);
    }

    [Fact]
    public void GetData_NullProject_ReturnsDefaultData()
    {
        var data = _service.GetData(null);
        Assert.Equal("Test Dashboard", data.Title);
    }

    // --- Projects subfolder fallback ---

    [Fact]
    public void GetData_ProjectsSubfolder_Fallback()
    {
        var projectsDir = Path.Combine(_tempDir, "projects");
        Directory.CreateDirectory(projectsDir);
        File.WriteAllText(Path.Combine(projectsDir, "atlas.json"), TestHelper.MinimalValidJson);

        using var service = TestHelper.CreateService(_tempDir);
        var data = service.GetData("atlas");
        Assert.Equal("Test Dashboard", data.Title);
    }

    // --- Phoenix project data tests ---

    [Fact]
    public void GetData_PhoenixProject_ReturnsDifferentTitle()
    {
        var data = _service.GetData("phoenix");
        Assert.Equal("Project Phoenix Release Roadmap", data.Title);
    }

    [Fact]
    public void GetData_PhoenixProject_HasTwoMilestones()
    {
        var data = _service.GetData("phoenix");
        Assert.Equal(2, data.Milestones.Count);
    }

    [Fact]
    public void GetData_PhoenixProject_HasCorrectMilestoneIds()
    {
        var data = _service.GetData("phoenix");
        Assert.Equal("P1", data.Milestones[0].Id);
        Assert.Equal("P2", data.Milestones[1].Id);
    }

    [Fact]
    public void GetData_PhoenixProject_HasCorrectMilestoneColors()
    {
        var data = _service.GetData("phoenix");
        Assert.Equal("#D32F2F", data.Milestones[0].Color);
        Assert.Equal("#7B1FA2", data.Milestones[1].Color);
    }

    [Fact]
    public void GetData_PhoenixProject_HasSixMonths()
    {
        var data = _service.GetData("phoenix");
        Assert.Equal(6, data.Months.Count);
        Assert.Equal("Feb", data.Months[0]);
        Assert.Equal("Jul", data.Months[5]);
    }

    [Fact]
    public void GetData_PhoenixProject_PassesValidation()
    {
        var ex = Record.Exception(() => _service.GetData("phoenix"));
        Assert.Null(ex);
    }

    [Fact]
    public void GetData_PhoenixProject_HasFourCategories()
    {
        var data = _service.GetData("phoenix");
        Assert.Equal(4, data.Categories.Count);
        Assert.Contains(data.Categories, c => c.Key == "shipped");
        Assert.Contains(data.Categories, c => c.Key == "inProgress");
        Assert.Contains(data.Categories, c => c.Key == "carryover");
        Assert.Contains(data.Categories, c => c.Key == "blockers");
    }

    [Fact]
    public void GetData_PhoenixProject_CurrentMonthIndexValid()
    {
        var data = _service.GetData("phoenix");
        Assert.InRange(data.CurrentMonthIndex, 0, data.Months.Count - 1);
        Assert.Equal("Apr", data.Months[data.CurrentMonthIndex]);
    }

    [Fact]
    public void GetData_PhoenixProject_MilestonesWithinTimelineRange()
    {
        var data = _service.GetData("phoenix");
        foreach (var milestone in data.Milestones)
        {
            foreach (var marker in milestone.Markers)
            {
                Assert.True(marker.Date >= data.TimelineStart,
                    $"Marker date {marker.Date} is before timelineStart {data.TimelineStart}");
                Assert.True(marker.Date <= data.TimelineEnd,
                    $"Marker date {marker.Date} is after timelineEnd {data.TimelineEnd}");
            }
        }
    }

    [Fact]
    public void GetData_PhoenixProject_DifferentSubtitle()
    {
        var defaultData = _service.GetData();
        var phoenixData = _service.GetData("phoenix");
        Assert.NotEqual(defaultData.Subtitle, phoenixData.Subtitle);
    }

    [Fact]
    public void GetData_NonexistentProject_ThrowsWithSearchedPaths()
    {
        var ex = Assert.Throws<DashboardDataException>(() => _service.GetData("nonexistent"));
        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("nonexistent", ex.Message);
    }

    public void Dispose()
    {
        _service.Dispose();
        try { Directory.Delete(_tempDir, true); } catch { }
    }
}