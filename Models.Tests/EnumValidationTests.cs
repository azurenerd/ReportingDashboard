using System;
using System.Linq;
using Xunit;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Models.Tests
{
    public class EnumValidationTests
    {
        [Fact]
        public void MilestoneStatusEnum_AllRequiredValuesPresent()
        {
            // Arrange & Act
            var values = Enum.GetValues(typeof(MilestoneStatus)).Cast<MilestoneStatus>().ToList();

            // Assert
            Assert.Contains(MilestoneStatus.Completed, values);
            Assert.Contains(MilestoneStatus.InProgress, values);
            Assert.Contains(MilestoneStatus.AtRisk, values);
            Assert.Contains(MilestoneStatus.Future, values);
            Assert.Equal(4, values.Count);
        }

        [Fact]
        public void WorkItemStatusEnum_AllRequiredValuesPresent()
        {
            // Arrange & Act
            var values = Enum.GetValues(typeof(WorkItemStatus)).Cast<WorkItemStatus>().ToList();

            // Assert
            Assert.Contains(WorkItemStatus.Shipped, values);
            Assert.Contains(WorkItemStatus.InProgress, values);
            Assert.Contains(WorkItemStatus.CarriedOver, values);
            Assert.Equal(3, values.Count);
        }

        [Fact]
        public void HealthStatusEnum_AllRequiredValuesPresent()
        {
            // Arrange & Act
            var values = Enum.GetValues(typeof(HealthStatus)).Cast<HealthStatus>().ToList();

            // Assert
            Assert.Contains(HealthStatus.OnTrack, values);
            Assert.Contains(HealthStatus.AtRisk, values);
            Assert.Contains(HealthStatus.Blocked, values);
            Assert.Equal(3, values.Count);
        }

        [Fact]
        public void MilestoneStatusEnum_CanParseFromString()
        {
            // Act & Assert
            Assert.True(Enum.TryParse<MilestoneStatus>("Completed", out var completed));
            Assert.Equal(MilestoneStatus.Completed, completed);

            Assert.True(Enum.TryParse<MilestoneStatus>("InProgress", out var inProgress));
            Assert.Equal(MilestoneStatus.InProgress, inProgress);
        }

        [Fact]
        public void WorkItemStatusEnum_CanParseFromString()
        {
            // Act & Assert
            Assert.True(Enum.TryParse<WorkItemStatus>("Shipped", out var shipped));
            Assert.Equal(WorkItemStatus.Shipped, shipped);

            Assert.True(Enum.TryParse<WorkItemStatus>("CarriedOver", out var carriedOver));
            Assert.Equal(WorkItemStatus.CarriedOver, carriedOver);
        }

        [Fact]
        public void HealthStatusEnum_CanParseFromString()
        {
            // Act & Assert
            Assert.True(Enum.TryParse<HealthStatus>("OnTrack", out var onTrack));
            Assert.Equal(HealthStatus.OnTrack, onTrack);

            Assert.True(Enum.TryParse<HealthStatus>("Blocked", out var blocked));
            Assert.Equal(HealthStatus.Blocked, blocked);
        }
    }
}