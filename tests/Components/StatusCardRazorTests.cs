using Xunit;
using Bunit;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Components;

namespace AgentSquad.Runner.Tests.Components
{
    public class StatusCardRazorTests : TestContext
    {
        [Fact]
        public void StatusCard_WithZeroTasks_DisplaysZeroCount()
        {
            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 0)
                    .Add(p => p.Tasks, new List<ProjectTask>())
                    .Add(p => p.CardColor, "success")
            );

            Assert.Contains("Shipped", component.Markup);
            Assert.Contains("0", component.Markup);
        }

        [Fact]
        public void StatusCard_WithNullTasks_DisplaysPlaceholder()
        {
            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 0)
                    .Add(p => p.Tasks, null)
                    .Add(p => p.CardColor, "success")
            );

            Assert.Contains("No tasks in this category", component.Markup);
        }

        [Fact]
        public void StatusCard_WithEmptyTaskList_DisplaysPlaceholder()
        {
            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "In-Progress")
                    .Add(p => p.TaskCount, 0)
                    .Add(p => p.Tasks, new List<ProjectTask>())
                    .Add(p => p.CardColor, "primary")
            );

            Assert.Contains("No tasks in this category", component.Markup);
        }

        [Fact]
        public void StatusCard_WithTasks_DisplaysTaskCount()
        {
            var tasks = new List<ProjectTask>
            {
                new ProjectTask { Id = "t1", Name = "Task 1", Status = TaskStatus.Shipped, AssignedTo = "Dev A", DueDate = DateTime.Now, EstimatedDays = 5 },
                new ProjectTask { Id = "t2", Name = "Task 2", Status = TaskStatus.Shipped, AssignedTo = "Dev B", DueDate = DateTime.Now, EstimatedDays = 8 }
            };

            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 2)
                    .Add(p => p.Tasks, tasks)
                    .Add(p => p.CardColor, "success")
            );

            Assert.Contains("2", component.Markup);
        }

        [Fact]
        public void StatusCard_WithTasks_DisplaysViewTasksButton()
        {
            var tasks = new List<ProjectTask>
            {
                new ProjectTask { Id = "t1", Name = "Task 1", Status = TaskStatus.Shipped, AssignedTo = "Dev A", DueDate = DateTime.Now, EstimatedDays = 5 }
            };

            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 1)
                    .Add(p => p.Tasks, tasks)
                    .Add(p => p.CardColor, "success")
            );

            Assert.Contains("View Tasks", component.Markup);
        }

        [Fact]
        public void StatusCard_WithTasks_DisplaysTaskNames()
        {
            var tasks = new List<ProjectTask>
            {
                new ProjectTask { Id = "t1", Name = "Implement Feature X", Status = TaskStatus.Shipped, AssignedTo = "Dev A", DueDate = DateTime.Now, EstimatedDays = 5 },
                new ProjectTask { Id = "t2", Name = "Fix Bug Y", Status = TaskStatus.Shipped, AssignedTo = "Dev B", DueDate = DateTime.Now, EstimatedDays = 8 }
            };

            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 2)
                    .Add(p => p.Tasks, tasks)
                    .Add(p => p.CardColor, "success")
            );

            var markup = component.Markup;
            Assert.Contains("Implement Feature X", markup);
            Assert.Contains("Fix Bug Y", markup);
        }

        [Fact]
        public void StatusCard_WithTasks_DisplaysAssignedTo()
        {
            var tasks = new List<ProjectTask>
            {
                new ProjectTask { Id = "t1", Name = "Task 1", Status = TaskStatus.Shipped, AssignedTo = "John Doe", DueDate = DateTime.Now, EstimatedDays = 5 }
            };

            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 1)
                    .Add(p => p.Tasks, tasks)
                    .Add(p => p.CardColor, "success")
            );

            Assert.Contains("John Doe", component.Markup);
            Assert.Contains("Assigned to", component.Markup);
        }

        [Fact]
        public void StatusCard_WithTasksHavingEstimatedDays_DisplaysEstimate()
        {
            var tasks = new List<ProjectTask>
            {
                new ProjectTask { Id = "t1", Name = "Task 1", Status = TaskStatus.Shipped, AssignedTo = "Dev A", DueDate = DateTime.Now, EstimatedDays = 10 }
            };

            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 1)
                    .Add(p => p.Tasks, tasks)
                    .Add(p => p.CardColor, "success")
            );

            Assert.Contains("Estimated", component.Markup);
            Assert.Contains("10 days", component.Markup);
        }

        [Fact]
        public void StatusCard_WithTasksHavingMilestone_DisplaysMilestone()
        {
            var tasks = new List<ProjectTask>
            {
                new ProjectTask { Id = "t1", Name = "Task 1", Status = TaskStatus.Shipped, AssignedTo = "Dev A", DueDate = DateTime.Now, EstimatedDays = 5, RelatedMilestone = "Phase 1" }
            };

            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 1)
                    .Add(p => p.Tasks, tasks)
                    .Add(p => p.CardColor, "success")
            );

            Assert.Contains("Milestone", component.Markup);
            Assert.Contains("Phase 1", component.Markup);
        }

        [Fact]
        public void StatusCard_SuccessCard_DisplaysGreenColor()
        {
            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 5)
                    .Add(p => p.Tasks, new List<ProjectTask>())
                    .Add(p => p.CardColor, "success")
            );

            Assert.Contains("text-success", component.Markup);
        }

        [Fact]
        public void StatusCard_PrimaryCard_DisplaysBlueColor()
        {
            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "In-Progress")
                    .Add(p => p.TaskCount, 3)
                    .Add(p => p.Tasks, new List<ProjectTask>())
                    .Add(p => p.CardColor, "primary")
            );

            Assert.Contains("text-primary", component.Markup);
        }

        [Fact]
        public void StatusCard_WarningCard_DisplaysOrangeColor()
        {
            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Carried-Over")
                    .Add(p => p.TaskCount, 2)
                    .Add(p => p.Tasks, new List<ProjectTask>())
                    .Add(p => p.CardColor, "warning")
            );

            Assert.Contains("text-warning", component.Markup);
        }

        [Fact]
        public void StatusCard_WithTaskHavingZeroEstimatedDays_DoesNotDisplayEstimate()
        {
            var tasks = new List<ProjectTask>
            {
                new ProjectTask { Id = "t1", Name = "Task 1", Status = TaskStatus.Shipped, AssignedTo = "Dev A", DueDate = DateTime.Now, EstimatedDays = 0 }
            };

            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 1)
                    .Add(p => p.Tasks, tasks)
                    .Add(p => p.CardColor, "success")
            );

            var markup = component.Markup;
            var hasDaysField = markup.Contains("0 days");
            Assert.False(hasDaysField);
        }
    }
}