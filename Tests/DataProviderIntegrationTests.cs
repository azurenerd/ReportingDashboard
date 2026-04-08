using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AgentSquad.Runner.Tests;

public class DataProviderIntegrationTests
{
    private readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

    [Fact]
    public void DataJson_LoadsSuccessfully()
    {
        var dataFilePath = GetDataJsonPath();
        var logger = _loggerFactory.CreateLogger<DataProvider>();

        var provider = new DataProvider(logger, dataFilePath);

        Assert.True(provider.IsLoaded, provider.ErrorMessage);
        Assert.Null(provider.ErrorMessage);
    }

    [Fact]
    public void DataJson_DeserializesCorrectly()
    {
        var dataFilePath = GetDataJsonPath();
        var logger = _loggerFactory.CreateLogger<DataProvider>();

        var provider = new DataProvider(logger, dataFilePath);
        var project = provider.GetProjectData();

        Assert.NotNull(project);
        Assert.NotEmpty(project.Name);
        Assert.NotNull(project.Milestones);
        Assert.NotNull(project.WorkItems);
        Assert.NotNull(project.Metrics);
    }

    [Fact]
    public void DataJson_HasRequiredFields()
    {
        var dataFilePath = GetDataJsonPath();
        var logger = _loggerFactory.CreateLogger<DataProvider>();

        var provider = new DataProvider(logger, dataFilePath);
        var project = provider.GetProjectData();

        Assert.NotEmpty(project.Name);
        Assert.NotEmpty(project.Description);
        Assert.NotEmpty(project.Milestones);
        Assert.NotEmpty(project.WorkItems);
        Assert.True(project.Milestones.Count >= 5, "Expected at least 5 milestones");
        Assert.True(project.WorkItems.Count >= 12, "Expected at least 12 work items");
    }

    [Fact]
    public void Milestones_HaveValidEnumValues()
    {
        var dataFilePath = GetDataJsonPath();
        var logger = _loggerFactory.CreateLogger<DataProvider>();

        var provider = new DataProvider(logger, dataFilePath);
        var project = provider.GetProjectData();

        foreach (var milestone in project.Milestones)
        {
            Assert.NotEmpty(milestone.Name);
            Assert.NotEmpty(milestone.Description);
            Assert.NotEqual(default(DateTime), milestone.TargetDate);
            Assert.True(Enum.IsDefined(typeof(MilestoneStatus), milestone.Status),
                $"Invalid MilestoneStatus: {milestone.Status}");
        }
    }

    [Fact]
    public void WorkItems_HaveValidEnumValues()
    {
        var dataFilePath = GetDataJsonPath();
        var logger = _loggerFactory.CreateLogger<DataProvider>();

        var provider = new DataProvider(logger, dataFilePath);
        var project = provider.GetProjectData();

        foreach (var item in project.WorkItems)
        {
            Assert.NotEmpty(item.Title);
            Assert.True(Enum.IsDefined(typeof(WorkItemStatus), item.Status),
                $"Invalid WorkItemStatus: {item.Status}");
        }
    }

    [Fact]
    public void Metrics_HaveValidValues()
    {
        var dataFilePath = GetDataJsonPath();
        var logger = _loggerFactory.CreateLogger<DataProvider>();

        var provider = new DataProvider(logger, dataFilePath);
        var project = provider.GetProjectData();

        Assert.InRange(project.Metrics.CompletionPercentage, 0, 100);
        Assert.True(Enum.IsDefined(typeof(HealthStatus), project.Metrics.HealthStatus),
            $"Invalid HealthStatus: {project.Metrics.HealthStatus}");
        Assert.True(project.Metrics.VelocityCount >= 0, "VelocityCount must be non-negative");
        Assert.True(project.Metrics.TotalMilestones >= 0, "TotalMilestones must be non-negative");
    }

    [Fact]
    public void WorkItems_DistributedAcrossStatuses()
    {
        var dataFilePath = GetDataJsonPath();
        var logger = _loggerFactory.CreateLogger<DataProvider>();

        var provider = new DataProvider(logger, dataFilePath);
        var project = provider.GetProjectData();

        var shippedCount = project.WorkItems.Count(w => w.Status == WorkItemStatus.ShippedThisMonth);
        var inProgressCount = project.WorkItems.Count(w => w.Status == WorkItemStatus.InProgress);
        var carriedOverCount = project.WorkItems.Count(w => w.Status == WorkItemStatus.CarriedOver);

        Assert.True(shippedCount > 0, "Expected work items in ShippedThisMonth status");
        Assert.True(inProgressCount > 0, "Expected work items in InProgress status");
        Assert.True(carriedOverCount > 0, "Expected work items in CarriedOver status");
    }

    [Fact]
    public void Milestones_MixedStatuses()
    {
        var dataFilePath = GetDataJsonPath();
        var logger = _loggerFactory.CreateLogger<DataProvider>();

        var provider = new DataProvider(logger, dataFilePath);
        var project = provider.GetProjectData();

        var statuses = project.Milestones.Select(m => m.Status).Distinct().Count();
        Assert.True(statuses >= 3, "Expected milestones with at least 3 different statuses");
    }

    [Fact]
    public void DataJson_IsValidJson()
    {
        var dataFilePath = GetDataJsonPath();
        Assert.True(File.Exists(dataFilePath), $"data.json not found at {dataFilePath}");

        var json = File.ReadAllText(dataFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var exception = Record.Exception(() =>
            JsonSerializer.Deserialize<Project>(json, options)
        );

        Assert.Null(exception);
    }

    private string GetDataJsonPath()
    {
        var testProjectDir = AppContext.BaseDirectory;
        var solutionDir = Path.GetFullPath(Path.Combine(testProjectDir, "..", "..", "..", ".."));
        return Path.Combine(solutionDir, "src", "AgentSquad.Runner", "data.json");
    }
}