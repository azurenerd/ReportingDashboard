using Xunit;
using AgentSquad.Dashboard.Services;
using System;

namespace AgentSquad.Dashboard.Tests.Unit.Enums
{
    [Trait("Category", "Unit")]
    public class TaskStatusEnumTests
    {
        [Fact]
        public void TaskStatus_Shipped_Exists()
        {
            // Act & Assert
            Assert.True(Enum.IsDefined(typeof(TaskStatus), TaskStatus.Shipped));
        }

        [Fact]
        public void TaskStatus_InProgress_Exists()
        {
            // Act & Assert
            Assert.True(Enum.IsDefined(typeof(TaskStatus), TaskStatus.InProgress));
        }

        [Fact]
        public void TaskStatus_CarriedOver_Exists()
        {
            // Act & Assert
            Assert.True(Enum.IsDefined(typeof(TaskStatus), TaskStatus.CarriedOver));
        }

        [Fact]
        public void TaskStatus_HasThreeValues()
        {
            // Act
            var values = Enum.GetValues(typeof(TaskStatus));

            // Assert
            Assert.Equal(3, values.Length);
        }
    }

    [Trait("Category", "Unit")]
    public class MilestoneStatusEnumTests
    {
        [Fact]
        public void MilestoneStatus_Completed_Exists()
        {
            // Act & Assert
            Assert.True(Enum.IsDefined(typeof(MilestoneStatus), MilestoneStatus.Completed));
        }

        [Fact]
        public void MilestoneStatus_InProgress_Exists()
        {
            // Act & Assert
            Assert.True(Enum.IsDefined(typeof(MilestoneStatus), MilestoneStatus.InProgress));
        }

        [Fact]
        public void MilestoneStatus_Pending_Exists()
        {
            // Act & Assert
            Assert.True(Enum.IsDefined(typeof(MilestoneStatus), MilestoneStatus.Pending));
        }

        [Fact]
        public void MilestoneStatus_HasThreeValues()
        {
            // Act
            var values = Enum.GetValues(typeof(MilestoneStatus));

            // Assert
            Assert.Equal(3, values.Length);
        }
    }
}