using System.Collections.Generic;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Data;
using Bunit;
using Xunit;

namespace AgentSquad.Runner.Tests.Components
{
    public class StatusCardTests : TestContext
    {
        [Fact]
        public void StatusCard_DisplaysStatusCategory()
        {
            var tasks = new List<ProjectTask>();

            var component = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 5)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "bg-success"));

            Assert.Contains("Shipped", component.Markup);
        }

        [Fact]
        public void StatusCard_DisplaysTaskCount()
        {
            var tasks = new List<ProjectTask>
            {
                new ProjectTask { Name = "Task 1", Status = "Shipped", Owner = "Alice" },
                new ProjectTask { Name = "Task 2", Status = "Shipped", Owner = "Bob" },
                new ProjectTask { Name = "Task 3", Status = "Shipped", Owner = "Charlie" }
            };

            var component = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 3)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "bg-success"));

            Assert.Contains("3", component.Markup);
        }

        [Fact]
        public void StatusCard_ShippedUsesGreenColor()
        {
            var tasks = new List<ProjectTask>();

            var component = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 0)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "bg-success"));

            Assert.Contains("bg-success", component.Markup);
        }

        [Fact]
        public void StatusCard_InProgressUsesBlueColor()
        {
            var tasks = new List<ProjectTask>();

            var component = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "In Progress")
                .Add(p => p.TaskCount, 0)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "bg-info"));

            Assert.Contains("bg-info", component.Markup);
        }

        [Fact]
        public void StatusCard_CarriedOverUsesOrangeColor()
        {
            var tasks = new List<ProjectTask>();

            var component = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "Carried Over")
                .Add(p => p.TaskCount, 0)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "bg-warning"));

            Assert.Contains("bg-warning", component.Markup);
        }

        [Fact]
        public void StatusCard_ListsAllTasks()
        {
            var tasks = new List<ProjectTask>
            {
                new ProjectTask { Name = "API Development", Status = "Shipped", Owner = "Alice" },
                new ProjectTask { Name = "UI Design", Status = "Shipped", Owner = "Bob" },
                new ProjectTask { Name = "Testing", Status = "Shipped", Owner = "Charlie" }
            };

            var component = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 3)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "bg-success"));

            Assert.Contains("API Development", component.Markup);
            Assert.Contains("UI Design", component.Markup);
            Assert.Contains("Testing", component.Markup);
        }

        [Fact]
        public void StatusCard_DisplaysTaskOwners()
        {
            var tasks = new List<ProjectTask>
            {
                new ProjectTask { Name = "Task 1", Status = "Shipped", Owner = "Alice" }
            };

            var component = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 1)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "bg-success"));

            Assert.Contains("Alice", component.Markup);
        }

        [Fact]
        public void StatusCard_WithEmptyTaskList_RendersSafely()
        {
            var tasks = new List<ProjectTask>();

            var component = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 0)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "bg-success"));

            Assert.NotNull(component.Markup);
        }

        [Fact]
        public void StatusCard_UsesResponsiveGrid()
        {
            var tasks = new List<ProjectTask>();

            var component = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 0)
                .Add(p => p.Tasks, tasks)
                .Add(p => p.CardColor, "bg-success"));

            Assert.Contains("col", component.Markup);
        }
    }
}