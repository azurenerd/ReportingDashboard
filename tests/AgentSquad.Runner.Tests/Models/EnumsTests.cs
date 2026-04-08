using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Tests.Models
{
    public class EnumsTests
    {
        [Fact]
        public void MilestoneStatus_HasExpectedOrdinalValues()
        {
            Assert.Equal(0, (int)MilestoneStatus.NotStarted);
            Assert.Equal(1, (int)MilestoneStatus.InProgress);
            Assert.Equal(2, (int)MilestoneStatus.Completed);
        }

        [Fact]
        public void WorkItemStatus_HasExpectedOrdinalValues()
        {
            Assert.Equal(0, (int)WorkItemStatus.Pending);
            Assert.Equal(1, (int)WorkItemStatus.InProgress);
            Assert.Equal(2, (int)WorkItemStatus.Done);
        }

        [Fact]
        public void HealthStatus_HasExpectedOrdinalValues()
        {
            Assert.Equal(0, (int)HealthStatus.Healthy);
            Assert.Equal(1, (int)HealthStatus.AtRisk);
            Assert.Equal(2, (int)HealthStatus.Critical);
        }

        [Theory]
        [InlineData(nameof(MilestoneStatus.NotStarted))]
        [InlineData(nameof(MilestoneStatus.InProgress))]
        [InlineData(nameof(MilestoneStatus.Completed))]
        public void MilestoneStatus_AllValuesValid(string status)
        {
            Assert.True(Enum.IsDefined(typeof(MilestoneStatus), status));
        }

        [Theory]
        [InlineData(nameof(WorkItemStatus.Pending))]
        [InlineData(nameof(WorkItemStatus.InProgress))]
        [InlineData(nameof(WorkItemStatus.Done))]
        public void WorkItemStatus_AllValuesValid(string status)
        {
            Assert.True(Enum.IsDefined(typeof(WorkItemStatus), status));
        }
    }
}