using System.Text.Json;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Models.Tests;

public class ModelJsonRoundtripTests
{
    [Fact]
    public void TestProjectJsonRoundtrip_SerializeDeserialize_Matches()
    {
        // Arrange
        var original = new Project
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = new DateTime(2024, 1, 1),
            TargetEndDate = new DateTime(2024, 12, 31),
            CompletionPercentage = 50,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 10,
            Milestones = [],
            WorkItems = []
        };

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Project>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.Description, deserialized.Description);
        Assert.Equal(original.CompletionPercentage, deserialized.CompletionPercentage);
        Assert.Equal(original.HealthStatus, deserialized.HealthStatus);
    }

    [Fact]
    public void TestMilestoneJsonRoundtrip_SerializeDeserialize_Matches()
    {
        // Arrange
        var original = new Milestone
        {
            Name = "Phase 1",
            TargetDate = new DateTime(2024, 6, 30),
            Status = MilestoneStatus.InProgress,
            Description = "Launch phase"
        };

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<Milestone>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Name, deserialized.Name);
        Assert.Equal(original.TargetDate, deserialized.TargetDate);
        Assert.Equal(original.Status, deserialized.Status);
        Assert.Equal(original.Description, deserialized.Description);
    }

    [Fact]
    public void TestWorkItemJsonRoundtrip_SerializeDeserialize_Matches()
    {
        // Arrange
        var original = new WorkItem
        {
            Title = "API Implementation",
            Description = "Build endpoints",
            Status = WorkItemStatus.InProgress,
            AssignedTo = "Team A"
        };

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<WorkItem>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Title, deserialized.Title);
        Assert.Equal(original.Description, deserialized.Description);
        Assert.Equal(original.Status, deserialized.Status);
        Assert.Equal(original.AssignedTo, deserialized.AssignedTo);
    }
}