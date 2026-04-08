using Xunit;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Models
{
    public class EnumsTests
    {
        [Fact]
        public void MilestoneStatus_HasAllRequiredValues()
        {
            Assert.True(System.Enum.IsDefined(typeof(MilestoneStatus), MilestoneStatus.Completed));
            Assert.True(System.Enum.IsDefined(typeof(MilestoneStatus), MilestoneStatus.InProgress));
            Assert.True(System.Enum.IsDefined(typeof(MilestoneStatus), MilestoneStatus.AtRisk));
            Assert.True(System.Enum.IsDefined(typeof(MilestoneStatus), MilestoneStatus.Future));
        }

        [Fact]
        public void WorkItemStatus_HasAllRequiredValues()
        {
            Assert.True(System.Enum.IsDefined(typeof(WorkItemStatus), WorkItemStatus.Shipped));
            Assert.True(System.Enum.IsDefined(typeof(WorkItemStatus), WorkItemStatus.InProgress));
            Assert.True(System.Enum.IsDefined(typeof(WorkItemStatus), WorkItemStatus.CarriedOver));
        }

        [Fact]
        public void HealthStatus_HasAllRequiredValues()
        {
            Assert.True(System.Enum.IsDefined(typeof(HealthStatus), HealthStatus.OnTrack));
            Assert.True(System.Enum.IsDefined(typeof(HealthStatus), HealthStatus.AtRisk));
            Assert.True(System.Enum.IsDefined(typeof(HealthStatus), HealthStatus.Blocked));
        }
    }
}