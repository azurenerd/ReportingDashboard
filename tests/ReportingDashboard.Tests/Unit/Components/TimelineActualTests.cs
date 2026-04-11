using Bunit;
using ReportingDashboard.Components;
using ReportingDashboard.Models;
using System.Globalization;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Tests for the actual Timeline.razor in ReportingDashboard.Components namespace.
/// The existing TimelineTests target Components.Sections with different CSS classes (.tl-labels, .tl-label, etc.).
/// These tests match the actual inline-styled implementation.
/// </summary>
[Trait("Category", "Unit")]
public class TimelineActualTests : TestContext
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
                new() { Date = "2026-02-15", Type = "poc", Label = "Feb 15" },
                new() { Date = "2026-05-01", Type = "production", Label = "May 1" },
                new() { Date = "2026-03-15", Type = "checkpoint", Label = "Mar 15" }
            }
        }).ToList()
    };

    #region Container Structure

    [Fact]
    public void Timeline_RendersTimelineAreaDiv()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.NotNull(cut.Find(".tl-area"));
    }

    [Fact]
    public void Timeline_RendersSvgBoxDiv()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.NotNull(cut.Find(".tl-svg-box"));
    }

    [Fact]
    public void Timeline_RendersSvgElement()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.NotNull(cut.Find("svg"));
    }

    #endregion

    #region SVG Dimensions

    [Fact]
    public void Timeline_SvgWidth_Is1560()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        var svg = cut.Find("svg");
        Assert.Equal("1560", svg.GetAttribute("width"));
    }

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

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        var svg = cut.Find("svg");
        var height = double.Parse(svg.GetAttribute("height")!, CultureInfo.InvariantCulture);
        Assert.True(height >= 185);
    }

    [Fact]
    public void Timeline_ManyTracks_SvgHeightScales()
    {
        var tl = CreateBasicTimeline(10);
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        var svg = cut.Find("svg");
        var height = double.Parse(svg.GetAttribute("height")!, CultureInfo.InvariantCulture);
        // 10 tracks × 56 = 560 which is > 185
        Assert.True(height >= 560);
    }

    #endregion

    #region Track Labels

    [Fact]
    public void Timeline_RendersTrackName()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("M1", cut.Markup);
    }

    [Fact]
    public void Timeline_RendersTrackLabel()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("Track 1", cut.Markup);
    }

    [Fact]
    public void Timeline_MultipleTrack_RendersAllNames()
    {
        var tl = CreateBasicTimeline(3);
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("M1", cut.Markup);
        Assert.Contains("M2", cut.Markup);
        Assert.Contains("M3", cut.Markup);
    }

    [Fact]
    public void Timeline_TrackName_HasColor()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Test", Color = "#4285F4", Milestones = new() }
            }
        };

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("#4285F4", cut.Markup);
    }

    #endregion

    #region NOW Line

    [Fact]
    public void Timeline_WithNowDate_RendersNowText()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("NOW", cut.Markup);
    }

    [Fact]
    public void Timeline_WithNowDate_RendersRedLine()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("#EA4335", cut.Markup);
    }

    [Fact]
    public void Timeline_NowLine_HasDashedStroke()
    {
        var cut = RenderComponent<Timeline>(p =>
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

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.DoesNotContain(">NOW<", cut.Markup);
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

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.DoesNotContain(">NOW<", cut.Markup);
    }

    #endregion

    #region Milestone Markers

    [Fact]
    public void Timeline_PocMilestone_RendersGoldPolygon()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("#F4B400", cut.Markup);
        Assert.Contains("polygon", cut.Markup);
    }

    [Fact]
    public void Timeline_ProductionMilestone_RendersGreenPolygon()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("#34A853", cut.Markup);
    }

    [Fact]
    public void Timeline_CheckpointMilestone_RendersCircle()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("circle", cut.Markup);
    }

    [Fact]
    public void Timeline_MilestoneLabels_AreRendered()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("Feb 15", cut.Markup);
        Assert.Contains("May 1", cut.Markup);
        Assert.Contains("Mar 15", cut.Markup);
    }

    #endregion

    #region Drop Shadow Filter

    [Fact]
    public void Timeline_HasDropShadowFilter()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("filter", cut.Markup);
        Assert.Contains("feDropShadow", cut.Markup);
    }

    [Fact]
    public void Timeline_FilterId_IsSh()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("id=\"sh\"", cut.Markup);
    }

    #endregion

    #region Month Grid Lines

    [Fact]
    public void Timeline_RendersMonthLabelsInSvg()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        Assert.Contains("Jan", cut.Markup);
        Assert.Contains("Feb", cut.Markup);
        Assert.Contains("Mar", cut.Markup);
        Assert.Contains("Apr", cut.Markup);
        Assert.Contains("May", cut.Markup);
        Assert.Contains("Jun", cut.Markup);
        Assert.Contains("Jul", cut.Markup);
    }

    #endregion

    #region Track Lines

    [Fact]
    public void Timeline_RenderHorizontalTrackLines()
    {
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, CreateBasicTimeline()));

        // Track lines have stroke-width="3"
        Assert.Contains("stroke-width=\"3\"", cut.Markup);
    }

    [Fact]
    public void Timeline_MultipleTracksRenderMultipleLines()
    {
        var tl = CreateBasicTimeline(3);
        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        var markup = cut.Markup;
        // Each track gets a line with stroke-width="3"
        var count = markup.Split("stroke-width=\"3\"").Length - 1;
        Assert.True(count >= 3);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Timeline_NoMilestones_StillRendersTrackLine()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "2026-04-10",
            Tracks = new List<TimelineTrack>
            {
                new() { Name = "M1", Label = "Empty Track", Color = "#FF0000", Milestones = new() }
            }
        };

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.Contains("Empty Track", cut.Markup);
        Assert.Contains("#FF0000", cut.Markup);
    }

    [Fact]
    public void Timeline_EmptyTracksList_StillRendersSvg()
    {
        var tl = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-07-01",
            NowDate = "",
            Tracks = new List<TimelineTrack>()
        };

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        Assert.NotNull(cut.Find("svg"));
    }

    [Fact]
    public void Timeline_MilestoneLabel_IsHtmlEncoded()
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
                    Name = "M1",
                    Label = "Test",
                    Color = "#000",
                    Milestones = new List<Milestone>
                    {
                        new() { Date = "2026-03-01", Type = "poc", Label = "Test <script>" }
                    }
                }
            }
        };

        var cut = RenderComponent<Timeline>(p =>
            p.Add(x => x.TimelineData, tl));

        // Should be HTML-encoded, not raw script tags
        Assert.DoesNotContain("<script>", cut.Markup);
        Assert.Contains("&lt;script&gt;", cut.Markup);
    }

    #endregion
}