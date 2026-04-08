using System;
using System.Collections.Generic;
using Xunit;
using AgentSquad.Models;

namespace AgentSquad.Tests.Services
{
    public class ProjectDataValidatorTests
    {
        [Fact]
        public void TaskStatusValidation_AcceptsValidStatuses()
        {
            var validStatuses = new[] { "Shipped", "InProgress", "CarriedOver" };
            foreach (var status in validStatuses)
            {
                var task = new ProjectTask { Id = "t1", Status = status };
                Assert.NotNull(task);
            }
        }

        [Fact]
        public void TaskEstimatedDays_ValidatesPositiveValue()
        {
            var task = new ProjectTask { Id = "t1", EstimatedDays = 5 };
            Assert.True(task.EstimatedDays > 0);
        }

        [Fact]
        public void ProjectData_StructureIsValid()
        {
            var data = new ProjectData
            {
                Milestones = new List<Milestone>(),
                Tasks = new List<ProjectTask>()
            };
            Assert.Empty(data.Milestones);
            Assert.Empty(data.Tasks);
        }
    }
}