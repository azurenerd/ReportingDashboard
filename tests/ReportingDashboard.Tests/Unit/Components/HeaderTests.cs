using Bunit;
using ReportingDashboard.Components;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class HeaderTests : TestContext
{
    private static DashboardData CreateData(
        string title = "My Project",
        string subtitle = "Team A - March 2026",
        string backlogLink = "https://ado.example.com",
        string currentMonth = "March") => new()
    {
        Title = title,
        Subtitle = subtitle,
        BacklogLink = backlogLink,
        CurrentMonth = currentMonth,
        Months = new List<string> { "January", "February", "March" },
        Timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-03-15",
            Tracks = new List<TimelineTrack>()
        },
        Heatmap = new HeatmapData()
    };

    [Fact]
    public void Header_RendersTitle()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData(title: "Executive Dashboard")));

        var h1 = cut.Find("h1");
        Assert.Contains("Executive Dashboard", h1.TextContent);
    }

    [Fact]
    public void Header_RendersSubtitle()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData(subtitle: "Platform Team - April 2026")));

        var sub = cut.Find(".sub");
        Assert.Equal("Platform Team - April 2026", sub.TextContent);
    }

    [Fact]
    public void Header_RendersBacklogLink_WhenProvided()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData(backlogLink: "https://ado.example.com/backlog")));

        var link = cut.Find("a");
        Assert.Equal("https://ado.example.com/backlog", link.GetAttribute("href"));
        Assert.Equal("_blank", link.GetAttribute("target"));
        Assert.Contains("ADO Backlog", link.TextContent);
    }

    [Fact]
    public void Header_HidesBacklogLink_WhenEmpty()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData(backlogLink: "")));

        var links = cut.FindAll("a");
        Assert.Empty(links);
    }

    [Fact]
    public void Header_HidesBacklogLink_WhenNull()
    {
        var data = CreateData();
        data.BacklogLink = null!;

        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, data));

        var links = cut.FindAll("a");
        Assert.Empty(links);
    }

    [Fact]
    public void Header_HasHdrTitleClass()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var hdrTitle = cut.Find(".hdr-title");
        Assert.NotNull(hdrTitle);
    }

    [Fact]
    public void Header_RendersLegendItems()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("PoC Milestone", cut.Markup);
        Assert.Contains("Production Release", cut.Markup);
        Assert.Contains("Checkpoint", cut.Markup);
    }

    [Fact]
    public void Header_LegendContainsPocMilestone()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("PoC Milestone", cut.Markup);
    }

    [Fact]
    public void Header_LegendContainsProductionRelease()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("Production Release", cut.Markup);
    }

    [Fact]
    public void Header_LegendContainsCheckpoint()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("Checkpoint", cut.Markup);
    }

    [Fact]
    public void Header_LegendNowContainsCurrentMonth()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData(currentMonth: "June")));

        // NowLabel includes month and year parsed from NowDate
        Assert.Contains("Now (June", cut.Markup);
    }

    [Fact]
    public void Header_LegendHasPocDiamondColor()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("#F4B400", cut.Markup);
    }

    [Fact]
    public void Header_LegendHasProductionDiamondColor()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("#34A853", cut.Markup);
    }

    [Fact]
    public void Header_LegendHasNowBarColor()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("#EA4335", cut.Markup);
    }

    [Fact]
    public void Header_BacklogLink_HasNoopener()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData(backlogLink: "https://link")));

        var link = cut.Find("a");
        var rel = link.GetAttribute("rel");
        Assert.Contains("noopener", rel ?? "");
    }
}