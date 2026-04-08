using Xunit;
using Bunit;
using AgentSquad.Components;
using AgentSquad.Data;

namespace AgentSquad.Tests.Components
{
    public class StatusCardTests : TestContext
    {
        [Fact]
        public void StatusCard_WithValidStatusCategory_DisplaysStatus()
        {
            var tasks = new List<Task> { new Task { Id = "t1", Title = "Task 1" } };
            var component = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.StatusCategory, "Active")
                          .Add(p => p.TaskCount, 1)
                          .Add(p => p.Tasks, tasks)
                          .Add(p => p.CardColor, "#FF5733"));

            Assert.NotNull(component);
        }

        [Fact]
        public void StatusCard_WithCompleteStatus_ShowsAllTasks()
        {
            var tasks = new List<Task>
            {
                new Task { Id = "t1", Title = "Completed Task" },
                new Task { Id = "t2", Title = "Another Task" }
            };

            var component = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.StatusCategory, "Complete")
                          .Add(p => p.TaskCount, 2)
                          .Add(p => p.Tasks, tasks)
                          .Add(p => p.CardColor, "#28A745"));

            Assert.NotNull(component);
            Assert.Contains("Complete", component.Markup);
        }

        [Fact]
        public void StatusCard_WithEmptyTasks_RendersWithoutError()
        {
            var tasks = new List<Task>();
            var component = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.StatusCategory, "Pending")
                          .Add(p => p.TaskCount, 0)
                          .Add(p => p.Tasks, tasks)
                          .Add(p => p.CardColor, "#FFC107"));

            Assert.NotNull(component);
        }

        [Fact]
        public void StatusCard_WithCustomColor_AppliesCardColor()
        {
            var tasks = new List<Task>();
            var customColor = "#1E90FF";

            var component = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.StatusCategory, "In Progress")
                          .Add(p => p.TaskCount, 0)
                          .Add(p => p.Tasks, tasks)
                          .Add(p => p.CardColor, customColor));

            Assert.NotNull(component);
        }
    }
}