using Bunit;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Services.Models;

namespace AgentSquad.Tests;

public class MilestoneTimelineTests : TestContext
{
    [Fact]
    public void MilestoneTimeline_DisplaysMilestoneNameDateAndStatus()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Phase 1",
                TargetDate = new DateTime(2026, 04, 15),
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones)
            .Add(p => p.ProjectDurationDays, 180)
            .Add(p => p.ProjectStartDate, new DateTime(2026, 02, 15))
            .Add(p => p.ProjectEndDate, new DateTime(2026, 08, 15)));

        component.MarkupMatches(@"<p class=""target-date"">2026-04-15</p>");
        component.Find(".milestone-name").TextContent.Should().Contain("Phase 1");
    }

    [Fact]
    public void MilestoneTimeline_ShowsGreenColorForCompletedStatus()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Completed Task",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var badge = component.Find(".status-indicator");
        badge.ClassList.Should().Contain("badge-completed");
    }

    [Fact]
    public void MilestoneTimeline_ShowsBlueColorForInProgressStatus()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m2",
                Name = "In Progress Task",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.InProgress,
                CompletionPercentage = 50
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var badge = component.Find(".status-indicator");
        badge.ClassList.Should().Contain("badge-in-progress");
    }

    [Fact]
    public void MilestoneTimeline_ShowsGrayColorForPendingStatus()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m3",
                Name = "Pending Task",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Pending,
                CompletionPercentage = 0
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var badge = component.Find(".status-indicator");
        badge.ClassList.Should().Contain("badge-pending");
    }

    [Fact]
    public void MilestoneTimeline_DisplaysNoMilestonesMessage_WhenListEmpty()
    {
        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, new List<Milestone>()));

        var noMilestones = component.Find(".no-milestones");
        noMilestones.TextContent.Should().Contain("No milestones available");
    }

    [Fact]
    public void MilestoneTimeline_DisplaysCompletionPercentage_WhenGreaterThanZero()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Phase 1",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.InProgress,
                CompletionPercentage = 65
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var completionValue = component.Find(".completion-value");
        completionValue.TextContent.Should().Contain("65%");
    }

    [Fact]
    public void MilestoneTimeline_HasResponsiveLayout()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Test Milestone",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var container = component.Find(".milestone-timeline-container");
        container.ClassList.Should().Contain("milestone-timeline-container");
        component.Find(".timeline-wrapper").Should().NotBeNull();
    }

    [Fact]
    public void MilestoneTimeline_DefaultParameterValues()
    {
        var component = RenderComponent<MilestoneTimeline>();

        component.Instance.Milestones.Should().NotBeNull();
        component.Instance.Milestones.Count.Should().Be(0);
        component.Instance.ProjectDurationDays.Should().Be(180);
        component.Instance.ProjectStartDate.Should().NotBe(default(DateTime));
        component.Instance.ProjectEndDate.Should().NotBe(default(DateTime));
    }
}