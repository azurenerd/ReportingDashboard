using Xunit;
using AgentSquad.Models;
using AgentSquad.Services;
using System.Text.Json;
using Moq;

namespace AgentSquad.Tests.Services
{
    public class ProjectDataServiceTests
    {
        private readonly ProjectDataService _projectDataService;

        public ProjectDataServiceTests()
        {
            _projectDataService = new ProjectDataService();
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidJson_ReturnsProjectData()
        {
            var projectData = await _projectDataService.LoadProjectDataAsync();
            
            Assert.NotNull(projectData);
            Assert.NotNull(projectData.Project);
            Assert.NotEmpty(projectData.Milestones);
            Assert.NotEmpty(projectData.Tasks);
        }

        [Fact]
        public async Task LoadProjectDataAsync_PopulatesAllRequiredFields()
        {
            var projectData = await _projectDataService.LoadProjectDataAsync();
            
            Assert.NotNull(projectData.Project.Name);
            Assert.NotNull(projectData.Project.Description);
            Assert.NotEqual(default(DateTime), projectData.Project.StartDate);
            Assert.NotEqual(default(DateTime), projectData.Project.EndDate);
        }

        [Fact]
        public async Task LoadProjectDataAsync_MilestonesContainRequiredFields()
        {
            var projectData = await _projectDataService.LoadProjectDataAsync();
            var milestone = projectData.Milestones.FirstOrDefault();
            
            Assert.NotNull(milestone);
            Assert.NotNull(milestone.Name);
            Assert.NotEqual(default(DateTime), milestone.TargetDate);
            Assert.InRange(milestone.CompletionPercentage, 0, 100);
        }

        [Fact]
        public async Task LoadProjectDataAsync_TasksContainRequiredFields()
        {
            var projectData = await _projectDataService.LoadProjectDataAsync();
            var task = projectData.Tasks.FirstOrDefault();
            
            Assert.NotNull(task);
            Assert.NotNull(task.Id);
            Assert.NotNull(task.Name);
            Assert.NotEqual(default(DateTime), task.DueDate);
        }

        [Fact]
        public async Task LoadProjectDataAsync_MetricsCalculatedCorrectly()
        {
            var projectData = await _projectDataService.LoadProjectDataAsync();
            
            Assert.NotNull(projectData.Metrics);
            Assert.InRange(projectData.Metrics.CompletionPercentage, 0, 100);
            Assert.InRange(projectData.Metrics.ShippedCount, 0, int.MaxValue);
            Assert.InRange(projectData.Metrics.InProgressCount, 0, int.MaxValue);
            Assert.InRange(projectData.Metrics.CarriedOverCount, 0, int.MaxValue);
        }

        [Fact]
        public async Task LoadProjectDataAsync_TaskCountsMatchMetrics()
        {
            var projectData = await _projectDataService.LoadProjectDataAsync();
            
            var shippedCount = projectData.Tasks.Count(t => t.Status == TaskStatus.Shipped);
            var inProgressCount = projectData.Tasks.Count(t => t.Status == TaskStatus.InProgress);
            var carriedOverCount = projectData.Tasks.Count(t => t.Status == TaskStatus.CarriedOver);
            
            Assert.Equal(shippedCount, projectData.Metrics.ShippedCount);
            Assert.Equal(inProgressCount, projectData.Metrics.InProgressCount);
            Assert.Equal(carriedOverCount, projectData.Metrics.CarriedOverCount);
        }

        [Fact]
        public async Task LoadProjectDataAsync_MilestoneStatusesAreValid()
        {
            var projectData = await _projectDataService.LoadProjectDataAsync();
            
            foreach (var milestone in projectData.Milestones)
            {
                Assert.True(Enum.IsDefined(typeof(MilestoneStatus), milestone.Status));
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_AllMilestonesHaveFutureDates()
        {
            var projectData = await _projectDataService.LoadProjectDataAsync();
            var projectEnd = projectData.Project.EndDate;
            
            foreach (var milestone in projectData.Milestones)
            {
                Assert.True(milestone.TargetDate <= projectEnd,
                    $"Milestone {milestone.Name} target date is after project end date");
            }
        }
    }
}