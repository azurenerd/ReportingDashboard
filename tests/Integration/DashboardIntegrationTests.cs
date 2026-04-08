using Xunit;
using Moq;
using AgentSquad.Services;
using AgentSquad.Data;
using AgentSquad.Components;

namespace AgentSquad.Tests.Integration
{
    public class DashboardIntegrationTests
    {
        [Fact]
        public async Task Dashboard_Integration_LoadsAndDisplaysProjectData()
        {
            var mockProjectData = new ProjectInfo
            {
                Name = "Integration Test Project",
                Status = "Active",
                Milestones = new List<Milestone>()
            };

            var mockDataService = new Mock<IProjectDataService>(MockBehavior.Strict);
            mockDataService
                .Setup(s => s.LoadProjectDataAsync())
                .ReturnsAsync(mockProjectData);
            mockDataService
                .Setup(s => s.GetCachedData())
                .Returns(mockProjectData);

            var dashboard = new Dashboard { DataService = mockDataService.Object };
            await dashboard.OnInitializedAsync();

            var displayedProject = dashboard.CurrentProject;
            Assert.NotNull(displayedProject);
            Assert.Equal("Integration Test Project", displayedProject.Name);
            
            mockDataService.Verify(s => s.LoadProjectDataAsync(), Times.Once);
        }
    }
}