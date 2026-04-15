using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using ReportingDashboard.Components.Shared;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class TimelineComponentTests : TestContext
{
    private static TimelineConfig CreateTimelineConfig(
        string startMonth = "2026-01",
        string endMonth = "2026-06",
        List<Track>? tracks = null)
    {
        return new TimelineConfig
        {
            StartMonth = startMonth,
            EndMonth = endMonth,
            Tracks = tracks ?? new List<Track>
            {
                new Track
                {
                    Id = "M1",
                    Label = "Core Platform",
                    Color = "#1A73E8",
                    Milestones = new List<Milestone>
                    {
                        new Milestone { Date = "2026-03-15", Type = "poc", Label = "PoC Done" },
                        new Milestone { Date = "2026-05-01", Type = "production", Label = "GA Release" }
                    }
                }
            }
        };
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Timeline_RendersContainerWithTlAreaClass()
    {
        var config = CreateTimelineConfig();
        var nowDate = new DateOnly(2026, 4, 15);

        var cut = RenderComponent<Timeline>(p => p
            .Add(x => x.TimelineData, config)
            .Add(x => x.NowDate, nowDate));

        cut.Find("div.tl-area").Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Timeline_RendersSidebarWithTrackLabels()
    {
        var config = CreateTimelineConfig(tracks: new List<Track>
        {
            new Track { Id = "M1", Label = "Core Platform", Color = "#1A73E8", Milestones = new List<Milestone>() },
            new Track { Id = "M2", Label = "Data Pipeline", Color = "#34A853", Milestones = new List<Milestone>() }
        });
        var nowDate = new DateOnly(2026, 4, 15);

        var cut = RenderComponent<Timeline>(p => p
            .Add(x => x.TimelineData, config)
            .Add(x => x.NowDate, nowDate));

        var markup = cut.Markup;
        markup.Should().Contain("M1");
        markup.Should().Contain("Core Platform");
        markup.Should().Contain("M2");
        markup.Should().Contain("Data Pipeline");
        markup.Should().Contain("color:#1A73E8;");
        markup.Should().Contain("color:#34A853;");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Timeline_RendersSvgWithCorrectDimensions()
    {
        var config = CreateTimelineConfig();
        var nowDate = new DateOnly(2026, 4, 15);

        var cut = RenderComponent<Timeline>(p => p
            .Add(x => x.TimelineData, config)
            .Add(x => x.NowDate, nowDate));

        var svg = cut.Find("svg");
        svg.GetAttribute("width").Should().Be("1560");
        svg.GetAttribute("height").Should().Be("185");
        svg.GetAttribute("style").Should().Contain("overflow:visible");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Timeline_RendersMilestoneTypesCorrectly()
    {
        // Source code: checkpoint uses r="7", checkpoint-small uses r="4"
        var config = CreateTimelineConfig(tracks: new List<Track>
        {
            new Track
            {
                Id = "M1",
                Label = "Test Track",
                Color = "#1A73E8",
                Milestones = new List<Milestone>
                {
                    new Milestone { Date = "2026-02-15", Type = "poc", Label = "PoC" },
                    new Milestone { Date = "2026-03-15", Type = "production", Label = "Prod" },
                    new Milestone { Date = "2026-04-15", Type = "checkpoint-small", Label = "Small CP" },
                    new Milestone { Date = "2026-05-15", Type = "checkpoint", Label = "Big CP" }
                }
            }
        });
        var nowDate = new DateOnly(2026, 4, 15);

        var cut = RenderComponent<Timeline>(p => p
            .Add(x => x.TimelineData, config)
            .Add(x => x.NowDate, nowDate));

        var markup = cut.Markup;

        // PoC diamond: gold fill with shadow filter
        markup.Should().Contain("fill=\"#F4B400\"");
        markup.Should().Contain("filter=\"url(#sh)\"");

        // Production diamond: green fill with shadow filter
        markup.Should().Contain("fill=\"#34A853\"");

        // Small checkpoint: circle r=4, fill #999
        markup.Should().Contain("r=\"4\"");
        markup.Should().Contain("fill=\"#999\"");

        // Standard checkpoint: circle r=7 (per source), white fill, track-colored stroke
        markup.Should().Contain("r=\"7\"");
        markup.Should().Contain("fill=\"white\"");
        markup.Should().Contain("stroke=\"#1A73E8\"");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Timeline_RendersNowLineWhenDateInRange()
    {
        var config = CreateTimelineConfig(startMonth: "2026-01", endMonth: "2026-06");
        var nowDate = new DateOnly(2026, 4, 15);

        var cut = RenderComponent<Timeline>(p => p
            .Add(x => x.TimelineData, config)
            .Add(x => x.NowDate, nowDate));

        var markup = cut.Markup;

        markup.Should().Contain("stroke=\"#EA4335\"");
        markup.Should().Contain("stroke-dasharray=\"5,3\"");
        markup.Should().Contain("stroke-width=\"2\"");
        markup.Should().Contain(">NOW</text>");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Timeline_NowLineClampedWhenDateOutOfRange()
    {
        // The source always renders the NOW line; GetXPosition uses Math.Clamp so
        // out-of-range dates clamp to 0 or SvgWidth. The NOW line is always present.
        var config = CreateTimelineConfig(startMonth: "2026-01", endMonth: "2026-06");
        var nowDate = new DateOnly(2025, 1, 1); // Before range

        var cut = RenderComponent<Timeline>(p => p
            .Add(x => x.TimelineData, config)
            .Add(x => x.NowDate, nowDate));

        var markup = cut.Markup;

        // NOW line still renders, clamped to x=0
        markup.Should().Contain("stroke=\"#EA4335\"");
        markup.Should().Contain(">NOW</text>");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Timeline_RendersMonthGridLines()
    {
        var config = CreateTimelineConfig(startMonth: "2026-01", endMonth: "2026-06");
        var nowDate = new DateOnly(2026, 4, 15);

        var cut = RenderComponent<Timeline>(p => p
            .Add(x => x.TimelineData, config)
            .Add(x => x.NowDate, nowDate));

        var markup = cut.Markup;
        markup.Should().Contain("Jan");
        markup.Should().Contain("Feb");
        markup.Should().Contain("Mar");
        markup.Should().Contain("Apr");
        markup.Should().Contain("May");
        markup.Should().Contain("Jun");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Timeline_RendersDropShadowFilter()
    {
        var config = CreateTimelineConfig();
        var nowDate = new DateOnly(2026, 4, 15);

        var cut = RenderComponent<Timeline>(p => p
            .Add(x => x.TimelineData, config)
            .Add(x => x.NowDate, nowDate));

        var markup = cut.Markup;
        markup.Should().Contain("id=\"sh\"");
        markup.Should().Contain("feDropShadow");
        markup.Should().Contain("flood-opacity=\"0.3\"");
    }
}