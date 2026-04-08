using Xunit;
using AgentSquad.Models;
using System;

namespace AgentSquad.Tests.Models
{
    public class MilestoneTests
    {
        [Fact]
        public void Milestone_ValidInitialization_Success()
        {
            var dueDate = DateTime.UtcNow.AddDays(30);
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "Release v1.0",
                Status = MilestoneStatus.InProgress,
                DueDate = dueDate,
                Description = "Initial release"
            };

            Assert.Equal("m1", milestone.Id);
            Assert.Equal("Release v1.0", milestone.Name);
            Assert.Equal(MilestoneStatus.InProgress, milestone.Status);
            Assert.Equal(dueDate, milestone.DueDate);
            Assert.Equal("Initial release", milestone.Description);
        }

        [Fact]
        public void Milestone_NullDescription_Allowed()
        {
            var milestone = new Milestone
            {
                Id = "m1",
                Name = "Release v1.0",
                Status = MilestoneStatus.NotStarted,
                DueDate = DateTime.UtcNow.AddDays(30),
                Description = null
            };

            Assert.Null(milestone.Description);
        }

        [Theory]
        [InlineData("2024-13-40")]
        [InlineData("2024-12-32")]
        [InlineData("invalid-date")]
        [InlineData("2024/12/25")]
        public void Milestone_InvalidDateFormat_ThrowsFormatException(string invalidDate)
        {
            Assert.Throws<FormatException>(() => DateTime.Parse(invalidDate));
        }

        [Fact]
        public void Milestone_ValidIso8601Date_ParsesCorrectly()
        {
            var iso8601 = "2024-12-25T10:30:00Z";
            var date = DateTime.Parse(iso8601);
            Assert.Equal(2024, date.Year);
            Assert.Equal(12, date.Month);
            Assert.Equal(25, date.Day);
        }
    }
}