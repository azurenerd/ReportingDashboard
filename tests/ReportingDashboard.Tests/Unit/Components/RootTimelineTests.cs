using Bunit;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Unit tests for Components/Timeline.razor (root-level inline SVG version from PR #521).
/// This component renders SVG with computed positions for milestones, track lines, and NOW marker.
/// </summary>
[Trait("Category", "Unit")]
public class RootTimelineTests : TestContext
{
    private static TimelineData CreateBasicTimeline(int trackCount = 1) => new()
    {
        StartDate = "2026-01-01",
        EndDate = "2026-07-01",
        NowDate = "2026-04-10",
        Tracks = Enumerable.Range(1, trackCount).Select(i => new TimelineTrack
        {
            Name = $"M{i}",
            Label = $"Track {i}",
            Color = $"#{i:D2}85F4",
            Milestones = new List<Milestone>
            {
                new() { Date = "2026-02-15", Type = "poc", Label = $"Feb 15 PoC {i}" },
                new() { Date = "2026-05-01", Type = "production", Label = $"May 1 GA {i}" },
                new() { Date = "2026-03-15", Type = "checkpoint", Label = $"Mar 15 Check {i}" }
            }
        }).ToList()
    };

    #region Structure

    [Fact]
    public void Timeline_RendersTimelineArea()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.NotNull(cut.Find(".tl-area"));
    }

    [Fact]
    public void Timeline_RendersSvgBox()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.NotNull(cut.Find(".tl-svg-box"));
    }

    [Fact]
    public void Timeline_RendersSvgElement()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        var svg = cut.Find("svg");
        Assert.NotNull(svg);
    }

    [Fact]
    public void Timeline_SvgWidthIs1560()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        var svg = cut.Find("svg");
        Assert.Equal("1560", svg.GetAttribute("width"));
    }

    #endregion

    #region Track Sidebar

    [Fact]
    public void Timeline_Sidebar_HasBorderRight()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        // Sidebar is first div inside tl-area with width:230px
        Assert.Contains("border-right:1px solid #E0E0E0", cut.Markup);
    }

    [Fact]
    public void Timeline_Sidebar_RendersTrackNames()
    {
        var tl = CreateBasicTimeline(3);
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("M1", cut.Markup);
        Assert.Contains("Track 1", cut.Markup);
        Assert.Contains("M2", cut.Markup);
        Assert.Contains("Track 2", cut.Markup);
        Assert.Contains("M3", cut.Markup);
        Assert.Contains("Track 3", cut.Markup);
    }

    [Fact]
    public void Timeline_Sidebar_TrackNameHasColor()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Core", Color = "#4285F4", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("color:#4285F4", cut.Markup);
    }

    [Fact]
    public void Timeline_SingleTrack_RendersSingleEntry()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline(1)));

        // Count track name occurrences
        Assert.Contains("M1", cut.Markup);
        Assert.DoesNotContain("M2", cut.Markup);
    }

    #endregion

    #region SVG Milestones

    [Fact]
    public void Timeline_PocMilestone_RendersGoldPolygon()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("#F4B400", cut.Markup);
        Assert.Contains("polygon", cut.Markup);
    }

    [Fact]
    public void Timeline_ProductionMilestone_RendersGreenPolygon()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("#34A853", cut.Markup);
    }

    [Fact]
    public void Timeline_CheckpointMilestone_RendersCircle()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("circle", cut.Markup);
    }

    [Fact]
    public void Timeline_MilestoneLabels_RenderedAsText()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("Feb 15 PoC 1", cut.Markup);
        Assert.Contains("May 1 GA 1", cut.Markup);
        Assert.Contains("Mar 15 Check 1", cut.Markup);
    }

    [Fact]
    public void Timeline_MilestoneLabels_AreHtmlEncoded()
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
                        new() { Date = "2026-03-01", Type = "poc", Label = "Test <script>" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        // WebUtility.HtmlEncode is used in the component
        Assert.DoesNotContain("<script>", cut.Markup);
    }

    #endregion

    #region NOW Line

    [Fact]
    public void Timeline_NowLine_RenderedWithRedColor()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("#EA4335", cut.Markup);
        Assert.Contains("NOW", cut.Markup);
    }

    [Fact]
    public void Timeline_NowLine_HasDashedStroke()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("stroke-dasharray=\"5,3\"", cut.Markup);
    }

    [Fact]
    public void Timeline_EmptyNowDate_NoNowLine()
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
    public void Timeline_NullNowDate_NoNowLine()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = null!,
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

    #region SVG Drop Shadow

    [Fact]
    public void Timeline_SvgContainsDropShadowFilter()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("<filter", cut.Markup);
        Assert.Contains("feDropShadow", cut.Markup);
        Assert.Contains("id=\"sh\"", cut.Markup);
    }

    [Fact]
    public void Timeline_Diamonds_UseDropShadowFilter()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("filter=\"url(#sh)\"", cut.Markup);
    }

    #endregion

    #region Month Grid Lines

    [Fact]
    public void Timeline_RendersMonthLabels()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("Jan", cut.Markup);
        Assert.Contains("Feb", cut.Markup);
        Assert.Contains("Mar", cut.Markup);
        Assert.Contains("Apr", cut.Markup);
        Assert.Contains("May", cut.Markup);
        Assert.Contains("Jun", cut.Markup);
        Assert.Contains("Jul", cut.Markup);
    }

    [Fact]
    public void Timeline_MonthGridLines_RenderAsLines()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        // Grid lines have stroke-opacity="0.4"
        Assert.Contains("stroke-opacity=\"0.4\"", cut.Markup);
    }

    #endregion

    #region Track Lines

    [Fact]
    public void Timeline_TrackLines_RenderedWithTrackColor()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Core", Color = "#FF0000", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("stroke=\"#FF0000\"", cut.Markup);
        Assert.Contains("stroke-width=\"3\"", cut.Markup);
    }

    #endregion

    #region SVG Height

    [Fact]
    public void Timeline_SingleTrack_SvgHeightMinimum185()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Test", Color = "#000", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        var svg = cut.Find("svg");
        var height = svg.GetAttribute("height");
        Assert.Equal("185", height);
    }

    [Fact]
    public void Timeline_FiveTracks_SvgHeightScales()
    {
        var tl = CreateBasicTimeline(5);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        var svg = cut.Find("svg");
        var height = double.Parse(svg.GetAttribute("height")!);
        // 5 tracks * 56 = 280 > 185
        Assert.Equal(280, height);
    }

    [Fact]
    public void Timeline_ThreeTracks_SvgHeightIs185()
    {
        // 3 * 56 = 168, which < 185, so minimum 185
        var tl = CreateBasicTimeline(3);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        var svg = cut.Find("svg");
        var height = double.Parse(svg.GetAttribute("height")!);
        Assert.Equal(185, height);
    }

    [Fact]
    public void Timeline_FourTracks_SvgHeightIs224()
    {
        // 4 * 56 = 224 > 185
        var tl = CreateBasicTimeline(4);

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        var svg = cut.Find("svg");
        var height = double.Parse(svg.GetAttribute("height")!);
        Assert.Equal(224, height);
    }

    #endregion

    #region Multiple Tracks

    [Fact]
    public void Timeline_MultipleTracks_RendersAllTrackLines()
    {
        var tl = CreateBasicTimeline(3);
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        // Each track should have its own horizontal line with stroke-width="3"
        Assert.Contains("#0185F4", cut.Markup);
        Assert.Contains("#0285F4", cut.Markup);
        Assert.Contains("#0385F4", cut.Markup);
    }

    [Fact]
    public void Timeline_MultipleTracks_EachHasMilestones()
    {
        var tl = CreateBasicTimeline(2);
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("Feb 15 PoC 1", cut.Markup);
        Assert.Contains("Feb 15 PoC 2", cut.Markup);
        Assert.Contains("May 1 GA 1", cut.Markup);
        Assert.Contains("May 1 GA 2", cut.Markup);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Timeline_NoTracks_RendersSvgWithoutCrash()
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
    public void Timeline_TrackWithNoMilestones_RendersLineOnly()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Empty Track", Color = "#ABC123", Milestones = new() }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("stroke=\"#ABC123\"", cut.Markup);
        Assert.Contains("Empty Track", cut.Markup);
        // No diamond polygons for this track
    }

    [Fact]
    public void Timeline_CheckpointCircle_HasWhiteFillAndTrackStroke()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "",
            Tracks = new List<TimelineTrack>
            {
                new()
                {
                    Name = "M1", Label = "Test", Color = "#FF5733",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-03-01", Type = "checkpoint", Label = "Check" }
                    }
                }
            }
        };

        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("fill=\"white\"", cut.Markup);
        Assert.Contains("stroke=\"#FF5733\"", cut.Markup);
        Assert.Contains("stroke-width=\"2.5\"", cut.Markup);
    }

    [Fact]
    public void Timeline_NowLine_HasStrokeWidth2()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("stroke-width=\"2\"", cut.Markup);
    }

    [Fact]
    public void Timeline_SvgHasOverflowVisible()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        var svg = cut.Find("svg");
        var style = svg.GetAttribute("style") ?? "";
        Assert.Contains("overflow:visible", style);
    }

    [Fact]
    public void Timeline_DefaultTimelineData_DoesNotThrow()
    {
        // Default TimelineData has empty dates - should not crash before accessing SVG
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>();
        Assert.NotNull(cut.Find(".tl-area"));
    }

    #endregion

    #region Milestone Title Tooltips

    [Fact]
    public void Timeline_PocDiamond_HasTitleTooltip()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("<title>Feb 15 PoC 1</title>", cut.Markup);
    }

    [Fact]
    public void Timeline_ProductionDiamond_HasTitleTooltip()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("<title>May 1 GA 1</title>", cut.Markup);
    }

    [Fact]
    public void Timeline_Checkpoint_HasTitleTooltip()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("<title>Mar 15 Check 1</title>", cut.Markup);
    }

    #endregion
}