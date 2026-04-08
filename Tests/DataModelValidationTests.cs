namespace AgentSquad.Runner.Tests;

using System.ComponentModel.DataAnnotations;
using Xunit;
using AgentSquad.Runner.Data;

/// <summary>
/// Unit tests for data model validation attributes and constraints.
/// </summary>
public class DataModelValidationTests
{
    [Fact]
    public void ProjectInfo_RequiredAttribute_Name()
    {
        var property = typeof(ProjectInfo).GetProperty("Name");
        var hasRequired = property?.GetCustomAttributes(typeof(RequiredAttribute), false).Any() ?? false;
        Assert.True(hasRequired);
    }

    [Fact]
    public void ProjectInfo_RequiredAttribute_AllFields()
    {
        var requiredFields = new[] { "Name", "Description", "StartDate", "EndDate", "Status", "Sponsor", "ProjectManager" };
        var properties = typeof(ProjectInfo).GetProperties();

        foreach (var fieldName in requiredFields)
        {
            var prop = properties.FirstOrDefault(p => p.Name == fieldName);
            var hasRequired = prop?.GetCustomAttributes(typeof(RequiredAttribute), false).Any() ?? false;
            Assert.True(hasRequired, $"ProjectInfo.{fieldName} should have [Required]");
        }
    }

    [Fact]
    public void Milestone_RequiredAttribute_AllFields()
    {
        var requiredFields = new[] { "Id", "Name", "TargetDate", "Status", "CompletionPercentage" };
        var properties = typeof(Milestone).GetProperties();

        foreach (var fieldName in requiredFields)
        {
            var prop = properties.FirstOrDefault(p => p.Name == fieldName);
            var hasRequired = prop?.GetCustomAttributes(typeof(RequiredAttribute), false).Any() ?? false;
            Assert.True(hasRequired, $"Milestone.{fieldName} should have [Required]");
        }
    }

    [Fact]
    public void Task_RequiredAttribute_AllFields()
    {
        var requiredFields = new[] { "Id", "Name", "Status", "AssignedTo", "DueDate", "EstimatedDays" };
        var properties = typeof(Task).GetProperties();

        foreach (var fieldName in requiredFields)
        {
            var prop = properties.FirstOrDefault(p => p.Name == fieldName);
            var hasRequired = prop?.GetCustomAttributes(typeof(RequiredAttribute), false).Any() ?? false;
            Assert.True(hasRequired, $"Task.{fieldName} should have [Required]");
        }
    }

    [Fact]
    public void ProjectMetrics_RequiredAttribute_AllFields()
    {
        var requiredFields = new[] { "TotalTasks", "CompletedTasks", "InProgressTasks", "CarriedOverTasks", "ProjectStartDate", "ProjectEndDate" };
        var properties = typeof(ProjectMetrics).GetProperties();

        foreach (var fieldName in requiredFields)
        {
            var prop = properties.FirstOrDefault(p => p.Name == fieldName);
            var hasRequired = prop?.GetCustomAttributes(typeof(RequiredAttribute), false).Any() ?? false;
            Assert.True(hasRequired, $"ProjectMetrics.{fieldName} should have [Required]");
        }
    }

    [Fact]
    public void Milestone_RangeAttribute_CompletionPercentage()
    {
        var property = typeof(Milestone).GetProperty("CompletionPercentage");
        var rangeAttr = property?.GetCustomAttributes(typeof(RangeAttribute), false).FirstOrDefault() as RangeAttribute;
        
        Assert.NotNull(rangeAttr);
        Assert.Equal(0, rangeAttr.Minimum);
        Assert.Equal(100, rangeAttr.Maximum);
    }

    [Fact]
    public void Task_RangeAttribute_EstimatedDays()
    {
        var property = typeof(Task).GetProperty("EstimatedDays");
        var rangeAttr = property?.GetCustomAttributes(typeof(RangeAttribute), false).FirstOrDefault() as RangeAttribute;
        
        Assert.NotNull(rangeAttr);
        Assert.Equal(1, rangeAttr.Minimum);
    }

    [Fact]
    public void JsonPropertyName_Attributes_PresentOnAllProperties()
    {
        var projectInfoProps = typeof(ProjectInfo).GetProperties();
        foreach (var prop in projectInfoProps)
        {
            var hasJsonProp = prop?.GetCustomAttributes(typeof(JsonPropertyNameAttribute), false).Any() ?? false;
            Assert.True(hasJsonProp, $"ProjectInfo.{prop.Name} should have [JsonPropertyName]");
        }
    }

    [Fact]
    public void ValidateObject_ProjectInfo_WithValidData()
    {
        var projectInfo = new ProjectInfo
        {
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(30),
            Status = "OnTrack",
            Sponsor = "Test Sponsor",
            ProjectManager = "Test Manager"
        };

        var results = ValidateObject(projectInfo);
        Assert.Empty(results);
    }

    [Fact]
    public void ValidateObject_Milestone_WithValidData()
    {
        var milestone = new Milestone
        {
            Id = "m1",
            Name = "Test Milestone",
            TargetDate = DateTime.Now.AddDays(30),
            Status = MilestoneStatus.Pending,
            CompletionPercentage = 50
        };

        var results = ValidateObject(milestone);
        Assert.Empty(results);
    }

    [Fact]
    public void ValidateObject_Task_WithValidData()
    {
        var task = new Task
        {
            Id = "t1",
            Name = "Test Task",
            Status = TaskStatus.Shipped,
            AssignedTo = "Test User",
            DueDate = DateTime.Now,
            EstimatedDays = 5
        };

        var results = ValidateObject(task);
        Assert.Empty(results);
    }

    [Fact]
    public void ValidateObject_ProjectMetrics_WithValidData()
    {
        var metrics = new ProjectMetrics
        {
            TotalTasks = 10,
            CompletedTasks = 3,
            InProgressTasks = 5,
            CarriedOverTasks = 2,
            ProjectStartDate = DateTime.Now,
            ProjectEndDate = DateTime.Now.AddDays(30)
        };

        var results = ValidateObject(metrics);
        Assert.Empty(results);
    }

    private static List<ValidationResult> ValidateObject(object obj)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(obj, serviceProvider: null, items: null);
        Validator.TryValidateObject(obj, context, results, validateAllProperties: true);
        return results;
    }
}