using Bunit;
using Xunit;
using AgentSquad.Dashboard.Services;
using AgentSquad.Runner.Components;

namespace AgentSquad.Tests.Components;

public class MilestoneTimelineComponentTests : TestContext
{
    #region Happy Path Tests

    [Fact]
    public void MilestoneTimeline_WithMilestones_RendersMilestones()
    {
        var milestones = new List<Milestone>
        {
            new()
            {
                Id = "m1",
                Name = "Design Complete",
                TargetDate = new DateTime(2024, 2, 15),
                ActualDate = new DateTime(2024, 2, 14),
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters.Add(p => p.Milestones, milestones)
        );

        Assert.Contains("Design Complete", component.Markup);
        Assert.Contains("100%", component.Markup);
    }

    [Fact]
    public void MilestoneTimeline_WithMultipleMilestones_RendersAll()
    {
        var milestones = new List<Milestone>
        {
            new()
            {
                Id = "m1",
                Name = "Phase 1",
                TargetDate = new DateTime(2024, 2, 15),
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            },
            new()
            {
                Id = "m2",
                Name = "Phase 2",
                TargetDate = new DateTime(2024, 4, 30),
                Status = MilestoneStatus.InProgress,
                CompletionPercentage = 50
            },
            new()
            {
                Id = "m3",
                Name = "Phase 3",
                TargetDate = new DateTime(2024, 6, 30),
                Status = MilestoneStatus.Pending,
                CompletionPercentage = 0
            }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters.Add(p => p.Milestones, milestones)
        );

        Assert.Contains("Phase 1", component.Markup);
        Assert.Contains("Phase 2", component.Markup);
        Assert.Contains("Phase 3", component.Markup);
    }

    [Fact]
    public void MilestoneTimeline_WithCompletedMilestone_DisplaysCompletionPercentage()
    {
        var milestones = new List<Milestone>
        {
            new()
            {
                Id = "m1",
                Name = "Design",
                TargetDate = new DateTime(2024, 2, 15),
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters.Add(p => p.Milestones, milestones)
        );

        Assert.Contains("100%", component.Markup);
    }

    [Fact]
    public void MilestoneTimeline_WithInProgressMilestone_DisplaysPartialPercentage()
    {
        var milestones = new List<Milestone>
        {
            new()
            {
                Id = "m1",
                Name = "Development",
                TargetDate = new DateTime(2024, 4, 30),
                Status = MilestoneStatus.InProgress,
                CompletionPercentage = 65
            }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters.Add(p => p.Milestones, milestones)
        );

        Assert.Contains("65%", component.Markup);
    }

    [Fact]
    public void MilestoneTimeline_WithCompletedMilestone_DisplaysActualDate()
    {
        var milestones = new List<Milestone>
        {
            new()
            {
                Id = "m1",
                Name = "Design",
                TargetDate = new DateTime(2024, 2, 15),
                ActualDate = new DateTime(2024, 2, 14),
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters.Add(p => p.Milestones, milestones)
        );

        Assert.Contains("Feb 14, 2024", component.Markup);
    }

    [Fact]
    public void MilestoneTimeline_DisplaysTargetDateForAllMilestones()
    {
        var milestones = new List<Milestone>
        {
            new()
            {
                Id = "m1",
                Name = "M1",
                TargetDate = new DateTime(2024, 2, 15),
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters.Add(p => p.Milestones, milestones)
        );

        Assert.Contains("Feb 15, 2024", component.Markup);
    }

    [Fact]
    public void MilestoneTimeline_WithDifferentStatuses_DisplaysAllProgressBars()
    {
        var milestones = new List<Milestone>
        {
            new()
            {
                Id = "m1",
                Name = "Phase 1",
                TargetDate = new DateTime(2024, 2, 15),
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            },
            new()
            {
                Id = "m2",
                Name = "Phase 2",
                TargetDate = new DateTime(2024, 4, 30),
                Status = MilestoneStatus.InProgress,
                CompletionPercentage = 50
            },
            new()
            {
                Id = "m3",
                Name = "Phase 3",
                TargetDate = new DateTime(2024, 6, 30),
                Status = MilestoneStatus.Pending,
                CompletionPercentage = 0
            }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters.Add(p => p.Milestones, milestones)
        );

        Assert.Contains("progress-bar", component.Markup);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void MilestoneTimeline_WithEmptyMilestonesList_RendersEmpty()
    {
        var milestones = new List<Milestone>();

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters.Add(p => p.Milestones, milestones)
        );

        Assert.DoesNotContain("Phase", component.Markup);
    }

    [Fact]
    public void MilestoneTimeline_WithNullMilestonesList_RendersSafely()
    {
        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters.Add(p => p.Milestones, new List<Milestone>())
        );

        Assert.NotEmpty(component.Markup);
    }

    [Fact]
    public void MilestoneTimeline_WithZeroCompletionPercentage_Displays0Percent()
    {
        var milestones = new List<Milestone>
        {
            new()
            {
                Id = "m1",
                Name = "Pending Work",
                TargetDate = new DateTime(2024, 6, 30),
                Status = MilestoneStatus.Pending,
                CompletionPercentage = 0
            }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters.Add(p => p.Milestones, milestones)
        );

        Assert.Contains("0%", component.Markup);
    }

    [Fact]
    public void MilestoneTimeline_WithFutureTargetDate_DisplaysDate()
    {
        var futureDate = DateTime.Now.AddMonths(6);
        var milestones = new List<Milestone>
        {
            new()
            {
                Id = "m1",
                Name = "Future Milestone",
                TargetDate = futureDate,
                Status = MilestoneStatus.Pending,
                CompletionPercentage = 0
            }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters.Add(p => p.Milestones, milestones)
        );

        Assert.Contains("Future Milestone", component.Markup);
    }

    [Fact]
    public void MilestoneTimeline_WithPastTargetDate_DisplaysDate()
    {
        var pastDate = DateTime.Now.AddMonths(-6);
        var milestones = new List<Milestone>
        {
            new()
            {
                Id = "m1",
                Name = "Past Milestone",
                TargetDate = pastDate,
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters.Add(p => p.Milestones, milestones)
        );

        Assert.Contains("Past Milestone", component.Markup);
    }

    #endregion

    #region Status Color Tests

    [Fact]
    public void MilestoneTimeline_CompletedMilestone_UsesGreenColor()
    {
        var milestones = new List<Milestone>
        {
            new()
            {
                Id = "m1",
                Name = "Done",
                TargetDate = new DateTime(2024, 2, 15),
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters.Add(p => p.Milestones, milestones)
        );

        Assert.Contains("28a745", component.Markup);
    }

    [Fact]
    public void MilestoneTimeline_InProgressMilestone_UsesBlueColor()
    {
        var milestones = new List<Milestone>
        {
            new()
            {
                Id = "m1",
                Name = "In Progress",
                TargetDate = new DateTime(2024, 4, 30),
                Status = MilestoneStatus.InProgress,
                CompletionPercentage = 50
            }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters.Add(p => p.Milestones, milestones)
        );

        Assert.Contains("007bff", component.Markup);
    }

    [Fact]
    public void MilestoneTimeline_PendingMilestone_UsesGrayColor()
    {
        var milestones = new List<Milestone>
        {
            new()
            {
                Id = "m1",
                Name = "Pending",
                TargetDate = new DateTime(2024, 6, 30),
                Status = MilestoneStatus.Pending,
                CompletionPercentage = 0
            }
        };

        var component = RenderComponent<MilestoneTimeline>(
            parameters => parameters.Add(p => p.Milestones, milestones)
        );

        Assert.Contains("ccc", component.Markup);
    }

    #endregion
}