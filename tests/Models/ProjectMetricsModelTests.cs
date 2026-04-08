using Xunit;
using AgentSquad.Runner.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Tests.Models;

public class ProjectMetricsModelTests
{
    [Fact]
    public void ProjectMetrics_Constructor_CreatesValidInstance()
    {
        var metrics = new ProjectMetrics();

        Assert.NotNull(metrics);
        Assert.Equal(0, metrics.CompletionPercentage);
        Assert.Equal(0, metrics.VelocityThisMonth);
        Assert.Equal(0, metrics.TotalMilestones);
        Assert.Equal(0, metrics.CompletedMilestones);
    }

    [Fact]
    public void ProjectMetrics_PropertiesAssignable()
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

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(99)]
    [InlineData(100)]
    public void CompletionPercentage_ValidRange_0To100(int percentage)
    {
        var metrics = new ProjectMetrics { CompletionPercentage = percentage };

        Assert.InRange(metrics.CompletionPercentage, 0, 100);
        Assert.Equal(percentage, metrics.CompletionPercentage);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(150)]
    public void CompletionPercentage_OutOfRange_StoresValue(int percentage)
    {
        var metrics = new ProjectMetrics { CompletionPercentage = percentage };

        Assert.Equal(percentage, metrics.CompletionPercentage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(12)]
    [InlineData(100)]
    [InlineData(1000)]
    public void VelocityThisMonth_StoresValidValues(int velocity)
    {
        var metrics = new ProjectMetrics { VelocityThisMonth = velocity };

        Assert.Equal(velocity, metrics.VelocityThisMonth);
    }

    [Fact]
    public void VelocityThisMonth_NegativeValue_StoresValue()
    {
        var metrics = new ProjectMetrics { VelocityThisMonth = -5 };

        Assert.Equal(-5, metrics.VelocityThisMonth);
    }

    [Fact]
    public void MilestoneProgress_AllCompleted()
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
    public void MilestoneProgress_PartiallyCompleted()
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
    public void MilestoneProgress_NoneCompleted()
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
    public void MilestoneProgress_CompletedExceedsTotal()
    {
        var metrics = new ProjectMetrics
        {
            TotalMilestones = 5,
            CompletedMilestones = 7
        };

        Assert.Equal(5, metrics.TotalMilestones);
        Assert.Equal(7, metrics.CompletedMilestones);
    }

    [Theory]
    [InlineData(HealthStatus.OnTrack)]
    [InlineData(HealthStatus.AtRisk)]
    [InlineData(HealthStatus.Blocked)]
    public void HealthStatus_AllEnumValues_Valid(HealthStatus status)
    {
        var metrics = new ProjectMetrics { HealthStatus = status };

        Assert.Equal(status, metrics.HealthStatus);
    }

    [Fact]
    public void HealthStatus_Enum_HasThreeValues()
    {
        var values = Enum.GetValues(typeof(HealthStatus));

        Assert.Equal(3, values.Length);
    }

    [Fact]
    public void HealthStatus_OnTrack_Defined()
    {
        Assert.True(Enum.IsDefined(typeof(HealthStatus), HealthStatus.OnTrack));
    }

    [Fact]
    public void HealthStatus_AtRisk_Defined()
    {
        Assert.True(Enum.IsDefined(typeof(HealthStatus), HealthStatus.AtRisk));
    }

    [Fact]
    public void HealthStatus_Blocked_Defined()
    {
        Assert.True(Enum.IsDefined(typeof(HealthStatus), HealthStatus.Blocked));
    }

    [Fact]
    public void HealthStatus_HasCorrectNames()
    {
        var names = Enum.GetNames(typeof(HealthStatus));

        Assert.Contains("OnTrack", names);
        Assert.Contains("AtRisk", names);
        Assert.Contains("Blocked", names);
    }

    [Fact]
    public void ProjectMetrics_JsonSerialization_OnTrackStatus()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = 50,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 10,
            TotalMilestones = 5,
            CompletedMilestones = 2
        };

        var json = JsonSerializer.Serialize(metrics, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        });

        Assert.Contains("\"healthStatus\":\"OnTrack\"", json);
    }

    [Fact]
    public void ProjectMetrics_JsonDeserialization_OnTrackStatus()
    {
        var json = @"{
            ""completionPercentage"": 50,
            ""healthStatus"": ""OnTrack"",
            ""velocityThisMonth"": 10,
            ""totalMilestones"": 5,
            ""completedMilestones"": 2
        }";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        var metrics = JsonSerializer.Deserialize<ProjectMetrics>(json, options);

        Assert.NotNull(metrics);
        Assert.Equal(50, metrics.CompletionPercentage);
        Assert.Equal(HealthStatus.OnTrack, metrics.HealthStatus);
        Assert.Equal(10, metrics.VelocityThisMonth);
        Assert.Equal(5, metrics.TotalMilestones);
        Assert.Equal(2, metrics.CompletedMilestones);
    }

    [Fact]
    public void ProjectMetrics_JsonDeserialization_AtRiskStatus()
    {
        var json = @"{
            ""completionPercentage"": 30,
            ""healthStatus"": ""AtRisk"",
            ""velocityThisMonth"": 5,
            ""totalMilestones"": 8,
            ""completedMilestones"": 1
        }";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        var metrics = JsonSerializer.Deserialize<ProjectMetrics>(json, options);

        Assert.NotNull(metrics);
        Assert.Equal(HealthStatus.AtRisk, metrics.HealthStatus);
    }

    [Fact]
    public void ProjectMetrics_JsonDeserialization_BlockedStatus()
    {
        var json = @"{
            ""completionPercentage"": 10,
            ""healthStatus"": ""Blocked"",
            ""velocityThisMonth"": 0,
            ""totalMilestones"": 10,
            ""completedMilestones"": 0
        }";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        var metrics = JsonSerializer.Deserialize<ProjectMetrics>(json, options);

        Assert.NotNull(metrics);
        Assert.Equal(HealthStatus.Blocked, metrics.HealthStatus);
    }

    [Fact]
    public void ProjectMetrics_Immutability_PropertiesCanBeModified()
    {
        var metrics = new ProjectMetrics { CompletionPercentage = 50 };
        Assert.Equal(50, metrics.CompletionPercentage);

        metrics.CompletionPercentage = 75;

        Assert.Equal(75, metrics.CompletionPercentage);
    }

    [Fact]
    public void ProjectMetrics_MultipleInstances_Independent()
    {
        var metrics1 = new ProjectMetrics { CompletionPercentage = 50 };
        var metrics2 = new ProjectMetrics { CompletionPercentage = 75 };

        Assert.NotEqual(metrics1.CompletionPercentage, metrics2.CompletionPercentage);
    }

    [Fact]
    public void ProjectMetrics_EdgeCase_MaxIntValues()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = int.MaxValue,
            VelocityThisMonth = int.MaxValue,
            TotalMilestones = int.MaxValue,
            CompletedMilestones = int.MaxValue
        };

        Assert.Equal(int.MaxValue, metrics.CompletionPercentage);
        Assert.Equal(int.MaxValue, metrics.VelocityThisMonth);
        Assert.Equal(int.MaxValue, metrics.TotalMilestones);
        Assert.Equal(int.MaxValue, metrics.CompletedMilestones);
    }

    [Fact]
    public void ProjectMetrics_EdgeCase_MinIntValues()
    {
        var metrics = new ProjectMetrics
        {
            CompletionPercentage = int.MinValue,
            VelocityThisMonth = int.MinValue,
            TotalMilestones = int.MinValue,
            CompletedMilestones = int.MinValue
        };

        Assert.Equal(int.MinValue, metrics.CompletionPercentage);
        Assert.Equal(int.MinValue, metrics.VelocityThisMonth);
    }
}