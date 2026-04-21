using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Web.Components;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class ComponentRenderTests : TestContext
{
    [Fact]
    public void DashboardHeader_RendersTitle_Subtitle_BacklogLink_And_Legend()
    {
        var cut = RenderComponent<DashboardHeader>(p => p
            .Add(x => x.Title, "My Project")
            .Add(x => x.Subtitle, "Workstream Alpha")
            .Add(x => x.BacklogUrl, "https://dev.azure.com/backlog")
            .Add(x => x.CurrentDate, "2026-04-10"));

        cut.Markup.Should().Contain("My Project");
        cut.Markup.Should().Contain("Workstream Alpha");
        cut.Find("a[href='https://dev.azure.com/backlog']").Should().NotBeNull();
        cut.FindAll(".legend-item").Count.Should().Be(4);
        cut.Markup.Should().Contain("PoC Milestone");
        cut.Markup.Should().Contain("Production Release");
    }

    [Fact]
    public void HeatmapCell_WithItems_RendersBulletItems()
    {
        var items = new List<string> { "Item A", "Item B", "Item C" };

        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, items)
            .Add(x => x.ColorClass, "ship")
            .Add(x => x.IsCurrentMonth, false));

        cut.FindAll(".it").Count.Should().Be(3);
        cut.Markup.Should().Contain("Item A");
        cut.Markup.Should().Contain("Item B");
        cut.Markup.Should().Contain("Item C");
    }

    [Fact]
    public void HeatmapCell_Empty_RendersDash()
    {
        var cut = RenderComponent<HeatmapCell>(p => p
            .Add(x => x.Items, new List<string>())
            .Add(x => x.ColorClass, "ship")
            .Add(x => x.IsCurrentMonth, false));

        cut.Find(".empty-dash").TextContent.Should().Be("-");
    }

    [Fact]
    public void HeatmapGrid_RendersCorrectColumnHeaders()
    {
        var heatmap = new HeatmapData
        {
            Months = new List<string> { "Jan", "Feb", "Mar" },
            CurrentMonth = "Feb",
            Categories = new List<HeatmapCategory>
            {
                new() { Name = "Shipped", ColorClass = "ship", Items = new Dictionary<string, List<string>> { ["Jan"] = new() { "Done1" } } },
                new() { Name = "In Progress", ColorClass = "prog", Items = new Dictionary<string, List<string>>() },
                new() { Name = "Carryover", ColorClass = "carry", Items = new Dictionary<string, List<string>>() },
                new() { Name = "Blockers", ColorClass = "block", Items = new Dictionary<string, List<string>>() }
            }
        };

        var cut = RenderComponent<HeatmapGrid>(p => p
            .Add(x => x.Heatmap, heatmap));

        cut.FindAll(".hm-col-hdr").Count.Should().Be(3);
        cut.Find(".current-month-hdr").TextContent.Should().Contain("Feb");
        cut.Markup.Should().Contain("SHIPPED");
    }

    [Fact]
    public void Dashboard_ShowsError_WhenServiceReturnsError()
    {
        Services.AddSingleton<IDataService>(new FakeErrorService("File not found error"));

        var cut = RenderComponent<ReportingDashboard.Web.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("File not found error");
    }

    [Fact]
    public void Dashboard_RendersData()
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

        cut.Markup.Should().Contain("Test Dashboard");
    }

    private class FakeDataService : IDataService
    {
        private readonly DashboardData _data;
        public FakeDataService(DashboardData data) => _data = data;
        public DashboardData? GetData() => _data;
        public string? GetError() => null;
    }

    private class FakeErrorService : IDataService
    {
        private readonly string _error;
        public FakeErrorService(string error) => _error = error;
        public DashboardData? GetData() => null;
        public string? GetError() => _error;
    }
}