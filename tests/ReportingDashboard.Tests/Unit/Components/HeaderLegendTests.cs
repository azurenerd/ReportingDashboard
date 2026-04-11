using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class HeaderLegendTests : TestContext
{
    private static DashboardData CreateData(
        string? nowDate = "2026-04-10",
        string currentMonth = "Apr",
        string title = "Test Project",
        string subtitle = "Test Subtitle",
        string backlogLink = "https://dev.azure.com/test",
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
                NowDate = nowDate ?? ""
            }
        };
    }

    private IRenderedComponent<ReportingDashboard.Components.Header> RenderHeader(DashboardData data)
    {
        return RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));
    }

    // --- Legend Container Tests ---

    [Fact]
    public void Legend_RendersFourItems()
    {
        var cut = RenderHeader(CreateData());
        var legendContainer = cut.Find("div[style*='gap:22px']");
        var legendItems = legendContainer.QuerySelectorAll(":scope > span");
        legendItems.Length.Should().Be(4);
    }

    [Fact]
    public void LegendContainer_HasCorrectFlexStyles()
    {
        var cut = RenderHeader(CreateData());
        var legendContainer = cut.Find("div[style*='gap:22px']");
        var style = legendContainer.GetAttribute("style") ?? "";
        style.Should().Contain("display:flex");
        style.Should().Contain("gap:22px");
        style.Should().Contain("align-items:center");
    }

    // --- PoC Milestone Symbol Tests ---

    [Fact]
    public void PocMilestone_RendersGoldDiamondSymbol()
    {
        var cut = RenderHeader(CreateData());
        var pocSymbol = cut.Find("span[style*='#F4B400']");
        var style = pocSymbol.GetAttribute("style") ?? "";
        style.Should().Contain("width:12px");
        style.Should().Contain("height:12px");
        style.Should().Contain("background:#F4B400");
        style.Should().Contain("rotate(45deg)");
    }

    [Fact]
    public void PocMilestone_HasCorrectLabelText()
    {
        var cut = RenderHeader(CreateData());
        cut.Markup.Should().Contain("PoC Milestone");
    }

    [Fact]
    public void PocMilestone_LabelHas12pxFont()
    {
        var cut = RenderHeader(CreateData());
        var legendContainer = cut.Find("div[style*='gap:22px']");
        var firstItem = legendContainer.QuerySelectorAll(":scope > span")[0];
        var labelSpan = firstItem.QuerySelectorAll("span[style*='font-size:12px']");
        labelSpan.Length.Should().BeGreaterThan(0);
    }

    // --- Production Release Symbol Tests ---

    [Fact]
    public void ProductionRelease_RendersGreenDiamondSymbol()
    {
        var cut = RenderHeader(CreateData());
        var prodSymbol = cut.Find("span[style*='#34A853']");
        var style = prodSymbol.GetAttribute("style") ?? "";
        style.Should().Contain("width:12px");
        style.Should().Contain("height:12px");
        style.Should().Contain("background:#34A853");
        style.Should().Contain("rotate(45deg)");
    }

    [Fact]
    public void ProductionRelease_HasCorrectLabelText()
    {
        var cut = RenderHeader(CreateData());
        cut.Markup.Should().Contain("Production Release");
    }

    // --- Checkpoint Symbol Tests ---

    [Fact]
    public void Checkpoint_RendersGrayCircleSymbol()
    {
        var cut = RenderHeader(CreateData());
        var checkpointSymbol = cut.Find("span[style*='#999']");
        var style = checkpointSymbol.GetAttribute("style") ?? "";
        style.Should().Contain("width:8px");
        style.Should().Contain("height:8px");
        style.Should().Contain("border-radius:50%");
        style.Should().Contain("background:#999");
    }

    [Fact]
    public void Checkpoint_HasCorrectLabelText()
    {
        var cut = RenderHeader(CreateData());
        cut.Markup.Should().Contain("Checkpoint");
    }

    // --- Now Line Symbol Tests ---

    [Fact]
    public void NowLine_RendersRedBarSymbol()
    {
        var cut = RenderHeader(CreateData());
        var nowSymbol = cut.Find("span[style*='#EA4335']");
        var style = nowSymbol.GetAttribute("style") ?? "";
        style.Should().Contain("width:2px");
        style.Should().Contain("height:14px");
        style.Should().Contain("background:#EA4335");
    }

    // --- NowLabel Dynamic Tests ---

    [Fact]
    public void NowLabel_ValidNowDate_FormatsAsMonthYear()
    {
        var cut = RenderHeader(CreateData(nowDate: "2026-04-10"));
        cut.Markup.Should().Contain("Now (April 2026)");
    }

    [Fact]
    public void NowLabel_DifferentValidDate_FormatsCorrectly()
    {
        var cut = RenderHeader(CreateData(nowDate: "2025-12-25"));
        cut.Markup.Should().Contain("Now (December 2025)");
    }

    [Fact]
    public void NowLabel_JanuaryDate_FormatsCorrectly()
    {
        var cut = RenderHeader(CreateData(nowDate: "2026-01-15"));
        cut.Markup.Should().Contain("Now (January 2026)");
    }

    [Fact]
    public void NowLabel_EmptyNowDate_FallsBackToCurrentMonth()
    {
        var cut = RenderHeader(CreateData(nowDate: "", currentMonth: "Apr"));
        cut.Markup.Should().Contain("Now (Apr)");
    }

    [Fact]
    public void NowLabel_NullNowDate_FallsBackToCurrentMonth()
    {
        // NowDate is init-only, so we pass empty string to simulate null-like behavior
        var cut = RenderHeader(CreateData(nowDate: "", currentMonth: "Mar"));
        cut.Markup.Should().Contain("Now (Mar)");
    }

    [Fact]
    public void NowLabel_WhitespaceNowDate_FallsBackToCurrentMonth()
    {
        var cut = RenderHeader(CreateData(nowDate: "   ", currentMonth: "Feb"));
        cut.Markup.Should().Contain("Now (Feb)");
    }

    [Fact]
    public void NowLabel_InvalidNowDate_FallsBackToCurrentMonth()
    {
        var cut = RenderHeader(CreateData(nowDate: "not-a-date", currentMonth: "Mar"));
        cut.Markup.Should().Contain("Now (Mar)");
    }

    [Fact]
    public void NowLabel_GarbageDateString_FallsBackToCurrentMonth()
    {
        var cut = RenderHeader(CreateData(nowDate: "xyz123", currentMonth: "Jun"));
        cut.Markup.Should().Contain("Now (Jun)");
    }

    [Fact]
    public void NowLabel_EmptyNowDateAndEmptyCurrentMonth_FallsBackToNow()
    {
        var cut = RenderHeader(CreateData(nowDate: "", currentMonth: ""));
        cut.Markup.Should().Contain(">Now<");
    }

    [Fact]
    public void NowLabel_EmptyNowDateAndWhitespaceCurrentMonth_FallsBackToNow()
    {
        var cut = RenderHeader(CreateData(nowDate: "", currentMonth: "   "));
        cut.Markup.Should().Contain(">Now<");
    }

    [Fact]
    public void NowLabel_NullTimeline_FallsBackToCurrentMonth()
    {
        var cut = RenderHeader(CreateData(nullTimeline: true, currentMonth: "Sep"));
        cut.Markup.Should().Contain("Now (Sep)");
    }

    [Fact]
    public void NowLabel_NullTimelineAndEmptyCurrentMonth_FallsBackToNow()
    {
        var cut = RenderHeader(CreateData(nullTimeline: true, currentMonth: ""));
        cut.Markup.Should().Contain(">Now<");
    }

    // --- Header Title & Metadata Tests ---

    [Fact]
    public void Header_RendersTitle()
    {
        var cut = RenderHeader(CreateData(title: "My Dashboard"));
        cut.Find("h1").TextContent.Should().Contain("My Dashboard");
    }

    [Fact]
    public void Header_RendersSubtitle()
    {
        var cut = RenderHeader(CreateData(subtitle: "Team Alpha - Sprint 42"));
        cut.Find(".sub").TextContent.Should().Be("Team Alpha - Sprint 42");
    }

    [Fact]
    public void Header_RendersBacklogLink_WhenProvided()
    {
        var cut = RenderHeader(CreateData(backlogLink: "https://dev.azure.com/myproject"));
        var link = cut.Find("a[href='https://dev.azure.com/myproject']");
        link.Should().NotBeNull();
        link.GetAttribute("target").Should().Be("_blank");
        link.TextContent.Should().Contain("ADO Backlog");
    }

    [Fact]
    public void Header_HidesBacklogLink_WhenEmpty()
    {
        var cut = RenderHeader(CreateData(backlogLink: ""));
        var links = cut.FindAll("a[target='_blank']");
        links.Count.Should().Be(0);
    }

    [Fact]
    public void Header_HasHdrCssClass()
    {
        var cut = RenderHeader(CreateData());
        cut.Find(".hdr").Should().NotBeNull();
    }

    [Fact]
    public void Header_BacklogLink_HasMicrosoftBlueColor()
    {
        var cut = RenderHeader(CreateData(backlogLink: "https://example.com"));
        var link = cut.Find("a");
        var style = link.GetAttribute("style") ?? "";
        style.Should().Contain("#0078D4");
    }

    [Fact]
    public void Header_Title_Has24pxBoldFont()
    {
        var cut = RenderHeader(CreateData());
        var h1 = cut.Find("h1");
        var style = h1.GetAttribute("style") ?? "";
        style.Should().Contain("font-size:24px");
        style.Should().Contain("font-weight:700");
    }

    // --- Legend Symbol Inline-block and Flex-shrink Tests ---

    [Fact]
    public void PocSymbol_HasInlineBlockDisplay()
    {
        var cut = RenderHeader(CreateData());
        var symbol = cut.Find("span[style*='#F4B400']");
        var style = symbol.GetAttribute("style") ?? "";
        style.Should().Contain("display:inline-block");
        style.Should().Contain("flex-shrink:0");
    }

    [Fact]
    public void ProductionSymbol_HasInlineBlockDisplay()
    {
        var cut = RenderHeader(CreateData());
        var symbol = cut.Find("span[style*='#34A853']");
        var style = symbol.GetAttribute("style") ?? "";
        style.Should().Contain("display:inline-block");
        style.Should().Contain("flex-shrink:0");
    }

    [Fact]
    public void CheckpointSymbol_HasInlineBlockDisplay()
    {
        var cut = RenderHeader(CreateData());
        var symbol = cut.Find("span[style*='#999']");
        var style = symbol.GetAttribute("style") ?? "";
        style.Should().Contain("display:inline-block");
        style.Should().Contain("flex-shrink:0");
    }

    [Fact]
    public void NowSymbol_HasInlineBlockDisplay()
    {
        var cut = RenderHeader(CreateData());
        var symbol = cut.Find("span[style*='#EA4335']");
        var style = symbol.GetAttribute("style") ?? "";
        style.Should().Contain("display:inline-block");
        style.Should().Contain("flex-shrink:0");
    }

    // --- Legend Item Wrapper Alignment Tests ---

    [Fact]
    public void EachLegendItem_HasFlexAlignCenterGap6px()
    {
        var cut = RenderHeader(CreateData());
        var legendContainer = cut.Find("div[style*='gap:22px']");
        var items = legendContainer.QuerySelectorAll(":scope > span");
        foreach (var item in items)
        {
            var style = item.GetAttribute("style") ?? "";
            style.Should().Contain("display:flex");
            style.Should().Contain("align-items:center");
            style.Should().Contain("gap:6px");
        }
    }

    // --- Label nowrap Tests ---

    [Fact]
    public void LegendLabels_HaveWhiteSpaceNowrap()
    {
        var cut = RenderHeader(CreateData());
        var labels = cut.FindAll("span[style*='white-space:nowrap']");
        labels.Count.Should().BeGreaterOrEqualTo(4);
    }

    // --- Date Boundary Tests ---

    [Fact]
    public void NowLabel_LeapYearDate_FormatsCorrectly()
    {
        var cut = RenderHeader(CreateData(nowDate: "2024-02-29"));
        cut.Markup.Should().Contain("Now (February 2024)");
    }

    [Fact]
    public void NowLabel_EndOfYear_FormatsCorrectly()
    {
        var cut = RenderHeader(CreateData(nowDate: "2026-12-31"));
        cut.Markup.Should().Contain("Now (December 2026)");
    }

    [Fact]
    public void NowLabel_StartOfYear_FormatsCorrectly()
    {
        var cut = RenderHeader(CreateData(nowDate: "2026-01-01"));
        cut.Markup.Should().Contain("Now (January 2026)");
    }

    [Fact]
    public void NowLabel_ISOFormatWithTime_ParsesCorrectly()
    {
        var cut = RenderHeader(CreateData(nowDate: "2026-06-15T14:30:00"));
        cut.Markup.Should().Contain("Now (June 2026)");
    }
}