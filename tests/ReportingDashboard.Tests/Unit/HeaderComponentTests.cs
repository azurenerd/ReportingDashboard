using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using ReportingDashboard.Components.Shared;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class HeaderComponentTests : TestContext
{
    private static DashboardData CreateTestData()
    {
        return new DashboardData
        {
            Title = "Test Dashboard",
            Subtitle = "Test Team \u2022 Test Workstream \u2022 April 2026",
            BacklogUrl = "https://dev.azure.com/test",
            Timeline = new TimelineConfig
            {
                StartMonth = "2026-01",
                EndMonth = "2026-06",
                Tracks = new List<Track>()
            },
            Heatmap = new HeatmapConfig
            {
                Months = new List<string>(),
                CurrentMonth = "April",
                Categories = new List<HeatmapCategory>()
            }
        };
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Header_RendersProjectTitle()
    {
        var data = CreateTestData();

        var cut = RenderComponent<Header>(p => p
            .Add(x => x.DashboardData, data));

        var markup = cut.Markup;
        markup.Should().Contain("Test Dashboard");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Header_RendersSubtitle()
    {
        var data = CreateTestData();

        var cut = RenderComponent<Header>(p => p
            .Add(x => x.DashboardData, data));

        var markup = cut.Markup;
        markup.Should().Contain("Test Team");
        markup.Should().Contain("April 2026");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Header_RendersBacklogLink()
    {
        var data = CreateTestData();

        var cut = RenderComponent<Header>(p => p
            .Add(x => x.DashboardData, data));

        var markup = cut.Markup;
        markup.Should().Contain("https://dev.azure.com/test");
        markup.Should().Contain("ADO Backlog");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Header_RendersFourLegendItems_InCorrectOrder()
    {
        var data = CreateTestData();

        var cut = RenderComponent<Header>(p => p
            .Add(x => x.DashboardData, data));

        var markup = cut.Markup;

        // Verify legend text content is present in the rendered markup
        markup.Should().Contain("PoC Milestone");
        markup.Should().Contain("Production Release");
        markup.Should().Contain("Checkpoint");
        markup.Should().Contain("Now");

        // Verify correct order: PoC before Production before Checkpoint before Now
        var pocIndex = markup.IndexOf("PoC Milestone");
        var prodIndex = markup.IndexOf("Production Release");
        var cpIndex = markup.IndexOf("Checkpoint");
        var nowIndex = markup.IndexOf("Now", cpIndex);

        pocIndex.Should().BeLessThan(prodIndex);
        prodIndex.Should().BeLessThan(cpIndex);
        cpIndex.Should().BeLessThan(nowIndex);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Header_RendersHdrClass()
    {
        var data = CreateTestData();

        var cut = RenderComponent<Header>(p => p
            .Add(x => x.DashboardData, data));

        cut.Find("div.hdr").Should().NotBeNull();
    }
}