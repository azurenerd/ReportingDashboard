using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Tests.Models;

public class ProjectInfoTests
{
    [Fact]
    public void Deserialize_ValidJson_ReturnsProjectInfo()
    {
        var json = """
        {
            "projectName": "Project Atlas",
            "executiveSponsor": "Sarah Chen",
            "reportingPeriod": "Q1 2026",
            "overallStatus": "OnTrack",
            "summary": "On track for delivery."
        }
        """;

        var result = JsonSerializer.Deserialize<ProjectInfo>(json);

        Assert.NotNull(result);
        Assert.Equal("Project Atlas", result.ProjectName);
        Assert.Equal("Sarah Chen", result.ExecutiveSponsor);
        Assert.Equal("Q1 2026", result.ReportingPeriod);
        Assert.Equal("OnTrack", result.OverallStatus);
        Assert.Equal("On track for delivery.", result.Summary);
    }

    [Fact]
    public void Defaults_AreApplied_WhenPropertiesMissing()
    {
        var result = JsonSerializer.Deserialize<ProjectInfo>("{}");

        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.ProjectName);
        Assert.Equal("OnTrack", result.OverallStatus);
    }
}