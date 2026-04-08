using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Tests.Models;

public class TaskItemTests
{
    [Fact]
    public void TaskItem_NewInstance_HasValidDefaults()
    {
        // Act
        var task = new TaskItem();

        // Assert
        Assert.NotEmpty(task.Id);
        Assert.Equal(string.Empty, task.Name);
        Assert.Equal(string.Empty, task.Owner);
        Assert.Equal(TaskStatus.Pending, task.Status);
        Assert.Equal(string.Empty, task.Description);
        Assert.True(task.CreatedDate > DateTime.Now.AddSeconds(-1));
    }

    [Fact]
    public void TaskItem_WithProperties_StoresValuesCorrectly()
    {
        // Arrange & Act
        var task = new TaskItem
        {
            Id = "task-123",
            Name = "Test Task",
            Owner = "John Doe",
            Status = TaskStatus.InProgress,
            Description = "Task description",
            CreatedDate = new DateTime(2024, 1, 1)
        };

        // Assert
        Assert.Equal("task-123", task.Id);
        Assert.Equal("Test Task", task.Name);
        Assert.Equal("John Doe", task.Owner);
        Assert.Equal(TaskStatus.InProgress, task.Status);
        Assert.Equal("Task description", task.Description);
        Assert.Equal(new DateTime(2024, 1, 1), task.CreatedDate);
    }

    [Theory]
    [InlineData(TaskStatus.Shipped)]
    [InlineData(TaskStatus.InProgress)]
    [InlineData(TaskStatus.CarriedOver)]
    [InlineData(TaskStatus.Pending)]
    public void TaskItem_WithDifferentStatuses_StoresStatusCorrectly(TaskStatus status)
    {
        // Arrange & Act
        var task = new TaskItem { Status = status };

        // Assert
        Assert.Equal(status, task.Status);
    }
}