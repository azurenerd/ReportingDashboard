using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using ReportingDashboard.Components.Shared;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class TimelineComponentTests : TestContext
{
    /// <summary>
    /// Creates a minimal valid TimelineConfig for testing.
    /// </summary>
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
        // Arrange
        var config = CreateTimelineConfig();
        var nowDate = new DateOnly(2026, 4, 15);

        // Act
        var cut = RenderComponent<Timeline>(p => p
            .Add(x => x.TimelineData, config)
            .Add(x => x.NowDate, nowDate));

        // Assert
        cut.Find("div.tl-area").Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Timeline_RendersSidebarWithTrackLabels()
    {
        // Arrange
        var config = CreateTimelineConfig(tracks: new List<Track>
        {
            new Track { Id = "M1", Label = "Core Platform", Color = "#1A73E8", Milestones = new List<Milestone>() },
            new Track { Id = "M2", Label = "Data Pipeline", Color = "#34A853", Milestones = new List<Milestone>() }
        });
        var nowDate = new DateOnly(2026, 4, 15);

        // Act
        var cut = RenderComponent<Timeline>(p => p
            .Add(x => x.TimelineData, config)
            .Add(x => x.NowDate, nowDate));

        // Assert — sidebar should contain track IDs and labels
        var markup = cut.Markup;
        markup.Should().Contain("M1");
        markup.Should().Contain("Core Platform");
        markup.Should().Contain("M2");
        markup.Should().Contain("Data Pipeline");

        // Track ID should be rendered in the track's color
        markup.Should().Contain("color:#1A73E8;");
        markup.Should().Contain("color:#34A853;");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Timeline_RendersSvgWithCorrectDimensions()
    {
        // Arrange
        var config = CreateTimelineConfig();
        var nowDate = new DateOnly(2026, 4, 15);

        // Act
        var cut = RenderComponent<Timeline>(p => p
            .Add(x => x.TimelineData, config)
            .Add(x => x.NowDate, nowDate));

        // Assert
        var svg = cut.Find("svg");
        svg.GetAttribute("width").Should().Be("1560");
        svg.GetAttribute("height").Should().Be("185");
        svg.GetAttribute("style").Should().Contain("overflow:visible");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Timeline_RendersMilestoneTypesCorrectly()
    {
        // Arrange — one of each milestone type on a single track
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
        var nowDate = new DateOnly(2026, 1, 1); // Before range so NOW line won't render

        // Act
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

        // Standard checkpoint: circle r=6, white fill, track-colored stroke
        markup.Should().Contain("r=\"6\"");
        markup.Should().Contain("fill=\"white\"");
        markup.Should().Contain("stroke=\"#1A73E8\"");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Timeline_RendersNowLineWhenDateInRange()
    {
        // Arrange
        var config = CreateTimelineConfig(startMonth: "2026-01", endMonth: "2026-06");
        var nowDate = new DateOnly(2026, 4, 15); // Mid-range

        // Act
        var cut = RenderComponent<Timeline>(p => p
            .Add(x => x.TimelineData, config)
            .Add(x => x.NowDate, nowDate));

        var markup = cut.Markup;

        // NOW line: dashed red line
        markup.Should().Contain("stroke=\"#EA4335\"");
        markup.Should().Contain("stroke-dasharray=\"5,3\"");
        markup.Should().Contain("stroke-width=\"2\"");

        // NOW label
        markup.Should().Contain(">NOW</text>");
    }
}