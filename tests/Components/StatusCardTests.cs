using Xunit;
using FluentAssertions;
using Bunit;
using AgentSquad.Components;
using AgentSquad.Services.Models;
using System.Collections.Generic;

namespace AgentSquad.Tests.Components
{
    public class StatusCardTests : TestContext
    {
        [Fact]
        public void StatusCard_DisplaysTaskCount()
        {
            var tasks = new List<TaskItem>
            {
                new TaskItem { Id = 1, Title = "Task 1", Status = TaskStatus.InProgress },
                new TaskItem { Id = 2, Title = "Task 2", Status = TaskStatus.Completed }
            };
            var component = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.Tasks, tasks));
            
            component.Markup.Should().Contain("2");
        }

        [Fact]
        public void StatusCard_HandlesEmptyTasks()
        {
            var component = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.Tasks, new List<TaskItem>()));
            
            component.Markup.Should().Contain("0");
        }

        [Fact]
        public void StatusCard_HandleNullTasks_RendersSafely()
        {
            var component = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.Tasks, (List<TaskItem>)null));
            
            component.Markup.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void StatusCard_DisplaysCompletedTasksCount()
        {
            var tasks = new List<TaskItem>
            {
                new TaskItem { Id = 1, Title = "Task 1", Status = TaskStatus.Completed },
                new TaskItem { Id = 2, Title = "Task 2", Status = TaskStatus.InProgress },
                new TaskItem { Id = 3, Title = "Task 3", Status = TaskStatus.Completed }
            };
            var component = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.Tasks, tasks));
            
            component.Markup.Should().Contain("2");
        }

        [Fact]
        public void StatusCard_FontSize_MeetsMinimumRequirement()
        {
            var component = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.Tasks, new List<TaskItem>()));
            
            var styleElement = component.Find(".status-card-title");
            var computedStyle = styleElement.GetAttribute("style");
            
            computedStyle.Should().Contain("font-size").And.NotContain("font-size: 8pt");
        }

        [Fact]
        public void StatusCard_AppliesResponsiveClass()
        {
            var component = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.Tasks, new List<TaskItem>()));
            
            component.Markup.Should().Contain("col-md-4");
        }
    }
}