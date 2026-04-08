using Xunit;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests;

public class ProjectMetricsModelTests
{
    [Fact]
    public void HealthStatus_OnTrack_EnumExists()
    {
        Assert.True(Enum.IsDefined(typeof(HealthStatus), HealthStatus.OnTrack));
    }

    [Fact]
    public void HealthStatus_AtRisk_EnumExists()
    {
        Assert.True(Enum.IsDefined(typeof(HealthStatus), HealthStatus.AtRisk));
    }

    [Fact]
    public void HealthStatus_Blocked_EnumExists()
    {
        Assert.True(Enum.IsDefined(typeof(HealthStatus), HealthStatus.Blocked));
    }

    [Fact]
    public void HealthStatus_EnumHasThreeValues()
    {
        var values = Enum.GetValues(typeof(HealthStatus));
        Assert.Equal(3, values.Length);
    }

    [Fact]
    public void ProjectMetrics_CreateWithMinimumValues()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 0,
            HealthStatus = HealthStatus.Blocked,
            VelocityThisMonth = 0,
            TotalMilestones = 0,
            CompletedMilestones = 0
        };

        Assert.Equal(0, metrics.CompletionPercentage);
        Assert.Equal(HealthStatus.Blocked, metrics.HealthStatus);
        Assert.Equal(0, metrics.VelocityThisMonth);
        Assert.Equal(0, metrics.TotalMilestones);
        Assert.Equal(0, metrics.CompletedMilestones);
    }

    [Fact]
    public void ProjectMetrics_CompletionPercentage_ZeroPercent()
    {
        var metrics = new ProjectMetrics { CompletionPercentage = 0 };
        Assert.Equal(0, metrics.CompletionPercentage);
    }

    [Fact]
    public void ProjectMetrics_CompletionPercentage_FiftyPercent()
    {
        var metrics = new ProjectMetrics { CompletionPercentage = 50 };
        Assert.Equal(50, metrics.CompletionPercentage);
    }

    [Fact]
    public void ProjectMetrics_CompletionPercentage_HundredPercent()
    {
        var metrics = new ProjectMetrics { CompletionPercentage = 100 };
        Assert.Equal(100, metrics.CompletionPercentage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(99)]
    [InlineData(100)]
    public void ProjectMetrics_CompletionPercentage_ValidRange(int percentage)
    {
        var metrics = new ProjectMetrics { CompletionPercentage = percentage };
        Assert.InRange(metrics.CompletionPercentage, 0, 100);
    }

    [Fact]
    public void ProjectMetrics_VelocityThisMonth_Zero()
    {
        var metrics = new ProjectMetrics { VelocityThisMonth = 0 };
        Assert.Equal(0, metrics.VelocityThisMonth);
    }

    [Fact]
    public void ProjectMetrics_VelocityThisMonth_One()
    {
        var metrics = new ProjectMetrics { VelocityThisMonth = 1 };
        Assert.Equal(1, metrics.VelocityThisMonth);
    }

    [Fact]
    public void ProjectMetrics_VelocityThisMonth_Thousand()
    {
        var metrics = new ProjectMetrics { VelocityThisMonth = 1000 };
        Assert.Equal(1000, metrics.VelocityThisMonth);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(100)]
    [InlineData(1000)]
    public void ProjectMetrics_VelocityThisMonth_ValidValues(int velocity)
    {
        var metrics = new ProjectMetrics { VelocityThisMonth = velocity };
        Assert.Equal(velocity, metrics.VelocityThisMonth);
    }

    [Fact]
    public void ProjectMetrics_MilestoneProgress_AllCompleted()
    {
        var metrics = new ProjectMetrics
        {
            TotalMilestones = 5,
            CompletedMilestones = 5
        };

        Assert.Equal(5, metrics.TotalMilestones);
        Assert.Equal(5, metrics.CompletedMilestones);
        Assert.Equal(metrics.CompletedMilestones, metrics.TotalMilestones);
    }

    [Fact]
    public void ProjectMetrics_MilestoneProgress_PartiallyCompleted()
    {
        var metrics = new ProjectMetrics
        {
            TotalMilestones = 10,
            CompletedMilestones = 3
        };

        Assert.Equal(10, metrics.TotalMilestones);
        Assert.Equal(3, metrics.CompletedMilestones);
        Assert.True(metrics.CompletedMilestones < metrics.TotalMilestones);
    }

    [Fact]
    public void ProjectMetrics_MilestoneProgress_NoneCompleted()
    {
        var metrics = new ProjectMetrics
        {
            TotalMilestones = 8,
            CompletedMilestones = 0
        };

        Assert.Equal(8, metrics.TotalMilestones);
        Assert.Equal(0, metrics.CompletedMilestones);
    }

    [Fact]
    public void ProjectMetrics_HealthStatus_OnTrack_IsValid()
    {
        var metrics = new ProjectMetrics { HealthStatus = HealthStatus.OnTrack };
        Assert.Equal(HealthStatus.OnTrack, metrics.HealthStatus);
    }

    [Fact]
    public void ProjectMetrics_HealthStatus_AtRisk_IsValid()
    {
        var metrics = new ProjectMetrics { HealthStatus = HealthStatus.AtRisk };
        Assert.Equal(HealthStatus.AtRisk, metrics.HealthStatus);
    }

    [Fact]
    public void ProjectMetrics_HealthStatus_Blocked_IsValid()
    {
        var metrics = new ProjectMetrics { HealthStatus = HealthStatus.Blocked };
        Assert.Equal(HealthStatus.Blocked, metrics.HealthStatus);
    }

    [Theory]
    [InlineData(HealthStatus.OnTrack)]
    [InlineData(HealthStatus.AtRisk)]
    [InlineData(HealthStatus.Blocked)]
    public void ProjectMetrics_HealthStatus_AllValuesValid(HealthStatus status)
    {
        var metrics = new ProjectMetrics { HealthStatus = status };
        Assert.Equal(status, metrics.HealthStatus);
    }

    [Fact]
    public void ProjectMetrics_CompleteObject_AllPropertiesSet()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 75,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 24,
            TotalMilestones = 6,
            CompletedMilestones = 4
        };

        Assert.Equal(75, metrics.CompletionPercentage);
        Assert.Equal(HealthStatus.OnTrack, metrics.HealthStatus);
        Assert.Equal(24, metrics.VelocityThisMonth);
        Assert.Equal(6, metrics.TotalMilestones);
        Assert.Equal(4, metrics.CompletedMilestones);
    }

    [Fact]
    public void ProjectMetrics_DefaultConstruction_CreatesValidObject()
    {
        var metrics = new ProjectMetrics();
        Assert.NotNull(metrics);
        Assert.Equal(0, metrics.CompletionPercentage);
        Assert.Equal(0, metrics.VelocityThisMonth);
        Assert.Equal(0, metrics.TotalMilestones);
        Assert.Equal(0, metrics.CompletedMilestones);
    }

    [Fact]
    public void ProjectMetrics_BoundaryCondition_CompletionPercentageNegative()
    {
        var metrics = new ProjectMetrics { CompletionPercentage = -1 };
        Assert.Equal(-1, metrics.CompletionPercentage);
    }

    [Fact]
    public void ProjectMetrics_BoundaryCondition_CompletionPercentageOverHundred()
    {
        var metrics = new ProjectMetrics { CompletionPercentage = 101 };
        Assert.Equal(101, metrics.CompletionPercentage);
    }

    [Fact]
    public void ProjectMetrics_BoundaryCondition_VelocityNegative()
    {
        var metrics = new ProjectMetrics { VelocityThisMonth = -1 };
        Assert.Equal(-1, metrics.VelocityThisMonth);
    }

    [Fact]
    public void ProjectMetrics_Immutability_PropertiesCanBeModified()
    {
        var metrics = new ProjectMetrics { CompletionPercentage = 50 };
        metrics.CompletionPercentage = 60;
        Assert.Equal(60, metrics.CompletionPercentage);
    }
}