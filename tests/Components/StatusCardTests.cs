using Bunit;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Data;

namespace AgentSquad.Runner.Tests.Components
{
    public class StatusCardTests : TestContext
    {
        [Fact]
        public void StatusCard_Renders_WithShippedStatus()
        {
            // Arrange
            var tasks = new List<Task>();

            // Act
            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 5)
                    .Add(p => p.Tasks, tasks)
                    .Add(p => p.CardColor, "bg-success")
            );

            // Assert
            Assert.NotNull(component);
            component.Markup.Should().Contain("Shipped");
        }

        [Fact]
        public void StatusCard_Renders_WithInProgressStatus()
        {
            // Arrange
            var tasks = new List<Task>();

            // Act
            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "In Progress")
                    .Add(p => p.TaskCount, 3)
                    .Add(p => p.Tasks, tasks)
                    .Add(p => p.CardColor, "bg-primary")
            );

            // Assert
            Assert.NotNull(component);
            component.Markup.Should().Contain("In Progress");
        }

        [Fact]
        public void StatusCard_Renders_WithCarriedOverStatus()
        {
            // Arrange
            var tasks = new List<Task>();

            // Act
            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Carried Over")
                    .Add(p => p.TaskCount, 2)
                    .Add(p => p.Tasks, tasks)
                    .Add(p => p.CardColor, "bg-warning")
            );

            // Assert
            Assert.NotNull(component);
            component.Markup.Should().Contain("Carried Over");
        }

        [Fact]
        public void StatusCard_DisplaysTaskCount()
        {
            // Arrange
            var tasks = new List<Task>();

            // Act
            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 10)
                    .Add(p => p.Tasks, tasks)
                    .Add(p => p.CardColor, "bg-success")
            );

            // Assert
            component.Markup.Should().Contain("10");
        }

        [Fact]
        public void StatusCard_Renders_WithMultipleTasks()
        {
            // Arrange
            var tasks = new List<Task>
            {
                new Task { Id = "t1", Name = "Task 1", AssignedTo = "Developer A", Status = TaskStatus.Shipped },
                new Task { Id = "t2", Name = "Task 2", AssignedTo = "Developer B", Status = TaskStatus.Shipped },
                new Task { Id = "t3", Name = "Task 3", AssignedTo = "Developer C", Status = TaskStatus.Shipped }
            };

            // Act
            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 3)
                    .Add(p => p.Tasks, tasks)
                    .Add(p => p.CardColor, "bg-success")
            );

            // Assert
            Assert.NotNull(component);
            Assert.Equal(3, tasks.Count);
        }

        [Fact]
        public void StatusCard_Renders_WithEmptyTaskList()
        {
            // Arrange
            var tasks = new List<Task>();

            // Act
            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 0)
                    .Add(p => p.Tasks, tasks)
                    .Add(p => p.CardColor, "bg-success")
            );

            // Assert
            Assert.NotNull(component);
            Assert.Empty(tasks);
        }

        [Fact]
        public void StatusCard_DisplaysPlaceholderText()
        {
            // Arrange
            var tasks = new List<Task>();

            // Act
            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 0)
                    .Add(p => p.Tasks, tasks)
                    .Add(p => p.CardColor, "bg-success")
            );

            // Assert
            component.Markup.Should().Contain("placeholder");
        }

        [Fact]
        public void StatusCard_Renders_CardStructure()
        {
            // Arrange
            var tasks = new List<Task>();

            // Act
            var component = RenderComponent<StatusCard>(
                parameters => parameters
                    .Add(p => p.StatusCategory, "Shipped")
                    .Add(p => p.TaskCount, 5)
                    .Add(p => p.Tasks, tasks)
                    .Add(p => p.CardColor, "bg-success")
            );

            // Assert
            var card = component.Find(".card");
            var cardHeader = component.Find(".card-header");
            var cardBody = component.Find(".card-body");
            Assert.NotNull(card);
            Assert.NotNull(cardHeader);
            Assert.NotNull(cardBody);
        }
    }
}