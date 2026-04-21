using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Data;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteTempJson(string content)
    {
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, content);
        return path;
    }

    /// <summary>
    /// Builds a complete valid JSON string that satisfies all model required properties
    /// and passes all DashboardDataService validation checks.
    /// HeatmapRow requires: category, label, cssClass. ProjectInfo requires: title, subtitle, backlogUrl.
    /// </summary>
    private static string BuildValidJson() => """
    {
        "project": {
            "title": "Test Project",
            "subtitle": "Test Org - Test Workstream - April 2026",
            "backlogUrl": "https://dev.azure.com/test"
        },
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-15",
            "tracks": [
                {
                    "id": "M1",
                    "name": "Track 1",
                    "color": "#0078D4",
                    "milestones": []
                }
            ]
        },
        "heatmap": {
            "columns": ["Jan", "Feb"],
            "currentColumn": "Jan",
            "rows": [
                {
                    "category": "Shipped",
                    "label": "Shipped",
                    "cssClass": "ship",
                    "items": { "Jan": ["Item1"] }
                }
            ]
        }
    }
    """;

    /// <summary>
    /// Builds a valid JSON with all required model fields but empty timeline tracks,
    /// so that the service's own validation (not deserialization) catches the issue.
    /// </summary>
    private static string BuildJsonWithEmptyTracks() => """
    {
        "project": {
            "title": "Test",
            "subtitle": "Sub",
            "backlogUrl": "https://test"
        },
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-01",
            "tracks": []
        },
        "heatmap": {
            "columns": ["Jan"],
            "currentColumn": "Jan",
            "rows": [
                {
                    "category": "Shipped",
                    "label": "Shipped",
                    "cssClass": "ship",
                    "items": {}
                }
            ]
        }
    }
    """;

    [Fact]
    public void FileNotFound_SetsCorrectErrorState()
    {
        var path = Path.Combine(_tempDir, "nonexistent.json");

        var service = new DashboardDataService(path);

        service.HasData.Should().BeFalse();
        service.ErrorType.Should().Be(DataErrorType.FileNotFound);
        service.ErrorMessage.Should().Contain("Dashboard data not found");
        service.ErrorMessage.Should().Contain("data.json exists in the Data/ directory");
        service.ErrorDetails.Should().Be(path);
    }

    [Fact]
    public void MalformedJson_SetsJsonParseError_WithLineInfo()
    {
        var path = WriteTempJson("{ \"project\": invalid }");

        var service = new DashboardDataService(path);

        service.HasData.Should().BeFalse();
        service.ErrorType.Should().Be(DataErrorType.JsonParseError);
        service.ErrorMessage.Should().Contain("Error reading data.json");
        service.ErrorMessage.Should().Contain("fix the JSON syntax and refresh");
        service.ErrorDetails.Should().Contain("Line");
        service.ErrorDetails.Should().Contain("Character");
    }

    [Fact]
    public void MissingRequiredSections_SetsValidationError_ListsFields()
    {
        // Explicit nulls so deserialization succeeds but top-level sections are null
        var path = WriteTempJson("""{"project": null, "timeline": null, "heatmap": null}""");

        var service = new DashboardDataService(path);

        service.HasData.Should().BeFalse();
        service.ErrorType.Should().Be(DataErrorType.ValidationError);
        service.ErrorMessage.Should().Contain("missing required fields");
        service.ErrorMessage.Should().Contain("project");
        service.ErrorMessage.Should().Contain("timeline");
        service.ErrorMessage.Should().Contain("heatmap");
        service.ErrorMessage.Should().Contain("Please check the file format");
    }

    [Fact]
    public void EmptyTimelineTracks_SetsValidationError()
    {
        var path = WriteTempJson(BuildJsonWithEmptyTracks());

        var service = new DashboardDataService(path);

        service.HasData.Should().BeFalse();
        service.ErrorType.Should().Be(DataErrorType.ValidationError);
        service.ErrorDetails.Should().Contain("timeline.tracks must have at least 1 entry");
    }

    [Fact]
    public void ValidJson_LoadsSuccessfully()
    {
        var path = WriteTempJson(BuildValidJson());

        var service = new DashboardDataService(path);

        service.HasData.Should().BeTrue();
        service.ErrorType.Should().Be(DataErrorType.None);
        service.ErrorMessage.Should().BeNull();
        service.ErrorDetails.Should().BeNull();
        service.Data.Should().NotBeNull();
        service.Data!.Project.Should().NotBeNull();
        service.Data.Timeline.Should().NotBeNull();
        service.Data.Heatmap.Should().NotBeNull();
    }
}