using Bunit;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for Components/Timeline.razor (root-level inline-styled SVG version).
/// This component renders an SVG timeline with track lines, milestone markers,
/// month grid lines, and a NOW indicator. All positioning is calculated from dates.
/// </summary>
[Trait("Category", "Unit")]
public class InlineTimelineTests : TestContext
{
    private static TimelineData CreateTimeline(
        string startDate = "2026-01-01",
        string endDate = "2026-07-01",
        string nowDate = "2026-04-10",
        int trackCount = 1)
    {
        return new TimelineData
        {
            StartDate = startDate,
            EndDate = endDate,
            NowDate = nowDate,
            Tracks = Enumerable.Range(1, trackCount).Select(i => new TimelineTrack
            {
                Name = $"M{i}",
                Label = $"Track {i}",
                Color = $"#00{i:D4}",
                Milestones = new List<Milestone>
                {
                    new() { Date = "2026-02-15", Type = "poc", Label = "Feb 15 PoC" },
                    new() { Date = "2026-05-01", Type = "production", Label = "May 1 GA" },
                    new() { Date = "2026-03-15", Type = "checkpoint", Label = "Mar 15 Check" }
                }
            }).ToList()
        };
    }

    #region Container Structure

    [Fact]
    public void Timeline_RendersOuterFlexContainer()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        // Outer div has display:flex and height:196px
        Assert.Contains("display:flex", cut.Markup);
        Assert.Contains("height:196px", cut.Markup);
    }

    [Fact]
    public void Timeline_RendersTrackSidebar_230pxWide()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        Assert.Contains("width:230px", cut.Markup);
    }

    [Fact]
    public void Timeline_RendersSvgBox()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        Assert.NotNull(cut.Find(".tl-svg-box"));
    }

    [Fact]
    public void Timeline_HasFafafaBackground()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        Assert.Contains("background:#FAFAFA", cut.Markup);
    }

    #endregion

    #region Track Labels

    [Fact]
    public void Timeline_RendersTrackName()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        Assert.Contains("M1", cut.Markup);
    }

    [Fact]
    public void Timeline_RendersTrackLabel()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        Assert.Contains("Track 1", cut.Markup);
    }

    [Fact]
    public void Timeline_MultipleTracksRenderAllLabels()
    {
        var tl = CreateTimeline(trackCount: 3);
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("M1", cut.Markup);
        Assert.Contains("M2", cut.Markup);
        Assert.Contains("M3", cut.Markup);
        Assert.Contains("Track 1", cut.Markup);
        Assert.Contains("Track 2", cut.Markup);
        Assert.Contains("Track 3", cut.Markup);
    }

    [Fact]
    public void Timeline_TrackNameHasColor()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Test", Color = "#FF5733", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("#FF5733", cut.Markup);
    }

    #endregion

    #region SVG Element

    [Fact]
    public void Timeline_RendersSvgElement()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        var svg = cut.Find("svg");
        Assert.NotNull(svg);
    }

    [Fact]
    public void Timeline_SvgHasWidth1560()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        var svg = cut.Find("svg");
        Assert.Equal("1560", svg.GetAttribute("width"));
    }

    [Fact]
    public void Timeline_SvgContainsDropShadowFilter()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        Assert.Contains("filter", cut.Markup);
        Assert.Contains("feDropShadow", cut.Markup);
        Assert.Contains("id=\"sh\"", cut.Markup);
    }

    [Fact]
    public void Timeline_SvgHasOverflowVisible()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        var svg = cut.Find("svg");
        var style = svg.GetAttribute("style") ?? "";
        Assert.Contains("overflow:visible", style);
    }

    #endregion

    #region Month Grid Lines and Labels

    [Fact]
    public void Timeline_RendersMonthLabels()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        // Jan-Jun range from 2026-01-01 to 2026-07-01
        Assert.Contains("Jan", cut.Markup);
        Assert.Contains("Feb", cut.Markup);
        Assert.Contains("Mar", cut.Markup);
        Assert.Contains("Apr", cut.Markup);
        Assert.Contains("May", cut.Markup);
        Assert.Contains("Jun", cut.Markup);
    }

    [Fact]
    public void Timeline_MonthGridLines_RenderedAsLines()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        var lines = cut.FindAll("line");
        // Should have grid lines, track lines, and NOW line
        Assert.True(lines.Count > 0);
    }

    #endregion

    #region Milestone Rendering

    [Fact]
    public void Timeline_PocMilestone_RenderedAsGoldDiamond()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        // PoC milestone should render a polygon with #F4B400 fill
        Assert.Contains("#F4B400", cut.Markup);
        Assert.Contains("polygon", cut.Markup);
    }

    [Fact]
    public void Timeline_ProductionMilestone_RenderedAsGreenDiamond()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        Assert.Contains("#34A853", cut.Markup);
    }

    [Fact]
    public void Timeline_CheckpointMilestone_RenderedAsCircle()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        Assert.Contains("circle", cut.Markup);
    }

    [Fact]
    public void Timeline_MilestoneLabels_RenderedAsText()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        Assert.Contains("Feb 15 PoC", cut.Markup);
        Assert.Contains("May 1 GA", cut.Markup);
        Assert.Contains("Mar 15 Check", cut.Markup);
    }

    [Fact]
    public void Timeline_MilestoneDiamonds_HaveDropShadow()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        // Diamonds reference the filter url(#sh)
        Assert.Contains("filter=\"url(#sh)\"", cut.Markup);
    }

    [Fact]
    public void Timeline_MilestoneLabels_HaveTextAnchorMiddle()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        Assert.Contains("text-anchor=\"middle\"", cut.Markup);
    }

    [Fact]
    public void Timeline_MilestoneWithInvalidDate_SkippedGracefully()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Test", Color = "#000",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "not-a-date", Type = "poc", Label = "Bad Date" },
                        new() { Date = "2026-03-15", Type = "poc", Label = "Good Date" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        // Good milestone should render, bad one should be skipped
        Assert.Contains("Good Date", cut.Markup);
    }

    [Fact]
    public void Timeline_TrackWithNoMilestones_RendersTrackLine()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Empty Track", Color = "#4285F4", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("M1", cut.Markup);
        Assert.Contains("Empty Track", cut.Markup);
        // Track line should still exist
        Assert.Contains("#4285F4", cut.Markup);
    }

    #endregion

    #region NOW Line

    [Fact]
    public void Timeline_RendersNowLine()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        Assert.Contains("NOW", cut.Markup);
    }

    [Fact]
    public void Timeline_NowLine_HasRedColor()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        Assert.Contains("#EA4335", cut.Markup);
    }

    [Fact]
    public void Timeline_NowLine_HasDashedStroke()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        Assert.Contains("stroke-dasharray=\"5,3\"", cut.Markup);
    }

    [Fact]
    public void Timeline_NowLine_HasBoldLabel()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        Assert.Contains("font-weight=\"700\"", cut.Markup);
    }

    [Fact]
    public void Timeline_EmptyNowDate_NoNowLineRendered()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Test", Color = "#000", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.DoesNotContain("NOW", cut.Markup);
    }

    [Fact]
    public void Timeline_InvalidNowDate_NoNowLineRendered()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "invalid-date",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Test", Color = "#000", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.DoesNotContain("NOW", cut.Markup);
    }

    #endregion

    #region SVG Height Scaling

    [Fact]
    public void Timeline_SingleTrack_SvgHeightAtLeast185()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline(trackCount: 1)));

        var svg = cut.Find("svg");
        var height = svg.GetAttribute("height");
        Assert.NotNull(height);
        var h = double.Parse(height!);
        Assert.True(h >= 185, $"SVG height {h} should be at least 185");
    }

    [Fact]
    public void Timeline_ManyTracks_SvgHeightScalesUp()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline(trackCount: 5)));

        var svg = cut.Find("svg");
        var height = svg.GetAttribute("height");
        Assert.NotNull(height);
        var h = double.Parse(height!);
        // 5 tracks * 56 = 280, which is > 185
        Assert.True(h >= 280, $"SVG height {h} should scale for 5 tracks");
    }

    #endregion

    #region Track Lines

    [Fact]
    public void Timeline_TrackLines_UseStrokeWidth3()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        Assert.Contains("stroke-width=\"3\"", cut.Markup);
    }

    [Fact]
    public void Timeline_TrackLines_SpanFullSvgWidth()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        // Track lines go from x1=0 to x2=1560
        Assert.Contains("x2=\"1560", cut.Markup);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Timeline_EmptyTracks_RendersWithoutError()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>()
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.NotNull(cut.Find("svg"));
    }

    [Fact]
    public void Timeline_ShortDateRange_RendersAtLeastOneMonth()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-01-15",
            NowDate = "2026-01-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Short", Color = "#000", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("Jan", cut.Markup);
    }

    [Fact]
    public void Timeline_CheckpointCircle_HasWhiteFillAndTrackStroke()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Test", Color = "#4285F4",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-03-15", Type = "checkpoint", Label = "Check" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        // Checkpoint circle: fill="white" stroke="trackColor"
        Assert.Contains("fill=\"white\"", cut.Markup);
        Assert.Contains("stroke=\"#4285F4\"", cut.Markup);
        Assert.Contains("stroke-width=\"2.5\"", cut.Markup);
    }

    [Fact]
    public void Timeline_MilestoneTitle_RenderedForTooltip()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateTimeline()));

        // SVG <title> elements provide tooltips
        Assert.Contains("<title>Feb 15 PoC</title>", cut.Markup);
        Assert.Contains("<title>May 1 GA</title>", cut.Markup);
    }

    #endregion
}