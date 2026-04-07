using System.Text.Json;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Models.Tests;

/// <summary>
/// Tests for ProjectMetrics model including boundary conditions for KPI values.
/// Validates metrics used for executive dashboard health indicators.
/// </summary>
public class ProjectMetricsModelTests
{
    [Fact]
    public void ProjectMetrics_DefaultConstruction_AllPropertiesInitialized()
    {
        // Act
        var metrics = new ProjectMetrics();

        // Assert
        Assert.Equal(0, metrics.CompletionPercentage);
        Assert.Equal(default(HealthStatus), metrics.HealthStatus);
        Assert.Equal(0, metrics.VelocityThisMonth);
        Assert.Equal(0, metrics.TotalMilestones);
        Assert.Equal(0, metrics.CompletedMilestones);
    }

    [Fact]
    public void ProjectMetrics_ConstructWithAllProperties_AllSet()
    {
        // Act
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 75,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 12,
            TotalMilestones = 10,
            CompletedMilestones = 7
        };

        // Assert
        Assert.Equal(75, metrics.CompletionPercentage);
        Assert.Equal(HealthStatus.OnTrack, metrics.HealthStatus);
        Assert.Equal(12, metrics.VelocityThisMonth);
        Assert.Equal(10, metrics.TotalMilestones);
        Assert.Equal(7, metrics.CompletedMilestones);
    }

    [Fact]
    public void ProjectMetrics_CompletionPercentage_MinimumBoundary()
    {
        // Act
        var metrics = new ProjectMetrics { CompletionPercentage = 0 };

        // Assert
        Assert.Equal(0, metrics.CompletionPercentage);
    }

    [Fact]
    public void ProjectMetrics_CompletionPercentage_MaximumBoundary()
    {
        // Act
        var metrics = new ProjectMetrics { CompletionPercentage = 100 };

        // Assert
        Assert.Equal(100, metrics.CompletionPercentage);
    }

    [Fact]
    public void ProjectMetrics_CompletionPercentage_MidRange()
    {
        // Act
        var metrics = new ProjectMetrics { CompletionPercentage = 50 };

        // Assert
        Assert.Equal(50, metrics.CompletionPercentage);
    }

    [Fact]
    public void ProjectMetrics_VelocityThisMonth_ZeroValue()
    {
        // Act
        var metrics = new ProjectMetrics { VelocityThisMonth = 0 };

        // Assert
        Assert.Equal(0, metrics.VelocityThisMonth);
    }

    [Fact]
    public void ProjectMetrics_VelocityThisMonth_HighValue()
    {
        // Act
        var metrics = new ProjectMetrics { VelocityThisMonth = 999 };

        // Assert
        Assert.Equal(999, metrics.VelocityThisMonth);
    }

    [Theory]
    [InlineData(HealthStatus.OnTrack)]
    [InlineData(HealthStatus.AtRisk)]
    [InlineData(HealthStatus.Blocked)]
    public void ProjectMetrics_AllHealthStatuses_Supported(HealthStatus status)
    {
        // Act
        var metrics = new ProjectMetrics { HealthStatus = status };

        // Assert
        Assert.Equal(status, metrics.HealthStatus);
    }

    [Fact]
    public void ProjectMetrics_MilestonesCounts_CompletedNotExceedTotal()
    {
        // Act
        var metrics = new ProjectMetrics
        {
            TotalMilestones = 10,
            CompletedMilestones = 5
        };

        // Assert
        Assert.True(metrics.CompletedMilestones <= metrics.TotalMilestones);
    }

    [Fact]
    public void ProjectMetrics_JsonDeserialization_AllProperties()
    {
        // Arrange
        var json = @"{
            ""completionPercentage"": 60,
            ""healthStatus"": ""AtRisk"",
            ""velocityThisMonth"": 9,
            ""totalMilestones"": 8,
            ""completedMilestones"": 4
        }";

        // Act
        var metrics = JsonSerializer.Deserialize<ProjectMetrics>(json);

        // Assert
        Assert.NotNull(metrics);
        Assert.Equal(60, metrics.CompletionPercentage);
        Assert.Equal(HealthStatus.AtRisk, metrics.HealthStatus);
        Assert.Equal(9, metrics.VelocityThisMonth);
        Assert.Equal(8, metrics.TotalMilestones);
        Assert.Equal(4, metrics.CompletedMilestones);
    }

    [Fact]
    public void ProjectMetrics_Serialization_RoundtripPreservesData()
    {
        // Arrange
        var original = new ProjectMetrics
        {
            CompletionPercentage = 35,
            HealthStatus = HealthStatus.Blocked,
            VelocityThisMonth = 3,
            TotalMilestones = 20,
            CompletedMilestones = 7
        };

        // Act
        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<ProjectMetrics>(json);

        // Assert
        Assert.Equal(original.CompletionPercentage, restored.CompletionPercentage);
        Assert.Equal(original.HealthStatus, restored.HealthStatus);
        Assert.Equal(original.VelocityThisMonth, restored.VelocityThisMonth);
        Assert.Equal(original.TotalMilestones, restored.TotalMilestones);
        Assert.Equal(original.CompletedMilestones, restored.CompletedMilestones);
    }

    [Fact]
    public void ProjectMetrics_OnTrackHealth_Indicates100PercentCompletion()
    {
        // Act
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 100,
            HealthStatus = HealthStatus.OnTrack
        };

        // Assert
        Assert.Equal(100, metrics.CompletionPercentage);
        Assert.Equal(HealthStatus.OnTrack, metrics.HealthStatus);
    }

    [Fact]
    public void ProjectMetrics_BlockedHealth_WithLowCompletion()
    {
        // Act
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 10,
            HealthStatus = HealthStatus.Blocked
        };

        // Assert
        Assert.Equal(10, metrics.CompletionPercentage);
        Assert.Equal(HealthStatus.Blocked, metrics.HealthStatus);
    }
}