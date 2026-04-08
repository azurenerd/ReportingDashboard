using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgentSquad.Runner.Tests.Services;

public class DataValidatorTests
{
    private readonly IDataValidator _validator;
    private readonly Mock<ILogger<DataValidator>> _mockLogger;

    public DataValidatorTests()
    {
        _mockLogger = new Mock<ILogger<DataValidator>>();
        _validator = new DataValidator(_mockLogger.Object);
    }

    [Fact]
    public void ValidateProjectData_WithValidData_ReturnsSuccess()
    {
        var project = InvalidDataFixtures.ValidProject;

        var result = _validator.ValidateProjectData(project);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateProjectData_WithNullProject_ReturnsValidationError()
    {
        var project = InvalidDataFixtures.NullProject;

        var result = _validator.ValidateProjectData(project);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.ErrorCode == "PROJECT_NULL");
    }

    [Fact]
    public void ValidateProjectData_WithEmptyName_ReturnsValidationError()
    {
        var project = InvalidDataFixtures.ProjectWithEmptyName;

        var result = _validator.ValidateProjectData(project);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == "NAME_EMPTY");
        Assert.Contains(result.Errors, e => e.FieldName == nameof(Project.Name));
    }

    [Fact]
    public void ValidateProjectData_WithNullName_ReturnsValidationError()
    {
        var project = InvalidDataFixtures.ProjectWithNullName;

        var result = _validator.ValidateProjectData(project);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == "NAME_EMPTY");
    }

    [Fact]
    public void ValidateProjectData_WithNullMilestones_ReturnsValidationError()
    {
        var project = InvalidDataFixtures.ProjectWithNullMilestones;

        var result = _validator.ValidateProjectData(project);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == "MILESTONES_NULL");
    }

    [Fact]
    public void ValidateProjectData_WithZeroMilestones_ReturnsValidationError()
    {
        var project = InvalidDataFixtures.ProjectWithEmptyMilestones;

        var result = _validator.ValidateProjectData(project);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == "MILESTONES_EMPTY");
    }

    [Fact]
    public void ValidateProjectData_WithNullMilestoneInList_ReturnsValidationError()
    {
        var project = InvalidDataFixtures.ProjectWithNullMilestoneInList;

        var result = _validator.ValidateProjectData(project);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == "MILESTONE_NULL");
    }

    [Fact]
    public void ValidateProjectData_WithEmptyMilestoneName_ReturnsValidationError()
    {
        var project = InvalidDataFixtures.ProjectWithNullMilstoneName;

        var result = _validator.ValidateProjectData(project);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == "MILESTONE_NAME_EMPTY");
    }

    [Fact]
    public void ValidateProjectData_WithInvalidCompletionPercentage_ReturnsValidationError()
    {
        var project = InvalidDataFixtures.ProjectWithInvalidCompletionPercentage;

        var result = _validator.ValidateProjectData(project);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == "COMPLETION_PERCENTAGE_INVALID");
    }

    [Fact]
    public void ValidateProjectData_WithNegativeCompletionPercentage_ReturnsValidationError()
    {
        var project = InvalidDataFixtures.ProjectWithNegativeCompletionPercentage;

        var result = _validator.ValidateProjectData(project);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == "COMPLETION_PERCENTAGE_INVALID");
    }

    [Fact]
    public void ValidateProjectData_WithNullWorkItems_ReturnsValidationError()
    {
        var project = InvalidDataFixtures.ProjectWithNullWorkItems;

        var result = _validator.ValidateProjectData(project);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == "WORKITEMS_NULL");
    }

    [Fact]
    public void ValidateProjectData_WithNullWorkItemInList_ReturnsValidationError()
    {
        var project = InvalidDataFixtures.ProjectWithNullWorkItemInList;

        var result = _validator.ValidateProjectData(project);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == "WORKITEM_NULL");
    }

    [Fact]
    public void ValidateProjectData_WithNullWorkItemTitle_ReturnsValidationError()
    {
        var project = InvalidDataFixtures.ProjectWithNullWorkItemTitle;

        var result = _validator.ValidateProjectData(project);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorCode == "WORKITEM_TITLE_EMPTY");
    }

    [Fact]
    public void ValidateProjectData_MissingMultipleFields_ReturnsAllErrors()
    {
        var project = new Project
        {
            Name = string.Empty,
            Milestones = new(),
            WorkItems = null!,
            CompletionPercentage = 150,
            HealthStatus = HealthStatus.OnTrack
        };

        var result = _validator.ValidateProjectData(project);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 4);
        Assert.Contains(result.Errors, e => e.ErrorCode == "NAME_EMPTY");
        Assert.Contains(result.Errors, e => e.ErrorCode == "MILESTONES_EMPTY");
        Assert.Contains(result.Errors, e => e.ErrorCode == "WORKITEMS_NULL");
        Assert.Contains(result.Errors, e => e.ErrorCode == "COMPLETION_PERCENTAGE_INVALID");
    }

    [Fact]
    public void ValidateProjectData_GetErrorSummary_ReturnsFormattedString()
    {
        var project = InvalidDataFixtures.ProjectWithEmptyName;

        var result = _validator.ValidateProjectData(project);

        var summary = result.GetErrorSummary();
        Assert.NotEmpty(summary);
        Assert.Contains("failed", summary.ToLower());
    }

    [Fact]
    public void ValidateProjectData_ErrorTimestamp_IsSet()
    {
        var beforeValidation = DateTime.UtcNow;
        var project = InvalidDataFixtures.ProjectWithEmptyName;

        var result = _validator.ValidateProjectData(project);

        var afterValidation = DateTime.UtcNow;
        foreach (var error in result.Errors)
        {
            Assert.True(error.Timestamp >= beforeValidation);
            Assert.True(error.Timestamp <= afterValidation);
        }
    }
}