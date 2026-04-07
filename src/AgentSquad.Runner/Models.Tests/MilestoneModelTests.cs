using System.Text.Json;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Models.Tests;

/// <summary>
/// Tests for Milestone model including all status types and date handling.
/// Ensures milestone data correctly serializes and deserializes.
/// </summary>
public class MilestoneModelTests
{
    [Fact]
    public void Milestone_DefaultConstruction_HasEmptyName()
    {
        // Act
        var milestone = new Milestone();

        // Assert
        Assert.Equal(string.Empty, milestone.Name);
        Assert.Null(milestone.Description);
    }

    [Fact]
    public void Milestone_ConstructWithAllProperties_AllSet()
    {
        // Arrange
        var targetDate = new DateTime(2024, 6, 30, 23, 59, 59);

        // Act
        var milestone = new Milestone
        {
            Name = "Phase 1 Launch",
            TargetDate = targetDate,
            Status = MilestoneStatus.InProgress,
            Description = "Core feature rollout"
        };

        // Assert
        Assert.Equal("Phase 1 Launch", milestone.Name);
        Assert.Equal(targetDate, milestone.TargetDate);
        Assert.Equal(MilestoneStatus.InProgress, milestone.Status);
        Assert.Equal("Core feature rollout", milestone.Description);
    }

    [Theory]
    [InlineData(MilestoneStatus.Completed)]
    [InlineData(MilestoneStatus.InProgress)]
    [InlineData(MilestoneStatus.AtRisk)]
    [InlineData(MilestoneStatus.Future)]
    public void Milestone_AllStatusValues_Deserialize(MilestoneStatus status)
    {
        // Arrange
        var statusName = status.ToString();
        var json = $@"{{
            ""name"": ""Test"",
            ""targetDate"": ""2024-06-30"",
            ""status"": ""{statusName}"",
            ""description"": ""Description""
        }}";

        // Act
        var milestone = JsonSerializer.Deserialize<Milestone>(json);

        // Assert
        Assert.NotNull(milestone);
        Assert.Equal(status, milestone.Status);
    }

    [Fact]
    public void Milestone_JsonDeserialization_CompletedStatus()
    {
        // Arrange
        var json = @"{
            ""name"": ""Phase 1"",
            ""targetDate"": ""2024-03-31"",
            ""status"": ""Completed"",
            ""description"": ""Initial launch""
        }";

        // Act
        var milestone = JsonSerializer.Deserialize<Milestone>(json);

        // Assert
        Assert.NotNull(milestone);
        Assert.Equal(MilestoneStatus.Completed, milestone.Status);
        Assert.Equal("Phase 1", milestone.Name);
    }

    [Fact]
    public void Milestone_JsonDeserialization_AtRiskStatus()
    {
        // Arrange
        var json = @"{
            ""name"": ""Phase 2"",
            ""targetDate"": ""2024-06-30"",
            ""status"": ""AtRisk"",
            ""description"": ""May miss deadline""
        }";

        // Act
        var milestone = JsonSerializer.Deserialize<Milestone>(json);

        // Assert
        Assert.NotNull(milestone);
        Assert.Equal(MilestoneStatus.AtRisk, milestone.Status);
    }

    [Fact]
    public void Milestone_NullDescription_Allowed()
    {
        // Arrange
        var json = @"{
            ""name"": ""Phase 1"",
            ""targetDate"": ""2024-03-31"",
            ""status"": ""Completed"",
            ""description"": null
        }";

        // Act
        var milestone = JsonSerializer.Deserialize<Milestone>(json);

        // Assert
        Assert.NotNull(milestone);
        Assert.Null(milestone.Description);
    }

    [Fact]
    public void Milestone_DateRange_Past()
    {
        // Arrange
        var pastDate = DateTime.Now.AddMonths(-6);

        // Act
        var milestone = new Milestone { TargetDate = pastDate };

        // Assert
        Assert.Equal(pastDate, milestone.TargetDate);
        Assert.True(milestone.TargetDate < DateTime.Now);
    }

    [Fact]
    public void Milestone_DateRange_Future()
    {
        // Arrange
        var futureDate = DateTime.Now.AddMonths(6);

        // Act
        var milestone = new Milestone { TargetDate = futureDate };

        // Assert
        Assert.Equal(futureDate, milestone.TargetDate);
        Assert.True(milestone.TargetDate > DateTime.Now);
    }

    [Fact]
    public void Milestone_Serialization_RoundtripPreservesData()
    {
        // Arrange
        var original = new Milestone
        {
            Name = "Beta Release",
            TargetDate = new DateTime(2024, 6, 30),
            Status = MilestoneStatus.InProgress,
            Description = "Public beta launch"
        };

        // Act
        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<Milestone>(json);

        // Assert
        Assert.Equal(original.Name, restored.Name);
        Assert.Equal(original.TargetDate, restored.TargetDate);
        Assert.Equal(original.Status, restored.Status);
        Assert.Equal(original.Description, restored.Description);
    }

    [Fact]
    public void Milestone_EmptyName_Allowed()
    {
        // Act
        var milestone = new Milestone { Name = string.Empty, Status = MilestoneStatus.Future };

        // Assert
        Assert.Equal(string.Empty, milestone.Name);
    }

    [Fact]
    public void Milestone_LongName_Allowed()
    {
        // Arrange
        var longName = new string('A', 500);

        // Act
        var milestone = new Milestone { Name = longName };

        // Assert
        Assert.Equal(longName, milestone.Name);
        Assert.Equal(500, milestone.Name.Length);
    }
}