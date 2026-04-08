using Xunit;
using AgentSquad.Runner.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentSquad.Runner.Tests.Integration;

public class ProjectMetricsIntegrationTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public void Project_ToProjectMetrics_ExtractsAllValues()
    {
        var project = new Project
        {
            Name = "Test Project",
            CompletionPercentage = 60,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 15,
            TotalMilestones = 10,
            CompletedMilestones = 6
        };

        var metrics = project.ToProjectMetrics();

        Assert.NotNull(metrics);
        Assert.Equal(60, metrics.CompletionPercentage);
        Assert.Equal(HealthStatus.OnTrack, metrics.HealthStatus);
        Assert.Equal(15, metrics.VelocityThisMonth);
        Assert.Equal(10, metrics.TotalMilestones);
        Assert.Equal(6, metrics.CompletedMilestones);
    }

    [Fact]
    public void Project_ToProjectMetrics_WithAtRiskStatus()
    {
        var project = new Project
        {
            CompletionPercentage = 35,
            HealthStatus = HealthStatus.AtRisk,
            VelocityThisMonth = 8,
            TotalMilestones = 10,
            CompletedMilestones = 2
        };

        var metrics = project.ToProjectMetrics();

        Assert.Equal(HealthStatus.AtRisk, metrics.HealthStatus);
    }

    [Fact]
    public void Project_JsonDeserialize_WithMetricsFields()
    {
        var json = @"{
            ""name"": ""Executive Dashboard"",
            ""description"": ""Q3 Initiative"",
            ""completionPercentage"": 45,
            ""healthStatus"": ""OnTrack"",
            ""velocityThisMonth"": 12,
            ""totalMilestones"": 8,
            ""completedMilestones"": 3,
            ""milestones"": [],
            ""workItems"": []
        }";

        var project = JsonSerializer.Deserialize<Project>(json, _jsonOptions);

        Assert.NotNull(project);
        Assert.Equal("Executive Dashboard", project.Name);
        Assert.Equal(45, project.CompletionPercentage);
        Assert.Equal(HealthStatus.OnTrack, project.HealthStatus);
        Assert.Equal(12, project.VelocityThisMonth);
        Assert.Equal(8, project.TotalMilestones);
        Assert.Equal(3, project.CompletedMilestones);
    }

    [Fact]
    public void Project_CompleteDataFlow_JsonToMetrics()
    {
        var projectJson = @"{
            ""name"": ""Production Release"",
            ""completionPercentage"": 78,
            ""healthStatus"": ""OnTrack"",
            ""velocityThisMonth"": 24,
            ""totalMilestones"": 12,
            ""completedMilestones"": 9,
            ""milestones"": [],
            ""workItems"": []
        }";

        var project = JsonSerializer.Deserialize<Project>(projectJson, _jsonOptions);
        Assert.NotNull(project);

        var metrics = project.ToProjectMetrics();

        Assert.Equal(78, metrics.CompletionPercentage);
        Assert.Equal(HealthStatus.OnTrack, metrics.HealthStatus);
        Assert.Equal(24, metrics.VelocityThisMonth);
        Assert.Equal(12, metrics.TotalMilestones);
        Assert.Equal(9, metrics.CompletedMilestones);
    }

    [Fact]
    public void ProjectMetrics_BoundaryValues_ZeroCompletion()
    {
        var json = @"{
            ""completionPercentage"": 0,
            ""healthStatus"": ""Blocked"",
            ""velocityThisMonth"": 0,
            ""totalMilestones"": 5,
            ""completedMilestones"": 0
        }";

        var metrics = JsonSerializer.Deserialize<ProjectMetrics>(json, _jsonOptions);

        Assert.NotNull(metrics);
        Assert.Equal(0, metrics.CompletionPercentage);
        Assert.Equal(HealthStatus.Blocked, metrics.HealthStatus);
        Assert.Equal(0, metrics.VelocityThisMonth);
    }

    [Fact]
    public void ProjectMetrics_BoundaryValues_FullCompletion()
    {
        var json = @"{
            ""completionPercentage"": 100,
            ""healthStatus"": ""OnTrack"",
            ""velocityThisMonth"": 50,
            ""totalMilestones"": 5,
            ""completedMilestones"": 5
        }";

        var metrics = JsonSerializer.Deserialize<ProjectMetrics>(json, _jsonOptions);

        Assert.NotNull(metrics);
        Assert.Equal(100, metrics.CompletionPercentage);
        Assert.Equal(HealthStatus.OnTrack, metrics.HealthStatus);
        Assert.Equal(5, metrics.CompletedMilestones);
        Assert.Equal(5, metrics.TotalMilestones);
    }

    [Fact]
    public void ProjectMetrics_HealthStatusTransition_OnTrackToAtRisk()
    {
        var metrics = new ProjectMetrics { HealthStatus = HealthStatus.OnTrack };
        Assert.Equal(HealthStatus.OnTrack, metrics.HealthStatus);

        metrics.HealthStatus = HealthStatus.AtRisk;
        Assert.Equal(HealthStatus.AtRisk, metrics.HealthStatus);
    }

    [Fact]
    public void ProjectMetrics_HealthStatusTransition_AtRiskToBlocked()
    {
        var metrics = new ProjectMetrics { HealthStatus = HealthStatus.AtRisk };
        Assert.Equal(HealthStatus.AtRisk, metrics.HealthStatus);

        metrics.HealthStatus = HealthStatus.Blocked;
        Assert.Equal(HealthStatus.Blocked, metrics.HealthStatus);
    }

    [Fact]
    public void ProjectMetrics_ComplexScenario_MidProjectStatus()
    {
        var projectJson = @"{
            ""name"": ""Mid-Cycle Review"",
            ""completionPercentage"": 52,
            ""healthStatus"": ""AtRisk"",
            ""velocityThisMonth"": 18,
            ""totalMilestones"": 15,
            ""completedMilestones"": 7,
            ""milestones"": [],
            ""workItems"": []
        }";

        var project = JsonSerializer.Deserialize<Project>(projectJson, _jsonOptions);
        Assert.NotNull(project);

        var metrics = project.ToProjectMetrics();

        Assert.Equal(52, metrics.CompletionPercentage);
        Assert.Equal(HealthStatus.AtRisk, metrics.HealthStatus);
        Assert.Equal(18, metrics.VelocityThisMonth);
        Assert.Equal(15, metrics.TotalMilestones);
        Assert.Equal(7, metrics.CompletedMilestones);
        Assert.True(metrics.CompletedMilestones < metrics.TotalMilestones);
    }

    [Fact]
    public void ProjectMetrics_Roundtrip_SerializeDeserialize()
    {
        var originalMetrics = new ProjectMetrics
        {
            CompletionPercentage = 66,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 19,
            TotalMilestones = 7,
            CompletedMilestones = 4
        };

        var json = JsonSerializer.Serialize(originalMetrics, _jsonOptions);
        var deserializedMetrics = JsonSerializer.Deserialize<ProjectMetrics>(json, _jsonOptions);

        Assert.NotNull(deserializedMetrics);
        Assert.Equal(originalMetrics.CompletionPercentage, deserializedMetrics.CompletionPercentage);
        Assert.Equal(originalMetrics.HealthStatus, deserializedMetrics.HealthStatus);
        Assert.Equal(originalMetrics.VelocityThisMonth, deserializedMetrics.VelocityThisMonth);
        Assert.Equal(originalMetrics.TotalMilestones, deserializedMetrics.TotalMilestones);
        Assert.Equal(originalMetrics.CompletedMilestones, deserializedMetrics.CompletedMilestones);
    }

    [Fact]
    public void ProjectMetrics_InvalidJson_ThrowsException()
    {
        var invalidJson = @"{""completionPercentage"": ""invalid""}";

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<ProjectMetrics>(invalidJson, _jsonOptions));
    }

    [Fact]
    public void ProjectMetrics_MissingOptionalFields_DeserializesWithDefaults()
    {
        var minimalJson = @"{
            ""completionPercentage"": 30,
            ""healthStatus"": ""AtRisk""
        }";

        var metrics = JsonSerializer.Deserialize<ProjectMetrics>(minimalJson, _jsonOptions);

        Assert.NotNull(metrics);
        Assert.Equal(30, metrics.CompletionPercentage);
        Assert.Equal(HealthStatus.AtRisk, metrics.HealthStatus);
        Assert.Equal(0, metrics.VelocityThisMonth);
        Assert.Equal(0, metrics.TotalMilestones);
        Assert.Equal(0, metrics.CompletedMilestones);
    }
}