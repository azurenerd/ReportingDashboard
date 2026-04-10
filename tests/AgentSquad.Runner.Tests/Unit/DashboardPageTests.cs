using Bunit;
using Xunit;
using Moq;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AgentSquad.Runner.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class DashboardPageTests : TestContext
    {
        private readonly Mock<IProjectDataService> _mockDataService;

        public DashboardPageTests()
        {
            _mockDataService = new Mock<IProjectDataService>();
            Services.AddScoped(_ => _mockDataService.Object);
        }

        [Fact]
        public void DashboardPage_WithInitializedService_RendersDashboard()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test Project",
                Milestones = new(),
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new()
            };

            _mockDataService.Setup(s => s.IsInitialized).Returns(true);
            _mockDataService.Setup(s => s.GetDashboard()).Returns(dashboard);
            _mockDataService.Setup(s => s.LastError).Returns((string?)null);

            var cut = RenderComponent<DashboardPage>();

            Assert.NotNull(cut);
        }

        [Fact]
        public void DashboardPage_WithServiceError_DisplaysErrorAlert()
        {
            _mockDataService.Setup(s => s.IsInitialized).Returns(false);
            _mockDataService.Setup(s => s.LastError).Returns("Data file not found");

            var cut = RenderComponent<DashboardPage>();

            cut.WaitForAssertion(() =>
            {
                var markup = cut.Markup;
                Assert.Contains("Data file not found", markup);
            });
        }

        [Fact]
        public void DashboardPage_WithValidData_CascadesDashboard()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Cascaded Project",
                Milestones = new() { new() { Id = "m1", Name = "M1", Status = "Completed", TargetDate = DateTime.Now } },
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new()
            };

            _mockDataService.Setup(s => s.IsInitialized).Returns(true);
            _mockDataService.Setup(s => s.GetDashboard()).Returns(dashboard);
            _mockDataService.Setup(s => s.LastError).Returns((string?)null);

            var cut = RenderComponent<DashboardPage>();

            cut.WaitForAssertion(() =>
            {
                var markup = cut.Markup;
                Assert.Contains("Cascaded Project", markup);
            });
        }

        [Fact]
        public void DashboardPage_ErrorMessage_DoesNotIncludeStackTrace()
        {
            _mockDataService.Setup(s => s.IsInitialized).Returns(false);
            _mockDataService.Setup(s => s.LastError).Returns("User-friendly error message");

            var cut = RenderComponent<DashboardPage>();

            cut.WaitForAssertion(() =>
            {
                var markup = cut.Markup;
                Assert.DoesNotContain("at ", markup.ToLower());
                Assert.Contains("User-friendly error message", markup);
            });
        }

        [Fact]
        public void DashboardPage_WithCompleteValidDashboard_RendersAllSections()
        {
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Complete Project",
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
                    new() { Id = "w2", Title = "In Progress Item" }
                },
                CarriedOver = new()
                {
                    new() { Id = "w3", Title = "Carried Over Item" }
                },
                Metrics = new()
                {
                    TotalPlanned = 3,
                    Completed = 1,
                    InFlight = 2,
                    HealthScore = 33.33m
                }
            };

            _mockDataService.Setup(s => s.IsInitialized).Returns(true);
            _mockDataService.Setup(s => s.GetDashboard()).Returns(dashboard);
            _mockDataService.Setup(s => s.LastError).Returns((string?)null);

            var cut = RenderComponent<DashboardPage>();

            cut.WaitForAssertion(() =>
            {
                var markup = cut.Markup;
                Assert.Contains("Complete Project", markup);
                Assert.Contains("Shipped Item", markup);
                Assert.Contains("In Progress Item", markup);
                Assert.Contains("Carried Over Item", markup);
            });
        }

        [Fact]
        public void DashboardPage_WithLargeDataset_RendersWith45Items()
        {
            var shipped = Enumerable.Range(1, 15).Select(i => new WorkItem { Id = $"w{i}", Title = $"Shipped {i}" }).ToList();
            var inProgress = Enumerable.Range(16, 15).Select(i => new WorkItem { Id = $"w{i}", Title = $"InProgress {i}" }).ToList();
            var carriedOver = Enumerable.Range(31, 15).Select(i => new WorkItem { Id = $"w{i}", Title = $"CarriedOver {i}" }).ToList();

            var dashboard = new ProjectDashboard
            {
                ProjectName = "Large Project",
                Milestones = new(),
                Shipped = shipped,
                InProgress = inProgress,
                CarriedOver = carriedOver
            };

            _mockDataService.Setup(s => s.IsInitialized).Returns(true);
            _mockDataService.Setup(s => s.GetDashboard()).Returns(dashboard);
            _mockDataService.Setup(s => s.LastError).Returns((string?)null);

            var cut = RenderComponent<DashboardPage>();

            cut.WaitForAssertion(() =>
            {
                var markup = cut.Markup;
                Assert.Contains("Shipped 1", markup);
                Assert.Contains("Shipped 15", markup);
                Assert.Contains("InProgress 16", markup);
                Assert.Contains("CarriedOver 45", markup);
            });
        }
    }
}