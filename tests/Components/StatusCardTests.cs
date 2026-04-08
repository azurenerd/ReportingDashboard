using Bunit;
using Xunit;
using AgentSquad.Components;
using AgentSquad.Models;

namespace AgentSquad.Tests.Components
{
    public class StatusCardTests : TestContext
    {
        private readonly List<TaskItem> _testTasks;

        public StatusCardTests()
        {
            _testTasks = new List<TaskItem>
            {
                new TaskItem { Id = "T1", Name = "Dashboard UI", Status = TaskStatus.Shipped, AssignedTo = "Engineer 1", DueDate = new DateTime(2026, 1, 15) },
                new TaskItem { Id = "T2", Name = "Error Handling", Status = TaskStatus.Shipped, AssignedTo = "Engineer 2", DueDate = new DateTime(2026, 2, 1) },
                new TaskItem { Id = "T3", Name = "Data Service", Status = TaskStatus.Shipped, AssignedTo = "Engineer 1", DueDate = new DateTime(2026, 1, 30) }
            };
        }

        [Fact]
        public void StatusCard_ThrowsWhenStatusCategoryNotProvided()
        {
            var parameters = new ComponentParameter[]
            {
                ComponentParameter.CreateParameter(nameof(StatusCard.TaskCount), 3),
                ComponentParameter.CreateParameter(nameof(StatusCard.Tasks), _testTasks),
                ComponentParameter.CreateParameter(nameof(StatusCard.CardColor), "status-card-green")
            };

            var exception = Assert.Throws<InvalidOperationException>(() =>
                RenderComponent<StatusCard>(parameters));

            Assert.NotNull(exception);
        }

        [Fact]
        public void StatusCard_DisplaysStatusCategory()
        {
            var component = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 3)
                .Add(p => p.Tasks, _testTasks)
                .Add(p => p.CardColor, "status-card-green"));

            Assert.Contains("Shipped", component.Markup);
        }

        [Fact]
        public void StatusCard_DisplaysTaskCount()
        {
            var component = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 3)
                .Add(p => p.Tasks, _testTasks)
                .Add(p => p.CardColor, "status-card-green"));

            Assert.Contains("3", component.Markup);
        }

        [Fact]
        public void StatusCard_DisplaysCardColor()
        {
            var component = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 3)
                .Add(p => p.Tasks, _testTasks)
                .Add(p => p.CardColor, "status-card-green"));

            Assert.Contains("status-card-green", component.Markup);
        }

        [Fact]
        public void StatusCard_DisplaysTaskList()
        {
            var component = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 3)
                .Add(p => p.Tasks, _testTasks)
                .Add(p => p.CardColor, "status-card-green"));

            Assert.Contains("View Tasks", component.Markup);
            Assert.Contains("Dashboard UI", component.Markup);
            Assert.Contains("Error Handling", component.Markup);
        }

        [Fact]
        public void StatusCard_DisplaysAssignedTo()
        {
            var component = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 3)
                .Add(p => p.Tasks, _testTasks)
                .Add(p => p.CardColor, "status-card-green"));

            Assert.Contains("Engineer 1", component.Markup);
            Assert.Contains("Engineer 2", component.Markup);
        }

        [Fact]
        public void StatusCard_ShowsMessageWhenNoTasks()
        {
            var component = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 0)
                .Add(p => p.Tasks, new List<TaskItem>())
                .Add(p => p.CardColor, "status-card-green"));

            Assert.Contains("No tasks", component.Markup);
        }

        [Fact]
        public void StatusCard_UsesDifferentColorsForStatuses()
        {
            var greenComponent = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 12)
                .Add(p => p.Tasks, _testTasks)
                .Add(p => p.CardColor, "status-card-green"));

            var blueComponent = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "In Progress")
                .Add(p => p.TaskCount, 5)
                .Add(p => p.Tasks, new List<TaskItem>())
                .Add(p => p.CardColor, "status-card-blue"));

            var orangeComponent = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "Carried Over")
                .Add(p => p.TaskCount, 2)
                .Add(p => p.Tasks, new List<TaskItem>())
                .Add(p => p.CardColor, "status-card-orange"));

            Assert.Contains("status-card-green", greenComponent.Markup);
            Assert.Contains("status-card-blue", blueComponent.Markup);
            Assert.Contains("status-card-orange", orangeComponent.Markup);
        }

        [Fact]
        public void StatusCard_ResponsiveGridLayout()
        {
            var component = RenderComponent<StatusCard>(parameters => parameters
                .Add(p => p.StatusCategory, "Shipped")
                .Add(p => p.TaskCount, 12)
                .Add(p => p.Tasks, _testTasks)
                .Add(p => p.CardColor, "status-card-green"));

            Assert.Contains(".card", component.Markup);
        }
    }
}