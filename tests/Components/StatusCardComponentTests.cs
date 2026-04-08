using Bunit;
using Xunit;
using AgentSquad.Dashboard.Services;
using AgentSquad.Runner.Components;

namespace AgentSquad.Tests.Components;

public class StatusCardComponentTests : TestContext
{
    #region Happy Path Tests

    [Fact]
    public void StatusCard_WithShippedStatus_DisplaysCorrectTitle()
    {
        var tasks = new List<Task>();
        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 5)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "success")
        );

        Assert.Contains("Shipped", component.Markup);
    }

    [Fact]
    public void StatusCard_WithInProgressStatus_DisplaysCorrectTitle()
    {
        var tasks = new List<Task>();
        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "In-Progress")
                .Add(p => p.TaskCount, 3)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "info")
        );

        Assert.Contains("In-Progress", component.Markup);
    }

    [Fact]
    public void StatusCard_WithCarriedOverStatus_DisplaysCorrectTitle()
    {
        var tasks = new List<Task>();
        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Carried-Over")
                .Add(p => p.TaskCount, 2)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "warning")
        );

        Assert.Contains("Carried-Over", component.Markup);
    }

    [Fact]
    public void StatusCard_DisplaysTaskCount()
    {
        var tasks = new List<Task>();
        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 7)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "success")
        );

        Assert.Contains("7", component.Markup);
    }

    [Fact]
    public void StatusCard_WithTasks_DisplaysShowTasksButton()
    {
        var tasks = new List<Task>
        {
            new()
            {
                Id = "t1",
                Name = "Task 1",
                Status = TaskStatus.Shipped,
                AssignedTo = "John",
                DueDate = new DateTime(2024, 3, 1),
                EstimatedDays = 5,
                RelatedMilestone = "m1"
            }
        };

        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 1)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "success")
        );

        Assert.Contains("Show Tasks", component.Markup);
    }

    [Fact]
    public void StatusCard_WhenShowTasksClicked_DisplaysTaskList()
    {
        var tasks = new List<Task>
        {
            new()
            {
                Id = "t1",
                Name = "API Integration",
                Status = TaskStatus.Shipped,
                AssignedTo = "Bob Johnson",
                DueDate = new DateTime(2024, 3, 1),
                EstimatedDays = 5,
                RelatedMilestone = "m1"
            }
        };

        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 1)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "success")
        );

        var button = component.Find("button");
        button?.Click();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("API Integration", component.Markup);
        });
    }

    [Fact]
    public void StatusCard_DisplaysTaskName()
    {
        var tasks = new List<Task>
        {
            new()
            {
                Id = "t1",
                Name = "Database Migration",
                Status = TaskStatus.InProgress,
                AssignedTo = "Carol",
                DueDate = new DateTime(2024, 4, 15),
                EstimatedDays = 8,
                RelatedMilestone = "m2"
            }
        };

        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "In-Progress")
                .Add(p => p.TaskCount, 1)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "info")
        );

        var button = component.Find("button");
        button?.Click();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("Database Migration", component.Markup);
        });
    }

    [Fact]
    public void StatusCard_DisplaysAssigneeForTask()
    {
        var tasks = new List<Task>
        {
            new()
            {
                Id = "t1",
                Name = "Task 1",
                Status = TaskStatus.Shipped,
                AssignedTo = "Alice Smith",
                DueDate = new DateTime(2024, 3, 1),
                EstimatedDays = 5,
                RelatedMilestone = "m1"
            }
        };

        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 1)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "success")
        );

        var button = component.Find("button");
        button?.Click();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("Alice Smith", component.Markup);
        });
    }

    [Fact]
    public void StatusCard_DisplaysMultipleTasks()
    {
        var tasks = new List<Task>
        {
            new()
            {
                Id = "t1",
                Name = "Task 1",
                Status = TaskStatus.Shipped,
                AssignedTo = "John",
                DueDate = new DateTime(2024, 3, 1),
                EstimatedDays = 5,
                RelatedMilestone = "m1"
            },
            new()
            {
                Id = "t2",
                Name = "Task 2",
                Status = TaskStatus.Shipped,
                AssignedTo = "Jane",
                DueDate = new DateTime(2024, 3, 15),
                EstimatedDays = 3,
                RelatedMilestone = "m1"
            }
        };

        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 2)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "success")
        );

        var button = component.Find("button");
        button?.Click();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("Task 1", component.Markup);
            Assert.Contains("Task 2", component.Markup);
        });
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void StatusCard_WithZeroTasks_DisplaysNoTasksMessage()
    {
        var tasks = new List<Task>();

        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 0)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "success")
        );

        Assert.Contains("No tasks in this category", component.Markup);
    }

    [Fact]
    public void StatusCard_WithZeroTasks_NoShowTasksButton()
    {
        var tasks = new List<Task>();

        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 0)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "success")
        );

        Assert.DoesNotContain("Show Tasks", component.Markup);
    }

    [Fact]
    public void StatusCard_WithTaskWithoutAssignee_DisplaysTaskName()
    {
        var tasks = new List<Task>
        {
            new()
            {
                Id = "t1",
                Name = "Unassigned Task",
                Status = TaskStatus.CarriedOver,
                AssignedTo = "",
                DueDate = new DateTime(2024, 5, 1),
                EstimatedDays = 7,
                RelatedMilestone = "m2"
            }
        };

        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Carried-Over")
                .Add(p => p.TaskCount, 1)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "warning")
        );

        var button = component.Find("button");
        button?.Click();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("Unassigned Task", component.Markup);
        });
    }

    [Fact]
    public void StatusCard_ToggleTaskList_HidesWhenClickedAgain()
    {
        var tasks = new List<Task>
        {
            new()
            {
                Id = "t1",
                Name = "Task 1",
                Status = TaskStatus.Shipped,
                AssignedTo = "John",
                DueDate = new DateTime(2024, 3, 1),
                EstimatedDays = 5,
                RelatedMilestone = "m1"
            }
        };

        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 1)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "success")
        );

        var button = component.Find("button");
        button?.Click();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("Task 1", component.Markup);
        });

        button?.Click();

        component.WaitForAssertion(() =>
        {
            Assert.Contains("Show Tasks", component.Markup);
        });
    }

    [Fact]
    public void StatusCard_SuccessColor_DisplaysGreen()
    {
        var tasks = new List<Task>();

        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 5)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "success")
        );

        Assert.Contains("28a745", component.Markup);
    }

    [Fact]
    public void StatusCard_InfoColor_DisplaysBlue()
    {
        var tasks = new List<Task>();

        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "In-Progress")
                .Add(p => p.TaskCount, 3)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "info")
        );

        Assert.Contains("17a2b8", component.Markup);
    }

    [Fact]
    public void StatusCard_WarningColor_DisplaysOrange()
    {
        var tasks = new List<Task>();

        var component = RenderComponent<StatusCard>(
            parameters => parameters
                .Add(p => p.StatusCategory, "Carried-Over")
                .Add(p => p.TaskCount, 2)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "warning")
        );

        Assert.Contains("ffc107", component.Markup);
    }

    #endregion
}