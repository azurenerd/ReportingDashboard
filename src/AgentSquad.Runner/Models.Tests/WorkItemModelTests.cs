using System.Text.Json;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Models.Tests;

/// <summary>
/// Tests for WorkItem model including all status types and optional fields.
/// Validates work item tracking for shipped, in-progress, and carried-over items.
/// </summary>
public class WorkItemModelTests
{
    [Fact]
    public void WorkItem_DefaultConstruction_HasEmptyStrings()
    {
        // Act
        var workItem = new WorkItem();

        // Assert
        Assert.Equal(string.Empty, workItem.Title);
        Assert.Null(workItem.Description);
        Assert.Null(workItem.AssignedTo);
    }

    [Fact]
    public void WorkItem_ConstructWithAllProperties_AllSet()
    {
        // Act
        var workItem = new WorkItem
        {
            Title = "API Development",
            Description = "Implement REST endpoints",
            Status = WorkItemStatus.InProgress,
            AssignedTo = "Team A"
        };

        // Assert
        Assert.Equal("API Development", workItem.Title);
        Assert.Equal("Implement REST endpoints", workItem.Description);
        Assert.Equal(WorkItemStatus.InProgress, workItem.Status);
        Assert.Equal("Team A", workItem.AssignedTo);
    }

    [Theory]
    [InlineData(WorkItemStatus.Shipped)]
    [InlineData(WorkItemStatus.InProgress)]
    [InlineData(WorkItemStatus.CarriedOver)]
    public void WorkItem_AllStatusValues_Deserialize(WorkItemStatus status)
    {
        // Arrange
        var statusName = status.ToString();
        var json = $@"{{
            ""title"": ""Task"",
            ""description"": ""Description"",
            ""status"": ""{statusName}"",
            ""assignedTo"": ""Developer""
        }}";

        // Act
        var workItem = JsonSerializer.Deserialize<WorkItem>(json);

        // Assert
        Assert.NotNull(workItem);
        Assert.Equal(status, workItem.Status);
    }

    [Fact]
    public void WorkItem_JsonDeserialization_ShippedStatus()
    {
        // Arrange
        var json = @"{
            ""title"": ""Dashboard UI"",
            ""description"": ""Build Blazor components"",
            ""status"": ""Shipped"",
            ""assignedTo"": ""Frontend Team""
        }";

        // Act
        var workItem = JsonSerializer.Deserialize<WorkItem>(json);

        // Assert
        Assert.NotNull(workItem);
        Assert.Equal(WorkItemStatus.Shipped, workItem.Status);
        Assert.Equal("Dashboard UI", workItem.Title);
    }

    [Fact]
    public void WorkItem_JsonDeserialization_CarriedOverStatus()
    {
        // Arrange
        var json = @"{
            ""title"": ""Database Migration"",
            ""description"": ""Migrate legacy data"",
            ""status"": ""CarriedOver"",
            ""assignedTo"": ""DBA Team""
        }";

        // Act
        var workItem = JsonSerializer.Deserialize<WorkItem>(json);

        // Assert
        Assert.NotNull(workItem);
        Assert.Equal(WorkItemStatus.CarriedOver, workItem.Status);
    }

    [Fact]
    public void WorkItem_NullDescription_Allowed()
    {
        // Arrange
        var json = @"{
            ""title"": ""Quick Fix"",
            ""description"": null,
            ""status"": ""Shipped"",
            ""assignedTo"": ""Dev""
        }";

        // Act
        var workItem = JsonSerializer.Deserialize<WorkItem>(json);

        // Assert
        Assert.NotNull(workItem);
        Assert.Null(workItem.Description);
    }

    [Fact]
    public void WorkItem_NullAssignedTo_Allowed()
    {
        // Arrange
        var json = @"{
            ""title"": ""Unassigned Task"",
            ""description"": ""No assignee"",
            ""status"": ""InProgress"",
            ""assignedTo"": null
        }";

        // Act
        var workItem = JsonSerializer.Deserialize<WorkItem>(json);

        // Assert
        Assert.NotNull(workItem);
        Assert.Null(workItem.AssignedTo);
    }

    [Fact]
    public void WorkItem_BothDescriptionAndAssignedToNull()
    {
        // Arrange
        var json = @"{
            ""title"": ""Task"",
            ""description"": null,
            ""status"": ""InProgress"",
            ""assignedTo"": null
        }";

        // Act
        var workItem = JsonSerializer.Deserialize<WorkItem>(json);

        // Assert
        Assert.NotNull(workItem);
        Assert.Null(workItem.Description);
        Assert.Null(workItem.AssignedTo);
        Assert.Equal("Task", workItem.Title);
    }

    [Fact]
    public void WorkItem_Serialization_RoundtripPreservesData()
    {
        // Arrange
        var original = new WorkItem
        {
            Title = "Performance Testing",
            Description = "Load and stress testing",
            Status = WorkItemStatus.CarriedOver,
            AssignedTo = "QA Team"
        };

        // Act
        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<WorkItem>(json);

        // Assert
        Assert.Equal(original.Title, restored.Title);
        Assert.Equal(original.Description, restored.Description);
        Assert.Equal(original.Status, restored.Status);
        Assert.Equal(original.AssignedTo, restored.AssignedTo);
    }

    [Fact]
    public void WorkItem_EmptyTitle_Allowed()
    {
        // Act
        var workItem = new WorkItem { Title = string.Empty, Status = WorkItemStatus.Shipped };

        // Assert
        Assert.Equal(string.Empty, workItem.Title);
    }

    [Fact]
    public void WorkItem_LongTitle_Allowed()
    {
        // Arrange
        var longTitle = new string('X', 1000);

        // Act
        var workItem = new WorkItem { Title = longTitle };

        // Assert
        Assert.Equal(1000, workItem.Title.Length);
    }
}