using Xunit;
using Bunit;
using AgentSquad.Dashboard.Components;
using AgentSquad.Dashboard.Models;

namespace AgentSquad.Dashboard.Tests.Components;

public class MilestoneTimelineTests : TestContext
{
    [Fact]
    public void MilestoneTimeline_RendersMilestones()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "M1",
                Name = "Phase 1",
                TargetDate = DateTime.Now.AddDays(30),
                Status = MilestoneStatus.InProgress,
                CompletionPercentage = 50
            },
            new Milestone
            {
                Id = "M2",
                Name = "Phase 2",
                TargetDate = DateTime.Now.AddDays(60),
                Status = MilestoneStatus.Pending,
                CompletionPercentage = 0
            }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters
                .Add(p => p.Milestones, milestones)
                .Add(p => p.ProjectStartDate, DateTime.Now)
                .Add(p => p.ProjectEndDate, DateTime.Now.AddDays(90))
        );

        var timelineItems = component.FindAll(".timeline-item");
        Assert.Equal(2, timelineItems.Count);
    }

    [Fact]
    public void MilestoneTimeline_DisplaysMilestoneNames()
    {
        var milestones = new List<Milestone>
        {
            new Milestone { Id = "M1", Name = "Phase 1", TargetDate = DateTime.Now.AddDays(30), Status = MilestoneStatus.Completed, CompletionPercentage = 100 }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters
                .Add(p => p.Milestones, milestones)
                .Add(p => p.ProjectStartDate, DateTime.Now)
                .Add(p => p.ProjectEndDate, DateTime.Now.AddDays(90))
        );

        Assert.Contains("Phase 1", component.Markup);
    }

    [Fact]
    public void MilestoneTimeline_ShowsCompletionPercentage()
    {
        var milestones = new List<Milestone>
        {
            new Milestone { Id = "M1", Name = "Phase 1", TargetDate = DateTime.Now.AddDays(30), Status = MilestoneStatus.InProgress, CompletionPercentage = 75 }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters
                .Add(p => p.Milestones, milestones)
                .Add(p => p.ProjectStartDate, DateTime.Now)
                .Add(p => p.ProjectEndDate, DateTime.Now.AddDays(90))
        );

        Assert.Contains("75%", component.Markup);
    }

    [Fact]
    public void MilestoneTimeline_IndicatesCompletedStatus()
    {
        var milestones = new List<Milestone>
        {
            new Milestone { Id = "M1", Name = "Phase 1", TargetDate = DateTime.Now.AddDays(-10), ActualDate = DateTime.Now.AddDays(-5), Status = MilestoneStatus.Completed, CompletionPercentage = 100 }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters
                .Add(p => p.Milestones, milestones)
                .Add(p => p.ProjectStartDate, DateTime.Now.AddDays(-30))
                .Add(p => p.ProjectEndDate, DateTime.Now.AddDays(60))
        );

        var completedBadge = component.FindAll(".badge.bg-success");
        Assert.NotEmpty(completedBadge);
    }

    [Fact]
    public void MilestoneTimeline_HandleEmptyMilestonesList()
    {
        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters
                .Add(p => p.Milestones, new List<Milestone>())
                .Add(p => p.ProjectStartDate, DateTime.Now)
                .Add(p => p.ProjectEndDate, DateTime.Now.AddDays(90))
        );

        var timelineItems = component.FindAll(".timeline-item");
        Assert.Empty(timelineItems);
    }

    [Fact]
    public void MilestoneTimeline_DisplaysTargetDate()
    {
        var targetDate = new DateTime(2026, 06, 30);
        var milestones = new List<Milestone>
        {
            new Milestone { Id = "M1", Name = "Phase 1", TargetDate = targetDate, Status = MilestoneStatus.Pending, CompletionPercentage = 0 }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters
                .Add(p => p.Milestones, milestones)
                .Add(p => p.ProjectStartDate, DateTime.Now)
                .Add(p => p.ProjectEndDate, DateTime.Now.AddDays(90))
        );

        Assert.Contains("Jun 30, 2026", component.Markup);
    }

    [Fact]
    public void MilestoneTimeline_DisplaysActualDateWhenCompleted()
    {
        var actualDate = new DateTime(2026, 06, 28);
        var milestones = new List<Milestone>
        {
            new Milestone { Id = "M1", Name = "Phase 1", TargetDate = new DateTime(2026, 06, 30), ActualDate = actualDate, Status = MilestoneStatus.Completed, CompletionPercentage = 100 }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters
                .Add(p => p.Milestones, milestones)
                .Add(p => p.ProjectStartDate, DateTime.Now)
                .Add(p => p.ProjectEndDate, DateTime.Now.AddDays(90))
        );

        Assert.Contains("Completed", component.Markup);
        Assert.Contains("Jun 28, 2026", component.Markup);
    }
}