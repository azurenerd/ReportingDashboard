using Xunit;
using AgentSquad.Models;
using System;
using System.Collections.Generic;

namespace AgentSquad.Tests.Models
{
    public class ProjectTests
    {
        [Fact]
        public void Project_ValidInitialization_Success()
        {
            var startDate = DateTime.UtcNow;
            var project = new Project
            {
                Id = "p1",
                Name = "Mobile App",
                StartDate = startDate,
                EndDate = startDate.AddMonths(6),
                Milestones = new List<Milestone>(),
                WorkItems = new List<WorkItem>()
            };

            Assert.Equal("p1", project.Id);
            Assert.Equal("Mobile App", project.Name);
            Assert.Equal(startDate, project.StartDate);
        }

        [Theory]
        [InlineData("2024-13-01")]
        [InlineData("2024-12-32")]
        public void Project_InvalidDateFormat_ThrowsFormatException(string invalidDate)
        {
            Assert.Throws<FormatException>(() => DateTime.Parse(invalidDate));
        }

        [Fact]
        public void Project_ValidIso8601Dates_ParsesCorrectly()
        {
            var iso8601Start = "2024-01-01T00:00:00Z";
            var iso8601End = "2024-12-31T23:59:59Z";
            var startDate = DateTime.Parse(iso8601Start);
            var endDate = DateTime.Parse(iso8601End);

            Assert.True(endDate > startDate);
        }

        [Fact]
        public void Project_EmptyMilestonesList_Success()
        {
            var project = new Project
            {
                Id = "p1",
                Name = "Project",
                Milestones = new List<Milestone>()
            };

            Assert.Empty(project.Milestones);
        }
    }
}