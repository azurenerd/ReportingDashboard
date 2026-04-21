using Bunit;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Components;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Tests.Components;

public class DashboardComponentTests : TestContext
{
    [Fact]
    public void DashboardHeader_RendersTitle()
    {
        var cut = RenderComponent<DashboardHeader>(parameters => parameters
            .Add(p => p.Title, "My Project")
            .Add(p => p.Subtitle, "Subtitle text")
            .Add(p => p.BacklogUrl, "https://example.com")
            .Add(p => p.CurrentDate, "2026-04-10"));

        var title = cut.Find("h1");
        Assert.Contains("My Project", title.TextContent);
    }

    [Fact]
    public void DashboardHeader_RendersBacklogLink()
    {
        var cut = RenderComponent<DashboardHeader>(parameters => parameters
            .Add(p => p.Title, "Project")
            .Add(p => p.Subtitle, "")
            .Add(p => p.BacklogUrl, "https://example.com/backlog")
            .Add(p => p.CurrentDate, "2026-04-10"));

        var link = cut.Find("h1 a");
        Assert.Equal("https://example.com/backlog", link.GetAttribute("href"));
    }

    [Fact]
    public void DashboardHeader_RendersLegend_WithMilestoneTypes()
    {
        var cut = RenderComponent<DashboardHeader>(parameters => parameters
            .Add(p => p.Title, "Project")
            .Add(p => p.Subtitle, "")
            .Add(p => p.BacklogUrl, "")
            .Add(p => p.CurrentDate, "2026-04-10"));

        var legendItems = cut.FindAll(".legend-item");
        Assert.Equal(4, legendItems.Count);
        Assert.Contains("PoC Milestone", cut.Markup);
        Assert.Contains("Production Release", cut.Markup);
        Assert.Contains("Checkpoint", cut.Markup);
    }

    [Fact]
    public void HeatmapCell_WithItems_RendersBulletItems()
    {
        var cut = RenderComponent<HeatmapCell>(parameters => parameters
            .Add(p => p.Items, new List<string> { "Item A", "Item B" })
            .Add(p => p.ColorClass, "ship")
            .Add(p => p.IsCurrentMonth, false));

        var items = cut.FindAll(".it");
        Assert.Equal(2, items.Count);
        Assert.Equal("Item A", items[0].TextContent);
    }

    [Fact]
    public void HeatmapCell_EmptyItems_RendersDash()
    {
        var cut = RenderComponent<HeatmapCell>(parameters => parameters
            .Add(p => p.Items, new List<string>())
            .Add(p => p.ColorClass, "ship")
            .Add(p => p.IsCurrentMonth, false));

        var dash = cut.Find(".empty-dash");
        Assert.Equal("-", dash.TextContent);
    }

    [Fact]
    public void HeatmapCell_CurrentMonth_HasClass()
    {
        var cut = RenderComponent<HeatmapCell>(parameters => parameters
            .Add(p => p.Items, new List<string> { "X" })
            .Add(p => p.ColorClass, "ship")
            .Add(p => p.IsCurrentMonth, true));

        Assert.Contains("current-month", cut.Find(".hm-cell").ClassList);
    }

    [Fact]
    public void HeatmapGrid_RendersRowHeaders()
    {
        var heatmap = new HeatmapData
        {
            Months = new List<string> { "Jan" },
            CurrentMonth = "Jan",
            Categories = new List<HeatmapCategory>
            {
                new() { Name = "Shipped", ColorClass = "ship", Items = new Dictionary<string, List<string>> { ["Jan"] = new() { "A", "B" } } },
                new() { Name = "In Progress", ColorClass = "prog", Items = new Dictionary<string, List<string>>() },
                new() { Name = "Carryover", ColorClass = "carry", Items = new Dictionary<string, List<string>>() },
                new() { Name = "Blockers", ColorClass = "block", Items = new Dictionary<string, List<string>>() }
            }
        };

        var cut = RenderComponent<HeatmapGrid>(parameters => parameters
            .Add(p => p.Heatmap, heatmap));

        var headers = cut.FindAll(".hm-row-hdr");
        Assert.Equal(4, headers.Count);
        Assert.Contains("SHIPPED (2)", headers[0].TextContent);
    }

    [Fact]
    public void Dashboard_ShowsError_WhenDataMissing()
    {
        Services.AddSingleton<IDataService>(new FakeErrorDataService("Test error message"));

        var cut = RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        Assert.Contains("Test error message", cut.Markup);
    }

    [Fact]
    public void Dashboard_RendersData_WhenAvailable()
    {
        Services.AddSingleton<IDataService>(new FakeDataService(new DashboardData
        {
            Title = "Test Dashboard",
            Subtitle = "Sub",
            BacklogUrl = "",
            CurrentDate = "2026-04-10",
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-06-30",
                Tracks = new List<TimelineTrack>()
            },
            Heatmap = new HeatmapData
            {
                Months = new List<string> { "Apr" },
                CurrentMonth = "Apr",
                Categories = new List<HeatmapCategory>()
            }
        }));

        var cut = RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        Assert.Contains("Test Dashboard", cut.Markup);
    }

    private class FakeDataService : IDataService
    {
        private readonly DashboardData? _data;
        public FakeDataService(DashboardData data) => _data = data;
        public DashboardData? GetData() => _data;
        public string? GetError() => null;
    }

    private class FakeErrorDataService : IDataService
    {
        private readonly string _error;
        public FakeErrorDataService(string error) => _error = error;
        public DashboardData? GetData() => null;
        public string? GetError() => _error;
    }
}