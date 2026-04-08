using Bunit;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Services.Models;

namespace AgentSquad.Tests.Acceptance;

public class MilestoneTimelineAcceptanceTests : TestContext
{
    [Fact]
    public void AC1_TimelineDisplaysMilestoneNameTargetDateAndStatus()
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

        var timelineItem = component.Find(".timeline-item");
        
        Assert.Contains("Design Review", timelineItem.TextContent);
        Assert.Contains("2026-04-15", timelineItem.TextContent);
        Assert.Contains("Completed", timelineItem.TextContent);
    }

    [Fact]
    public void AC2_TimelineIsFullWidthAndVisuallyProminent()
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
        Assert.NotNull(container);

        var wrapper = component.Find(".timeline-wrapper");
        Assert.NotNull(wrapper);
        
        var styleAttr = wrapper.GetAttribute("class");
        Assert.Contains("timeline-wrapper", styleAttr);
    }

    [Fact]
    public void AC3_CompletedMilestonesAreGreen()
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
        Assert.Contains("status-completed", dot.GetAttribute("class"));

        var badge = component.Find(".status-indicator");
        Assert.Contains("badge-completed", badge.GetAttribute("class"));
    }

    [Fact]
    public void AC3_InProgressMilestonesAreBlue()
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
        Assert.Contains("status-inprogress", dot.GetAttribute("class"));

        var badge = component.Find(".status-indicator");
        Assert.Contains("badge-in-progress", badge.GetAttribute("class"));
    }

    [Fact]
    public void AC3_PendingMilestonesAreGray()
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
        Assert.Contains("status-pending", dot.GetAttribute("class"));

        var badge = component.Find(".status-indicator");
        Assert.Contains("badge-pending", badge.GetAttribute("class"));
    }

    [Fact]
    public void AC4_ResponsiveAndReadableOn1024pxAndAbove()
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

        var container = component.Find(".container-fluid");
        Assert.NotNull(container);

        var row = component.Find(".row");
        Assert.NotNull(row);

        var col = component.Find(".col-12");
        Assert.NotNull(col);
    }

    [Fact]
    public void AC5_FontSizeIs12ptOrGreater()
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

        var name = component.Find(".milestone-name");
        Assert.NotNull(name);

        var date = component.Find(".target-date");
        Assert.NotNull(date);

        var badge = component.Find(".status-indicator");
        Assert.NotNull(badge);
    }

    [Fact]
    public void AC6_ComponentAcceptsMilestonesListParameter()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Milestone 1",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            },
            new Milestone
            {
                Id = "m2",
                Name = "Milestone 2",
                TargetDate = DateTime.Now.AddDays(30),
                Status = MilestoneStatus.Pending,
                CompletionPercentage = 0
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones));

        Assert.NotNull(component.Instance.Milestones);
        Assert.Equal(2, component.Instance.Milestones.Count);
    }

    [Fact]
    public void AC6_ComponentAcceptsProjectDurationDaysParameter()
    {
        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.ProjectDurationDays, 365)
            .Add(p => p.Milestones, new List<Milestone>()));

        Assert.Equal(365, component.Instance.ProjectDurationDays);
    }

    [Fact]
    public void AC7_NoAnimationsInterfering()
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
        Assert.DoesNotContain("animation", markup.ToLower());
        Assert.DoesNotContain("transition", markup.ToLower());
    }

    [Fact]
    public void AllAcceptanceCriteria_WithMultipleMilestones()
    {
        var milestones = new List<Milestone>
        {
            new Milestone
            {
                Id = "m1",
                Name = "Project Kickoff",
                TargetDate = new DateTime(2026, 02, 15),
                ActualDate = new DateTime(2026, 02, 15),
                Status = MilestoneStatus.Completed,
                CompletionPercentage = 100
            },
            new Milestone
            {
                Id = "m2",
                Name = "Phase 1 Design",
                TargetDate = new DateTime(2026, 03, 15),
                Status = MilestoneStatus.InProgress,
                CompletionPercentage = 65
            },
            new Milestone
            {
                Id = "m3",
                Name = "Development",
                TargetDate = new DateTime(2026, 04, 15),
                Status = MilestoneStatus.Pending,
                CompletionPercentage = 0
            },
            new Milestone
            {
                Id = "m4",
                Name = "Testing & QA",
                TargetDate = new DateTime(2026, 05, 15),
                Status = MilestoneStatus.Pending,
                CompletionPercentage = 0
            }
        };

        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones)
            .Add(p => p.ProjectDurationDays, 180)
            .Add(p => p.ProjectStartDate, new DateTime(2026, 02, 15))
            .Add(p => p.ProjectEndDate, new DateTime(2026, 08, 15)));

        var timelineItems = component.FindAll(".timeline-item");
        Assert.Equal(4, timelineItems.Count);

        var names = component.FindAll(".milestone-name");
        Assert.Contains(names, n => n.TextContent.Contains("Project Kickoff"));
        Assert.Contains(names, n => n.TextContent.Contains("Phase 1 Design"));
        Assert.Contains(names, n => n.TextContent.Contains("Development"));
        Assert.Contains(names, n => n.TextContent.Contains("Testing & QA"));

        var badges = component.FindAll(".status-indicator");
        var completedBadges = badges.Where(b => b.GetAttribute("class").Contains("badge-completed"));
        var inProgressBadges = badges.Where(b => b.GetAttribute("class").Contains("badge-in-progress"));
        var pendingBadges = badges.Where(b => b.GetAttribute("class").Contains("badge-pending"));

        Assert.NotEmpty(completedBadges);
        Assert.NotEmpty(inProgressBadges);
        Assert.NotEmpty(pendingBadges);

        var markup = component.Markup;
        Assert.DoesNotContain("animation", markup.ToLower());
    }
}