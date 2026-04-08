using System;
using Xunit;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Models
{
    public class MilestoneTests
    {
        [Fact]
        public void Milestone_WithValidData_InitializesSuccessfully()
        {
            var targetDate = new DateTime(2024, 3, 15);
            var milestone = new Milestone
            {
                Name = "Version 1.0 Release",
                TargetDate = targetDate,
                Status = MilestoneStatus.InProgress,
                Description = "Major product release"
            };

            Assert.Equal("Version 1.0 Release", milestone.Name);
            Assert.Equal(targetDate, milestone.TargetDate);
            Assert.Equal(MilestoneStatus.InProgress, milestone.Status);
            Assert.Equal("Major product release", milestone.Description);
        }

        [Theory]
        [InlineData(MilestoneStatus.Completed)]
        [InlineData(MilestoneStatus.InProgress)]
        [InlineData(MilestoneStatus.AtRisk)]
        [InlineData(MilestoneStatus.Future)]
        public void Milestone_AcceptsAllStatuses(MilestoneStatus status)
        {
            var milestone = new Milestone { Status = status };
            Assert.Equal(status, milestone.Status);
        }

        [Fact]
        public void Milestone_AllowsNullDescription()
        {
            var milestone = new Milestone { Description = null };
            Assert.Null(milestone.Description);
        }

        [Fact]
        public void Milestone_WithEmptyName_IsAllowed()
        {
            var milestone = new Milestone { Name = string.Empty };
            Assert.Empty(milestone.Name);
        }
    }
}