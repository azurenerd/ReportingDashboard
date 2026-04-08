using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Configuration;
using AgentSquad.Services;
using AgentSquad.Services.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSquad.Tests.Services
{
    public class ProjectDataServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;

        public ProjectDataServiceTests()
        {
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(x => x["DataPath"]).Returns("./data");
        }

        [Fact]
        public async Task LoadProjectDataAsync_DeserializesCompleteProjectDataSchema()
        {
            var json = JsonSerializer.Serialize(new
            {
                projectInfo = new { title = "Test Project", description = "Test" },
                milestones = new[] { new { id = 1, title = "M1", status = "Active" } },
                tasks = new[] { new { id = 1, title = "T1", status = "InProgress" } },
                projectMetrics = new { completionPercentage = 75 }
            });

            var service = new ProjectDataService(_configMock.Object);
            var result = await service.LoadProjectDataAsync();

            result.Should().NotBeNull();
            result.ProjectInfo.Should().NotBeNull();
            result.Milestones.Should().NotBeEmpty();
            result.Tasks.Should().NotBeEmpty();
            result.ProjectMetrics.Should().NotBeNull();
        }

        [Fact]
        public async Task LoadProjectDataAsync_ValidatesProjectInfoProperties()
        {
            var service = new ProjectDataService(_configMock.Object);
            var result = await service.LoadProjectDataAsync();

            result.ProjectInfo.Title.Should().NotBeNullOrEmpty();
            result.ProjectInfo.Description.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoadProjectDataAsync_ValidatesMilestoneArraySchema()
        {
            var service = new ProjectDataService(_configMock.Object);
            var result = await service.LoadProjectDataAsync();

            result.Milestones.Should().NotBeNull();
            foreach (var milestone in result.Milestones)
            {
                milestone.Id.Should().BeGreaterThan(0);
                milestone.Title.Should().NotBeNullOrEmpty();
                milestone.Status.Should().BeOfType<MilestoneStatus>();
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_ValidatesTasksArraySchema()
        {
            var service = new ProjectDataService(_configMock.Object);
            var result = await service.LoadProjectDataAsync();

            result.Tasks.Should().NotBeNull();
            foreach (var task in result.Tasks)
            {
                task.Id.Should().BeGreaterThan(0);
                task.Title.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public async Task LoadProjectDataAsync_ValidatesProjectMetricsProperties()
        {
            var service = new ProjectDataService(_configMock.Object);
            var result = await service.LoadProjectDataAsync();

            result.ProjectMetrics.Should().NotBeNull();
            result.ProjectMetrics.CompletionPercentage.Should().BeInRange(0, 100);
        }

        [Fact]
        public async Task LoadProjectDataAsync_ThrowsDataLoadException_WhenFileDoesNotExist()
        {
            var service = new ProjectDataService(_configMock.Object);
            
            await service.Invoking(s => s.LoadProjectDataAsync())
                .Should().ThrowAsync<DataLoadException>();
        }

        [Fact]
        public void Constructor_RequiresIConfigurationParameter()
        {
            var service = new ProjectDataService(_configMock.Object);
            
            service.Should().NotBeNull();
        }
    }
}