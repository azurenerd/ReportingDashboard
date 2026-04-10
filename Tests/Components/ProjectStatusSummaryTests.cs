using Xunit;
using Bunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;
using System;
using System.Collections.Generic;

namespace AgentSquad.Runner.Tests.Components
{
    public class ProjectStatusSummaryTests : TestContext
    {
        [Fact]
        public void Component_DisplaysZeroCountsForEmptyTaskList()
        {
            var component = RenderComponent<ProjectStatusSummary>(parameters => parameters
                .Add(p => p.Tasks, new List<Models.Task>()));

            var html = component.Markup;
            Assert.Contains("0", html);
            Assert.Contains("Project Status Summary", html);
        }

        [Fact]
        public void Component_CalculatesCompletedTaskCount()
        {
            var tasks = new List<Models.Task>
            {
                new Models.Task { Id = "t1", Title = "Task 1", Description = "Desc 1", Status = Models.TaskStatus.Completed, AssignedTo = "Alice", DueDate = DateTime.UtcNow },
                new Models.Task { Id = "t2", Title = "Task 2", Description = "Desc 2", Status = Models.TaskStatus.Completed, AssignedTo = "Bob", DueDate = DateTime.UtcNow },
                new Models.Task { Id = "t3", Title = "Task 3", Description = "Desc 3", Status = Models.TaskStatus.InProgress, AssignedTo = "Charlie", DueDate = DateTime.UtcNow }
            };

            var component = RenderComponent<ProjectStatusSummary>(parameters => parameters
                .Add(p => p.Tasks, tasks));

            var html = component.Markup;
            Assert.Contains("kpi-completed", html);
            Assert.Contains("Project Status Summary", html);
        }

        [Fact]
        public void Component_CalculatesInProgressTaskCount()
        {
            var tasks = new List<Models.Task>
            {
                new Models.Task { Id = "t1", Title = "Task 1", Description = "Desc 1", Status = Models.TaskStatus.Completed, AssignedTo = "Alice", DueDate = DateTime.UtcNow },
                new Models.Task { Id = "t2", Title = "Task 2", Description = "Desc 2", Status = Models.TaskStatus.InProgress, AssignedTo = "Bob", DueDate = DateTime.UtcNow },
                new Models.Task { Id = "t3", Title = "Task 3", Description = "Desc 3", Status = Models.TaskStatus.InProgress, AssignedTo = "Charlie", DueDate = DateTime.UtcNow }
            };

            var component = RenderComponent<ProjectStatusSummary>(parameters => parameters
                .Add(p => p.Tasks, tasks));

            var html = component.Markup;
            Assert.Contains("kpi-inprogress", html);
        }

        [Fact]
        public void Component_CalculatesCarriedOverTaskCount()
        {
            var tasks = new List<Models.Task>
            {
                new Models.Task { Id = "t1", Title = "Task 1", Description = "Desc 1", Status = Models.TaskStatus.CarriedOver, AssignedTo = "Alice", DueDate = DateTime.UtcNow },
                new Models.Task { Id = "t2", Title = "Task 2", Description = "Desc 2", Status = Models.TaskStatus.InProgress, AssignedTo = "Bob", DueDate = DateTime.UtcNow }
            };

            var component = RenderComponent<ProjectStatusSummary>(parameters => parameters
                .Add(p => p.Tasks, tasks));

            var html = component.Markup;
            Assert.Contains("kpi-carriedover", html);
        }

        [Fact]
        public void Component_RendersAllThreeKPICards()
        {
            var tasks = new List<Models.Task>
            {
                new Models.Task { Id = "t1", Title = "Task 1", Description = "Desc 1", Status = Models.TaskStatus.Completed, AssignedTo = "Alice", DueDate = DateTime.UtcNow },
                new Models.Task { Id = "t2", Title = "Task 2", Description = "Desc 2", Status = Models.TaskStatus.InProgress, AssignedTo = "Bob", DueDate = DateTime.UtcNow },
                new Models.Task { Id = "t3", Title = "Task 3", Description = "Desc 3", Status = Models.TaskStatus.CarriedOver, AssignedTo = "Charlie", DueDate = DateTime.UtcNow }
            };

            var component = RenderComponent<ProjectStatusSummary>(parameters => parameters
                .Add(p => p.Tasks, tasks));

            var html = component.Markup;
            Assert.Contains("Tasks Completed", html);
            Assert.Contains("In Progress", html);
            Assert.Contains("Carried Over", html);
        }

        [Fact]
        public void Component_HandleNullTaskList()
        {
            var component = RenderComponent<ProjectStatusSummary>(parameters => parameters
                .Add(p => p.Tasks, (List<Models.Task>)null));

            Assert.NotNull(component);
        }

        [Fact]
        public void Component_CalculatesPercentagesCorrectly()
        {
            var tasks = new List<Models.Task>
            {
                new Models.Task { Id = "t1", Title = "Task 1", Description = "Desc 1", Status = Models.TaskStatus.Completed, AssignedTo = "Alice", DueDate = DateTime.UtcNow },
                new Models.Task { Id = "t2", Title = "Task 2", Description = "Desc 2", Status = Models.TaskStatus.Completed, AssignedTo = "Bob", DueDate = DateTime.UtcNow },
                new Models.Task { Id = "t3", Title = "Task 3", Description = "Desc 3", Status = Models.TaskStatus.InProgress, AssignedTo = "Charlie", DueDate = DateTime.UtcNow },
                new Models.Task { Id = "t4", Title = "Task 4", Description = "Desc 4", Status = Models.TaskStatus.CarriedOver, AssignedTo = "David", DueDate = DateTime.UtcNow }
            };

            var component = RenderComponent<ProjectStatusSummary>(parameters => parameters
                .Add(p => p.Tasks, tasks));

            var html = component.Markup;
            Assert.Contains("kpi-cards", html);
        }

        [Fact]
        public void Component_ResponsiveDesign()
        {
            var tasks = new List<Models.Task>
            {
                new Models.Task { Id = "t1", Title = "Task 1", Description = "Desc 1", Status = Models.TaskStatus.Completed, AssignedTo = "Alice", DueDate = DateTime.UtcNow }
            };

            var component = RenderComponent<ProjectStatusSummary>(parameters => parameters
                .Add(p => p.Tasks, tasks));

            var html = component.Markup;
            Assert.Contains("status-summary-container", html);
            Assert.Contains("kpi-cards", html);
        }
    }
}