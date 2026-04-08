using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Tests.Models;

public class MilestoneTests
{
    [Fact]
    public void Milestone_NewInstance_HasValidDefaults()
    {
        // Act
        var milestone = new Milestone();

        // Assert
        Assert.NotEmpty(milestone.Id);
        Assert.Equal(string.Empty, milestone.Name);
        Assert.Equal(MilestoneStatus.Pending, milestone.Status);
        Assert.Equal(0, milestone.CompletionPercentage);
    }

    [Fact]
    public void Milestone_WithProperties_StoresValuesCorrectly()
    {
        // Arrange & Act
        var targetDate = new DateTime(2024, 6, 30);
        var milestone = new Milestone
        {
            Id = "milestone-1",
            Name = "Phase 1 Complete",
            TargetDate = targetDate,
            Status = MilestoneStatus.Completed,
            CompletionPercentage = 100
        };

        // Assert
        Assert.Equal("milestone-1", milestone.Id);
        Assert.Equal("Phase 1 Complete", milestone.Name);
        Assert.Equal(targetDate, milestone.TargetDate);
        Assert.Equal(MilestoneStatus.Completed, milestone.Status);
        Assert.Equal(100, milestone.CompletionPercentage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Milestone_WithDifferentCompletionPercentages_StoresValuesCorrectly(int percentage)
    {
        // Arrange & Act
        var milestone = new Milestone { CompletionPercentage = percentage };

        // Assert
        Assert.Equal(percentage, milestone.CompletionPercentage);
    }
}