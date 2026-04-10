using Bunit;
using Xunit;
using Moq;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AgentSquad.Runner.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class ComponentServiceIntegrationTests : TestContext
    {
        private readonly Mock<IProjectDataService> _mockDataService;

        public ComponentServiceIntegrationTests()
        {
            _mockDataService = new Mock<IProjectDataService>();
            Services.AddScoped(_ => _mockDataService.Object);
        }

        [Fact]
        public void DashboardPage_ResolvesService_FromDIContainer()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "DI Test",
                Milestones = new(),
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new()
            };

            _mockDataService.Setup(s => s.IsInitialized).Returns(true);
            _mockDataService.Setup(s => s.GetDashboard()).Returns(dashboard);
            _mockDataService.Setup(s => s.LastError).Returns((string?)null);

            var cut = RenderComponent<DashboardPage>();

            _mockDataService.Verify(s => s.IsInitialized, Times.AtLeastOnce);
            Assert.NotNull(cut);
        }

        [Fact]
        public void TimelinePanel_ReceivesCascadingData_FromParent()
        {
            var dashboard = new ProjectDashboard
            {
                Milestones = new()
                {
                    new() { Id = "m1", Name = "Phase 1", Status = "Completed", TargetDate = DateTime.Now }
                }
            };

            var cut = RenderComponent<TimelinePanel>(
                builder => builder.CascadingValue(dashboard)
            );

            var markup = cut.Markup;
            Assert.Contains("Phase 1", markup);
        }

        [Fact]
        public void StatusColumn_ReceivesParameters_AndRendersCorrectly()
        {
            var items = new List<WorkItem>
            {
                new() { Id = "w1", Title = "Item 1", CompletedDate = DateTime.Now },
                new() { Id = "w2", Title = "Item 2", CompletedDate = DateTime.Now }
            };

            var cut = RenderComponent<StatusColumn>(parameters =>
                parameters
                    .Add(p => p.Title, "Test Column")
                    .Add(p => p.Items, items)
                    .Add(p => p.BadgeClass, "bg-success")
            );

            var markup = cut.Markup;
            Assert.Contains(">2<", markup);
            Assert.Contains("Test Column", markup);
        }

        [Fact]
        public void DashboardPage_DisplaysErrorMessage_WhenServiceHasError()
        {
            _mockDataService.Setup(s => s.IsInitialized).Returns(false);
            _mockDataService.Setup(s => s.LastError).Returns("Critical error: Data file corrupted");

            var cut = RenderComponent<DashboardPage>();

            cut.WaitForAssertion(() =>
            {
                var markup = cut.Markup;
                Assert.Contains("Critical error", markup);
            });
        }

        [Fact]
        public void DashboardPage_CascadesDashboard_ToAllChildren()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Cascade Test",
                Milestones = new()
                {
                    new() { Id = "m1", Name = "M1", Status = "Completed", TargetDate = DateTime.Now }
                },
                Shipped = new()
                {
                    new() { Id = "w1", Title = "Shipped Item" }
                },
                InProgress = new()
                {
                    new() { Id = "w2", Title = "InProgress Item" }
                },
                CarriedOver = new()
                {
                    new() { Id = "w3", Title = "CarriedOver Item" }
                }
            };

            _mockDataService.Setup(s => s.IsInitialized).Returns(true);
            _mockDataService.Setup(s => s.GetDashboard()).Returns(dashboard);
            _mockDataService.Setup(s => s.LastError).Returns((string?)null);

            var cut = RenderComponent<DashboardPage>();

            cut.WaitForAssertion(() =>
            {
                var markup = cut.Markup;
                Assert.Contains("Cascade Test", markup);
                Assert.Contains("M1", markup);
                Assert.Contains("Shipped Item", markup);
                Assert.Contains("InProgress Item", markup);
                Assert.Contains("CarriedOver Item", markup);
            });
        }

        [Fact]
        public void Service_Integration_WithCompleteDataSet()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Integration Test",
                Description = "Full integration test",
                StartDate = new DateTime(2026, 1, 15),
                PlannedCompletion = new DateTime(2026, 6, 30),
                Milestones = new()
                {
                    new() { Id = "m1", Name = "Phase 1", Status = "Completed", TargetDate = new DateTime(2026, 2, 28) },
                    new() { Id = "m2", Name = "Phase 2", Status = "OnTrack", TargetDate = new DateTime(2026, 4, 15) }
                },
                Shipped = new()
                {
                    new() { Id = "w1", Title = "Feature 1" },
                    new() { Id = "w2", Title = "Feature 2" }
                },
                InProgress = new()
                {
                    new() { Id = "w3", Title = "Feature 3" }
                },
                CarriedOver = new()
                {
                    new() { Id = "w4", Title = "Feature 4" }
                },
                Metrics = new()
                {
                    TotalPlanned = 4,
                    Completed = 2,
                    InFlight = 2,
                    HealthScore = 50m
                }
            };

            _mockDataService.Setup(s => s.IsInitialized).Returns(true);
            _mockDataService.Setup(s => s.GetDashboard()).Returns(dashboard);
            _mockDataService.Setup(s => s.LastError).Returns((string?)null);

            var cut = RenderComponent<DashboardPage>();

            cut.WaitForAssertion(() =>
            {
                var markup = cut.Markup;
                Assert.Contains("Integration Test", markup);
                Assert.Contains("Phase 1", markup);
                Assert.Contains("Phase 2", markup);
                Assert.Contains("Feature 1", markup);
                Assert.Contains("Feature 2", markup);
                Assert.Contains("Feature 3", markup);
                Assert.Contains("Feature 4", markup);
            });
        }
    }
}