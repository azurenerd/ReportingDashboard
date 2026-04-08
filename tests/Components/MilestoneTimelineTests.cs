using Bunit;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Services.Models;

namespace AgentSquad.Tests.Components;

public class MilestoneTimelineTests : TestContext
{
    [Fact]
    public void RendersMilestoneTimeline_WhenMilestonesProvided()
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

        component.Find(".milestone-timeline-container").Should().NotBeNull();
        component.Find(".milestone-section-title").TextContent.Should().Contain("Project Milestones");
    }

    [Fact]
    public void DisplaysMilestoneNameDateAndStatus_ForEachMilestone()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Design Review",
                TargetDate = new DateTime(2026, 04, 15),
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var timeline = component.FindAll(".timeline-item");
        timeline.Should().HaveCount(1);
        
        var milestone = timeline[0];
        milestone.TextContent.Should().Contain("Design Review");
        milestone.TextContent.Should().Contain("2026-04-15");
        milestone.TextContent.Should().Contain("Completed");
    }

    [Fact]
    public void DisplaysMultipleMilestones_InHorizontalLayout()
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
            },
            new Milestone
            {
                Id = "m2",
                Name = "Phase 2",
                TargetDate = new DateTime(2026, 05, 15),
                Status = MilestoneStatus.InProgress,
                CompletionPercentage = 50
            },
            new Milestone
            {
                Id = "m3",
                Name = "Phase 3",
                TargetDate = new DateTime(2026, 06, 15),
                Status = MilestoneStatus.Pending,
                CompletionPercentage = 0
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var timeline = component.FindAll(".timeline-item");
        timeline.Should().HaveCount(3);

        var flexContainer = component.Find(".timeline-flex-container");
        flexContainer.GetAttribute("class").Should().Contain("flex");
    }

    [Fact]
    public void AppliesGreenColorClass_ForCompletedStatus()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Completed",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var dot = component.Find(".milestone-dot");
        dot.ClassList.Should().Contain("status-completed");

        var badge = component.Find(".status-indicator");
        badge.ClassList.Should().Contain("badge-completed");
        badge.TextContent.Should().Be("COMPLETED");
    }

    [Fact]
    public void AppliesBlueColorClass_ForInProgressStatus()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "In Progress",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.InProgress,
                CompletionPercentage = 50
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var dot = component.Find(".milestone-dot");
        dot.ClassList.Should().Contain("status-inprogress");

        var badge = component.Find(".status-indicator");
        badge.ClassList.Should().Contain("badge-in-progress");
        badge.TextContent.Should().Be("IN PROGRESS");
    }

    [Fact]
    public void AppliesGrayColorClass_ForPendingStatus()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Pending",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Pending,
                CompletionPercentage = 0
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var dot = component.Find(".milestone-dot");
        dot.ClassList.Should().Contain("status-pending");

        var badge = component.Find(".status-indicator");
        badge.ClassList.Should().Contain("badge-pending");
        badge.TextContent.Should().Be("PENDING");
    }

    [Fact]
    public void DisplaysCompletionPercentage_WhenGreaterThanZero()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Milestone",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.InProgress,
                CompletionPercentage = 75
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var completion = component.Find(".completion-value");
        completion.TextContent.Should().Contain("75%");
    }

    [Fact]
    public void DoesNotDisplayCompletionPercentage_WhenZero()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Milestone",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Pending,
                CompletionPercentage = 0
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var completions = component.FindAll(".completion-value");
        completions.Should().BeEmpty();
    }

    [Fact]
    public void DisplaysNoMilestonesMessage_WhenListEmpty()
    {
        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, new List<Milestone>()));

        var noMilestones = component.Find(".no-milestones");
        noMilestones.TextContent.Should().Contain("No milestones available");
    }

    [Fact]
    public void RendersTimeline_WhenMilestonesNull()
    {
        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, null));

        var noMilestones = component.Find(".no-milestones");
        noMilestones.Should().NotBeNull();
    }

    [Fact]
    public void FormatsDates_AsYYYYMMDD()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Test",
                TargetDate = new DateTime(2026, 03, 05),
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var dateElement = component.Find(".target-date");
        dateElement.TextContent.Should().Be("2026-03-05");
    }

    [Fact]
    public void RendersFullWidthContainer()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Test",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var container = component.Find(".milestone-timeline-container");
        container.GetAttribute("class").Should().Contain("milestone-timeline-container");
    }

    [Fact]
    public void HasResponsiveDesign_WithBootstrapGrid()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Test",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var containerFluid = component.Find(".container-fluid");
        containerFluid.Should().NotBeNull();

        var row = component.Find(".row");
        row.Should().NotBeNull();

        var col = component.Find(".col-12");
        col.Should().NotBeNull();
    }

    [Fact]
    public void DefaultProjectDurationDays_Is180()
    {
        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, new List<Milestone>()));

        component.Instance.ProjectDurationDays.Should().Be(180);
    }

    [Fact]
    public void DefaultProjectStartDate_IsBeforeEndDate()
    {
        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, new List<Milestone>()));

        component.Instance.ProjectStartDate.Should().BeBefore(component.Instance.ProjectEndDate);
    }

    [Fact]
    public void AcceptsProjectDurationDaysParameter()
    {
        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.ProjectDurationDays, 365)
            .Add(p => p.Milestones, new List<Milestone>()));

        component.Instance.ProjectDurationDays.Should().Be(365);
    }

    [Fact]
    public void AcceptsProjectStartDateParameter()
    {
        var startDate = new DateTime(2026, 01, 01);
        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.ProjectStartDate, startDate)
            .Add(p => p.Milestones, new List<Milestone>()));

        component.Instance.ProjectStartDate.Should().Be(startDate);
    }

    [Fact]
    public void AcceptsProjectEndDateParameter()
    {
        var endDate = new DateTime(2026, 12, 31);
        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.ProjectEndDate, endDate)
            .Add(p => p.Milestones, new List<Milestone>()));

        component.Instance.ProjectEndDate.Should().Be(endDate);
    }

    [Fact]
    public void HandlesMilestoneWithoutCompletionPercentage()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Test",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Pending,
                CompletionPercentage = 0
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var milestone = component.Find(".timeline-item");
        milestone.Should().NotBeNull();
    }

    [Fact]
    public void HandlesMilestoneWithActualDate()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Completed Task",
                TargetDate = new DateTime(2026, 04, 15),
                ActualDate = new DateTime(2026, 04, 14),
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var milestone = component.Find(".timeline-item");
        milestone.TextContent.Should().Contain("2026-04-15");
    }

    [Fact]
    public void DisplaysSectionTitle_AsProjectMilestones()
    {
        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, new List<Milestone>()));

        var title = component.Find(".milestone-section-title");
        title.TextContent.Should().Be("Project Milestones");
    }

    [Fact]
    public void AllMilestoneItemsAreAccessible()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "First",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            },
            new Milestone
            {
                Id = "m2",
                Name = "Second",
                TargetDate = DateTime.Now.AddDays(30),
                Status = MilestoneStatus.InProgress,
                CompletionPercentage = 50
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var names = component.FindAll(".milestone-name");
        names.Should().HaveCount(2);
        names[0].TextContent.Should().Contain("First");
        names[1].TextContent.Should().Contain("Second");
    }

    [Fact]
    public void NoAnimationsInMarkup()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Test",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var markup = component.Markup;
        markup.Should().NotContain("animation");
        markup.Should().NotContain("transition");
        markup.Should().NotContain("keyframes");
    }

    [Fact]
    public void MilestoneNameIsBold()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Important Milestone",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var name = component.Find(".milestone-name");
        name.GetAttribute("class").Should().Contain("milestone-name");
    }

    [Fact]
    public void MilestoneDotHasCorrectSize()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Test",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var dot = component.Find(".milestone-dot");
        dot.GetAttribute("class").Should().Contain("milestone-dot");
    }

    [Fact]
    public void UpdatesWhenMilestonesParameterChanges()
    {
        var milestones1 = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "First",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones1));

        component.FindAll(".timeline-item").Should().HaveCount(1);

        var milestones2 = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "First",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            },
            new Milestone
            {
                Id = "m2",
                Name = "Second",
                TargetDate = DateTime.Now.AddDays(30),
                Status = MilestoneStatus.Pending,
                CompletionPercentage = 0
            }
        };

        component.SetParametersAndRender(parameters => parameters
            .Add(p => p.Milestones, milestones2));

        component.FindAll(".timeline-item").Should().HaveCount(2);
    }

    [Fact]
    public void HandlesHighCompletionPercentage()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Test",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.InProgress,
                CompletionPercentage = 99
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        var completion = component.Find(".completion-value");
        completion.TextContent.Should().Contain("99%");
    }

    [Fact]
    public void RendersWithBothOldAndNewMilestones_AfterParameterUpdate()
    {
        var milestones1 = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Old Milestone",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones1));

        var newMilestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m2",
                Name = "New Milestone",
                TargetDate = DateTime.Now.AddDays(30),
                Status = MilestoneStatus.Pending,
                CompletionPercentage = 0
            }
        };

        component.SetParametersAndRender(parameters => parameters
            .Add(p => p.Milestones, newMilestones));

        var names = component.FindAll(".milestone-name");
        names[0].TextContent.Should().Contain("New Milestone");
    }
}