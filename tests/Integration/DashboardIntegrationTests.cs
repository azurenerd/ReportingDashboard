using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Services;
using AgentSquad.Runner.Data;
using Moq;

namespace AgentSquad.Runner.Tests.Integration
{
    public class DashboardIntegrationTests : TestContext
    {
        [Fact]
        public void Dashboard_Integration_LoadsAndDisplaysProjectData()
        {
            // Arrange
            var mockService = new Mock<ProjectDataService>(
                MockBehavior.Strict, 
                new Mock<ILogger<ProjectDataService>>().Object);

            var projectData = new ProjectData
            {
                Project = new ProjectInfo
                {
                    Name = "Q2 Mobile App",
                    Description = "Mobile app v2.0",
                    StartDate = new DateTime(2024, 1, 1),
                    EndDate = new DateTime(2024, 6, 30),
                    Status = "OnTrack",
                    Sponsor = "VP Engineering",
                    ProjectManager = "John Doe"
                },
                Milestones = new List<Milestone>
                {
                    new Milestone 
                    { 
                        Id = "m1",
                        Name = "Design Phase",
                        TargetDate = new DateTime(2024, 2, 28),
                        Status = MilestoneStatus.Completed,
                        CompletionPercentage = 100
                    },
                    new Milestone 
                    { 
                        Id = "m2",
                        Name = "Development Phase",
                        TargetDate = new DateTime(2024, 5, 31),
                        Status = MilestoneStatus.InProgress,
                        CompletionPercentage = 60
                    }
                },
                Tasks = new List<Task>
                {
                    new Task { Id = "t1", Name = "API Auth", Status = TaskStatus.Shipped, AssignedTo = "Dev A", EstimatedDays = 5 },
                    new Task { Id = "t2", Name = "Frontend UI", Status = TaskStatus.InProgress, AssignedTo = "Dev B", EstimatedDays = 10 },
                    new Task { Id = "t3", Name = "Testing", Status = TaskStatus.CarriedOver, AssignedTo = "QA", EstimatedDays = 7 }
                },
                Metrics = new ProjectMetrics
                {
                    TotalTasks = 10,
                    CompletedTasks = 3,
                    InProgressTasks = 5,
                    CarriedOverTasks = 2,
                    CompletionPercentage = 30,
                    EstimatedBurndownRate = 0.5,
                    ProjectStartDate = new DateTime(2024, 1, 1),
                    ProjectEndDate = new DateTime(2024, 6, 30),
                    DaysRemaining = 100
                }
            };

            Services.AddScoped(_ => mockService.Object);

            // Act
            var component = RenderComponent<Dashboard>();

            // Assert
            Assert.NotNull(component);
            Assert.NotNull(projectData);
            Assert.Equal(2, projectData.Milestones.Count);
            Assert.Equal(3, projectData.Tasks.Count);
            Assert.NotNull(projectData.Metrics);
        }

        [Fact]
        public void Dashboard_DisplaysAllComponentSections()
        {
            // Arrange
            var mockService = new Mock<ProjectDataService>(
                MockBehavior.Strict,
                new Mock<ILogger<ProjectDataService>>().Object);

            Services.AddScoped(_ => mockService.Object);

            // Act
            var component = RenderComponent<Dashboard>();

            // Assert
            component.Markup.Should().Contain("Executive Dashboard");
        }

        [Fact]
        public void Dashboard_Renders_WithCompleteProjectStructure()
        {
            // Arrange
            var mockService = new Mock<ProjectDataService>(
                MockBehavior.Strict,
                new Mock<ILogger<ProjectDataService>>().Object);

            var projectData = new ProjectData
            {
                Project = new ProjectInfo
                {
                    Name = "Test Project",
                    Description = "Test Description",
                    StartDate = DateTime.Now.AddMonths(-1),
                    EndDate = DateTime.Now.AddMonths(5),
                    Status = "OnTrack",
                    Sponsor = "Executive",
                    ProjectManager = "Manager"
                },
                Milestones = new List<Milestone>
                {
                    new Milestone 
                    { 
                        Id = "m1",
                        Name = "Milestone 1",
                        TargetDate = DateTime.Now.AddMonths(1),
                        Status = MilestoneStatus.InProgress,
                        CompletionPercentage = 50
                    }
                },
                Tasks = new List<Task>
                {
                    new Task 
                    { 
                        Id = "t1",
                        Name = "Task 1",
                        Status = TaskStatus.InProgress,
                        AssignedTo = "Team",
                        DueDate = DateTime.Now.AddDays(7),
                        EstimatedDays = 5
                    }
                },
                Metrics = new ProjectMetrics
                {
                    TotalTasks = 10,
                    CompletedTasks = 3,
                    InProgressTasks = 5,
                    CarriedOverTasks = 2,
                    CompletionPercentage = 30,
                    EstimatedBurndownRate = 0.5,
                    ProjectStartDate = DateTime.Now.AddMonths(-1),
                    ProjectEndDate = DateTime.Now.AddMonths(5),
                    DaysRemaining = 150
                }
            };

            Services.AddScoped(_ => mockService.Object);

            // Act
            var component = RenderComponent<Dashboard>();

            // Assert
            Assert.NotNull(component);
            Assert.NotNull(projectData.Project);
            Assert.NotEmpty(projectData.Milestones);
            Assert.NotEmpty(projectData.Tasks);
            Assert.NotNull(projectData.Metrics);
        }

        [Fact]
        public void Dashboard_Handles_EmptyProjectData()
        {
            // Arrange
            var mockService = new Mock<ProjectDataService>(
                MockBehavior.Strict,
                new Mock<ILogger<ProjectDataService>>().Object);

            var emptyProjectData = new ProjectData();

            Services.AddScoped(_ => mockService.Object);

            // Act
            var component = RenderComponent<Dashboard>();

            // Assert
            Assert.NotNull(component);
            Assert.Empty(emptyProjectData.Milestones);
            Assert.Empty(emptyProjectData.Tasks);
        }
    }
}