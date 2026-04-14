using Xunit;
using ReportingDashboard.Models;

namespace ReportingDashboard.Tests.Unit;

/// <summary>
/// Tests for DashboardHeader data contract validation.
/// These verify the data model properties that the DashboardHeader component consumes.
/// bUnit is not available in this project; component rendering is verified via manual integration testing.
/// </summary>
[Trait("Category", "Unit")]
public class DashboardHeaderComponentTests
{
    private static DashboardData CreateTestData(
        string title = "Test Project Title",
        string subtitle = "Org \u2022 Workstream \u2022 April 2026",
        string backlogUrl = "https://dev.azure.com/org/project")
    {
        return new DashboardData
        {
            Title = title,
            Subtitle = subtitle,
            BacklogUrl = backlogUrl,
            CurrentDate = new DateTime(2026, 4, 14),
            Months = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
            CurrentMonthIndex = 3,
            TimelineStart = new DateTime(2026, 1, 1),
            TimelineEnd = new DateTime(2026, 6, 30),
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = "M1",
                    Label = "M1",
                    Description = "Milestone One",
                    Color = "#0078D4",
                    Markers = new List<MilestoneMarker>
                    {
                        new MilestoneMarker { Date = new DateTime(2026, 2, 15), Type = "checkpoint", Label = "Feb Check" }
                    }
                }
            },
            Categories = new List<HeatmapCategory>
            {
                new HeatmapCategory { Name = "Shipped", Key = "shipped", Items = new Dictionary<string, List<string>>() },
                new HeatmapCategory { Name = "In Progress", Key = "inProgress", Items = new Dictionary<string, List<string>>() },
                new HeatmapCategory { Name = "Carryover", Key = "carryover", Items = new Dictionary<string, List<string>>() },
                new HeatmapCategory { Name = "Blockers", Key = "blockers", Items = new Dictionary<string, List<string>>() }
            }
        };
    }

    [Fact]
    public void DashboardData_Title_IsSetCorrectly()
    {
        var data = CreateTestData(title: "Phoenix Roadmap");
        Assert.Equal("Phoenix Roadmap", data.Title);
    }

    [Fact]
    public void DashboardData_BacklogUrl_IsSetCorrectly()
    {
        var data = CreateTestData(backlogUrl: "https://dev.azure.com/org/project");
        Assert.Equal("https://dev.azure.com/org/project", data.BacklogUrl);
    }

    [Fact]
    public void DashboardData_Subtitle_IsSetCorrectly()
    {
        var data = CreateTestData(subtitle: "Trusted Platform \u2022 Privacy Automation");
        Assert.Contains("Trusted Platform", data.Subtitle);
        Assert.Contains("Privacy Automation", data.Subtitle);
    }

    [Fact]
    public void DashboardData_HandlesEmptyBacklogUrl()
    {
        var data = CreateTestData(backlogUrl: "");
        Assert.Equal(string.Empty, data.BacklogUrl);
        // Title and subtitle remain valid
        Assert.Equal("Test Project Title", data.Title);
        Assert.NotEmpty(data.Subtitle);
    }

    [Fact]
    public void DashboardData_HasRequiredFieldsForHeader()
    {
        var data = CreateTestData();
        Assert.NotNull(data.Title);
        Assert.NotNull(data.Subtitle);
        Assert.NotNull(data.BacklogUrl);
        Assert.NotEmpty(data.Milestones);
        Assert.Equal(4, data.Categories.Count);
    }

    [Fact]
    public void DashboardData_DefaultStrings_AreEmpty()
    {
        var data = new DashboardData();
        Assert.Equal(string.Empty, data.Title);
        Assert.Equal(string.Empty, data.Subtitle);
        Assert.Equal(string.Empty, data.BacklogUrl);
    }
}