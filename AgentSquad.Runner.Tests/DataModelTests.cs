namespace AgentSquad.Runner.Tests;

using System.Text.Json;
using Xunit;
using AgentSquad.Runner.Data;

/// <summary>
/// Unit tests for data model classes and JSON deserialization.
/// </summary>
public class DataModelTests
{
    private readonly string _validJsonSample = """
        {
          "project": {
            "name": "Q2 Mobile App Release",
            "description": "iOS and Android mobile app version 2.0 with new payment integration",
            "startDate": "2026-04-01",
            "endDate": "2026-06-30",
            "status": "OnTrack",
            "sponsor": "VP of Product",
            "projectManager": "Jane Smith"
          },
          "milestones": [
            {
              "id": "m1",
              "name": "Design Review Complete",
              "targetDate": "2026-04-15",
              "actualDate": "2026-04-12",
              "status": "Completed",
              "completionPercentage": 100
            },
            {
              "id": "m2",
              "name": "Development Sprint 1 Done",
              "targetDate": "2026-05-01",
              "actualDate": null,
              "status": "InProgress",
              "completionPercentage": 65
            },
            {
              "id": "m3",
              "name": "QA Testing Complete",
              "targetDate": "2026-06-01",
              "actualDate": null,
              "status": "Pending",
              "completionPercentage": 0
            }
          ],
          "tasks": [
            {
              "id": "t1",
              "name": "API Authentication Module",
              "status": "Shipped",
              "assignedTo": "John Doe",
              "dueDate": "2026-04-20",
              "estimatedDays": 5,
              "relatedMilestone": "m1"
            },
            {
              "id": "t2",
              "name": "Payment Integration",
              "status": "InProgress",
              "assignedTo": "Alice Brown",
              "dueDate": "2026-05-10",
              "estimatedDays": 8,
              "relatedMilestone": "m2"
            },
            {
              "id": "t3",
              "name": "iOS Push Notifications",
              "status": "CarriedOver",
              "assignedTo": "Bob Wilson",
              "dueDate": "2026-05-15",
              "estimatedDays": 6,
              "relatedMilestone": "m2"
            }
          ],
          "metrics": {
            "totalTasks": 10,
            "completedTasks": 3,
            "inProgressTasks": 5,
            "carriedOverTasks": 2,
            "estimatedBurndownRate": 1.2,
            "projectStartDate": "2026-04-01",
            "projectEndDate": "2026-06-30"
          }
        }
        """;

    [Fact]
    public void DeserializeValidJson_ReturnsProjectDataObject()
    {
        // Arrange
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, options);

