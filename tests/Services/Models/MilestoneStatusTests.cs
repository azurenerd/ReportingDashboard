using Xunit;
using FluentAssertions;
using AgentSquad.Services.Models;

namespace AgentSquad.Tests.Services.Models
{
    public class MilestoneStatusTests
    {
        [Fact]
        public void MilestoneStatus_HasActiveValue()
        {
            MilestoneStatus.Active.Should().Be(MilestoneStatus.Active);
        }

        [Fact]
        public void MilestoneStatus_HasCompletedValue()
        {
            MilestoneStatus.Completed.Should().Be(MilestoneStatus.Completed);
        }

        [Fact]
        public void MilestoneStatus_HasBlockedValue()
        {
            MilestoneStatus.Blocked.Should().Be(MilestoneStatus.Blocked);
        }
    }
}