using Xunit;
using AgentSquad.Models;

namespace AgentSquad.Tests.Models
{
    public class EnumsTests
    {
        [Theory]
        [InlineData(nameof(MilestoneStatus.NotStarted))]
        [InlineData(nameof(MilestoneStatus.InProgress))]
        [InlineData(nameof(MilestoneStatus.Completed))]
        public void MilestoneStatus_ValidEnumValues_ParseCorrectly(string value)
        {
            var result = System.Enum.Parse<MilestoneStatus>(value);
            Assert.NotEqual(default, result);
        }

        [Theory]
        [InlineData("InvalidStatus")]
        [InlineData("")]
        [InlineData("null")]
        public void MilestoneStatus_InvalidEnumValues_ThrowsArgumentException(string value)
        {
            Assert.Throws<ArgumentException>(() => System.Enum.Parse<MilestoneStatus>(value));
        }

        [Theory]
        [InlineData(nameof(WorkItemStatus.Todo))]
        [InlineData(nameof(WorkItemStatus.InProgress))]
        [InlineData(nameof(WorkItemStatus.Done))]
        public void WorkItemStatus_ValidEnumValues_ParseCorrectly(string value)
        {
            var result = System.Enum.Parse<WorkItemStatus>(value);
            Assert.NotEqual(default, result);
        }

        [Theory]
        [InlineData("InvalidStatus")]
        [InlineData("DONE")]
        public void WorkItemStatus_InvalidEnumValues_ThrowsArgumentException(string value)
        {
            Assert.Throws<ArgumentException>(() => System.Enum.Parse<WorkItemStatus>(value));
        }

        [Theory]
        [InlineData(nameof(HealthStatus.Healthy))]
        [InlineData(nameof(HealthStatus.AtRisk))]
        [InlineData(nameof(HealthStatus.Critical))]
        public void HealthStatus_ValidEnumValues_ParseCorrectly(string value)
        {
            var result = System.Enum.Parse<HealthStatus>(value);
            Assert.NotEqual(default, result);
        }

        [Theory]
        [InlineData("InvalidHealth")]
        [InlineData("warning")]
        public void HealthStatus_InvalidEnumValues_ThrowsArgumentException(string value)
        {
            Assert.Throws<ArgumentException>(() => System.Enum.Parse<HealthStatus>(value));
        }
    }
}