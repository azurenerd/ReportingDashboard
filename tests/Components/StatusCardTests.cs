using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;
using Bunit;
using Xunit;

namespace AgentSquad.Runner.Tests.Components;

public class StatusCardTests : TestContext
{
    [Fact]
    public void StatusCard_WithValidParameters_RendersSuccessfully()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new() { Id = "1", Name = "Task 1", Owner = "Alice", Status = TaskStatus.Shipped }
        };

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 1)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "bg-success"));

        // Assert
        Assert.NotNull(component);
        component.MarkupMatches(@"
            <div class=""card card-status h-100 shadow-sm"">
                <div class=""card-header bg-success"">
                    <div class=""d-flex justify-content-between align-items-center"">
                        <h5 class=""mb-0 text-white"">
                            <strong>Shipped</strong>
                        </h5>
                        <span class=""badge bg-light text-dark"">1</span>
                    </div>
                </div>
                <div class=""card-body"">
                    <button class=""btn btn-link btn-sm p-0 text-decoration-none w-100 text-start collapse-toggle"" 
                            type=""button"" 
                            data-bs-toggle=""collapse"" 
                            data-bs-target=""#collapse-shipped"" 
                            aria-expanded=""false"" 
                            aria-controls=""collapse-shipped"">
                        <span class=""toggle-icon"">
                            <i class=""bi bi-chevron-down""></i>
                        </span>
                        <span class=""toggle-text text-muted"">
                            Show 1 task
                        </span>
                    </button>
                    <div class=""collapse mt-3"" id=""collapse-shipped"">
                        <div class=""task-list"">
                            <div class=""task-item d-flex align-items-start mb-3"">
                                <div class=""form-check me-3"">
                                    <input type=""checkbox"" 
                                           class=""form-check-input"" 
                                           id=""task-checkbox-1"" 
                                           disabled />
                                </div>
                                <div class=""flex-grow-1"">
                                    <label class=""task-name"" for=""task-checkbox-1"">
                                        Task 1
                                    </label>
                                    <div class=""task-owner"">
                                        <small class=""text-muted"">Alice</small>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        ");
    }

    [Fact]
    public void StatusCard_WithNoTasks_RendersEmptyState()
    {
        // Arrange
        var tasks = new List<TaskItem>();

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "In-Progress")
            .Add(p => p.TaskCount, 0)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "bg-primary"));

        // Assert
        var markup = component.Markup;
        Assert.Contains("No tasks", markup);
    }

    [Fact]
    public void StatusCard_WithMultipleTasks_RendersAllTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new() { Id = "1", Name = "Task 1", Owner = "Alice", Status = TaskStatus.Shipped },
            new() { Id = "2", Name = "Task 2", Owner = "Bob", Status = TaskStatus.Shipped },
            new() { Id = "3", Name = "Task 3", Owner = "Charlie", Status = TaskStatus.Shipped }
        };

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 3)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "bg-success"));

        // Assert
        var markup = component.Markup;
        Assert.Contains("Task 1", markup);
        Assert.Contains("Task 2", markup);
        Assert.Contains("Task 3", markup);
        Assert.Contains("Alice", markup);
        Assert.Contains("Bob", markup);
        Assert.Contains("Charlie", markup);
        Assert.Contains("Show 3 tasks", markup);
    }

    [Fact]
    public void StatusCard_WithInProgressStatus_UsesBlueColor()
    {
        // Arrange
        var tasks = new List<TaskItem>();

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "In-Progress")
            .Add(p => p.TaskCount, 0)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "bg-primary"));

        // Assert
        var markup = component.Markup;
        Assert.Contains("bg-primary", markup);
    }

    [Fact]
    public void StatusCard_WithCarriedOverStatus_UsesOrangeColor()
    {
        // Arrange
        var tasks = new List<TaskItem>();

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Carried-Over")
            .Add(p => p.TaskCount, 0)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "bg-warning"));

        // Assert
        var markup = component.Markup;
        Assert.Contains("bg-warning", markup);
    }

    [Fact]
    public void StatusCard_WithTaskMissingOwner_RenderTaskWithoutOwnerInfo()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new() { Id = "1", Name = "Task Without Owner", Owner = "", Status = TaskStatus.Shipped }
        };

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 1)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "bg-success"));

        // Assert
        var markup = component.Markup;
        Assert.Contains("Task Without Owner", markup);
        Assert.DoesNotContain("task-owner", markup);
    }

    [Fact]
    public void StatusCard_WithNullTaskInList_SkipsNullTask()
    {
        // Arrange
        var tasks = new List<TaskItem?>
        {
            new() { Id = "1", Name = "Task 1", Owner = "Alice", Status = TaskStatus.Shipped },
            null,
            new() { Id = "3", Name = "Task 3", Owner = "Charlie", Status = TaskStatus.Shipped }
        }.Cast<TaskItem>().ToList();

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 2)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "bg-success"));

        // Assert
        var markup = component.Markup;
        Assert.Contains("Task 1", markup);
        Assert.Contains("Task 3", markup);
    }

    [Fact]
    public void StatusCard_BadgeDisplaysCorrectTaskCount()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new() { Id = "1", Name = "Task 1", Owner = "Alice", Status = TaskStatus.Shipped },
            new() { Id = "2", Name = "Task 2", Owner = "Bob", Status = TaskStatus.Shipped }
        };

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 2)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "bg-success"));

        // Assert
        var markup = component.Markup;
        Assert.Contains("<span class=\"badge bg-light text-dark\">2</span>", markup);
    }

    [Fact]
    public void StatusCard_WithSpecialCharactersInTaskName_RendersCorrectly()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new() { Id = "1", Name = "Task & Testing \"Special\" <Chars>", Owner = "Alice", Status = TaskStatus.Shipped }
        };

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 1)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "bg-success"));

        // Assert
        var markup = component.Markup;
        Assert.Contains("Task &amp; Testing &quot;Special&quot; &lt;Chars&gt;", markup);
    }

    [Fact]
    public void StatusCard_TaskCountMismatchShowsWarning()
    {
        // Arrange - TaskCount doesn't match actual tasks
        var tasks = new List<TaskItem>
        {
            new() { Id = "1", Name = "Task 1", Owner = "Alice", Status = TaskStatus.Shipped }
        };

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 5) // Mismatch: 5 claimed but 1 actual
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "bg-success"));

        // Assert - Component should still render, showing actual count
        var markup = component.Markup;
        Assert.Contains("Task 1", markup);
        Assert.Contains("<span class=\"badge bg-light text-dark\">1</span>", markup);
    }

    [Fact]
    public void StatusCard_NullTasksList_InitializesEmptyList()
    {
        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 0)
            .Add(p => p.Tasks, null)
            .Add(p => p.CardColor, "bg-success"));

        // Assert
        var markup = component.Markup;
        Assert.Contains("No tasks", markup);
    }
}