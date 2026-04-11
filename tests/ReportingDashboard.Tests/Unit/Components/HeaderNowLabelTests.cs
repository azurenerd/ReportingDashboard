using Bunit;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for the NowLabel computed property in Components/Header.razor.
/// The NowLabel logic: if nowDate parses, returns "CurrentMonth Year";
/// else if startDate parses, returns "CurrentMonth Year"; else just CurrentMonth.
/// </summary>
[Trait("Category", "Unit")]
public class HeaderNowLabelTests : TestContext
{
    private static DashboardData CreateData(
        string currentMonth = "April",
        string nowDate = "2026-04-10",
        string startDate = "2026-01-01") => new()
    {
        Title = "Test",
        Subtitle = "Sub",
        BacklogLink = "https://link",
        CurrentMonth = currentMonth,
        Months = new List<string> { "April" },
        Timeline = new TimelineData
        {
            StartDate = startDate,
            EndDate = "2026-07-01",
            NowDate = nowDate,
            Tracks = new List<TimelineTrack>()
        },
        Heatmap = new HeatmapData()
    };

    [Fact]
    public void Header_NowLabel_WithValidNowDate_ShowsMonthAndYear()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(currentMonth: "April", nowDate: "2026-04-10")));

        Assert.Contains("Now (April 2026)", cut.Markup);
    }

    [Fact]
    public void Header_NowLabel_WithEmptyNowDate_FallsBackToStartDate()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(
                currentMonth: "March",
                nowDate: "",
                startDate: "2026-01-01")));

        Assert.Contains("Now (March 2026)", cut.Markup);
    }

    [Fact]
    public void Header_NowLabel_WithInvalidNowAndStartDate_ShowsJustMonth()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(
                currentMonth: "June",
                nowDate: "invalid",
                startDate: "also-invalid")));

        Assert.Contains("Now (June)", cut.Markup);
    }

    [Fact]
    public void Header_NowLabel_WithInvalidNowDate_UsesStartDateYear()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData(
                currentMonth: "May",
                nowDate: "not-a-date",
                startDate: "2027-05-01")));

        Assert.Contains("Now (May 2027)", cut.Markup);
    }

    [Fact]
    public void Header_NowLabel_NullTimeline_ShowsJustMonth()
    {
        var data = new DashboardData
        {
            Title = "Test",
            Subtitle = "Sub",
            BacklogLink = "https://link",
            CurrentMonth = "Dec",
            Months = new List<string> { "Dec" },
            Timeline = null!,
            Heatmap = new HeatmapData()
        };

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now (Dec)", cut.Markup);
    }

    [Fact]
    public void Header_BacklogLink_WhenWhitespace_NoLinkRendered()
    {
        var data = CreateData();
        data.BacklogLink = "   ";

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        var links = cut.FindAll("a");
        Assert.Empty(links);
    }

    [Fact]
    public void Header_LegendItems_AllFourPresent()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("PoC Milestone", cut.Markup);
        Assert.Contains("Production Release", cut.Markup);
        Assert.Contains("Checkpoint", cut.Markup);
        Assert.Contains("Now (", cut.Markup);
    }

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
    public void Header_LegendCheckpoint_HasGrayCircle()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        // Checkpoint circle: 8px, border-radius:50%, background:#999
        Assert.Contains("border-radius:50%", cut.Markup);
        Assert.Contains("background:#999", cut.Markup);
    }

    [Fact]
    public void Header_LegendNowBar_HasRedColor()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("#EA4335", cut.Markup);
    }

    [Fact]
    public void Header_LegendGap_Is22px()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, CreateData()));

        Assert.Contains("gap:22px", cut.Markup);
    }
}