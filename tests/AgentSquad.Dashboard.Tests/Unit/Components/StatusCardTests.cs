using Xunit;
using AgentSquad.Dashboard.Components;
using System.Collections.Generic;

namespace AgentSquad.Dashboard.Tests.Unit.Components
{
    [Trait("Category", "Unit")]
    public class StatusCardTests
    {
        [Fact]
        public void GetHeaderClass_WithSuccessColor_ReturnsSuccessClass()
        {
            // Arrange
            var card = new StatusCard { CardColor = "success" };

            // Act
            var result = card.GetHeaderClass();

            // Assert
            Assert.Equal("bg-success", result);
        }

        [Fact]
        public void GetHeaderClass_WithPrimaryColor_ReturnsPrimaryClass()
        {
            // Arrange
            var card = new StatusCard { CardColor = "primary" };

            // Act
            var result = card.GetHeaderClass();

            // Assert
            Assert.Equal("bg-primary", result);
        }

        [Fact]
        public void GetHeaderClass_WithWarningColor_ReturnsWarningClass()
        {
            // Arrange
            var card = new StatusCard { CardColor = "warning" };

            // Act
            var result = card.GetHeaderClass();

            // Assert
            Assert.Equal("bg-warning", result);
        }

        [Fact]
        public void GetHeaderClass_WithUnknownColor_ReturnsSecondaryClass()
        {
            // Arrange
            var card = new StatusCard { CardColor = "unknown" };

            // Act
            var result = card.GetHeaderClass();

            // Assert
            Assert.Equal("bg-secondary", result);
        }

        [Fact]
        public void GetHeaderClass_WithNullColor_ReturnsSecondaryClass()
        {
            // Arrange
            var card = new StatusCard { CardColor = null };

            // Act
            var result = card.GetHeaderClass();

            // Assert
            Assert.Equal("bg-secondary", result);
        }

        [Fact]
        public void ToggleExpanded_FromFalse_BecomesTrue()
        {
            // Arrange
            var card = new StatusCard();

            // Act
            card.ToggleExpanded();

            // Assert
            Assert.True(card.IsExpanded);
        }

        [Fact]
        public void ToggleExpanded_FromTrue_BecomesFalse()
        {
            // Arrange
            var card = new StatusCard { IsExpanded = true };

            // Act
            card.ToggleExpanded();

            // Assert
            Assert.False(card.IsExpanded);
        }

        [Fact]
        public void ToggleExpanded_CalledTwice_ReturnsToOriginalState()
        {
            // Arrange
            var card = new StatusCard();
            var originalState = card.IsExpanded;

            // Act
            card.ToggleExpanded();
            card.ToggleExpanded();

            // Assert
            Assert.Equal(originalState, card.IsExpanded);
        }

        [Fact]
        public void StatusCategory_WhenSet_CanBeRetrieved()
        {
            // Arrange
            var card = new StatusCard { StatusCategory = "Shipped" };

            // Act
            var result = card.StatusCategory;

            // Assert
            Assert.Equal("Shipped", result);
        }

        [Fact]
        public void TaskCount_WhenSet_CanBeRetrieved()
        {
            // Arrange
            var card = new StatusCard { TaskCount = 5 };

            // Act
            var result = card.TaskCount;

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public void TaskCount_WithZero_CanBeSet()
        {
            // Arrange
            var card = new StatusCard { TaskCount = 0 };

            // Act
            var result = card.TaskCount;

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void Tasks_WhenSet_CanBeRetrieved()
        {
            // Arrange
            var tasks = new List<Task>
            {
                new Task { Name = "Task1" }
            };
            var card = new StatusCard { Tasks = tasks };

            // Act
            var result = card.Tasks;

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void Tasks_WithNullValue_DefaultsToNull()
        {
            // Arrange
            var card = new StatusCard();

            // Act
            var result = card.Tasks;

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Tasks_WithEmptyList_CanBeSet()
        {
            // Arrange
            var card = new StatusCard { Tasks = new List<Task>() };

            // Act
            var result = card.Tasks;

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void CardColor_WithValidColor_CanBeSet()
        {
            // Arrange
            var card = new StatusCard();

            // Act
            card.CardColor = "success";

            // Assert
            Assert.Equal("success", card.CardColor);
        }
    }
}