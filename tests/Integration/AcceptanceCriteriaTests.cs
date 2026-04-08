using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AgentSquad.Runner.Tests.Integration;

public class AcceptanceCriteriaTests : TestContext
{
    [Fact]
    public void AC_StatusCard_DisplaysTaskCountInBoldHeader()
    {
        // Acceptance Criteria: Displays task count for assigned status category in bold header

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
        Assert.Contains("<strong>Shipped</strong>", markup);
        Assert.Contains("<span class=\"badge bg-light text-dark\">2</span>", markup);
    }

    [Fact]
    public void AC_StatusCard_UsesColorCodedHeaderStripe()
    {
        // Acceptance Criteria: Color-coded header stripe: green (shipped), blue (in-progress), orange (carried-over)

        // Test Green (Shipped)
        var shippedComponent = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 0)
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CardColor, "bg-success"));
        Assert.Contains("bg-success", shippedComponent.Markup);

        // Test Blue (In-Progress)
        var inProgressComponent = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "In-Progress")
            .Add(p => p.TaskCount, 0)
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CardColor, "bg-primary"));
        Assert.Contains("bg-primary", inProgressComponent.Markup);

        // Test Orange (Carried-Over)
        var carriedOverComponent = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Carried-Over")
            .Add(p => p.TaskCount, 0)
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CardColor, "bg-warning"));
        Assert.Contains("bg-warning", carriedOverComponent.Markup);
    }

    [Fact]
    public void AC_StatusCard_HasExpandableTaskList()
    {
        // Acceptance Criteria: Expandable task list using Bootstrap collapse component (initially collapsed on desktop)

        // Arrange
        var tasks = new List<TaskItem>
        {
            new() { Id = "1", Name = "Test Task", Owner = "Alice", Status = TaskStatus.Shipped }
        };

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 1)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "bg-success"));

        // Assert
        var markup = component.Markup;
        Assert.Contains("data-bs-toggle=\"collapse\"", markup);
        Assert.Contains("aria-expanded=\"false\"", markup);
        Assert.Contains("<div class=\"collapse mt-3\"", markup);
    }

    [Fact]
    public void AC_StatusCard_RendersTaskItemDetails()
    {
        // Acceptance Criteria: Each task item renders: checkbox icon + task name + assigned owner

        // Arrange
        var tasks = new List<TaskItem>
        {
            new() { Id = "1", Name = "API Integration", Owner = "John Smith", Status = TaskStatus.Shipped }
        };

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 1)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "bg-success"));

        // Assert
        var markup = component.Markup;
        Assert.Contains("type=\"checkbox\"", markup);
        Assert.Contains("API Integration", markup);
        Assert.Contains("John Smith", markup);
    }

    [Fact]
    public void AC_StatusCard_AcceptsRequiredParameters()
    {
        // Acceptance Criteria: Component accepts parameters: StatusCategory, TaskCount, Tasks, CardColor

        // Arrange
        var tasks = new List<TaskItem>
        {
            new() { Id = "1", Name = "Task", Owner = "Owner", Status = TaskStatus.Shipped }
        };

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 1)
            .Add(p => p.Tasks, tasks)
            .Add(p => p.CardColor, "bg-success"));

        // Assert - Verify component renders without exceptions
        Assert.NotNull(component);
        var markup = component.Markup;
        Assert.Contains("Shipped", markup);
        Assert.Contains("bg-success", markup);
    }

    [Fact]
    public void AC_StatusCard_NoAnimationsInterfering()
    {
        // Acceptance Criteria: No animations that interfere with screenshot capture

        // Act
        var component = RenderComponent<StatusCard>(parameters => parameters
            .Add(p => p.StatusCategory, "Shipped")
            .Add(p => p.TaskCount, 0)
            .Add(p => p.Tasks, new List<TaskItem>())
            .Add(p => p.CardColor, "bg-success"));

        // Assert - Verify no CSS animation classes are applied
        var markup = component.Markup;
        Assert.DoesNotContain("animate-", markup);
        Assert.DoesNotContain("transition-", markup);
        Assert.DoesNotContain("animation:", markup);
    }
}