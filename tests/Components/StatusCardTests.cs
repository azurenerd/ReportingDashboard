using Xunit;
using Bunit;
using AgentSquad.Dashboard.Components;
using AgentSquad.Dashboard.Models;

namespace AgentSquad.Dashboard.Tests.Components;

public class StatusCardTests : TestContext
{
    [Fact]
    public void StatusCard_DisplaysStatusCategory()
    {
        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 5)
                .Add(p => p.Tasks, new List<Task>())
                .Add(p => p.CardColor, "success")
        );

        Assert.Contains("Shipped", component.Markup);
    }

    [Fact]
    public void StatusCard_DisplaysTaskCount()
    {
        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "In-Progress")
                .Add(p => p.TaskCount, 3)
                .Add(p => p.Tasks, new List<Task>())
                .Add(p => p.CardColor, "info")
        );

        Assert.Contains("3", component.Markup);
    }

    [Fact]
    public void StatusCard_UsesCorrectColorClass()
    {
        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 5)
                .Add(p => p.Tasks, new List<Task>())
                .Add(p => p.CardColor, "success")
        );

        Assert.Contains("border-success", component.Markup);
        Assert.Contains("bg-success", component.Markup);
    }

    [Fact]
    public void StatusCard_DisplaysTaskList()
    {
        var tasks = new List<Task>
        {
            new Task { Id = "T1", Name = "Implement auth", Status = TaskStatus.Shipped, AssignedTo = "John Doe", DueDate = DateTime.Now, EstimatedDays = 5 },
            new Task { Id = "T2", Name = "Setup database", Status = TaskStatus.Shipped, AssignedTo = "Jane Smith", DueDate = DateTime.Now.AddDays(1), EstimatedDays = 3 }
        };

        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 2)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "success")
        );

        Assert.Contains("Implement auth", component.Markup);
        Assert.Contains("Setup database", component.Markup);
    }

    [Fact]
    public void StatusCard_DisplaysTaskAssignee()
    {
        var tasks = new List<Task>
        {
            new Task { Id = "T1", Name = "Task 1", Status = TaskStatus.Shipped, AssignedTo = "Alice Johnson", DueDate = DateTime.Now, EstimatedDays = 5 }
        };

        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 1)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "success")
        );

        Assert.Contains("Alice Johnson", component.Markup);
    }

    [Fact]
    public void StatusCard_HandleEmptyTaskList()
    {
        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Carried-Over")
                .Add(p => p.TaskCount, 0)
                .Add(p => p.Tasks, new List<Task>())
                .Add(p => p.CardColor, "warning")
        );

        var listItems = component.FindAll(".list-group-item");
        Assert.Empty(listItems);
    }

    [Fact]
    public void StatusCard_CarriedOverCardUsesOrangeColor()
    {
        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Carried-Over")
                .Add(p => p.TaskCount, 2)
                .Add(p => p.Tasks, new List<Task>())
                .Add(p => p.CardColor, "warning")
        );

        Assert.Contains("border-warning", component.Markup);
        Assert.Contains("bg-warning", component.Markup);
    }

    [Fact]
    public void StatusCard_InProgressCardUsesBlueColor()
    {
        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "In-Progress")
                .Add(p => p.TaskCount, 3)
                .Add(p => p.Tasks, new List<Task>())
                .Add(p => p.CardColor, "info")
        );

        Assert.Contains("border-info", component.Markup);
        Assert.Contains("bg-info", component.Markup);
    }
}