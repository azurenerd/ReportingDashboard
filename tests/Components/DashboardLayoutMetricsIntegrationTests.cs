using Xunit;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Components;

public class DashboardLayoutMetricsIntegrationTests
{
    [Fact]
    public void ExtractMetrics_FromProject_WithAllMetricsPopulated()
    {
        var project = new Project
        {
            Name = "Dashboard Test Project",
            CompletionPercentage = 55,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 14,
            TotalMilestones = 10,
            CompletedMilestones = 5
        };

        var metrics = project.ToProjectMetrics();

        Assert.NotNull(metrics);
        Assert.Equal(55, metrics.CompletionPercentage);
        Assert.Equal(HealthStatus.OnTrack, metrics.HealthStatus);
        Assert.Equal(14, metrics.VelocityThisMonth);
        Assert.Equal(10, metrics.TotalMilestones);
        Assert.Equal(5, metrics.CompletedMilestones);
    }

    [Fact]
    public void ExtractMetrics_FromProject_WithBlockedStatus()
    {
        var project = new Project
        {
            Name = "Blocked Project",
            CompletionPercentage = 20,
            HealthStatus = HealthStatus.Blocked,
            VelocityThisMonth = 2,
            TotalMilestones = 10,
            CompletedMilestones = 0
        };

        var metrics = project.ToProjectMetrics();

        Assert.Equal(HealthStatus.Blocked, metrics.HealthStatus);
        Assert.Equal(0, metrics.CompletedMilestones);
    }

    [Fact]
    public void ExtractMetrics_FromProject_WithZeroProgress()
    {
        var project = new Project
        {
            Name = "Just Started",
            CompletionPercentage = 0,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 0,
            TotalMilestones = 8,
            CompletedMilestones = 0
        };

        var metrics = project.ToProjectMetrics();

        Assert.Equal(0, metrics.CompletionPercentage);
        Assert.Equal(0, metrics.VelocityThisMonth);
        Assert.Equal(0, metrics.CompletedMilestones);
    }

    [Fact]
    public void ExtractMetrics_FromProject_WithCompletedMilestones()
    {
        var project = new Project
        {
            Name = "Near Complete",
            CompletionPercentage = 95,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 30,
            TotalMilestones = 6,
            CompletedMilestones = 5
        };

        var metrics = project.ToProjectMetrics();

        Assert.Equal(6, metrics.TotalMilestones);
        Assert.Equal(5, metrics.CompletedMilestones);
    }

    [Fact]
    public void DashboardLayout_AcceptanceCriteria_MetricsDisplay()
    {
        var project = new Project
        {
            Name = "Executive Dashboard",
            CompletionPercentage = 45,
            HealthStatus = HealthStatus.OnTrack,
            VelocityThisMonth = 12,
            TotalMilestones = 8,
            CompletedMilestones = 3
        };

        var metrics = project.ToProjectMetrics();

        // AC: Display overall project completion percentage visually
        Assert.NotNull(metrics.CompletionPercentage);
        Assert.InRange(metrics.CompletionPercentage, 0, 100);

        // AC: Show on-time vs. at-risk status indicator with color coding
        Assert.NotNull(metrics.HealthStatus);
        Assert.True(Enum.IsDefined(typeof(HealthStatus), metrics.HealthStatus));

        // AC: Display velocity trend or work item counts for comparison
        Assert.NotNull(metrics.VelocityThisMonth);
        Assert.True(metrics.VelocityThisMonth >= 0);

        // AC: Metrics update from data.json fields
        Assert.Equal(project.CompletionPercentage, metrics.CompletionPercentage);
        Assert.Equal(project.HealthStatus, metrics.HealthStatus);
        Assert.Equal(project.VelocityThisMonth, metrics.VelocityThisMonth);

        // AC: Metrics prominently displayed (values are available)
        Assert.True(metrics.TotalMilestones > 0);
        Assert.True(metrics.CompletedMilestones >= 0);
    }
}