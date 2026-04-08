namespace AgentSquad.Runner.Tests;

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Xunit;
using AgentSquad.Runner.Data;

/// <summary>
/// Comprehensive unit tests for data model classes and JSON deserialization.
/// Tests verify acceptance criteria from PR #121: Define Data Model Classes.
/// </summary>
public class DataModelTests
{
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

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

    // ==================== ACCEPTANCE CRITERIA ====================

    [Fact]
    public void AC1_AllClassesExistAndDeserialize()
    {
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, _jsonOptions);
        
        Assert.NotNull(projectData);
        Assert.IsType<ProjectData>(projectData);
        Assert.IsType<ProjectInfo>(projectData.Project);
        Assert.IsType<List<Milestone>>(projectData.Milestones);
        Assert.IsType<List<Task>>(projectData.Tasks);
        Assert.IsType<ProjectMetrics>(projectData.Metrics);
    }

    [Fact]
    public void AC2_AllPublicPropertiesMatchJsonSchema()
    {
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, _jsonOptions);
        var project = projectData!.Project;

        Assert.Equal("Q2 Mobile App Release", project.Name);
        Assert.Equal("iOS and Android mobile app version 2.0 with new payment integration", project.Description);
        Assert.Equal(new DateTime(2026, 4, 1), project.StartDate);
        Assert.Equal(new DateTime(2026, 6, 30), project.EndDate);
        Assert.Equal("OnTrack", project.Status);
        Assert.Equal("VP of Product", project.Sponsor);
        Assert.Equal("Jane Smith", project.ProjectManager);
    }

    [Fact]
    public void AC3_ClassesSupportsDeserialization_WithoutErrors()
    {
        var exception = Record.Exception(() =>
        {
            var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, _jsonOptions);
            Assert.NotNull(projectData);
        });
        Assert.Null(exception);
    }

    [Fact]
    public void AC4_ProjectMetricsComputedProperties_CompletionPercentage()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 3,
            ProjectStartDate = new DateTime(2026, 4, 1),
            ProjectEndDate = new DateTime(2026, 6, 30)
        };
        
        Assert.Equal(30, metrics.CompletionPercentage);
    }

    [Fact]
    public void AC4_ProjectMetricsComputedProperties_DaysRemaining()
    {
        var endDate = DateTime.Now.AddDays(10);
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 3,
            ProjectStartDate = DateTime.Now.AddDays(-5),
            ProjectEndDate = endDate
        };
        
        int daysRemaining = metrics.DaysRemaining;
        Assert.True(daysRemaining >= 9 && daysRemaining <= 10);
    }

    [Fact]
    public void AC5_EnumValuesAreExplicit_TaskStatus()
    {
        Assert.Equal(0, (int)TaskStatus.Shipped);
        Assert.Equal(1, (int)TaskStatus.InProgress);
        Assert.Equal(2, (int)TaskStatus.CarriedOver);
    }

    [Fact]
    public void AC5_EnumValuesAreExplicit_MilestoneStatus()
    {
        Assert.Equal(0, (int)MilestoneStatus.Completed);
        Assert.Equal(1, (int)MilestoneStatus.InProgress);
        Assert.Equal(2, (int)MilestoneStatus.Pending);
    }

    [Fact]
    public void AC6_RequiredAttributesPresent()
    {
        var projectInfoProps = typeof(ProjectInfo).GetProperties();
        var nameProperty = projectInfoProps.FirstOrDefault(p => p.Name == "Name");
        
        var hasRequired = nameProperty?.GetCustomAttributes(typeof(RequiredAttribute), false).Any() ?? false;
        Assert.True(hasRequired, "ProjectInfo.Name should have [Required]");
    }

    [Fact]
    public void AC7_ProjectDataIsRootDeserializable()
    {
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, _jsonOptions);
        
        Assert.NotNull(projectData);
        Assert.NotNull(projectData.Project);
        Assert.NotEmpty(projectData.Milestones);
        Assert.NotEmpty(projectData.Tasks);
        Assert.NotNull(projectData.Metrics);
    }

    [Fact]
    public void AC8_UnitTestsVerifyJsonDeserialization()
    {
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, _jsonOptions);
        
        Assert.Equal("Q2 Mobile App Release", projectData!.Project.Name);
        Assert.Equal(3, projectData.Milestones.Count);
        Assert.Equal(3, projectData.Tasks.Count);
    }

    // ==================== DESERIALIZATION TESTS ====================

    [Fact]
    public void Deserialization_MilestoneProperties()
    {
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, _jsonOptions);
        var milestone = projectData!.Milestones[0];

        Assert.Equal("m1", milestone.Id);
        Assert.Equal("Design Review Complete", milestone.Name);
        Assert.Equal(new DateTime(2026, 4, 15), milestone.TargetDate);
        Assert.Equal(new DateTime(2026, 4, 12), milestone.ActualDate);
        Assert.Equal(MilestoneStatus.Completed, milestone.Status);
        Assert.Equal(100, milestone.CompletionPercentage);
    }

    [Fact]
    public void Deserialization_TaskProperties()
    {
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, _jsonOptions);
        var task = projectData!.Tasks[0];

        Assert.Equal("t1", task.Id);
        Assert.Equal("API Authentication Module", task.Name);
        Assert.Equal(TaskStatus.Shipped, task.Status);
        Assert.Equal("John Doe", task.AssignedTo);
        Assert.Equal(new DateTime(2026, 4, 20), task.DueDate);
        Assert.Equal(5, task.EstimatedDays);
        Assert.Equal("m1", task.RelatedMilestone);
    }

    [Fact]
    public void Deserialization_TaskStatus_AllValues()
    {
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, _jsonOptions);

        var shipped = projectData!.Tasks.FirstOrDefault(t => t.Status == TaskStatus.Shipped);
        var inProgress = projectData.Tasks.FirstOrDefault(t => t.Status == TaskStatus.InProgress);
        var carriedOver = projectData.Tasks.FirstOrDefault(t => t.Status == TaskStatus.CarriedOver);

        Assert.NotNull(shipped);
        Assert.NotNull(inProgress);
        Assert.NotNull(carriedOver);
    }

    [Fact]
    public void Deserialization_MilestoneStatus_AllValues()
    {
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, _jsonOptions);

        var completed = projectData!.Milestones.FirstOrDefault(m => m.Status == MilestoneStatus.Completed);
        var inProgress = projectData.Milestones.FirstOrDefault(m => m.Status == MilestoneStatus.InProgress);
        var pending = projectData.Milestones.FirstOrDefault(m => m.Status == MilestoneStatus.Pending);

        Assert.NotNull(completed);
        Assert.NotNull(inProgress);
        Assert.NotNull(pending);
    }

    // ==================== NULLABLE FIELDS ====================

    [Fact]
    public void NullableField_MilestoneActualDate_CanBeNull()
    {
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, _jsonOptions);
        
        var completed = projectData!.Milestones[0];
        var inProgress = projectData.Milestones[1];

        Assert.NotNull(completed.ActualDate);
        Assert.Null(inProgress.ActualDate);
    }

    [Fact]
    public void NullableField_TaskRelatedMilestone_CanBeNull()
    {
        var json = """
        {
          "project": {"name": "Test","description": "Test","startDate": "2026-04-01","endDate": "2026-06-30","status": "OnTrack","sponsor": "Test","projectManager": "Test"},
          "milestones": [],
          "tasks": [{"id": "t1","name": "Task","status": "Shipped","assignedTo": "User","dueDate": "2026-04-20","estimatedDays": 5}],
          "metrics": {"totalTasks": 1,"completedTasks": 0,"inProgressTasks": 0,"carriedOverTasks": 0,"estimatedBurndownRate": 1.0,"projectStartDate": "2026-04-01","projectEndDate": "2026-06-30"}
        }
        """;

        var projectData = JsonSerializer.Deserialize<ProjectData>(json, _jsonOptions);
        Assert.Null(projectData!.Tasks[0].RelatedMilestone);
    }

    // ==================== VALIDATION ====================

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 1, 100)]
    [InlineData(3, 10, 30)]
    [InlineData(5, 10, 50)]
    [InlineData(10, 10, 100)]
    public void Theory_CompletionPercentage_VariousValues(int completed, int total, int expected)
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

    [Fact]
    public void Validation_CompletionPercentage_HandlesZeroTasks()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 0,
            CompletedTasks = 0,
            ProjectStartDate = new DateTime(2026, 4, 1),
            ProjectEndDate = new DateTime(2026, 6, 30)
        };
        
        Assert.Equal(0, metrics.CompletionPercentage);
    }

    [Fact]
    public void Validation_CompletionPercentage_At100Percent()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 10,
            ProjectStartDate = new DateTime(2026, 4, 1),
            ProjectEndDate = new DateTime(2026, 6, 30)
        };
        
        Assert.Equal(100, metrics.CompletionPercentage);
    }

    [Fact]
    public void Validation_DaysRemaining_PastEndDate()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 10,
            ProjectStartDate = DateTime.Now.AddDays(-30),
            ProjectEndDate = DateTime.Now.AddDays(-5)
        };
        
        int daysRemaining = metrics.DaysRemaining;
        Assert.True(daysRemaining < 0);
    }

    [Fact]
    public void DateRange_ProjectInfo_StartBeforeEnd()
    {
        var projectData = JsonSerializer.Deserialize<ProjectData>(_validJsonSample, _jsonOptions);
        var project = projectData!.Project;

        Assert.True(project.StartDate < project.EndDate);
    }

    [Fact]
    public void Constructor_AllClassesHaveParameterlessConstructors()
    {
        Assert.NotNull(new ProjectInfo());
        Assert.NotNull(new Milestone());
        Assert.NotNull(new Task());
        Assert.NotNull(new ProjectMetrics());
        Assert.NotNull(new ProjectData());
    }

    [Fact]
    public void CaseInsensitive_JsonDeserialization()
    {
        var json = """
        {
          "PROJECT": {"NAME": "Test","DESCRIPTION": "Test","STARTDATE": "2026-04-01","ENDDATE": "2026-06-30","STATUS": "OnTrack","SPONSOR": "Sponsor","PROJECTMANAGER": "Manager"},
          "MILESTONES": [],
          "TASKS": [],
          "METRICS": {"TOTALTASKS": 0,"COMPLETEDTASKS": 0,"INPROGRESSTASKS": 0,"CARRIEDOVERTASKS": 0,"ESTIMATEDBURNDOWNRATE": 1.0,"PROJECTSTARTDATE": "2026-04-01","PROJECTENDDATE": "2026-06-30"}
        }
        """;

        var projectData = JsonSerializer.Deserialize<ProjectData>(json, _jsonOptions);
        Assert.Equal("Test", projectData!.Project.Name);
    }
}