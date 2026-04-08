using System.Text.Json;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Models.Tests;

/// <summary>
/// Tests for enum types ensuring all required values are present and properly serialized.
/// Validates MilestoneStatus, WorkItemStatus, and HealthStatus enums.
/// </summary>
public class EnumTests
{
    [Fact]
    public void MilestoneStatusEnum_HasCompletedValue()
    {
        // Assert
        Assert.Equal(MilestoneStatus.Completed, MilestoneStatus.Completed);
    }

    [Fact]
    public void MilestoneStatusEnum_HasInProgressValue()
    {
        // Assert
        Assert.Equal(MilestoneStatus.InProgress, MilestoneStatus.InProgress);
    }

    [Fact]
    public void MilestoneStatusEnum_HasAtRiskValue()
    {
        // Assert
        Assert.Equal(MilestoneStatus.AtRisk, MilestoneStatus.AtRisk);
    }

    [Fact]
    public void MilestoneStatusEnum_HasFutureValue()
    {
        // Assert
        Assert.Equal(MilestoneStatus.Future, MilestoneStatus.Future);
    }

    [Fact]
    public void MilestoneStatusEnum_TotalCount_IsFour()
    {
        // Act
        var values = Enum.GetValues(typeof(MilestoneStatus)).Cast<MilestoneStatus>().ToList();

        // Assert
        Assert.Equal(4, values.Count);
    }

    [Fact]
    public void MilestoneStatusEnum_CanParseCompletedFromString()
    {
        // Act & Assert
        Assert.True(Enum.TryParse<MilestoneStatus>("Completed", out var result));
        Assert.Equal(MilestoneStatus.Completed, result);
    }

    [Fact]
    public void MilestoneStatusEnum_CanParseInProgressFromString()
    {
        // Act & Assert
        Assert.True(Enum.TryParse<MilestoneStatus>("InProgress", out var result));
        Assert.Equal(MilestoneStatus.InProgress, result);
    }

    [Fact]
    public void MilestoneStatusEnum_CanParseAtRiskFromString()
    {
        // Act & Assert
        Assert.True(Enum.TryParse<MilestoneStatus>("AtRisk", out var result));
        Assert.Equal(MilestoneStatus.AtRisk, result);
    }

    [Fact]
    public void MilestoneStatusEnum_CanParseFutureFromString()
    {
        // Act & Assert
        Assert.True(Enum.TryParse<MilestoneStatus>("Future", out var result));
        Assert.Equal(MilestoneStatus.Future, result);
    }

    [Fact]
    public void WorkItemStatusEnum_HasShippedValue()
    {
        // Assert
        Assert.Equal(WorkItemStatus.Shipped, WorkItemStatus.Shipped);
    }

    [Fact]
    public void WorkItemStatusEnum_HasInProgressValue()
    {
        // Assert
        Assert.Equal(WorkItemStatus.InProgress, WorkItemStatus.InProgress);
    }

    [Fact]
    public void WorkItemStatusEnum_HasCarriedOverValue()
    {
        // Assert
        Assert.Equal(WorkItemStatus.CarriedOver, WorkItemStatus.CarriedOver);
    }

    [Fact]
    public void WorkItemStatusEnum_TotalCount_IsThree()
    {
        // Act
        var values = Enum.GetValues(typeof(WorkItemStatus)).Cast<WorkItemStatus>().ToList();

        // Assert
        Assert.Equal(3, values.Count);
    }

    [Fact]
    public void WorkItemStatusEnum_CanParseShippedFromString()
    {
        // Act & Assert
        Assert.True(Enum.TryParse<WorkItemStatus>("Shipped", out var result));
        Assert.Equal(WorkItemStatus.Shipped, result);
    }

    [Fact]
    public void WorkItemStatusEnum_CanParseCarriedOverFromString()
    {
        // Act & Assert
        Assert.True(Enum.TryParse<WorkItemStatus>("CarriedOver", out var result));
        Assert.Equal(WorkItemStatus.CarriedOver, result);
    }

    [Fact]
    public void HealthStatusEnum_HasOnTrackValue()
    {
        // Assert
        Assert.Equal(HealthStatus.OnTrack, HealthStatus.OnTrack);
    }

    [Fact]
    public void HealthStatusEnum_HasAtRiskValue()
    {
        // Assert
        Assert.Equal(HealthStatus.AtRisk, HealthStatus.AtRisk);
    }

    [Fact]
    public void HealthStatusEnum_HasBlockedValue()
    {
        // Assert
        Assert.Equal(HealthStatus.Blocked, HealthStatus.Blocked);
    }

    [Fact]
    public void HealthStatusEnum_TotalCount_IsThree()
    {
        // Act
        var values = Enum.GetValues(typeof(HealthStatus)).Cast<HealthStatus>().ToList();

        // Assert
        Assert.Equal(3, values.Count);
    }

    [Fact]
    public void HealthStatusEnum_CanParseOnTrackFromString()
    {
        // Act & Assert
        Assert.True(Enum.TryParse<HealthStatus>("OnTrack", out var result));
        Assert.Equal(HealthStatus.OnTrack, result);
    }

    [Fact]
    public void HealthStatusEnum_CanParseBlockedFromString()
    {
        // Act & Assert
        Assert.True(Enum.TryParse<HealthStatus>("Blocked", out var result));
        Assert.Equal(HealthStatus.Blocked, result);
    }

    [Fact]
    public void MilestoneStatusEnum_JsonSerialization_PreservesValue()
    {
        // Arrange
        var milestone = new Milestone { Status = MilestoneStatus.AtRisk };

        // Act
        var json = JsonSerializer.Serialize(milestone);
        var restored = JsonSerializer.Deserialize<Milestone>(json);

        // Assert
        Assert.Equal(MilestoneStatus.AtRisk, restored.Status);
    }

    [Fact]
    public void WorkItemStatusEnum_JsonSerialization_PreservesValue()
    {
        // Arrange
        var workItem = new WorkItem { Status = WorkItemStatus.CarriedOver };

        // Act
        var json = JsonSerializer.Serialize(workItem);
        var restored = JsonSerializer.Deserialize<WorkItem>(json);

        // Assert
        Assert.Equal(WorkItemStatus.CarriedOver, restored.Status);
    }

    [Fact]
    public void HealthStatusEnum_JsonSerialization_PreservesValue()
    {
        // Arrange
        var metrics = new ProjectMetrics { HealthStatus = HealthStatus.Blocked };

        // Act
        var json = JsonSerializer.Serialize(metrics);
        var restored = JsonSerializer.Deserialize<ProjectMetrics>(json);

        // Assert
        Assert.Equal(HealthStatus.Blocked, restored.HealthStatus);
    }
}