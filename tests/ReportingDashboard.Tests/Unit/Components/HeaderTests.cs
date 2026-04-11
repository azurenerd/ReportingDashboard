using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class HeaderTests : TestContext
{
    private static DashboardData CreateTestData(
        string title = "Test Project",
        string subtitle = "Team A · Workstream 1 · April 2026",
        string backlogLink = "https://dev.azure.com/test/backlog",
        string currentMonth = "April",
        TimelineData? timeline = null)
    {
        return new DashboardData
        {
            Title = title,
            Subtitle = subtitle,
            BacklogLink = backlogLink,
            CurrentMonth = currentMonth,
            Timeline = timeline
        };
    }

    // --- Title Rendering ---

    [Fact]
    public void Header_RendersTitle_InH1Element()
    {
        var data = CreateTestData(title: "Executive Dashboard Q2");

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var h1 = cut.Find("h1");
        h1.TextContent.Should().Contain("Executive Dashboard Q2");
    }

    [Fact]
    public void Header_TitleHasCorrectStyling_FontSizeAndWeight()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var h1 = cut.Find("h1");
        var style = h1.GetAttribute("style") ?? "";
        style.Should().Contain("font-size:24px");
        style.Should().Contain("font-weight:700");
        style.Should().Contain("color:#111");
    }

    [Fact]
    public void Header_TitleWithSpecialCharacters_RendersCorrectly()
    {
        var data = CreateTestData(title: "Project <Alpha> & \"Beta\"");

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var h1 = cut.Find("h1");
        h1.TextContent.Should().Contain("Project");
    }

    [Fact]
    public void Header_EmptyTitle_RendersH1WithoutError()
    {
        var data = CreateTestData(title: "");

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var h1 = cut.Find("h1");
        h1.Should().NotBeNull();
    }

    // --- Backlog Link ---

    [Fact]
    public void Header_RendersBacklogLink_WithCorrectHref()
    {
        var data = CreateTestData(backlogLink: "https://dev.azure.com/org/project");

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var link = cut.Find("a");
        link.GetAttribute("href").Should().Be("https://dev.azure.com/org/project");
    }

    [Fact]
    public void Header_BacklogLink_OpensInNewTab()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var link = cut.Find("a");
        link.GetAttribute("target").Should().Be("_blank");
    }

    [Fact]
    public void Header_BacklogLink_HasNoOpenerRel()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var link = cut.Find("a");
        link.GetAttribute("rel").Should().Contain("noopener");
    }

    [Fact]
    public void Header_BacklogLink_StyledInMicrosoftBlue()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var link = cut.Find("a");
        var style = link.GetAttribute("style") ?? "";
        style.Should().Contain("color:#0078D4");
        style.Should().Contain("text-decoration:none");
    }

    [Fact]
    public void Header_BacklogLink_ContainsAdoBacklogText()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var link = cut.Find("a");
        link.TextContent.Should().Contain("ADO Backlog");
    }

    [Fact]
    public void Header_EmptyBacklogLink_DoesNotRenderAnchor()
    {
        var data = CreateTestData(backlogLink: "");

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var links = cut.FindAll("a");
        links.Should().BeEmpty();
    }

    [Fact]
    public void Header_NullBacklogLink_DoesNotRenderAnchor()
    {
        var data = CreateTestData();
        data.BacklogLink = null!;

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var links = cut.FindAll("a");
        links.Should().BeEmpty();
    }

    // --- Subtitle ---

    [Fact]
    public void Header_RendersSubtitle_InSubDiv()
    {
        var data = CreateTestData(subtitle: "Engineering · Core Platform · April 2026");

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var sub = cut.Find("div.sub");
        sub.TextContent.Should().Be("Engineering · Core Platform · April 2026");
    }

    [Fact]
    public void Header_EmptySubtitle_RendersEmptySubDiv()
    {
        var data = CreateTestData(subtitle: "");

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var sub = cut.Find("div.sub");
        sub.TextContent.Should().BeEmpty();
    }

    // --- Root Element CSS ---

    [Fact]
    public void Header_RootDiv_HasHdrClass()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var root = cut.Find("div.hdr");
        root.Should().NotBeNull();
    }

    // --- Legend Section ---

    [Fact]
    public void Header_RendersExactlyFourLegendItems()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        // The legend container is the second child div inside .hdr with gap:22px
        var legendContainer = cut.FindAll("div.hdr > div")[1];
        var legendItems = legendContainer.QuerySelectorAll(":scope > span");
        legendItems.Should().HaveCount(4);
    }

    [Fact]
    public void Header_LegendContains_PoCMilestoneLabel()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("PoC Milestone");
    }

    [Fact]
    public void Header_LegendContains_ProductionReleaseLabel()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("Production Release");
    }

    [Fact]
    public void Header_LegendContains_CheckpointLabel()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("Checkpoint");
    }

    [Fact]
    public void Header_LegendContains_NowLabel()
    {
        var data = CreateTestData(currentMonth: "April");

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("Now (April)");
    }

    // --- Legend Diamond Colors ---

    [Fact]
    public void Header_PoCDiamond_HasGoldBackground()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("background:#F4B400");
    }

    [Fact]
    public void Header_ProductionDiamond_HasGreenBackground()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("background:#34A853");
    }

    [Fact]
    public void Header_PoCDiamond_HasRotate45Transform()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("transform:rotate(45deg)");
    }

    [Fact]
    public void Header_PoCDiamond_Is12x12Pixels()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        // Both PoC and Production diamonds are 12x12
        cut.Markup.Should().Contain("width:12px;height:12px;background:#F4B400");
        cut.Markup.Should().Contain("width:12px;height:12px;background:#34A853");
    }

    // --- Legend Checkpoint ---

    [Fact]
    public void Header_Checkpoint_HasGrayCircle()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("background:#999");
        cut.Markup.Should().Contain("border-radius:50%");
    }

    [Fact]
    public void Header_Checkpoint_Is8x8Pixels()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("width:8px;height:8px");
    }

    // --- Legend Now Bar ---

    [Fact]
    public void Header_NowBar_HasRedBackground()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("background:#EA4335");
    }

    [Fact]
    public void Header_NowBar_Is2x14Pixels()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("width:2px;height:14px;background:#EA4335");
    }

    // --- NowLabel Logic ---

    [Fact]
    public void Header_NowLabel_UsesTimelineNowDate_WhenAvailable()
    {
        var data = CreateTestData(
            currentMonth: "March",
            timeline: new TimelineData { NowDate = "2026-04-15" });

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("Now (April 2026)");
    }

    [Fact]
    public void Header_NowLabel_FallsBackToCurrentMonth_WhenTimelineNull()
    {
        var data = CreateTestData(currentMonth: "June", timeline: null);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("Now (June)");
    }

    [Fact]
    public void Header_NowLabel_FallsBackToCurrentMonth_WhenNowDateEmpty()
    {
        var data = CreateTestData(
            currentMonth: "May",
            timeline: new TimelineData { NowDate = "" });

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("Now (May)");
    }

    [Fact]
    public void Header_NowLabel_ReturnsPlainNow_WhenNoDateInfo()
    {
        var data = CreateTestData(currentMonth: "", timeline: null);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        // Should contain just "Now" without parenthetical
        cut.Markup.Should().Contain(">Now<");
    }

    [Fact]
    public void Header_NowLabel_ReturnsPlainNow_WhenWhitespaceCurrentMonth()
    {
        var data = CreateTestData(currentMonth: "   ", timeline: null);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain(">Now<");
    }

    [Fact]
    public void Header_NowLabel_HandlesInvalidNowDate_FallsBackToCurrentMonth()
    {
        var data = CreateTestData(
            currentMonth: "February",
            timeline: new TimelineData { NowDate = "not-a-date" });

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("Now (February)");
    }

    [Fact]
    public void Header_NowLabel_ParsesFullDateFormat()
    {
        var data = CreateTestData(
            timeline: new TimelineData { NowDate = "2026-12-25" });

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("Now (December 2026)");
    }

    [Fact]
    public void Header_NowLabel_ParsesJanuaryDate()
    {
        var data = CreateTestData(
            timeline: new TimelineData { NowDate = "2027-01-01" });

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("Now (January 2027)");
    }

    // --- Legend Layout ---

    [Fact]
    public void Header_LegendContainer_HasFlexboxWithGap22px()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var legendDiv = cut.FindAll("div.hdr > div")[1];
        var style = legendDiv.GetAttribute("style") ?? "";
        style.Should().Contain("gap:22px");
        style.Should().Contain("display:flex");
    }

    [Fact]
    public void Header_LegendLabels_Have12pxFontSize()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("font-size:12px");
    }

    // --- Full Markup Structure ---

    [Fact]
    public void Header_StructureIsCorrect_TwoMainDivsInsideHdr()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var hdr = cut.Find("div.hdr");
        var children = hdr.Children;
        // First div: title + subtitle; Second div: legend
        children.Length.Should().Be(2);
    }

    [Fact]
    public void Header_BacklogLink_HasCorrectFontSize()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var link = cut.Find("a");
        var style = link.GetAttribute("style") ?? "";
        style.Should().Contain("font-size:13px");
        style.Should().Contain("font-weight:400");
    }

    [Fact]
    public void Header_BacklogLink_HasMarginLeftFromTitle()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var link = cut.Find("a");
        var style = link.GetAttribute("style") ?? "";
        style.Should().Contain("margin-left:12px");
    }
}