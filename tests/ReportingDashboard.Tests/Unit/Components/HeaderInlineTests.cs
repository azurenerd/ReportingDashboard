using Bunit;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for Components/Header.razor (inline-styled version from PR #533).
/// The existing HeaderTests.cs covers Components/Sections/Header.razor which uses CSS classes.
/// This file covers the root Components/Header.razor that uses inline styles for legend items.
/// </summary>
[Trait("Category", "Unit")]
public class HeaderInlineTests : TestContext
{
    private static DashboardData CreateData(
        string title = "Test Project",
        string subtitle = "Team A - March 2026",
        string backlogLink = "https://ado.example.com/backlog",
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

    #region Title Rendering

    [Fact]
    public void Header_RendersTitle_InH1Element()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(title: "Executive Dashboard")));

        var h1 = cut.Find("h1");
        Assert.Contains("Executive Dashboard", h1.TextContent);
    }

    [Fact]
    public void Header_RendersLongTitle_WithoutBreaking()
    {
        var longTitle = new string('A', 100);
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(title: longTitle)));

        var h1 = cut.Find("h1");
        Assert.Contains(longTitle, h1.TextContent);
    }

    [Fact]
    public void Header_RendersTitle_WithSpecialCharacters()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(title: "Project <Alpha> & \"Beta\"")));

        var h1 = cut.Find("h1");
        Assert.Contains("Project", h1.TextContent);
        Assert.Contains("Alpha", h1.TextContent);
        Assert.Contains("Beta", h1.TextContent);
    }

    [Fact]
    public void Header_RendersEmptyTitle_WhenTitleIsEmpty()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(title: "")));

        var h1 = cut.Find("h1");
        Assert.NotNull(h1);
    }

    #endregion

    #region Subtitle Rendering

    [Fact]
    public void Header_RendersSubtitle_InSubDiv()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(subtitle: "Platform Team - April 2026")));

        var sub = cut.Find(".sub");
        Assert.Equal("Platform Team - April 2026", sub.TextContent);
    }

    [Fact]
    public void Header_RendersEmptySubtitle_WhenSubtitleIsEmpty()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(subtitle: "")));

        var sub = cut.Find(".sub");
        Assert.Equal("", sub.TextContent.Trim());
    }

    [Fact]
    public void Header_RendersSubtitle_WithUnicodeCharacters()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(subtitle: "Équipe développement — Avril 2026")));

        var sub = cut.Find(".sub");
        Assert.Contains("Équipe développement", sub.TextContent);
    }

    #endregion

    #region Backlog Link Rendering

    [Fact]
    public void Header_RendersBacklogLink_WithCorrectHref()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(backlogLink: "https://dev.azure.com/org/project/_backlogs")));

        var link = cut.Find("a");
        Assert.Equal("https://dev.azure.com/org/project/_backlogs", link.GetAttribute("href"));
    }

    [Fact]
    public void Header_BacklogLink_OpensInNewTab()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var link = cut.Find("a");
        Assert.Equal("_blank", link.GetAttribute("target"));
    }

    [Fact]
    public void Header_BacklogLink_HasNoopenerNoreferrer()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var link = cut.Find("a");
        var rel = link.GetAttribute("rel");
        Assert.Contains("noopener", rel ?? "");
    }

    [Fact]
    public void Header_BacklogLink_ContainsADOBacklogText()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var link = cut.Find("a");
        Assert.Contains("ADO Backlog", link.TextContent);
    }

    [Fact]
    public void Header_BacklogLink_RendersWithUrlEncodedCharacters()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(backlogLink: "https://ado.example.com/path?query=value&other=123")));

        var link = cut.Find("a");
        Assert.Contains("ado.example.com", link.GetAttribute("href") ?? "");
    }

    #endregion

    #region Header Container Structure

    [Fact]
    public void Header_HasHdrCssClass()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var hdr = cut.Find(".hdr");
        Assert.NotNull(hdr);
    }

    [Fact]
    public void Header_ContainsH1Element()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.NotNull(cut.Find("h1"));
    }

    [Fact]
    public void Header_ContainsSubDiv()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.NotNull(cut.Find(".sub"));
    }

    [Fact]
    public void Header_H1ContainsAnchorTag()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var link = cut.Find("h1 a");
        Assert.NotNull(link);
    }

    #endregion

    #region Legend Rendering

    [Fact]
    public void Header_RendersLegendContainer_WithFlexboxStyle()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("display:flex", cut.Markup);
        Assert.Contains("gap:22px", cut.Markup);
    }

    [Fact]
    public void Header_LegendContains_PocMilestoneLabel()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("PoC Milestone", cut.Markup);
    }

    [Fact]
    public void Header_LegendContains_ProductionReleaseLabel()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("Production Release", cut.Markup);
    }

    [Fact]
    public void Header_LegendContains_CheckpointLabel()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("Checkpoint", cut.Markup);
    }

    [Fact]
    public void Header_LegendNow_ContainsCurrentMonth()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(currentMonth: "June")));

        Assert.Contains("Now (June)", cut.Markup);
    }

    [Fact]
    public void Header_LegendNow_ReflectsDifferentMonths()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(currentMonth: "December")));

        Assert.Contains("Now (December)", cut.Markup);
    }

    [Fact]
    public void Header_LegendNow_HandlesEmptyCurrentMonth()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(currentMonth: "")));

        Assert.Contains("Now ()", cut.Markup);
    }

    #endregion

    #region Legend Symbol Colors

    [Fact]
    public void Header_LegendPocDiamond_HasGoldColor()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("#F4B400", cut.Markup);
    }

    [Fact]
    public void Header_LegendProductionDiamond_HasGreenColor()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("#34A853", cut.Markup);
    }

    [Fact]
    public void Header_LegendCheckpointCircle_HasGrayColor()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("#999", cut.Markup);
    }

    [Fact]
    public void Header_LegendNowBar_HasRedColor()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("#EA4335", cut.Markup);
    }

    #endregion

    #region Legend Symbol Shapes

    [Fact]
    public void Header_LegendDiamonds_HaveRotateTransform()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("rotate(45deg)", cut.Markup);
    }

    [Fact]
    public void Header_LegendCheckpoint_HasBorderRadius50Percent()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("border-radius:50%", cut.Markup);
    }

    [Fact]
    public void Header_LegendNowBar_Has2pxWidth()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("width:2px", cut.Markup);
        Assert.Contains("height:14px", cut.Markup);
    }

    [Fact]
    public void Header_LegendDiamonds_Are12x12()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("width:12px;height:12px", cut.Markup);
    }

    [Fact]
    public void Header_LegendCheckpoint_Is8x8()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("width:8px;height:8px", cut.Markup);
    }

    #endregion

    #region Legend Font Size

    [Fact]
    public void Header_LegendContainer_Has12pxFontSize()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("font-size:12px", cut.Markup);
    }

    #endregion

    #region Data Binding Verification

    [Fact]
    public void Header_AllFieldsBound_WhenDataProvided()
    {
        var data = CreateData(
            title: "Alpha Dashboard",
            subtitle: "DevOps Team - May 2026",
            backlogLink: "https://ado.example.com/alpha",
            currentMonth: "May");

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Alpha Dashboard", cut.Markup);
        Assert.Contains("DevOps Team - May 2026", cut.Markup);
        Assert.Contains("https://ado.example.com/alpha", cut.Markup);
        Assert.Contains("Now (May)", cut.Markup);
    }

    [Fact]
    public void Header_ReRendersCorrectly_WithDifferentData()
    {
        var data1 = CreateData(title: "Dashboard V1", currentMonth: "January");
        var data2 = CreateData(title: "Dashboard V2", currentMonth: "February");

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data1));
        Assert.Contains("Dashboard V1", cut.Markup);
        Assert.Contains("Now (January)", cut.Markup);

        cut.SetParametersAndRender(p =>
            p.Add(x => x.Data, data2));
        Assert.Contains("Dashboard V2", cut.Markup);
        Assert.Contains("Now (February)", cut.Markup);
    }

    #endregion

    #region Four Legend Items Present

    [Fact]
    public void Header_HasExactlyFourLegendLabels()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        var markup = cut.Markup;
        Assert.Contains("PoC Milestone", markup);
        Assert.Contains("Production Release", markup);
        Assert.Contains("Checkpoint", markup);
        Assert.Contains("Now (", markup);
    }

    #endregion
}