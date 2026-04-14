using Bunit;
using Xunit;
using ReportingDashboard.Models;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardHeaderComponentTests : TestContext
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
    public void DashboardHeader_RendersTitle_FromData()
    {
        var data = CreateTestData(title: "Phoenix Roadmap");

        var cut = RenderComponent<ReportingDashboard.Components.DashboardHeader>(
            p => p.Add(x => x.Data, data));

        var h1 = cut.Find("h1");
        Assert.Contains("Phoenix Roadmap", h1.TextContent);
    }

    [Fact]
    public void DashboardHeader_RendersBacklogLink_WithCorrectHref()
    {
        var data = CreateTestData(backlogUrl: "https://dev.azure.com/org/project");

        var cut = RenderComponent<ReportingDashboard.Components.DashboardHeader>(
            p => p.Add(x => x.Data, data));

        var link = cut.Find("h1 a");
        Assert.Equal("https://dev.azure.com/org/project", link.GetAttribute("href"));
        Assert.Equal("_blank", link.GetAttribute("target"));
        Assert.Contains("noopener", link.GetAttribute("rel")!);
        Assert.Contains("ADO Backlog", link.TextContent);
    }

    [Fact]
    public void DashboardHeader_RendersSubtitle_FromData()
    {
        var data = CreateTestData(subtitle: "Trusted Platform \u2022 Privacy Automation");

        var cut = RenderComponent<ReportingDashboard.Components.DashboardHeader>(
            p => p.Add(x => x.Data, data));

        var sub = cut.Find(".sub");
        Assert.Contains("Trusted Platform", sub.TextContent);
        Assert.Contains("Privacy Automation", sub.TextContent);
    }

    [Fact]
    public void DashboardHeader_RendersAllFourLegendItems()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.DashboardHeader>(
            p => p.Add(x => x.Data, data));

        var legendItems = cut.FindAll(".leg-item");
        Assert.Equal(4, legendItems.Count);
        Assert.Contains("PoC Milestone", legendItems[0].TextContent);
        Assert.Contains("Production Release", legendItems[1].TextContent);
        Assert.Contains("Checkpoint", legendItems[2].TextContent);
        Assert.Contains("Now", legendItems[3].TextContent);
    }

    [Fact]
    public void DashboardHeader_HandlesEmptyBacklogUrl()
    {
        var data = CreateTestData(backlogUrl: "");

        var cut = RenderComponent<ReportingDashboard.Components.DashboardHeader>(
            p => p.Add(x => x.Data, data));

        // Component renders without exception
        var link = cut.Find("h1 a");
        Assert.Equal("", link.GetAttribute("href"));
        Assert.Contains("ADO Backlog", link.TextContent);
        // Header structure is still intact
        Assert.NotNull(cut.Find("header.hdr"));
    }
}