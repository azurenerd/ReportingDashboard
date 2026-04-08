using Bunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Data;
using Xunit;

namespace AgentSquad.Runner.Tests.Components;

public class MilestoneTimelineTests : TestContext
{
    [Fact]
    public void MilestoneTimeline_WithEmptyList_DisplaysPlaceholder()
    {
        // Arrange
        var milestones = new List<Milestone>();

        // Act
        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones)
            .Add(p => p.ProjectStartDate, DateTime.Now)
            .Add(p => p.ProjectEndDate, DateTime.Now.AddMonths(6)));

        // Assert
        Assert.NotNull(component);
        var placeholder = component.Find(".timeline-placeholder");
        Assert.NotNull(placeholder);
        Assert.Contains("No milestones defined", placeholder.TextContent);
    }

    [Fact]
    public void MilestoneTimeline_WithMilestones_DisplaysAllMilestones()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 6, 30);
        var milestones = new List<Milestone>
        {
            new() { Id = "m1", Name = "Design", TargetDate = new DateTime(2024, 2, 1), Status = MilestoneStatus.Completed, CompletionPercentage = 100 },
            new() { Id = "m2", Name = "Development", TargetDate = new DateTime(2024, 4, 1), Status = MilestoneStatus.InProgress, CompletionPercentage = 50 },
            new() { Id = "m3", Name = "Testing", TargetDate = new DateTime(2024, 5, 1), Status = MilestoneStatus.Pending, CompletionPercentage = 0 }
        };

        // Act
        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones)
            .Add(p => p.ProjectStartDate, startDate)
            .Add(p => p.ProjectEndDate, endDate));

        // Assert
        var milestoneElements = component.FindAll(".milestone");
        Assert.Equal(3, milestoneElements.Count);
    }

    [Fact]
    public void MilestoneTimeline_DisplaysCorrectStatusClasses()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 6, 30);
        var milestones = new List<Milestone>
        {
            new() { Id = "m1", Name = "Design", TargetDate = new DateTime(2024, 2, 1), Status = MilestoneStatus.Completed, CompletionPercentage = 100 },
            new() { Id = "m2", Name = "Development", TargetDate = new DateTime(2024, 4, 1), Status = MilestoneStatus.InProgress, CompletionPercentage = 50 }
        };

        // Act
        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones)
            .Add(p => p.ProjectStartDate, startDate)
            .Add(p => p.ProjectEndDate, endDate));

        // Assert
        var completedMilestone = component.Find(".milestone.completed");
        Assert.NotNull(completedMilestone);
        var inProgressMilestone = component.Find(".milestone.in-progress");
        Assert.NotNull(inProgressMilestone);
    }

    [Fact]
    public void MilestoneTimeline_DisplaysCompletionPercentage()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 6, 30);
        var milestones = new List<Milestone>
        {
            new() { Id = "m1", Name = "Design", TargetDate = new DateTime(2024, 2, 1), Status = MilestoneStatus.InProgress, CompletionPercentage = 75 }
        };

        // Act
        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones)
            .Add(p => p.ProjectStartDate, startDate)
            .Add(p => p.ProjectEndDate, endDate));

        // Assert
        var content = component.Markup;
        Assert.Contains("75% complete", content);
    }

    [Fact]
    public void MilestoneTimeline_DisplaysDateRange()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 15);
        var endDate = new DateTime(2024, 6, 30);
        var milestones = new List<Milestone>();

        // Act
        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones)
            .Add(p => p.ProjectStartDate, startDate)
            .Add(p => p.ProjectEndDate, endDate));

        // Assert
        var content = component.Markup;
        Assert.Contains("Jan 2024", content);
        Assert.Contains("Jun 2024", content);
    }

    [Fact]
    public void MilestoneTimeline_CalculatesMilestonePosition()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);
        var milestones = new List<Milestone>
        {
            new() { Id = "m1", Name = "Midyear", TargetDate = new DateTime(2024, 7, 1), Status = MilestoneStatus.Pending, CompletionPercentage = 0 }
        };

        // Act
        var component = RenderComponent<MilestoneTimeline>(parameters => parameters
            .Add(p => p.Milestones, milestones)
            .Add(p => p.ProjectStartDate, startDate)
            .Add(p => p.ProjectEndDate, endDate));

        // Assert
        var milestoneElement = component.Find(".milestone");
        var style = milestoneElement.GetAttribute("style");
        // Midyear should be approximately 50% through the year
        Assert.NotNull(style);
        Assert.Contains("left:", style);
    }
}