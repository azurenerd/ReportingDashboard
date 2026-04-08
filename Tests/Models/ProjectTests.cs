using System;
using System.Collections.Generic;
using Xunit;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Models
{
    public class ProjectTests
    {
        [Fact]
        public void Project_WithValidData_InitializesSuccessfully()
        {
            var startDate = new DateTime(2024, 1, 1);
            var project = new Project
            {
                Name = "Dashboard Project",
                Description = "Executive dashboard",
                StartDate = startDate,
                Milestones = new List<Milestone>(),
                WorkItems = new List<WorkItem>(),
                Metrics = new ProjectMetrics()
            };

            Assert.Equal("Dashboard Project", project.Name);
            Assert.Equal("Executive dashboard", project.Description);
            Assert.Equal(startDate, project.StartDate);
            Assert.NotNull(project.Milestones);
            Assert.NotNull(project.WorkItems);
            Assert.NotNull(project.Metrics);
        }

        [Fact]
        public void Project_DefaultsToEmptyCollections()
        {
            var project = new Project();

            Assert.NotNull(project.Milestones);
            Assert.Empty(project.Milestones);
            Assert.NotNull(project.WorkItems);
            Assert.Empty(project.WorkItems);
        }

        [Fact]
        public void Project_CanContainMultipleMilestones()
        {
            var project = new Project
            {
                Milestones = new List<Milestone>
                {
                    new() { Name = "M1" },
                    new() { Name = "M2" },
                    new() { Name = "M3" }
                }
            };

            Assert.Equal(3, project.Milestones.Count);
        }

        [Fact]
        public void Project_CanContainMultipleWorkItems()
        {
            var project = new Project
            {
                WorkItems = new List<WorkItem>
                {
                    new() { Title = "WI1" },
                    new() { Title = "WI2" },
                    new() { Title = "WI3" }
                }
            };

            Assert.Equal(3, project.WorkItems.Count);
        }
    }
}