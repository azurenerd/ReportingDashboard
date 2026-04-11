using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

/// <summary>
/// Integration tests for the Header component rendered standalone with various
/// DashboardData configurations, verifying full rendering pipeline including
/// the NowLabel computed property and conditional backlog link logic.
/// </summary>
[Trait("Category", "Integration")]
public class HeaderComponentIntegrationTests : TestContext
{
    // ── NowLabel resolution chain: Timeline.NowDate → CurrentMonth → "Now" ──

    [Fact]
    public void Header_NowLabel_Priority1_TimelineNowDate_ParsedToMonthYear()
    {
        var data = new DashboardData
        {
            Title = "Test",
            Subtitle = "Sub",
            CurrentMonth = "March",
            Timeline = new TimelineData
            {
                NowDate = "2026-12-25",
                Tracks = new List<TimelineTrack>()
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("Now (December 2026)");
        cut.Markup.Should().NotContain("Now (March)");
    }

    [Fact]
    public void Header_NowLabel_Priority2_CurrentMonth_WhenTimelineNull()
    {
        var data = new DashboardData
        {
            Title = "Test",
            Subtitle = "Sub",
            CurrentMonth = "October",
            Timeline = null
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("Now (October)");
    }

    [Fact]
    public void Header_NowLabel_Priority2_CurrentMonth_WhenNowDateWhitespace()
    {
        var data = new DashboardData
        {
            Title = "Test",
            Subtitle = "Sub",
            CurrentMonth = "July",
            Timeline = new TimelineData
            {
                NowDate = "   ",
                Tracks = new List<TimelineTrack>()
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("Now (July)");
    }

    [Fact]
    public void Header_NowLabel_Priority2_CurrentMonth_WhenNowDateInvalid()
    {
        var data = new DashboardData
        {
            Title = "Test",
            Subtitle = "Sub",
            CurrentMonth = "August",
            Timeline = new TimelineData
            {
                NowDate = "invalid-date-format",
                Tracks = new List<TimelineTrack>()
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("Now (August)");
    }

    [Fact]
    public void Header_NowLabel_Priority3_PlainNow_WhenNoDateInfo()
    {
        var data = new DashboardData
        {
            Title = "Test",
            Subtitle = "Sub",
            CurrentMonth = "",
            Timeline = null
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain(">Now<");
    }

    [Fact]
    public void Header_NowLabel_Priority3_PlainNow_WhenCurrentMonthWhitespace()
    {
        var data = new DashboardData
        {
            Title = "Test",
            Subtitle = "Sub",
            CurrentMonth = "   ",
            Timeline = null
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain(">Now<");
    }

    // ── NowDate parsing for different months ──

    [Theory]
    [InlineData("2026-01-15", "January 2026")]
    [InlineData("2026-02-28", "February 2026")]
    [InlineData("2026-03-01", "March 2026")]
    [InlineData("2026-04-11", "April 2026")]
    [InlineData("2026-05-31", "May 2026")]
    [InlineData("2026-06-15", "June 2026")]
    [InlineData("2026-07-04", "July 2026")]
    [InlineData("2026-08-20", "August 2026")]
    [InlineData("2026-09-30", "September 2026")]
    [InlineData("2026-10-01", "October 2026")]
    [InlineData("2026-11-11", "November 2026")]
    [InlineData("2026-12-31", "December 2026")]
    public void Header_NowLabel_ParsesAllMonths(string nowDate, string expectedMonthYear)
    {
        var data = new DashboardData
        {
            Title = "Test",
            Subtitle = "Sub",
            Timeline = new TimelineData
            {
                NowDate = nowDate,
                Tracks = new List<TimelineTrack>()
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain($"Now ({expectedMonthYear})");
    }

    // ── Backlog link conditional rendering ──

    [Fact]
    public void Header_BacklogLink_RenderedWhenNonEmpty()
    {
        var data = new DashboardData
        {
            Title = "Project",
            Subtitle = "Sub",
            BacklogLink = "https://dev.azure.com/myorg"
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var links = cut.FindAll("a");
        links.Should().HaveCount(1);
        links[0].GetAttribute("href").Should().Be("https://dev.azure.com/myorg");
    }

    [Fact]
    public void Header_BacklogLink_HiddenWhenEmpty()
    {
        var data = new DashboardData
        {
            Title = "Project",
            Subtitle = "Sub",
            BacklogLink = ""
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.FindAll("a").Should().BeEmpty();
    }

    [Fact]
    public void Header_BacklogLink_HiddenWhenNull()
    {
        var data = new DashboardData
        {
            Title = "Project",
            Subtitle = "Sub",
            BacklogLink = null!
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.FindAll("a").Should().BeEmpty();
    }

    // ── Full component structure integration ──

    [Fact]
    public void Header_FullRender_ContainsAllExpectedElements()
    {
        var data = new DashboardData
        {
            Title = "Full Render Test",
            Subtitle = "Team · Workstream · April 2026",
            BacklogLink = "https://ado.example.com",
            CurrentMonth = "April",
            Timeline = new TimelineData
            {
                NowDate = "2026-04-11",
                Tracks = new List<TimelineTrack>()
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        // Root structure
        cut.Find("div.hdr").Should().NotBeNull();

        // Title
        var h1 = cut.Find("h1");
        h1.TextContent.Should().Contain("Full Render Test");

        // Link
        var link = cut.Find("a");
        link.GetAttribute("href").Should().Be("https://ado.example.com");
        link.TextContent.Should().Contain("ADO Backlog");

        // Subtitle
        cut.Find("div.sub").TextContent.Should().Be("Team · Workstream · April 2026");

        // Legend items
        cut.Markup.Should().Contain("PoC Milestone");
        cut.Markup.Should().Contain("Production Release");
        cut.Markup.Should().Contain("Checkpoint");
        cut.Markup.Should().Contain("Now (April 2026)");

        // Legend colors
        cut.Markup.Should().Contain("background:#F4B400");
        cut.Markup.Should().Contain("background:#34A853");
        cut.Markup.Should().Contain("background:#999");
        cut.Markup.Should().Contain("background:#EA4335");
    }

    [Fact]
    public void Header_LegendDiamonds_HaveRotate45deg()
    {
        var data = new DashboardData
        {
            Title = "Test",
            Subtitle = "Sub"
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        // Both PoC and Production diamonds should have rotate(45deg)
        var markup = cut.Markup;
        var rotateCount = markup.Split("transform:rotate(45deg)").Length - 1;
        rotateCount.Should().Be(2, "exactly two diamonds should be rotated 45 degrees");
    }

    [Fact]
    public void Header_LegendCheckpoint_IsCircularShape()
    {
        var data = new DashboardData
        {
            Title = "Test",
            Subtitle = "Sub"
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("width:8px;height:8px;border-radius:50%;background:#999");
    }

    [Fact]
    public void Header_LegendNowBar_Is2x14RedBar()
    {
        var data = new DashboardData
        {
            Title = "Test",
            Subtitle = "Sub"
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Markup.Should().Contain("width:2px;height:14px;background:#EA4335");
    }

    // ── Data-driven rendering with varied inputs ──

    [Theory]
    [InlineData("Dashboard Alpha", "Team A · Sprint 1")]
    [InlineData("Dashboard Beta", "Team B · Sprint 2")]
    [InlineData("Multi-Word Project Title With Special Chars!@#", "Dept · Stream · Month")]
    public void Header_RendersArbitraryTitleAndSubtitle(string title, string subtitle)
    {
        var data = new DashboardData
        {
            Title = title,
            Subtitle = subtitle
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Find("h1").TextContent.Should().Contain(title);
        cut.Find("div.sub").TextContent.Should().Be(subtitle);
    }

    [Theory]
    [InlineData("https://dev.azure.com/org1/project1")]
    [InlineData("https://dev.azure.com/org2/project2/_backlogs/backlog/Team/Backlog%20items")]
    [InlineData("https://example.com/backlog?q=items&page=1")]
    public void Header_BacklogLink_AcceptsVariousUrls(string url)
    {
        var data = new DashboardData
        {
            Title = "Test",
            Subtitle = "Sub",
            BacklogLink = url
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        cut.Find("a").GetAttribute("href").Should().Be(url);
    }

    // ── Styling integration ──

    [Fact]
    public void Header_H1_HasCorrectInlineStyles()
    {
        var data = new DashboardData { Title = "Styled", Subtitle = "Sub" };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var h1Style = cut.Find("h1").GetAttribute("style") ?? "";
        h1Style.Should().Contain("font-size:24px");
        h1Style.Should().Contain("font-weight:700");
        h1Style.Should().Contain("color:#111");
        h1Style.Should().Contain("margin:0");
    }

    [Fact]
    public void Header_BacklogLink_HasCorrectInlineStyles()
    {
        var data = new DashboardData
        {
            Title = "Test",
            Subtitle = "Sub",
            BacklogLink = "https://test.com"
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var linkStyle = cut.Find("a").GetAttribute("style") ?? "";
        linkStyle.Should().Contain("font-size:13px");
        linkStyle.Should().Contain("color:#0078D4");
        linkStyle.Should().Contain("text-decoration:none");
        linkStyle.Should().Contain("margin-left:12px");
    }

    [Fact]
    public void Header_LegendContainer_HasFlexLayoutWith22pxGap()
    {
        var data = new DashboardData { Title = "Test", Subtitle = "Sub" };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var legendDiv = cut.FindAll("div.hdr > div")[1];
        var style = legendDiv.GetAttribute("style") ?? "";
        style.Should().Contain("display:flex");
        style.Should().Contain("gap:22px");
        style.Should().Contain("align-items:center");
    }

    [Fact]
    public void Header_LegendLabels_AllHave12pxFontSize()
    {
        var data = new DashboardData { Title = "Test", Subtitle = "Sub" };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(
            p => p.Add(x => x.Data, data));

        var markup = cut.Markup;
        var fontSize12Count = markup.Split("font-size:12px").Length - 1;
        // 4 legend labels + potentially other 12px references, but at minimum 4
        fontSize12Count.Should().BeGreaterOrEqualTo(4);
    }
}