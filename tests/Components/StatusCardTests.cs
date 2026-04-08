using Bunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Data;
using Xunit;

namespace AgentSquad.Runner.Tests.Components;

public class StatusCardTests : TestContext
{
    [Fact]
    public void StatusCard_WithoutRequiredStatusCategory_ThrowsInvalidOperationException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.TaskCount, 5)
                .Add(p => p.Tasks, new List<Task>())
                .Add(p => p.CardColor, "shipped"));
        });
        Assert.Contains("StatusCategory parameter is required", exception.Message);
    }

    [Fact]
    public void StatusCard_WithEmptyTasks_DisplaysPlaceholder()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 0)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "shipped"));

        // Assert
        var content = component.Markup;
        Assert.Contains("No tasks in this category", content);
    }

    [Fact]
    public void StatusCard_DisplaysTaskCount()
    {
        // Arrange
        var tasks = new List<Task>
        {
            new() { Id = "t1", Name = "Task 1", Owner = "Alice", Status = TaskStatus.Shipped, DueDate = DateTime.Now }
        };

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 1)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "shipped"));

        // Assert
        var countElement = component.Find(".status-card-count");
        Assert.NotNull(countElement);
        Assert.Equal("1", countElement.TextContent.Trim());
    }

    [Fact]
    public void StatusCard_DisplaysStatusLabel()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "In Progress")
            .Add(p => p.TaskCount, 0)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "in-progress"));

        // Assert
        var labelElement = component.Find(".status-card-label");
        Assert.NotNull(labelElement);
        Assert.Equal("In Progress", labelElement.TextContent.Trim());
    }

    [Fact]
    public void StatusCard_WithTasks_DisplaysShowTasksButton()
    {
        // Arrange
        var tasks = new List<Task>
        {
            new() { Id = "t1", Name = "Task 1", Owner = "Alice", Status = TaskStatus.Shipped, DueDate = DateTime.Now },
            new() { Id = "t2", Name = "Task 2", Owner = "Bob", Status = TaskStatus.Shipped, DueDate = DateTime.Now }
        };

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 2)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "shipped"));

        // Assert
        var button = component.Find("button");
        Assert.NotNull(button);
        Assert.Contains("Show Tasks (2)", button.TextContent);
    }

    [Fact]
    public void StatusCard_DisplaysTaskDetails()
    {
        // Arrange
        var tasks = new List<Task>
        {
            new() { Id = "t1", Name = "UI Development", Owner = "Alice", Status = TaskStatus.InProgress, DueDate = DateTime.Now }
        };

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "In Progress")
            .Add(p => p.TaskCount, 1)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "in-progress"));

        // Assert
        var content = component.Markup;
        Assert.Contains("UI Development", content);
        Assert.Contains("Alice", content);
    }

    [Fact]
    public void StatusCard_AppliesCorrectColorClass()
    {
        // Arrange & Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 0)
            .Add(p => p.Tasks, new List<Task>())
            .Add(p => p.CardColor, "shipped"));

        // Assert
        var header = component.Find(".status-card-header");
        Assert.NotNull(header);
        var classList = header.GetAttribute("class");
        Assert.Contains("shipped", classList);
    }

    [Fact]
    public void StatusCard_DisplaysTaskOwner()
    {
        // Arrange
        var tasks = new List<Task>
        {
            new() { Id = "t1", Name = "Task", Owner = "TeamLead", Status = TaskStatus.CarriedOver, DueDate = DateTime.Now }
        };

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Carried Over")
            .Add(p => p.TaskCount, 1)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "carried-over"));

        // Assert
        var content = component.Markup;
        Assert.Contains("TeamLead", content);
    }

    [Fact]
    public void StatusCard_WithMultipleTasks_ShowsAllTasks()
    {
        // Arrange
        var tasks = new List<Task>
        {
            new() { Id = "t1", Name = "Task 1", Owner = "Alice", Status = TaskStatus.Shipped, DueDate = DateTime.Now },
            new() { Id = "t2", Name = "Task 2", Owner = "Bob", Status = TaskStatus.Shipped, DueDate = DateTime.Now },
            new() { Id = "t3", Name = "Task 3", Owner = "Charlie", Status = TaskStatus.Shipped, DueDate = DateTime.Now }
        };

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 3)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "shipped"));

        // Assert
        var taskItems = component.FindAll(".task-item");
        Assert.Equal(3, taskItems.Count);
    }
}