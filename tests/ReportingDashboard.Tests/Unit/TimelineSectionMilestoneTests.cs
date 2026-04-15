using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using ReportingDashboard.Web.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class TimelineSectionMilestoneTests : TestContext
{
    private static TimelineConfig CreateTimeline(
        List<Track>? tracks = null,
        List<Milestone>? milestones = null,
        DateTime? start = null,
        DateTime? end = null,
        DateTime? now = null)
    {
        tracks ??= new List<Track>
        {
            new Track("m1", "M1", "Core API", "#0078D4"),
            new Track("m2", "M2", "Data Layer", "#00897B"),
            new Track("m3", "M3", "Auto Review", "#546E7A")
        };

        milestones ??= new List<Milestone>();

        return new TimelineConfig(
            start ?? new DateTime(2026, 1, 1),
            end ?? new DateTime(2026, 12, 31),
            now ?? new DateTime(2026, 4, 15),
            tracks,
            milestones
        );
    }

    [Fact]
    public void PocMilestone_RendersGoldDiamondPolygon_WithDropShadow()
    {
        // Arrange
        var milestones = new List<Milestone>
        {
            new Milestone("m1", new DateTime(2026, 3, 26), "Mar 26 PoC", "poc", null)
        };
        var timeline = CreateTimeline(milestones: milestones);

        // Act
        var cut = RenderComponent<TimelineSection>(p => p.Add(x => x.Timeline, timeline));

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("<polygon");
        markup.Should().Contain("fill=\"#F4B400\"");
        markup.Should().Contain("filter=\"url(#sh)\"");
        markup.Should().Contain("Mar 26 PoC");
    }

    [Fact]
    public void ProductionMilestone_RendersGreenDiamondPolygon_WithDropShadow()
    {
        // Arrange
        var milestones = new List<Milestone>
        {
            new Milestone("m1", new DateTime(2026, 6, 15), "Jun 15 GA", "production", null)
        };
        var timeline = CreateTimeline(milestones: milestones);

        // Act
        var cut = RenderComponent<TimelineSection>(p => p.Add(x => x.Timeline, timeline));

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("<polygon");
        markup.Should().Contain("fill=\"#34A853\"");
        markup.Should().Contain("filter=\"url(#sh)\"");
        markup.Should().Contain("Jun 15 GA");
    }

    [Fact]
    public void CheckpointMilestone_RendersOpenCircle_WithTrackColorStroke()
    {
        // Arrange
        var milestones = new List<Milestone>
        {
            new Milestone("m2", new DateTime(2026, 5, 1), "May Checkpoint", "checkpoint", null)
        };
        var timeline = CreateTimeline(milestones: milestones);

        // Act
        var cut = RenderComponent<TimelineSection>(p => p.Add(x => x.Timeline, timeline));

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("<circle");
        markup.Should().Contain("r=\"7\"");
        markup.Should().Contain("fill=\"white\"");
        markup.Should().Contain("stroke=\"#00897B\"");
        markup.Should().Contain("stroke-width=\"2.5\"");
        markup.Should().Contain("May Checkpoint");
    }

    [Fact]
    public void MinorCheckpoint_RendersSmallFilledGrayCircle()
    {
        // Arrange
        var milestones = new List<Milestone>
        {
            new Milestone("m3", new DateTime(2026, 4, 10), "Minor CP", "checkpoint-minor", null)
        };
        var timeline = CreateTimeline(milestones: milestones);

        // Act
        var cut = RenderComponent<TimelineSection>(p => p.Add(x => x.Timeline, timeline));

        // Assert
        var markup = cut.Markup;
        // Verify the minor checkpoint circle is present with correct attributes
        markup.Should().Contain("r=\"4\"");
        markup.Should().Contain("fill=\"#999\"");
        markup.Should().Contain("Minor CP");
        // Minor checkpoint circle should not have a stroke-width attribute on it
        // (track lines use stroke but the circle element for minor checkpoints should not)
        markup.Should().NotContain("r=\"4\" fill=\"#999\" stroke");
    }

    [Fact]
    public void UnmatchedTrackId_IsSilentlySkipped_NoMarkerRendered()
    {
        // Arrange
        var milestones = new List<Milestone>
        {
            new Milestone("nonexistent", new DateTime(2026, 5, 1), "Ghost", "poc", null)
        };
        var timeline = CreateTimeline(milestones: milestones);

        // Act
        var cut = RenderComponent<TimelineSection>(p => p.Add(x => x.Timeline, timeline));

        // Assert
        var markup = cut.Markup;
        markup.Should().NotContain("<polygon");
        markup.Should().NotContain("Ghost");
    }
}