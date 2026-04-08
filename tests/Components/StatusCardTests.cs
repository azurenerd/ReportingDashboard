using Xunit;
using Bunit;
using AgentSquad.Components;
using AgentSquad.Data;

namespace AgentSquad.Tests.Components
{
    public class StatusCardTests : TestContext
    {
        [Fact]
        public void StatusCard_WithValidStatusCategory_DisplaysCategory()
        {
            var tasks = new List<Task> 
            { 
                new Task { Id = "t1", Name = "Task 1", Status = "In Progress", AssignedTo = "Dev", DueDate = DateTime.Now.AddDays(5) } 
            };

            var component = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.StatusCategory, "Active")
                          .Add(p => p.TaskCount, 1)
                          .Add(p => p.Tasks, tasks)
                          .Add(p => p.CardColor, "#FF5733"));

            Assert.Contains("Active", component.Markup);
        }

        [Fact]
        public void StatusCard_WithTaskList_DisplaysTaskNames()
        {
            var tasks = new List<Task>
            {
                new Task { Id = "t1", Name = "Review Code", Status = "Complete", AssignedTo = "Alice", DueDate = DateTime.Now },
                new Task { Id = "t2", Name = "Write Docs", Status = "In Progress", AssignedTo = "Bob", DueDate = DateTime.Now.AddDays(2) }
            };

            var component = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.StatusCategory, "Complete")
                          .Add(p => p.TaskCount, 2)
                          .Add(p => p.Tasks, tasks)
                          .Add(p => p.CardColor, "#28A745"));

            Assert.Contains("Review Code", component.Markup);
            Assert.Contains("Write Docs", component.Markup);
        }

        [Fact]
        public void StatusCard_WithEmptyTasks_RenderWithZeroCount()
        {
            var tasks = new List<Task>();
            var component = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.StatusCategory, "Pending")
                          .Add(p => p.TaskCount, 0)
                          .Add(p => p.Tasks, tasks)
                          .Add(p => p.CardColor, "#FFC107"));

            Assert.Contains("0", component.Markup);
        }

        [Fact]
        public void StatusCard_RequiresStatusCategoryParameter()
        {
            var tasks = new List<Task>();
            Assert.Throws<InvalidOperationException>(() =>
            {
                var component = RenderComponent<StatusCard>(parameters =>
                    parameters.Add(p => p.TaskCount, 0)
                              .Add(p => p.Tasks, tasks)
                              .Add(p => p.CardColor, "#FFC107"));
            });
        }

        [Fact]
        public void StatusCard_AppliesCardColor()
        {
            var customColor = "#1E90FF";
            var tasks = new List<Task>();

            var component = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.StatusCategory, "In Progress")
                          .Add(p => p.TaskCount, 0)
                          .Add(p => p.Tasks, tasks)
                          .Add(p => p.CardColor, customColor));

            Assert.Contains(customColor, component.Markup);
        }

        [Fact]
        public void StatusCard_DisplaysTaskStatus()
        {
            var tasks = new List<Task>
            {
                new Task { Id = "t1", Name = "Deploy", Status = "Blocked", AssignedTo = "Charlie", DueDate = DateTime.Now }
            };

            var component = RenderComponent<StatusCard>(parameters =>
                parameters.Add(p => p.StatusCategory, "Active")
                          .Add(p => p.TaskCount, 1)
                          .Add(p => p.Tasks, tasks)
                          .Add(p => p.CardColor, "#FFA500"));

            Assert.Contains("Blocked", component.Markup);
        }
    }
}