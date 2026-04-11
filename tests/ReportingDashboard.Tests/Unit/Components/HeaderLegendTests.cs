using Bunit;
using ReportingDashboard.Models;
using Xunit;
using FluentAssertions;
using AngleSharp.Dom;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for the inline-styled Header.razor component (Components/Header.razor)
/// focusing on the timeline legend added in PR #534.
/// The existing HeaderTests.cs targets Components/Sections/Header.razor (CSS-class variant).
/// </summary>
[Trait("Category", "Unit")]
public class HeaderLegendTests : TestContext
{
    private static DashboardData CreateData(
        string title = "Test Dashboard",
        string subtitle = "Team A - April 2026",
        string backlogLink = "https://ado.example.com/backlog",
        string currentMonth = "Apr 2026") => new()
    {
        Title = title,
        Subtitle = subtitle,
        BacklogLink = backlogLink,
        CurrentMonth = currentMonth,
        Months = new List<string> { "January", "February", "March", "April" },
        Timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>()
        },
        Heatmap = new HeatmapData()
    };

    private IRenderedComponent<ReportingDashboard.Components.Header> RenderHeader(DashboardData? data = null)
    {
        return RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data ?? CreateData()));
    }

    #region Header Structure

    [Fact]
    public void Header_RendersHdrDiv()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        hdr.Should().NotBeNull();
    }

    [Fact]
    public void Header_ContainsTwoDirectChildDivs()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var childDivs = hdr.Children.Where(c => c.TagName == "DIV").ToList();
        childDivs.Should().HaveCount(2, "header should have title div and legend div");
    }

    [Fact]
    public void Header_RendersTitle()
    {
        var cut = RenderHeader(CreateData(title: "Executive Report"));

        var h1 = cut.Find("h1");
        h1.TextContent.Should().Contain("Executive Report");
    }

    [Fact]
    public void Header_RendersSubtitle()
    {
        var cut = RenderHeader(CreateData(subtitle: "Platform Team - Q2 2026"));

        var sub = cut.Find(".sub");
        sub.TextContent.Should().Be("Platform Team - Q2 2026");
    }

    [Fact]
    public void Header_RendersBacklogLink_WithCorrectHref()
    {
        var cut = RenderHeader(CreateData(backlogLink: "https://dev.azure.com/myorg"));

        var link = cut.Find("a");
        link.GetAttribute("href").Should().Be("https://dev.azure.com/myorg");
    }

    [Fact]
    public void Header_BacklogLink_OpensInNewTab()
    {
        var cut = RenderHeader();

        var link = cut.Find("a");
        link.GetAttribute("target").Should().Be("_blank");
    }

    [Fact]
    public void Header_BacklogLink_HasNoopenerRel()
    {
        var cut = RenderHeader();

        var link = cut.Find("a");
        link.GetAttribute("rel").Should().Contain("noopener");
        link.GetAttribute("rel").Should().Contain("noreferrer");
    }

    [Fact]
    public void Header_BacklogLink_ContainsADOBacklogText()
    {
        var cut = RenderHeader();

        var link = cut.Find("a");
        link.TextContent.Should().Contain("ADO Backlog");
    }

    #endregion

    #region Legend Container

    [Fact]
    public void Legend_HasFlexDisplay()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var style = legendDiv.GetAttribute("style") ?? "";
        style.Should().Contain("display:flex", "legend container should use flexbox");
    }

    [Fact]
    public void Legend_Has22pxGap()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var style = legendDiv.GetAttribute("style") ?? "";
        style.Should().Contain("gap:22px");
    }

    [Fact]
    public void Legend_HasCenterAlignment()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var style = legendDiv.GetAttribute("style") ?? "";
        style.Should().Contain("align-items:center");
    }

    [Fact]
    public void Legend_Has12pxFontSize()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var style = legendDiv.GetAttribute("style") ?? "";
        style.Should().Contain("font-size:12px");
    }

    [Fact]
    public void Legend_ContainsFourItems()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var items = legendDiv.Children.Where(c => c.TagName == "SPAN").ToList();
        items.Should().HaveCount(4, "legend should have PoC, Production, Checkpoint, and Now items");
    }

    #endregion

    #region PoC Milestone Legend Item

    [Fact]
    public void Legend_PocMilestone_HasLabel()
    {
        var cut = RenderHeader();

        cut.Markup.Should().Contain("PoC Milestone");
    }

    [Fact]
    public void Legend_PocMilestone_DiamondHasGoldColor()
    {
        var cut = RenderHeader();

        // Find the first legend item's inner span (diamond symbol)
        var markup = cut.Markup;
        markup.Should().Contain("background:#F4B400");
    }

    [Fact]
    public void Legend_PocMilestone_DiamondIs12x12px()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var firstItem = legendDiv.Children.First();
        var diamond = firstItem.Children.First();
        var style = diamond.GetAttribute("style") ?? "";
        style.Should().Contain("width:12px");
        style.Should().Contain("height:12px");
    }

    [Fact]
    public void Legend_PocMilestone_DiamondIsRotated45Deg()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var firstItem = legendDiv.Children.First();
        var diamond = firstItem.Children.First();
        var style = diamond.GetAttribute("style") ?? "";
        style.Should().Contain("rotate(45deg)");
    }

    [Fact]
    public void Legend_PocMilestone_DiamondIsInlineBlock()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var firstItem = legendDiv.Children.First();
        var diamond = firstItem.Children.First();
        var style = diamond.GetAttribute("style") ?? "";
        style.Should().Contain("display:inline-block");
    }

    #endregion

    #region Production Release Legend Item

    [Fact]
    public void Legend_ProductionRelease_HasLabel()
    {
        var cut = RenderHeader();

        cut.Markup.Should().Contain("Production Release");
    }

    [Fact]
    public void Legend_ProductionRelease_DiamondHasGreenColor()
    {
        var cut = RenderHeader();

        cut.Markup.Should().Contain("background:#34A853");
    }

    [Fact]
    public void Legend_ProductionRelease_DiamondIs12x12pxAndRotated()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var items = legendDiv.Children.Where(c => c.TagName == "SPAN").ToList();
        var prodItem = items[1];
        var diamond = prodItem.Children.First();
        var style = diamond.GetAttribute("style") ?? "";
        style.Should().Contain("width:12px");
        style.Should().Contain("height:12px");
        style.Should().Contain("rotate(45deg)");
        style.Should().Contain("background:#34A853");
    }

    #endregion

    #region Checkpoint Legend Item

    [Fact]
    public void Legend_Checkpoint_HasLabel()
    {
        var cut = RenderHeader();

        cut.Markup.Should().Contain("Checkpoint");
    }

    [Fact]
    public void Legend_Checkpoint_CircleHasGrayColor()
    {
        var cut = RenderHeader();

        cut.Markup.Should().Contain("background:#999");
    }

    [Fact]
    public void Legend_Checkpoint_CircleIs8x8px()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var items = legendDiv.Children.Where(c => c.TagName == "SPAN").ToList();
        var checkpointItem = items[2];
        var circle = checkpointItem.Children.First();
        var style = circle.GetAttribute("style") ?? "";
        style.Should().Contain("width:8px");
        style.Should().Contain("height:8px");
    }

    [Fact]
    public void Legend_Checkpoint_CircleHasBorderRadius50()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var items = legendDiv.Children.Where(c => c.TagName == "SPAN").ToList();
        var checkpointItem = items[2];
        var circle = checkpointItem.Children.First();
        var style = circle.GetAttribute("style") ?? "";
        style.Should().Contain("border-radius:50%");
    }

    [Fact]
    public void Legend_Checkpoint_CircleDoesNotRotate()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var items = legendDiv.Children.Where(c => c.TagName == "SPAN").ToList();
        var checkpointItem = items[2];
        var circle = checkpointItem.Children.First();
        var style = circle.GetAttribute("style") ?? "";
        style.Should().NotContain("rotate");
    }

    #endregion

    #region Now Line Legend Item

    [Fact]
    public void Legend_NowLine_HasRedColor()
    {
        var cut = RenderHeader();

        cut.Markup.Should().Contain("background:#EA4335");
    }

    [Fact]
    public void Legend_NowLine_BarIs2x14px()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var items = legendDiv.Children.Where(c => c.TagName == "SPAN").ToList();
        var nowItem = items[3];
        var bar = nowItem.Children.First();
        var style = bar.GetAttribute("style") ?? "";
        style.Should().Contain("width:2px");
        style.Should().Contain("height:14px");
    }

    [Fact]
    public void Legend_NowLine_BarDoesNotRotate()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var items = legendDiv.Children.Where(c => c.TagName == "SPAN").ToList();
        var nowItem = items[3];
        var bar = nowItem.Children.First();
        var style = bar.GetAttribute("style") ?? "";
        style.Should().NotContain("rotate");
    }

    [Fact]
    public void Legend_NowLine_DisplaysCurrentMonth()
    {
        var cut = RenderHeader(CreateData(currentMonth: "Apr 2026"));

        cut.Markup.Should().Contain("Now (Apr 2026)");
    }

    [Fact]
    public void Legend_NowLine_UpdatesWhenCurrentMonthChanges()
    {
        var cut = RenderHeader(CreateData(currentMonth: "Jun 2026"));

        cut.Markup.Should().Contain("Now (Jun 2026)");
        cut.Markup.Should().NotContain("Now (Apr 2026)");
    }

    [Fact]
    public void Legend_NowLine_HandlesLongMonthName()
    {
        var cut = RenderHeader(CreateData(currentMonth: "September 2026"));

        cut.Markup.Should().Contain("Now (September 2026)");
    }

    [Fact]
    public void Legend_NowLine_HandlesEmptyCurrentMonth()
    {
        var cut = RenderHeader(CreateData(currentMonth: ""));

        cut.Markup.Should().Contain("Now ()");
    }

    #endregion

    #region Legend Item Container Styles

    [Fact]
    public void Legend_EachItemHasInlineFlexDisplay()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var items = legendDiv.Children.Where(c => c.TagName == "SPAN").ToList();

        foreach (var item in items)
        {
            var style = item.GetAttribute("style") ?? "";
            style.Should().Contain("inline-flex", $"legend item '{item.TextContent.Trim()}' should use inline-flex");
        }
    }

    [Fact]
    public void Legend_EachItemHasCenterAlignment()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var items = legendDiv.Children.Where(c => c.TagName == "SPAN").ToList();

        foreach (var item in items)
        {
            var style = item.GetAttribute("style") ?? "";
            style.Should().Contain("align-items:center");
        }
    }

    [Fact]
    public void Legend_EachItemHasGapBetweenSymbolAndLabel()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var items = legendDiv.Children.Where(c => c.TagName == "SPAN").ToList();

        foreach (var item in items)
        {
            var style = item.GetAttribute("style") ?? "";
            style.Should().Contain("gap:5px", $"legend item '{item.TextContent.Trim()}' should have gap between symbol and label");
        }
    }

    [Fact]
    public void Legend_EachItemHasExactlyOneSymbolSpan()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var items = legendDiv.Children.Where(c => c.TagName == "SPAN").ToList();

        foreach (var item in items)
        {
            var innerSpans = item.Children.Where(c => c.TagName == "SPAN").ToList();
            innerSpans.Should().HaveCount(1, $"each legend item should have exactly one symbol span");
        }
    }

    #endregion

    #region Data Binding Edge Cases

    [Fact]
    public void Header_WithSpecialCharactersInTitle_RendersCorrectly()
    {
        var cut = RenderHeader(CreateData(title: "Project <Alpha> & \"Beta\""));

        var h1 = cut.Find("h1");
        h1.TextContent.Should().Contain("Project <Alpha> & \"Beta\"");
    }

    [Fact]
    public void Header_WithUnicodeInSubtitle_RendersCorrectly()
    {
        var cut = RenderHeader(CreateData(subtitle: "チーム A — April 2026"));

        var sub = cut.Find(".sub");
        sub.TextContent.Should().Be("チーム A — April 2026");
    }

    [Fact]
    public void Header_WithEmptyTitle_RendersEmptyH1()
    {
        var cut = RenderHeader(CreateData(title: ""));

        var h1 = cut.Find("h1");
        h1.Should().NotBeNull();
    }

    [Fact]
    public void Header_WithEmptySubtitle_RendersEmptySubDiv()
    {
        var cut = RenderHeader(CreateData(subtitle: ""));

        var sub = cut.Find(".sub");
        sub.TextContent.Should().BeEmpty();
    }

    [Fact]
    public void Header_WithVeryLongTitle_RendersWithoutError()
    {
        var longTitle = new string('A', 500);
        var cut = RenderHeader(CreateData(title: longTitle));

        var h1 = cut.Find("h1");
        h1.TextContent.Should().Contain(longTitle);
    }

    [Fact]
    public void Header_WithVeryLongCurrentMonth_RendersWithoutError()
    {
        var longMonth = "September-October Extended Period 2026-2027";
        var cut = RenderHeader(CreateData(currentMonth: longMonth));

        cut.Markup.Should().Contain($"Now ({longMonth})");
    }

    [Fact]
    public void Header_LegendLabelsInCorrectOrder()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var items = legendDiv.Children.Where(c => c.TagName == "SPAN").ToList();

        items[0].TextContent.Should().Contain("PoC Milestone");
        items[1].TextContent.Should().Contain("Production Release");
        items[2].TextContent.Should().Contain("Checkpoint");
        items[3].TextContent.Should().Contain("Now");
    }

    #endregion

    #region Symbol Color Verification

    [Fact]
    public void Legend_AllFourDistinctColors_ArePresent()
    {
        var cut = RenderHeader();

        var markup = cut.Markup;
        markup.Should().Contain("#F4B400", "PoC gold color");
        markup.Should().Contain("#34A853", "Production green color");
        markup.Should().Contain("#999", "Checkpoint gray color");
        markup.Should().Contain("#EA4335", "Now line red color");
    }

    [Fact]
    public void Legend_DiamondSymbols_AreDistinctFromCircleAndBar()
    {
        var cut = RenderHeader();

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var items = legendDiv.Children.Where(c => c.TagName == "SPAN").ToList();

        // Diamonds (items 0, 1) should have rotate(45deg)
        items[0].Children.First().GetAttribute("style").Should().Contain("rotate(45deg)");
        items[1].Children.First().GetAttribute("style").Should().Contain("rotate(45deg)");

        // Circle (item 2) should have border-radius
        items[2].Children.First().GetAttribute("style").Should().Contain("border-radius");

        // Bar (item 3) should have neither rotate nor border-radius
        var barStyle = items[3].Children.First().GetAttribute("style") ?? "";
        barStyle.Should().NotContain("rotate");
        barStyle.Should().NotContain("border-radius");
    }

    #endregion

    #region Component Re-rendering

    [Fact]
    public void Header_ReRendersWhenDataParameterChanges()
    {
        var data = CreateData(currentMonth: "Mar 2026");
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("Now (Mar 2026)");

        var newData = CreateData(currentMonth: "May 2026");
        cut.SetParametersAndRender(p => p.Add(x => x.Data, newData));

        cut.Markup.Should().Contain("Now (May 2026)");
        cut.Markup.Should().NotContain("Now (Mar 2026)");
    }

    [Fact]
    public void Header_ReRendersTitle_WhenDataChanges()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(title: "Version 1")));

        cut.Find("h1").TextContent.Should().Contain("Version 1");

        cut.SetParametersAndRender(p =>
            p.Add(x => x.Data, CreateData(title: "Version 2")));

        cut.Find("h1").TextContent.Should().Contain("Version 2");
    }

    #endregion
}