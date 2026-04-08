using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Data;
using AgentSquad.Runner.Services;
using Bunit;
using Xunit;

namespace AgentSquad.Runner.Tests.Components
{
    public class DashboardTests : TestContext
    {
        private readonly MockProjectDataService _mockDataService;

        public DashboardTests()
        {
            _mockDataService = new MockProjectDataService();
            Services.AddScoped<ProjectDataService>(_ => _mockDataService);
        }

        [Fact]
        public void Dashboard_OnInitialized_DisplaysProjectTitle()
        {
            var projectData = new ProjectData
            {
                ProjectName = "Q2 Mobile App Release",
                ProjectStartDate = new DateTime(2024, 1, 1),
                ProjectEndDate = new DateTime(2024, 6, 30),
                Milestones = new List<Milestone>(),
                Tasks = new List<ProjectTask>()
            };

            _mockDataService.SetProjectData(projectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("Q2 Mobile App Release", component.Markup);
        }

        [Fact]
        public void Dashboard_OnInitialized_RendersMilestoneTimeline()
        {
            var projectData = new ProjectData
            {
                ProjectName = "Test Project",
                ProjectStartDate = new DateTime(2024, 1, 1),
                ProjectEndDate = new DateTime(2024, 12, 31),
                Milestones = new List<Milestone>
                {
                    new Milestone { Name = "Phase 1", TargetDate = new DateTime(2024, 3, 1), Status = "Completed", CompletionPercentage = 100 }
                },
                Tasks = new List<ProjectTask>()
            };

            _mockDataService.SetProjectData(projectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("Phase 1", component.Markup);
        }

        [Fact]
        public void Dashboard_OnInitialized_RendersStatusCards()
        {
            var projectData = new ProjectData
            {
                ProjectName = "Test Project",
                ProjectStartDate = new DateTime(2024, 1, 1),
                ProjectEndDate = new DateTime(2024, 12, 31),
                Milestones = new List<Milestone>(),
                Tasks = new List<ProjectTask>
                {
                    new ProjectTask { Name = "Task 1", Status = "Shipped", Owner = "Alice" },
                    new ProjectTask { Name = "Task 2", Status = "InProgress", Owner = "Bob" },
                    new ProjectTask { Name = "Task 3", Status = "CarriedOver", Owner = "Charlie" }
                }
            };

            _mockDataService.SetProjectData(projectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("Shipped", component.Markup);
            Assert.Contains("In Progress", component.Markup);
            Assert.Contains("Carried Over", component.Markup);
        }

        [Fact]
        public void Dashboard_WithLoadError_DisplaysErrorMessage()
        {
            _mockDataService.SetError("Failed to load project data");

            var component = RenderComponent<Dashboard>();

            Assert.Contains("error", component.Markup, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Dashboard_RendersProgressMetrics()
        {
            var projectData = new ProjectData
            {
                ProjectName = "Test Project",
                ProjectStartDate = new DateTime(2024, 1, 1),
                ProjectEndDate = new DateTime(2024, 12, 31),
                Milestones = new List<Milestone>(),
                Tasks = new List<ProjectTask>
                {
                    new ProjectTask { Name = "Task 1", Status = "Shipped", Owner = "Alice" },
                    new ProjectTask { Name = "Task 2", Status = "InProgress", Owner = "Bob" }
                }
            };

            _mockDataService.SetProjectData(projectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("Progress", component.Markup, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Dashboard_LayoutIsResponsiveBootstrap()
        {
            var projectData = new ProjectData
            {
                ProjectName = "Test Project",
                ProjectStartDate = new DateTime(2024, 1, 1),
                ProjectEndDate = new DateTime(2024, 12, 31),
                Milestones = new List<Milestone>(),
                Tasks = new List<ProjectTask>()
            };

            _mockDataService.SetProjectData(projectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("container", component.Markup);
            Assert.Contains("row", component.Markup);
        }

        [Fact]
        public void Dashboard_DisplaysAllMilestones()
        {
            var projectData = new ProjectData
            {
                ProjectName = "Test Project",
                ProjectStartDate = new DateTime(2024, 1, 1),
                ProjectEndDate = new DateTime(2024, 12, 31),
                Milestones = new List<Milestone>
                {
                    new Milestone { Name = "Milestone 1", TargetDate = new DateTime(2024, 3, 1), Status = "Completed" },
                    new Milestone { Name = "Milestone 2", TargetDate = new DateTime(2024, 6, 1), Status = "InProgress" },
                    new Milestone { Name = "Milestone 3", TargetDate = new DateTime(2024, 9, 1), Status = "Pending" }
                },
                Tasks = new List<ProjectTask>()
            };

            _mockDataService.SetProjectData(projectData);

            var component = RenderComponent<Dashboard>();

            Assert.Contains("Milestone 1", component.Markup);
            Assert.Contains("Milestone 2", component.Markup);
            Assert.Contains("Milestone 3", component.Markup);
        }
    }

    public class MockProjectDataService : ProjectDataService
    {
        private ProjectData _projectData;
        private string _error;

        public void SetProjectData(ProjectData data)
        {
            _projectData = data;
            _error = null;
        }

        public void SetError(string error)
        {
            _error = error;
            _projectData = null;
        }

        public override async Task<ProjectData> LoadProjectDataAsync(string filePath)
        {
            if (!string.IsNullOrEmpty(_error))
                throw new InvalidOperationException(_error);
            return _projectData;
        }
    }
}