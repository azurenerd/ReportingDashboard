using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class MilestoneTimelineComponentTests : TestContext
{
    private static TimelineConfig BuildTimeline(List<MilestoneTrack>? milestones = null) => new()
    {
        StartDate = new DateOnly(2026, 1, 1),
        EndDate = new DateOnly(2026, 12, 31),
        Milestones = milestones ?? []
    };

    [Fact]
    public void Renders_TlAreaDiv()
    {
        var cut = RenderComponent<ReportingDashboard.Components.MilestoneTimeline>(p => p
            .Add(x => x.Timeline, BuildTimeline())
            .Add(x => x.NowDate, new DateOnly(2026, 4, 17)));
        cut.Find(".tl-area");
    }

    [Fact]
    public void Renders_MilestoneLabelForEachTrack()
    {
        var milestones = new List<MilestoneTrack>
        {
            new() { Label = "M1", Description = "Track One", Color = "#0078D4", Events = [] },
            new() { Label = "M2", Description = "Track Two", Color = "#00897B", Events = [] }
        };
        var cut = RenderComponent<ReportingDashboard.Components.MilestoneTimeline>(p => p
            .Add(x => x.Timeline, BuildTimeline(milestones))
            .Add(x => x.NowDate, new DateOnly(2026, 4, 17)));
        Assert.Contains("M1", cut.Markup);
        Assert.Contains("M2", cut.Markup);
    }

    [Fact]
    public void Renders_SvgElement()
    {
        var cut = RenderComponent<ReportingDashboard.Components.MilestoneTimeline>(p => p
            .Add(x => x.Timeline, BuildTimeline())
            .Add(x => x.NowDate, new DateOnly(2026, 4, 17)));
        Assert.Contains("<svg", cut.Markup);
    }

    [Fact]
    public void Renders_NowLineInSvg()
    {
        var cut = RenderComponent<ReportingDashboard.Components.MilestoneTimeline>(p => p
            .Add(x => x.Timeline, BuildTimeline())
            .Add(x => x.NowDate, new DateOnly(2026, 4, 17)));
        Assert.Contains("NOW", cut.Markup);
        Assert.Contains("#EA4335", cut.Markup);
    }

    [Fact]
    public void Renders_CheckpointCircleForCheckpointEvent()
    {
        var milestones = new List<MilestoneTrack>
        {
            new()
            {
                Label = "M1", Description = "Test", Color = "#0078D4",
                Events =
                [
                    new MilestoneEvent { Date = new DateOnly(2026, 6, 1), Label = "CP1", Type = MilestoneEventType.Checkpoint }
                ]
            }
        };
        var cut = RenderComponent<ReportingDashboard.Components.MilestoneTimeline>(p => p
            .Add(x => x.Timeline, BuildTimeline(milestones))
            .Add(x => x.NowDate, new DateOnly(2026, 4, 17)));
        Assert.Contains("<circle", cut.Markup);
        Assert.Contains("CP1", cut.Markup);
    }
}