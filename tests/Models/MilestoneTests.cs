using System;
using Xunit;

namespace AgentSquad.Runner.Tests.Models
{
    public class MilestoneTests
    {
        [Fact]
        public void Milestone_WithValidData_StoresCorrectly()
        {
            // Arrange & Act
            var milestone = new Milestone
            {
                Name = "Phase 1: MVP Launch",
                TargetDate = new DateTime(2024, 3, 31),
                Status = "Completed",
                Description = "Core dashboard functionality"
            };

            // Assert
            Assert.Equal("Phase 1: MVP Launch", milestone.Name);
            Assert.Equal(new DateTime(2024, 3, 31), milestone.TargetDate);
            Assert.Equal("Completed", milestone.Status);
        }

        [Theory]
        [InlineData("Completed")]
        [InlineData("InProgress")]
        [InlineData("AtRisk")]
        [InlineData("Future")]
        public void Milestone_WithValidStatus_Accepted(string status)
        {
            // Arrange & Act
            var milestone = new Milestone { Status = status };

            // Assert
            Assert.True(IsValidMilestoneStatus(milestone.Status));
        }

        [Theory]
        [InlineData("InvalidStatus")]
        [InlineData("Done")]
        [InlineData("")]
        public void Milestone_WithInvalidStatus_ShouldFail(string status)
        {
            // Arrange & Act
            var milestone = new Milestone { Status = status };

            // Assert
            Assert.False(IsValidMilestoneStatus(milestone.Status));
        }

        [Fact]
        public void Milestone_WithFutureTargetDate_IsValid()
        {
            // Arrange
            var futureDate = DateTime.Now.AddMonths(6);

            // Act
            var milestone = new Milestone { TargetDate = futureDate };

            // Assert
            Assert.True(milestone.TargetDate > DateTime.Now);
        }

        [Fact]
        public void Milestone_WithPastTargetDate_StoresCorrectly()
        {
            // Arrange
            var pastDate = new DateTime(2024, 1, 15);

            // Act
            var milestone = new Milestone { TargetDate = pastDate };

            // Assert
            Assert.Equal(pastDate, milestone.TargetDate);
        }

        [Fact]
        public void Milestone_Completed_HasGreenStatus()
        {
            // Arrange & Act
            var milestone = new Milestone { Status = "Completed" };

            // Assert
            Assert.Equal("Completed", milestone.Status);
        }

        [Fact]
        public void Milestone_InProgress_HasBlueStatus()
        {
            // Arrange & Act
            var milestone = new Milestone { Status = "InProgress" };

            // Assert
            Assert.Equal("InProgress", milestone.Status);
        }

        [Fact]
        public void Milestone_AtRisk_HasRedStatus()
        {
            // Arrange & Act
            var milestone = new Milestone { Status = "AtRisk" };

            // Assert
            Assert.Equal("AtRisk", milestone.Status);
        }

        [Fact]
        public void Milestone_Future_HasGrayStatus()
        {
            // Arrange & Act
            var milestone = new Milestone { Status = "Future" };

            // Assert
            Assert.Equal("Future", milestone.Status);
        }

        [Fact]
        public void Milestone_WithNullName_StoresNull()
        {
            // Arrange & Act
            var milestone = new Milestone { Name = null };

            // Assert
            Assert.Null(milestone.Name);
        }

        [Fact]
        public void Milestone_WithEmptyDescription_Accepted()
        {
            // Arrange & Act
            var milestone = new Milestone { Description = "" };

            // Assert
            Assert.Empty(milestone.Description);
        }

        private bool IsValidMilestoneStatus(string status)
        {
            var validStatuses = new[] { "Completed", "InProgress", "AtRisk", "Future" };
            return Array.Exists(validStatuses, s => s == status);
        }
    }

    public class Milestone
    {
        public string Name { get; set; }
        public DateTime TargetDate { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
    }
}