        // Assert
        Assert.NotNull(projectData);
        Assert.NotNull(projectData.Project);
        Assert.NotNull(projectData.Milestones);
        Assert.NotNull(projectData.Tasks);
        Assert.NotNull(projectData.Metrics);
        Assert.Equal("Q2 Mobile App Release", projectData.Project.Name);
        Assert.Equal(3, projectData.Milestones.Count);
        Assert.Equal(3, projectData.Tasks.Count);
    }

    [Fact]
    public void DeserializeValidJson_ProjectInfoPropertiesMatch()
    {
        // Arrange
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, options);

        // Assert
        Assert.Equal("Q2 Mobile App Release", projectData.Project.Name);
        Assert.Equal("iOS and Android mobile app version 2.0 with new payment integration", projectData.Project.Description);
        Assert.Equal("OnTrack", projectData.Project.Status);
        Assert.Equal("VP of Product", projectData.Project.Sponsor);
        Assert.Equal("Jane Smith", projectData.Project.ProjectManager);
        Assert.Equal(new DateTime(2026, 4, 1), projectData.Project.StartDate);
        Assert.Equal(new DateTime(2026, 6, 30), projectData.Project.EndDate);
    }

    [Fact]
    public void DeserializeTask_CreatesTaskWithCorrectStatus()
    {
        // Arrange
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, options);
        var shippedTask = projectData.Tasks[0];
        var inProgressTask = projectData.Tasks[1];
        var carriedOverTask = projectData.Tasks[2];

        // Assert
        Assert.Equal(TaskStatus.Shipped, shippedTask.Status);
        Assert.Equal(TaskStatus.InProgress, inProgressTask.Status);
        Assert.Equal(TaskStatus.CarriedOver, carriedOverTask.Status);
        Assert.Equal("API Authentication Module", shippedTask.Name);
        Assert.Equal("Payment Integration", inProgressTask.Name);
        Assert.Equal("iOS Push Notifications", carriedOverTask.Name);
    }

    [Fact]
    public void DeserializeMilestone_CreatesMilestoneWithCorrectStatus()
    {
        // Arrange
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, options);
        var completedMilestone = projectData.Milestones[0];
        var inProgressMilestone = projectData.Milestones[1];
        var pendingMilestone = projectData.Milestones[2];

        // Assert
        Assert.Equal(MilestoneStatus.Completed, completedMilestone.Status);
        Assert.Equal(MilestoneStatus.InProgress, inProgressMilestone.Status);
        Assert.Equal(MilestoneStatus.Pending, pendingMilestone.Status);
    }

    [Fact]
    public void ProjectMetrics_CalculatesCompletionPercentageCorrectly()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 3,
            InProgressTasks = 5,
            CarriedOverTasks = 2,
            ProjectStartDate = new DateTime(2026, 4, 1),
            ProjectEndDate = new DateTime(2026, 6, 30)
        };

        // Act
        int completionPercentage = metrics.CompletionPercentage;

        // Assert
        Assert.Equal(30, completionPercentage);
    }

    [Fact]
    public void ProjectMetrics_CompletionPercentageHandlesZeroTasks()
    {
        // Arrange
        var metrics = new ProjectMetrics
        {
            TotalTasks = 0,
            CompletedTasks = 0,
            InProgressTasks = 0,
            CarriedOverTasks = 0,
            ProjectStartDate = new DateTime(2026, 4, 1),
            ProjectEndDate = new DateTime(2026, 6, 30)
        };

        // Act
        int completionPercentage = metrics.CompletionPercentage;

        // Assert
        Assert.Equal(0, completionPercentage);
    }

    [Fact]
    public void ProjectMetrics_CalculatesDaysRemaining()
    {
        // Arrange
        var endDate = DateTime.Now.AddDays(10);
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 3,
            ProjectStartDate = DateTime.Now.AddDays(-5),
            ProjectEndDate = endDate
        };

        // Act
        int daysRemaining = metrics.DaysRemaining;

        // Assert
        Assert.True(daysRemaining >= 9 && daysRemaining <= 10, $"Expected ~10 days, got {daysRemaining}");
    }

    [Fact]
    public void DeserializeValidJson_MilestoneWithNullActualDate()
    {
        // Arrange
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, options);
        var inProgressMilestone = projectData.Milestones[1];

        // Assert
        Assert.Null(inProgressMilestone.ActualDate);
        Assert.Equal(new DateTime(2026, 5, 1), inProgressMilestone.TargetDate);
    }

    [Fact]
    public void DeserializeValidJson_TaskWithRelatedMilestone()
    {
        // Arrange
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, options);
        var task = projectData.Tasks[0];

        // Assert
        Assert.Equal("m1", task.RelatedMilestone);
    }

    [Fact]
    public void ProjectData_IsValid_WithValidData()
    {
        // Arrange
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, options);

        // Act
        bool isValid = projectData.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ProjectData_Validate_ThrowsOnNullProject()
    {
        // Arrange
        var projectData = new ProjectData
        {
            Project = null,
            Milestones = new List<Milestone>(),
            Tasks = new List<Task>(),
            Metrics = new ProjectMetrics()
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => projectData.Validate());
        Assert.Contains("Project information is required", exception.Message);
    }

    [Fact]
    public void ProjectData_IsValid_ReturnsFalseOnNullProject()
    {
        // Arrange
        var projectData = new ProjectData
        {
            Project = null,
            Milestones = new List<Milestone>(),
            Tasks = new List<Task>(),
            Metrics = new ProjectMetrics()
        };

        // Act
        bool isValid = projectData.IsValid(out var errors);

        // Assert
        Assert.False(isValid);
        Assert.Contains("Project information is required", errors);
    }

    [Fact]
    public void ProjectData_Validate_ThrowsOnInvalidDateRange()
    {
        // Arrange
        var projectData = new ProjectData
        {
            Project = new ProjectInfo
            {
                Name = "Test",
                Description = "Test",
                StartDate = new DateTime(2026, 6, 30),
                EndDate = new DateTime(2026, 4, 1),
                Status = "OnTrack",
                Sponsor = "Test",
                ProjectManager = "Test"
            },
            Milestones = new List<Milestone>(),
            Tasks = new List<Task>(),
            Metrics = new ProjectMetrics()
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => projectData.Validate());
        Assert.Contains("start date", exception.Message);
    }

    [Fact]
    public void ProjectData_IsValid_ReturnsFalseOnMilestoneInvalidStatus()
    {
        // Arrange
        var projectData = new ProjectData
        {
            Project = new ProjectInfo
            {
                Name = "Test",
                Description = "Test",
                StartDate = new DateTime(2026, 4, 1),
                EndDate = new DateTime(2026, 6, 30),
                Status = "OnTrack",
                Sponsor = "Test",
                ProjectManager = "Test"
            },
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = "m1",
                    Name = "Test Milestone",
                    TargetDate = new DateTime(2026, 5, 1),
                    Status = (MilestoneStatus)999,
                    CompletionPercentage = 50
                }
            },
            Tasks = new List<Task>(),
            Metrics = new ProjectMetrics()
        };

        // Act
        bool isValid = projectData.IsValid(out var errors);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void ProjectData_IsValid_DetectsMilestoneReferenceMismatch()
    {
        // Arrange
        var projectData = new ProjectData
        {
            Project = new ProjectInfo
            {
                Name = "Test",
                Description = "Test",
                StartDate = new DateTime(2026, 4, 1),
                EndDate = new DateTime(2026, 6, 30),
                Status = "OnTrack",
                Sponsor = "Test",
                ProjectManager = "Test"
            },
            Milestones = new List<Milestone>
            {
                new Milestone
                {
                    Id = "m1",
                    Name = "Test Milestone",
                    TargetDate = new DateTime(2026, 5, 1),
                    Status = MilestoneStatus.Pending,
                    CompletionPercentage = 0
                }
            },
            Tasks = new List<Task>
            {
                new Task
                {
                    Id = "t1",
                    Name = "Test Task",
                    Status = TaskStatus.Shipped,
                    AssignedTo = "John",
                    DueDate = new DateTime(2026, 5, 15),
                    EstimatedDays = 5,
                    RelatedMilestone = "m99"
                }
            },
            Metrics = new ProjectMetrics()
        };

        // Act
        bool isValid = projectData.IsValid(out var errors);

        // Assert
        Assert.False(isValid);
        Assert.Contains("non-existent milestone", string.Join("; ", errors));
    }

    [Fact]
    public void ProjectMetrics_IsValid_DetectsTaskCountMismatch()
    {
        // Arrange
        var projectData = new ProjectData
        {
            Project = new ProjectInfo
            {
                Name = "Test",
                Description = "Test",
                StartDate = new DateTime(2026, 4, 1),
                EndDate = new DateTime(2026, 6, 30),
                Status = "OnTrack",
                Sponsor = "Test",
                ProjectManager = "Test"
            },
            Milestones = new List<Milestone>(),
            Tasks = new List<Task>(),
            Metrics = new ProjectMetrics
            {
                TotalTasks = 10,
                CompletedTasks = 3,
                InProgressTasks = 4,
                CarriedOverTasks = 2,
                ProjectStartDate = new DateTime(2026, 4, 1),
                ProjectEndDate = new DateTime(2026, 6, 30)
            }
        };

        // Act
        bool isValid = projectData.IsValid(out var errors);

        // Assert
        Assert.False(isValid);
        Assert.Contains("task sum mismatch", string.Join("; ", errors));
    }

    [Fact]
    public void EnumValues_MatchSpecification()
    {
        // Assert
        Assert.Equal(0, (int)TaskStatus.Shipped);
        Assert.Equal(1, (int)TaskStatus.InProgress);
        Assert.Equal(2, (int)TaskStatus.CarriedOver);

        Assert.Equal(0, (int)MilestoneStatus.Completed);
        Assert.Equal(1, (int)MilestoneStatus.InProgress);
        Assert.Equal(2, (int)MilestoneStatus.Pending);
    }

    [Fact]
    public void DeserializeValidJson_AllTasksPopulated()
    {
        // Arrange
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, options);

        // Assert
        foreach (var task in projectData.Tasks)
        {
            Assert.NotNull(task.Id);
            Assert.NotNull(task.Name);
            Assert.NotNull(task.AssignedTo);
            Assert.True(task.EstimatedDays > 0);
        }
    }

    [Fact]
    public void DeserializeValidJson_AllMilestonesPopulated()
    {
        // Arrange
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, options);

        // Assert
        foreach (var milestone in projectData.Milestones)
        {
            Assert.NotNull(milestone.Id);
            Assert.NotNull(milestone.Name);
            Assert.True(milestone.CompletionPercentage >= 0 && milestone.CompletionPercentage <= 100);
        }
    }

    [Fact]
    public void ProjectData_IsValid_WithBoundaryCompletionPercentage()
    {
        // Arrange
        var projectData = new ProjectData
        {
            Project = new ProjectInfo
            {
                Name = "Test",
                Description = "Test",
                StartDate = new DateTime(2026, 4, 1),
                EndDate = new DateTime(2026, 6, 30),
                Status = "OnTrack",
                Sponsor = "Test",
                ProjectManager = "Test"
            },
            Milestones = new List<Milestone>(),
            Tasks = new List<Task>(),
            Metrics = new ProjectMetrics
            {
                TotalTasks = 2,
                CompletedTasks = 2,
                InProgressTasks = 0,
                CarriedOverTasks = 0,
                ProjectStartDate = new DateTime(2026, 4, 1),
                ProjectEndDate = new DateTime(2026, 6, 30)
            }
        };

        // Act
        bool isValid = projectData.IsValid();

        // Assert
        Assert.True(isValid);
        Assert.Equal(100, projectData.Metrics.CompletionPercentage);
    }
}