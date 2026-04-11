using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

[Trait("Category", "Integration")]
public class HeaderDataServiceIntegrationTests : TestContext
{
    private static DashboardData CreateServiceData(
        string nowDate = "2026-04-10",
        string currentMonth = "Apr",
        string title = "Agent Squad – Executive Reporting Dashboard",
        string subtitle = "Platform Engineering · AI & ML Workstream · Apr 2026",
        string backlogLink = "https://dev.azure.com/contoso/agentsquad/_backlogs",
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
    public void Header_WithServiceData_RendersCompleteHeaderLayout()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateServiceData()));

        cut.Find("h1").TextContent.Should().Contain("Agent Squad");
        cut.Find(".sub").TextContent.Should().Contain("Platform Engineering");

        var link = cut.Find("a[target='_blank']");
        link.GetAttribute("href").Should().Be("https://dev.azure.com/contoso/agentsquad/_backlogs");

        var legend = cut.Find("div[style*='gap:22px']");
        legend.QuerySelectorAll(":scope > span").Length.Should().Be(4);
        cut.Markup.Should().Contain("Now (April 2026)");
    }

    [Fact]
    public void Header_WithServiceData_NowDateMatchesTimelineContext()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateServiceData()));
        cut.Markup.Should().Contain("Now (April 2026)");
    }

    [Fact]
    public void Header_DataServiceError_EmptyNowDate_UsesCurrentMonthFallback()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateServiceData(nowDate: "")));
        cut.Markup.Should().Contain("Now (Apr)");
    }

    [Fact]
    public void Header_DataServiceError_NullTimeline_UsesCurrentMonthFallback()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateServiceData(nullTimeline: true)));
        cut.Markup.Should().Contain("Now (Apr)");
    }

    [Fact]
    public void Header_TitleAndLegend_RenderInSameHdrContainer()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateServiceData()));
        var hdr = cut.Find(".hdr");
        hdr.QuerySelectorAll("h1").Length.Should().Be(1);
        hdr.QuerySelectorAll("div[style*='gap:22px']").Length.Should().Be(1);
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
            p.Add(x => x.Data, CreateServiceData(nowDate: nowDate)));
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
            p.Add(x => x.Data, CreateServiceData(nowDate: nowDate, currentMonth: currentMonth)));
        cut.Markup.Should().Contain(expectedLabel);
    }

    [Fact]
    public void Header_ViaRegisteredService_RendersCorrectly()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateServiceData()));

        cut.Find(".hdr").Should().NotBeNull();
        cut.Find("h1").TextContent.Should().Contain("Agent Squad");
        cut.Find(".sub").TextContent.Should().Contain("AI & ML Workstream");
        cut.Find("a").GetAttribute("href").Should().StartWith("https://dev.azure.com/");

        var legend = cut.Find("div[style*='gap:22px']");
        legend.QuerySelectorAll(":scope > span").Length.Should().Be(4);

        var items = legend.QuerySelectorAll(":scope > span");
        items[0].TextContent.Should().Contain("PoC Milestone");
        items[1].TextContent.Should().Contain("Production Release");
        items[2].TextContent.Should().Contain("Checkpoint");
        items[3].TextContent.Should().Contain("Now (April 2026)");
    }
}