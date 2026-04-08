namespace AgentSquad.Runner.Tests;

using System.Text.Json;
using Xunit;
using AgentSquad.Runner.Data;

/// <summary>
/// Unit tests for edge cases and boundary conditions in data models.
/// </summary>
public class DataModelEdgeCaseTests
{
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public void EdgeCase_SpecialCharactersInStrings()
    {
        var json = """
        {
          "project": {
            "name": "Project with 'quotes' and \"double quotes\"",
            "description": "Description with special chars: @#$%^&*()_+-=[]{}|;:',.<>?/~`",
            "startDate": "2026-04-01",
            "endDate": "2026-06-30",
            "status": "OnTrack",
            "sponsor": "Sponsor & Associates",
            "projectManager": "Jane O'Brien"
          },
          "milestones": [],
          "tasks": [],
          "metrics": {"totalTasks": 0,"completedTasks": 0,"inProgressTasks": 0,"carriedOverTasks": 0,"estimatedBurndownRate": 1.0,"projectStartDate": "2026-04-01","projectEndDate": "2026-06-30"}
        }
        """;

        var projectData = JsonSerializer.Deserialize<ProjectData>(json, _jsonOptions);
        Assert.NotNull(projectData);
        Assert.Contains("quotes", projectData.Project.Name);
        Assert.Contains("&", projectData.Project.Sponsor);
        Assert.Contains("'", projectData.Project.ProjectManager);
    }

    [Fact]
    public void EdgeCase_EmptyCollections()
    {
        var json = """
        {
          "project": {"name": "Test","description": "Test","startDate": "2026-04-01","endDate": "2026-06-30","status": "OnTrack","sponsor": "Test","projectManager": "Test"},
          "milestones": [],
          "tasks": [],
          "metrics": {"totalTasks": 0,"completedTasks": 0,"inProgressTasks": 0,"carriedOverTasks": 0,"estimatedBurndownRate": 1.0,"projectStartDate": "2026-04-01","projectEndDate": "2026-06-30"}
        }
        """;

        var projectData = JsonSerializer.Deserialize<ProjectData>(json, _jsonOptions);
        Assert.NotNull(projectData);
        Assert.Empty(projectData.Milestones);
        Assert.Empty(projectData.Tasks);
    }

    [Fact]
    public void EdgeCase_VeryLongStrings()
    {
        var longString = new string('a', 10000);
        var json = $$"""
        {
          "project": {"name": "{{longString}}","description": "Test","startDate": "2026-04-01","endDate": "2026-06-30","status": "OnTrack","sponsor": "Test","projectManager": "Test"},
          "milestones": [],
          "tasks": [],
          "metrics": {"totalTasks": 0,"completedTasks": 0,"inProgressTasks": 0,"carriedOverTasks": 0,"estimatedBurndownRate": 1.0,"projectStartDate": "2026-04-01","projectEndDate": "2026-06-30"}
        }
        """;

        var projectData = JsonSerializer.Deserialize<ProjectData>(json, _jsonOptions);
        Assert.NotNull(projectData);
        Assert.Equal(longString, projectData.Project.Name);
    }

    [Fact]
    public void EdgeCase_MinMaxDateValues()
    {
        var projectInfo = new ProjectInfo
        {
            Name = "Test",
            Description = "Test",
            StartDate = DateTime.MinValue.AddDays(1),
            EndDate = DateTime.MaxValue.AddDays(-1),
            Status = "OnTrack",
            Sponsor = "Test",
            ProjectManager = "Test"
        };

        Assert.NotNull(projectInfo);
        Assert.True(projectInfo.StartDate < projectInfo.EndDate);
    }

    [Fact]
    public void EdgeCase_CompletionPercentageRounding()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 3,
            CompletedTasks = 1,
            ProjectStartDate = DateTime.Now,
            ProjectEndDate = DateTime.Now.AddDays(30)
        };
        
        // (1 / 3) * 100 = 33.333... should truncate to 33
        int result = metrics.CompletionPercentage;
        Assert.Equal(33, result);
    }

    [Fact]
    public void EdgeCase_DaysRemaining_SameDay()
    {
        var now = DateTime.Now;
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 5,
            ProjectStartDate = now.AddDays(-5),
            ProjectEndDate = now.AddHours(1) // Same day
        };
        
        int daysRemaining = metrics.DaysRemaining;
        Assert.Equal(0, daysRemaining);
    }

    [Fact]
    public void EdgeCase_AllTasksCarriedOver()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 0,
            InProgressTasks = 0,
            CarriedOverTasks = 10,
            ProjectStartDate = DateTime.Now.AddDays(-60),
            ProjectEndDate = DateTime.Now
        };
        
        Assert.Equal(0, metrics.CompletionPercentage);
        Assert.True(metrics.CarriedOverTasks == metrics.TotalTasks);
    }

    [Fact]
    public void EdgeCase_ZeroEstimatedDays_InvalidButDeserializes()
    {
        var json = """
        {
          "project": {"name": "Test","description": "Test","startDate": "2026-04-01","endDate": "2026-06-30","status": "OnTrack","sponsor": "Test","projectManager": "Test"},
          "milestones": [],
          "tasks": [{"id": "t1","name": "Task","status": "Shipped","assignedTo": "User","dueDate": "2026-04-20","estimatedDays": 0}],
          "metrics": {"totalTasks": 1,"completedTasks": 0,"inProgressTasks": 0,"carriedOverTasks": 0,"estimatedBurndownRate": 1.0,"projectStartDate": "2026-04-01","projectEndDate": "2026-06-30"}
        }
        """;

        var projectData = JsonSerializer.Deserialize<ProjectData>(json, _jsonOptions);
        Assert.NotNull(projectData);
        Assert.Equal(0, projectData.Tasks[0].EstimatedDays);
    }

    [Fact]
    public void EdgeCase_NegativeCompletedTasks()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = -5, // Invalid but deserializes
            ProjectStartDate = DateTime.Now,
            ProjectEndDate = DateTime.Now.AddDays(30)
        };
        
        // Should calculate even with invalid data
        int result = metrics.CompletionPercentage;
        Assert.Equal(-50, result);
    }

    [Fact]
    public void EdgeCase_BoundaryCompletion_0_50_100()
    {
        var testCases = new[] { (0, 10, 0), (5, 10, 50), (10, 10, 100) };
        
        foreach (var (completed, total, expected) in testCases)
        {
            var metrics = new ProjectMetrics
            {
                TotalTasks = total,
                CompletedTasks = completed,
                ProjectStartDate = DateTime.Now,
                ProjectEndDate = DateTime.Now.AddDays(30)
            };
            
            Assert.Equal(expected, metrics.CompletionPercentage);
        }
    }
}