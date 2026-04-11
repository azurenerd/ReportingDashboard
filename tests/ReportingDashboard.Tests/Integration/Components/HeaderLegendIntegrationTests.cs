using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class HeaderLegendIntegrationTests : TestContext
{
    private static DashboardData CreateRealisticData(
        string nowDate = "2026-04-10",
        string currentMonth = "Apr",
        string title = "Agent Squad – Executive Status",
        string subtitle = "Platform Engineering · AI Workstream · Apr 2026",
        string backlogLink = "https://dev.azure.com/org/project/_backlogs",
        bool nullTimeline = false)
    {
        return new DashboardData
        {
            Title = title,
            Subtitle = subtitle,
            BacklogLink = backlogLink,
            CurrentMonth = currentMonth,
            Timeline = nullTimeline ? null! : new TimelineData
            {
                NowDate = nowDate
            }
        };
    }

    [Fact]
    public void Header_WithRealisticData_RendersAllSections()
    {
        var data = CreateRealisticData();
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        cut.Find("h1").TextContent.Should().Contain("Agent Squad");
        cut.Find(".sub").TextContent.Should().Contain("Platform Engineering");
        cut.Find("a[target='_blank']").TextContent.Should().Contain("ADO Backlog");

        var legendContainer = cut.Find("div[style*='gap:22px']");
        legendContainer.QuerySelectorAll(":scope > span").Length.Should().Be(4);
        cut.Markup.Should().Contain("Now (April 2026)");
    }

    [Fact]
    public void Header_WithRealisticData_LegendAndTitleCoexistInHdrContainer()
    {
        var data = CreateRealisticData();
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        var hdr = cut.Find(".hdr");
        hdr.Children.Length.Should().Be(2);
    }

    [Fact]
    public void Header_NowLabelReflectsTimelineNowDate_April()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(nowDate: "2026-04-10")));
        cut.Markup.Should().Contain("Now (April 2026)");
    }

    [Fact]
    public void Header_NowLabelReflectsTimelineNowDate_January()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(nowDate: "2026-01-15")));
        cut.Markup.Should().Contain("Now (January 2026)");
    }

    [Fact]
    public void Header_NowLabelReflectsTimelineNowDate_December()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(nowDate: "2025-12-31")));
        cut.Markup.Should().Contain("Now (December 2025)");
    }

    [Fact]
    public void Header_EmptyNowDate_FallsBackToCurrentMonth()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(nowDate: "", currentMonth: "Apr")));
        cut.Markup.Should().Contain("Now (Apr)");
        cut.Markup.Should().NotContain("Now (April");
    }

    [Fact]
    public void Header_InvalidNowDate_FallsBackToCurrentMonth()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(nowDate: "not-a-date", currentMonth: "Mar")));
        cut.Markup.Should().Contain("Now (Mar)");
    }

    [Fact]
    public void Header_NullTimeline_FallsBackToCurrentMonth()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(nullTimeline: true, currentMonth: "Jun")));
        cut.Markup.Should().Contain("Now (Jun)");
    }

    [Fact]
    public void Header_NullTimelineAndEmptyCurrentMonth_ShowsBareNow()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(nullTimeline: true, currentMonth: "")));
        cut.Markup.Should().Contain(">Now<");
    }

    [Fact]
    public void Header_AllFourLegendSymbolColors_PresentInMarkup()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData()));
        cut.Markup.Should().Contain("#F4B400");
        cut.Markup.Should().Contain("#34A853");
        cut.Markup.Should().Contain("#999");
        cut.Markup.Should().Contain("#EA4335");
    }

    [Fact]
    public void Header_AllFourLegendLabels_PresentInMarkup()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData()));
        cut.Markup.Should().Contain("PoC Milestone");
        cut.Markup.Should().Contain("Production Release");
        cut.Markup.Should().Contain("Checkpoint");
        cut.Markup.Should().Contain("Now (April 2026)");
    }

    [Fact]
    public void Header_LegendSymbols_HaveCorrectShapeStyles()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData()));

        var pocSymbol = cut.Find("span[style*='#F4B400']");
        pocSymbol.GetAttribute("style").Should().Contain("rotate(45deg)");
        pocSymbol.GetAttribute("style").Should().Contain("width:12px");

        var prodSymbol = cut.Find("span[style*='#34A853']");
        prodSymbol.GetAttribute("style").Should().Contain("rotate(45deg)");
        prodSymbol.GetAttribute("style").Should().Contain("width:12px");

        var checkSymbol = cut.Find("span[style*='#999']");
        checkSymbol.GetAttribute("style").Should().Contain("border-radius:50%");
        checkSymbol.GetAttribute("style").Should().Contain("width:8px");

        var nowSymbol = cut.Find("span[style*='#EA4335']");
        nowSymbol.GetAttribute("style").Should().Contain("width:2px");
        nowSymbol.GetAttribute("style").Should().Contain("height:14px");
    }

    [Fact]
    public void Header_WithBacklogLink_RendersLinkWithCorrectAttributes()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(backlogLink: "https://dev.azure.com/myorg/myproject/_backlogs")));

        var link = cut.Find("a");
        link.GetAttribute("href").Should().Be("https://dev.azure.com/myorg/myproject/_backlogs");
        link.GetAttribute("target").Should().Be("_blank");
        link.GetAttribute("rel").Should().Contain("noopener");
        link.GetAttribute("style").Should().Contain("#0078D4");
    }

    [Fact]
    public void Header_WithoutBacklogLink_OmitsAnchorElement()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(backlogLink: "")));
        cut.FindAll("a").Count.Should().Be(0);
    }

    [Fact]
    public void Header_ReRendersNowLabel_WhenDataChanges()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(nowDate: "2026-04-10")));
        cut.Markup.Should().Contain("Now (April 2026)");

        cut.SetParametersAndRender(p =>
            p.Add(x => x.Data, CreateRealisticData(nowDate: "2026-09-15")));
        cut.Markup.Should().Contain("Now (September 2026)");
    }

    [Fact]
    public void Header_ReRendersTitle_WhenDataChanges()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(title: "Original Title")));
        cut.Find("h1").TextContent.Should().Contain("Original Title");

        cut.SetParametersAndRender(p =>
            p.Add(x => x.Data, CreateRealisticData(title: "Updated Title")));
        cut.Find("h1").TextContent.Should().Contain("Updated Title");
    }

    [Fact]
    public void Header_TransitionsFromValidDate_ToFallback_OnReRender()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(nowDate: "2026-04-10")));
        cut.Markup.Should().Contain("Now (April 2026)");

        cut.SetParametersAndRender(p =>
            p.Add(x => x.Data, CreateRealisticData(nowDate: "invalid", currentMonth: "Q2")));
        cut.Markup.Should().Contain("Now (Q2)");
    }

    [Fact]
    public void Header_MinimalDashboardData_RendersWithoutCrash()
    {
        var data = new DashboardData
        {
            Title = "Minimal",
            Subtitle = "",
            BacklogLink = "",
            CurrentMonth = "",
            Timeline = new TimelineData { NowDate = "" }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        cut.Find("h1").TextContent.Should().Contain("Minimal");
        cut.Find("div[style*='gap:22px']").Should().NotBeNull();
        cut.Markup.Should().Contain(">Now<");
    }

    [Fact]
    public void Header_VeryLongTitle_RendersWithoutCrash()
    {
        var longTitle = new string('A', 500);
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(title: longTitle)));
        cut.Find("h1").TextContent.Should().Contain(longTitle);
    }

    [Fact]
    public void Header_SpecialCharactersInTitle_RendersEscaped()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(title: "Project <Alpha> & \"Beta\"")));
        cut.Find("h1").TextContent.Should().Contain("Project <Alpha> & \"Beta\"");
    }

    [Fact]
    public void Header_UnicodeInSubtitle_RendersCorrectly()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(subtitle: "チーム · ワークストリーム · 2026年4月")));
        cut.Find(".sub").TextContent.Should().Contain("チーム");
    }

    [Fact]
    public void Header_LegendContainerStyle_MatchesDesignSpec()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData()));
        var legend = cut.Find("div[style*='gap:22px']");
        var style = legend.GetAttribute("style")!;
        style.Should().Contain("display:flex");
        style.Should().Contain("gap:22px");
        style.Should().Contain("align-items:center");
    }

    [Fact]
    public void Header_EachLegendItem_ContainsSymbolAndLabelSpans()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData()));
        var legendContainer = cut.Find("div[style*='gap:22px']");
        var items = legendContainer.QuerySelectorAll(":scope > span");
        foreach (var item in items)
        {
            var childSpans = item.QuerySelectorAll("span");
            childSpans.Length.Should().Be(2, "each legend item should have exactly a symbol span and a label span");
        }
    }

    [Fact]
    public void Header_LegendItemOrder_MatchesDesignSpec()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(nowDate: "2026-04-10")));
        var legendContainer = cut.Find("div[style*='gap:22px']");
        var items = legendContainer.QuerySelectorAll(":scope > span");
        items[0].TextContent.Should().Contain("PoC Milestone");
        items[1].TextContent.Should().Contain("Production Release");
        items[2].TextContent.Should().Contain("Checkpoint");
        items[3].TextContent.Should().Contain("Now (April 2026)");
    }

    [Theory]
    [InlineData("2026-01-01", "Now (January 2026)")]
    [InlineData("2026-02-14", "Now (February 2026)")]
    [InlineData("2026-03-31", "Now (March 2026)")]
    [InlineData("2026-04-10", "Now (April 2026)")]
    [InlineData("2026-05-20", "Now (May 2026)")]
    [InlineData("2026-06-30", "Now (June 2026)")]
    [InlineData("2026-07-04", "Now (July 2026)")]
    [InlineData("2026-08-15", "Now (August 2026)")]
    [InlineData("2026-09-22", "Now (September 2026)")]
    [InlineData("2026-10-31", "Now (October 2026)")]
    [InlineData("2026-11-11", "Now (November 2026)")]
    [InlineData("2026-12-25", "Now (December 2026)")]
    public void Header_NowLabel_FormatsAllMonthsCorrectly(string nowDate, string expectedLabel)
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(nowDate: nowDate)));
        cut.Markup.Should().Contain(expectedLabel);
    }

    [Theory]
    [InlineData("", "Jan", "Now (Jan)")]
    [InlineData("", "Feb", "Now (Feb)")]
    [InlineData("", "Q1", "Now (Q1)")]
    [InlineData("garbage", "Apr", "Now (Apr)")]
    public void Header_NowLabelFallback_UsesCurrentMonthVariants(
        string nowDate, string currentMonth, string expectedLabel)
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateRealisticData(nowDate: nowDate, currentMonth: currentMonth)));
        cut.Markup.Should().Contain(expectedLabel);
    }
}