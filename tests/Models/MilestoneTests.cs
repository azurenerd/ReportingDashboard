using Xunit;
using AgentSquad.Models;
using System;

namespace AgentSquad.Tests.Models
{
    public class MilestoneTests
    {
        [Fact]
        public void Milestone_CanBeCreated_WithValidProperties()
        {
            var date = new DateTime(2026, 6, 15);
            var milestone = new Milestone
            {
                Name = "Phase 1",
                TargetDate = date,
                Status = MilestoneStatus.Completed,
                Description = "Core features"
            };

            Assert.Equal("Phase 1", milestone.Name);
            Assert.Equal(date, milestone.TargetDate);
            Assert.Equal(MilestoneStatus.Completed, milestone.Status);
            Assert.Equal("Core features", milestone.Description);
        }

        [Fact]
        public void Milestone_AllowsNullDescription()
        {
            var milestone = new Milestone
            {
                Name = "Phase 1",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Completed,
                Description = null
            };

            Assert.Null(milestone.Description);
        }

        [Fact]
        public void MilestoneStatus_HasAllExpectedValues()
        {
            Assert.Equal(0, (int)MilestoneStatus.Completed);
            Assert.Equal(1, (int)MilestoneStatus.InProgress);
            Assert.Equal(2, (int)MilestoneStatus.AtRisk);
            Assert.Equal(3, (int)MilestoneStatus.Future);
        }

        [Fact]
        public void Milestone_CanHaveEmptyDescription()
        {
            var milestone = new Milestone
            {
                Name = "Test",
                TargetDate = DateTime.Now,
                Status = MilestoneStatus.Future,
                Description = ""
            };

            Assert.Equal("", milestone.Description);
        }

        [Theory]
        [InlineData(MilestoneStatus.Completed)]
        [InlineData(MilestoneStatus.InProgress)]
        [InlineData(MilestoneStatus.AtRisk)]
        [InlineData(MilestoneStatus.Future)]
        public void Milestone_CanHaveAnyStatus(MilestoneStatus status)
        {
            var milestone = new Milestone
            {
                Name = "Test",
                TargetDate = DateTime.Now,
                Status = status
            };

            Assert.Equal(status, milestone.Status);
        }
    }
}