using Bunit;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Models;
using System.Collections.Generic;

namespace AgentSquad.Tests.Components
{
    public class StatusCardTests : TestContext
    {
        [Fact]
        public void StatusCard_RendersTasks()
        {
            var tasks = new List<ProjectTask>
            {
                new ProjectTask { Id = "t1", Name = "Task 1", Status = "InProgress", Owner = "John" },
                new ProjectTask { Id = "t2", Name = "Task 2", Status = "Shipped", Owner = "Jane" }
            };

            var component = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.Tasks, tasks)
            );

            var html = component.Markup;
            Assert.Contains("Task 1", html);
            Assert.Contains("Task 2", html);
            Assert.Contains("John", html);
            Assert.Contains("Jane", html);
        }

        [Fact]
        public void StatusCard_DisplaysTaskStatus()
        {
            var tasks = new List<ProjectTask>
            {
                new ProjectTask { Id = "t1", Name = "Test", Status = "InProgress", Owner = "User" }
            };

            var component = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.Tasks, tasks)
            );

            Assert.Contains("InProgress", component.Markup);
        }
    }
}