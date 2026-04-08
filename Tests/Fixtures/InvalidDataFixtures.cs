using AgentSquad.Runner.Models;
using System.Text.Json;

namespace AgentSquad.Runner.Tests.Fixtures;

public static class InvalidDataFixtures
{
    public static string MissingDataJsonPath => "nonexistent/data.json";

    public static string InvalidJsonString => @"
    {
        ""name"": ""Test Project"",
        ""completionPercentage"": 50,
        ""milestones"": [
            {
                ""name"": ""Milestone 1"",
                ""targetDate"": ""2024-03-31""
                ""status"": ""Completed""
            }
        ]
    }";

    public static string ValidJsonString => @"
    {
        ""name"": ""Test Project"",
        ""description"": ""A test project"",
        ""startDate"": ""2024-01-01"",
        ""targetEndDate"": ""2024-12-31"",
        ""completionPercentage"": 45,
        ""healthStatus"": ""OnTrack"",
        ""velocityThisMonth"": 12,
        ""milestones"": [
            {
                ""name"": ""Phase 1 Launch"",
                ""targetDate"": ""2024-03-31"",
                ""status"": ""Completed"",
                ""description"": ""Core feature rollout""
            },
            {
                ""name"": ""Phase 2 Launch"",
                ""targetDate"": ""2024-06-30"",
                ""status"": ""InProgress"",
                ""description"": ""Extended features""
            }
        ],
        ""workItems"": [
            {
                ""title"": ""API Integration"",
                ""description"": ""Connect to external data source"",
                ""status"": ""InProgress"",
                ""assignedTo"": ""Team A""
            },
            {
                ""title"": ""Database Migration"",
                ""description"": ""Migrate to new schema"",
                ""status"": ""Shipped"",
                ""assignedTo"": ""Team B""
            }
        ]
    }";

    public static Project? NullProject => null;

    public static Project ProjectWithNullName => new()
    {
        Name = null!,
        Milestones = new() { new Milestone { Name = "M1", TargetDate = DateTime.Now.AddMonths(1), Status = MilestoneStatus.Future } },
        WorkItems = new(),
        CompletionPercentage = 50,
        HealthStatus = HealthStatus.OnTrack,
        VelocityThisMonth = 5
    };

    public static Project ProjectWithEmptyName => new()
    {
        Name = string.Empty,
        Milestones = new() { new Milestone { Name = "M1", TargetDate = DateTime.Now.AddMonths(1), Status = MilestoneStatus.Future } },
        WorkItems = new(),
        CompletionPercentage = 50,
        HealthStatus = HealthStatus.OnTrack,
        VelocityThisMonth = 5
    };

    public static Project ProjectWithNullMilestones => new()
    {
        Name = "Valid Name",
        Milestones = null!,
        WorkItems = new(),
        CompletionPercentage = 50,
        HealthStatus = HealthStatus.OnTrack,
        VelocityThisMonth = 5
    };

    public static Project ProjectWithEmptyMilestones => new()
    {
        Name = "Valid Name",
        Milestones = new(),
        WorkItems = new(),
        CompletionPercentage = 50,
        HealthStatus = HealthStatus.OnTrack,
        VelocityThisMonth = 5
    };

    public static Project ProjectWithNullMilestoneInList => new()
    {
        Name = "Valid Name",
        Milestones = new() { null! },
        WorkItems = new(),
        CompletionPercentage = 50,
        HealthStatus = HealthStatus.OnTrack,
        VelocityThisMonth = 5
    };

    public static Project ProjectWithNullMilstoneName => new()
    {
        Name = "Valid Name",
        Milestones = new() { new Milestone { Name = null!, TargetDate = DateTime.Now.AddMonths(1), Status = MilestoneStatus.Future } },
        WorkItems = new(),
        CompletionPercentage = 50,
        HealthStatus = HealthStatus.OnTrack,
        VelocityThisMonth = 5
    };

    public static Project ProjectWithInvalidCompletionPercentage => new()
    {
        Name = "Valid Name",
        Milestones = new() { new Milestone { Name = "M1", TargetDate = DateTime.Now.AddMonths(1), Status = MilestoneStatus.Future } },
        WorkItems = new(),
        CompletionPercentage = 150,
        HealthStatus = HealthStatus.OnTrack,
        VelocityThisMonth = 5
    };

    public static Project ProjectWithNegativeCompletionPercentage => new()
    {
        Name = "Valid Name",
        Milestones = new() { new Milestone { Name = "M1", TargetDate = DateTime.Now.AddMonths(1), Status = MilestoneStatus.Future } },
        WorkItems = new(),
        CompletionPercentage = -10,
        HealthStatus = HealthStatus.OnTrack,
        VelocityThisMonth = 5
    };

    public static Project ProjectWithNullWorkItems => new()
    {
        Name = "Valid Name",
        Milestones = new() { new Milestone { Name = "M1", TargetDate = DateTime.Now.AddMonths(1), Status = MilestoneStatus.Future } },
        WorkItems = null!,
        CompletionPercentage = 50,
        HealthStatus = HealthStatus.OnTrack,
        VelocityThisMonth = 5
    };

    public static Project ProjectWithNullWorkItemInList => new()
    {
        Name = "Valid Name",
        Milestones = new() { new Milestone { Name = "M1", TargetDate = DateTime.Now.AddMonths(1), Status = MilestoneStatus.Future } },
        WorkItems = new() { null! },
        CompletionPercentage = 50,
        HealthStatus = HealthStatus.OnTrack,
        VelocityThisMonth = 5
    };

    public static Project ProjectWithNullWorkItemTitle => new()
    {
        Name = "Valid Name",
        Milestones = new() { new Milestone { Name = "M1", TargetDate = DateTime.Now.AddMonths(1), Status = MilestoneStatus.Future } },
        WorkItems = new() { new WorkItem { Title = null!, Status = WorkItemStatus.Shipped } },
        CompletionPercentage = 50,
        HealthStatus = HealthStatus.OnTrack,
        VelocityThisMonth = 5
    };

    public static Project ValidProject => JsonSerializer.Deserialize<Project>(ValidJsonString)!;
}