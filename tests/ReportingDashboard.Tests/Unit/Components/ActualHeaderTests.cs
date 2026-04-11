using Bunit;
using ReportingDashboard.Components;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for the actual Header.razor component at Components/ root,
/// which uses inline styles for legend (no .legend CSS class).
/// </summary>
[Trait("Category", "Unit")]
public class ActualHeaderTests : TestContext
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
        Timeline = new TimelineData(),
        Heatmap = new HeatmapData()
    };

    [Fact]
    public void Header_RendersHdrCssClass()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var hdr = cut.Find(".hdr");
        Assert.NotNull(hdr);
    }

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
    public void Header_RendersBacklogLink()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData(backlogLink: "https://ado.example.com/backlog")));

        var link = cut.Find("a");
        Assert.Equal("https://ado.example.com/backlog", link.GetAttribute("href"));
        Assert.Equal("_blank", link.GetAttribute("target"));
        Assert.Contains("ADO Backlog", link.TextContent);
    }

    [Fact]
    public void Header_BacklogLink_HasNoopenerNoreferrer()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData(backlogLink: "https://link")));

        var link = cut.Find("a");
        var rel = link.GetAttribute("rel");
        Assert.Contains("noopener", rel ?? "");
        Assert.Contains("noreferrer", rel ?? "");
    }

    [Fact]
    public void Header_AlwaysRendersBacklogLink()
    {
        // The actual Header.razor always renders the <a> tag regardless of backlogLink value
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData(backlogLink: "")));

        var links = cut.FindAll("a");
        Assert.Single(links);
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

        Assert.Contains("Now (June)", cut.Markup);
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
    public void Header_LegendHasCheckpointCircleColor()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("#999", cut.Markup);
    }

    [Fact]
    public void Header_LegendHasNowBarColor()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("#EA4335", cut.Markup);
    }

    [Fact]
    public void Header_LegendDiamondsHaveRotate45Transform()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("rotate(45deg)", cut.Markup);
    }

    [Fact]
    public void Header_LegendCheckpointHasBorderRadius50()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("border-radius:50%", cut.Markup);
    }

    [Fact]
    public void Header_LegendHas22pxGap()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("gap:22px", cut.Markup);
    }

    [Fact]
    public void Header_LegendHas12pxFontSize()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("font-size:12px", cut.Markup);
    }

    [Fact]
    public void Header_EmptyTitle_RendersWithoutCrash()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData(title: "")));

        Assert.Contains("hdr", cut.Markup);
    }

    [Fact]
    public void Header_SpecialCharactersInTitle_AreEncoded()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData(title: "Test <b>Bold</b>")));

        Assert.DoesNotContain("<b>", cut.Markup);
        Assert.Contains("&lt;b&gt;", cut.Markup);
    }

    [Fact]
    public void Header_EmptyCurrentMonth_RendersNowWithParens()
    {
        var cut = RenderComponent<Header>(p =>
            p.Add(x => x.Data, CreateData(currentMonth: "")));

        Assert.Contains("Now ()", cut.Markup);
    }
}