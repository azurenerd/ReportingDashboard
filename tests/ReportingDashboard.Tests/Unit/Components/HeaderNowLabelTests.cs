using Bunit;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for the NowLabel computed property logic in Components/Header.razor.
/// The NowLabel has three branches:
///   1. Timeline != null && NowDate parses → "Now (month year)"
///   2. Timeline != null && StartDate parses (NowDate doesn't) → "Now (month year)"
///   3. Fallback → "Now (month)"
/// Existing HeaderInlineTests and HeaderTests do not cover these branches.
/// </summary>
[Trait("Category", "Unit")]
public class HeaderNowLabelTests : TestContext
{
    private static DashboardData CreateData(
        string currentMonth = "April",
        string? nowDate = "2026-04-10",
        string? startDate = "2026-01-01",
        string? endDate = "2026-07-01",
        TimelineData? timeline = null,
        bool useNullTimeline = false)
    {
        var data = new DashboardData
        {
            Title = "Test",
            Subtitle = "Sub",
            BacklogLink = "https://link",
            CurrentMonth = currentMonth,
            Months = new List<string> { "January", "February", "March", "April" },
            Heatmap = new HeatmapData()
        };

        if (useNullTimeline)
        {
            data.Timeline = null!;
        }
        else if (timeline != null)
        {
            data.Timeline = timeline;
        }
        else
        {
            data.Timeline = new TimelineData
            {
                StartDate = startDate ?? "",
                EndDate = endDate ?? "",
                NowDate = nowDate ?? "",
                Tracks = new List<TimelineTrack>()
            };
        }

        return data;
    }

    #region Branch 1: Timeline with valid NowDate → "Now (month year)"

    [Fact]
    public void NowLabel_WithValidNowDate_IncludesYearFromNowDate()
    {
        var data = CreateData(currentMonth: "April", nowDate: "2026-04-10");
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now (April 2026)", cut.Markup);
    }

    [Fact]
    public void NowLabel_WithNowDateInDifferentYear_UsesNowDateYear()
    {
        var data = CreateData(currentMonth: "December", nowDate: "2027-12-15");
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now (December 2027)", cut.Markup);
    }

    [Fact]
    public void NowLabel_WithNowDateJanuary2025_ShowsCorrectYear()
    {
        var data = CreateData(currentMonth: "January", nowDate: "2025-01-01");
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now (January 2025)", cut.Markup);
    }

    [Theory]
    [InlineData("2026-04-10", "April", "Now (April 2026)")]
    [InlineData("2025-06-30", "June", "Now (June 2025)")]
    [InlineData("2030-01-15", "Jan", "Now (Jan 2030)")]
    public void NowLabel_WithVariousValidNowDates_FormatsCorrectly(
        string nowDate, string currentMonth, string expected)
    {
        var data = CreateData(currentMonth: currentMonth, nowDate: nowDate);
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains(expected, cut.Markup);
    }

    #endregion

    #region Branch 2: Timeline with invalid NowDate but valid StartDate → "Now (month year)"

    [Fact]
    public void NowLabel_WithInvalidNowDate_FallsBackToStartDateYear()
    {
        var data = CreateData(
            currentMonth: "March",
            nowDate: "not-a-date",
            startDate: "2026-01-01");
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now (March 2026)", cut.Markup);
    }

    [Fact]
    public void NowLabel_WithEmptyNowDate_FallsBackToStartDateYear()
    {
        var data = CreateData(
            currentMonth: "February",
            nowDate: "",
            startDate: "2027-02-01");
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now (February 2027)", cut.Markup);
    }

    [Fact]
    public void NowLabel_WithNullNowDate_FallsBackToStartDateYear()
    {
        var data = CreateData(currentMonth: "May", nowDate: null, startDate: "2026-05-01");
        // NowDate will be empty string from null coalescing
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now (May 2026)", cut.Markup);
    }

    [Fact]
    public void NowLabel_WithGarbageNowDateAndValidStartDate_UsesStartDateYear()
    {
        var data = CreateData(
            currentMonth: "October",
            nowDate: "xyz-garbage",
            startDate: "2028-10-01");
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now (October 2028)", cut.Markup);
    }

    #endregion

    #region Branch 3: Fallback → "Now (month)" without year

    [Fact]
    public void NowLabel_WithBothDatesInvalid_ShowsMonthOnly()
    {
        var data = CreateData(
            currentMonth: "April",
            nowDate: "invalid",
            startDate: "also-invalid");
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now (April)", cut.Markup);
        Assert.DoesNotContain("Now (April 20", cut.Markup);
    }

    [Fact]
    public void NowLabel_WithEmptyDates_ShowsMonthOnly()
    {
        var data = CreateData(currentMonth: "June", nowDate: "", startDate: "");
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now (June)", cut.Markup);
    }

    [Fact]
    public void NowLabel_WithNullTimeline_ShowsMonthOnly()
    {
        var data = CreateData(currentMonth: "September", useNullTimeline: true);
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now (September)", cut.Markup);
    }

    [Fact]
    public void NowLabel_WithEmptyCurrentMonth_ShowsEmptyParens()
    {
        var data = CreateData(currentMonth: "", useNullTimeline: true);
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now ()", cut.Markup);
    }

    [Fact]
    public void NowLabel_WithEmptyCurrentMonthAndValidNowDate_ShowsEmptyMonthWithYear()
    {
        var data = CreateData(currentMonth: "", nowDate: "2026-04-10");
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now ( 2026)", cut.Markup);
    }

    #endregion

    #region NowLabel Edge Cases

    [Fact]
    public void NowLabel_WithLeapYearDate_ParsesCorrectly()
    {
        var data = CreateData(currentMonth: "February", nowDate: "2028-02-29");
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now (February 2028)", cut.Markup);
    }

    [Fact]
    public void NowLabel_WithDateTimeFormatNowDate_ParsesCorrectly()
    {
        var data = CreateData(currentMonth: "March", nowDate: "2026-03-15T10:30:00");
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now (March 2026)", cut.Markup);
    }

    [Fact]
    public void NowLabel_PrefersNowDateOverStartDate()
    {
        // NowDate is 2027, StartDate is 2026 - should use NowDate's year
        var data = CreateData(
            currentMonth: "April",
            nowDate: "2027-04-10",
            startDate: "2026-01-01");
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        Assert.Contains("Now (April 2027)", cut.Markup);
        Assert.DoesNotContain("Now (April 2026)", cut.Markup);
    }

    [Fact]
    public void NowLabel_TimelineWithNullNowDateProperty_FallsBackToStartDate()
    {
        var timeline = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = null!,
            Tracks = new List<TimelineTrack>()
        };
        var data = CreateData(currentMonth: "April", timeline: timeline);
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        // NowDate is null, DateTime.TryParse(null) returns false,
        // falls to StartDate branch
        Assert.Contains("Now (April 2026)", cut.Markup);
    }

    #endregion
}