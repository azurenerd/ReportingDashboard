using Bunit;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests
{
    public class MetricsFooterTests : TestContext
    {
        [Fact]
        public void Renders_AllFourMetricBoxes_WithValidDashboard()
        {
            // Arrange
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test Project",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(1),
                Milestones = new(),
                Shipped = new() { new WorkItem { Id = "w1", Title = "Item 1" } },
                InProgress = new(),
                CarriedOver = new(),
                Metrics = new ProgressMetrics
                {
                    TotalPlanned = 1,
                    Completed = 1,
                    InFlight = 0,
                    HealthScore = 100m
                }
            };

            // Act
            var cut = RenderComponent<MetricsFooter>(parameters =>
                parameters.CascadingValue(dashboard));

            // Assert
            var boxes = cut.FindAll(".metric-box");
            Assert.Equal(4, boxes.Count);
        }

        [Fact]
        public void Displays_TotalPlannedCount_Correctly()
        {
            // Arrange
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(1),
                Milestones = new(),
                Shipped = new() 
                { 
                    new WorkItem { Id = "w1", Title = "Item 1" },
                    new WorkItem { Id = "w2", Title = "Item 2" }
                },
                InProgress = new() 
                { 
                    new WorkItem { Id = "w3", Title = "Item 3" }
                },
                CarriedOver = new(),
                Metrics = new ProgressMetrics
                {
                    TotalPlanned = 3,
                    Completed = 2,
                    InFlight = 1,
                    HealthScore = 67m
                }
            };

            // Act
            var cut = RenderComponent<MetricsFooter>(parameters =>
                parameters.CascadingValue(dashboard));

            // Assert
            var totalPlannedBox = cut.FindAll(".metric-value")[0];
            Assert.Contains("3", totalPlannedBox.TextContent);
        }

        [Fact]
        public void Displays_CompletedCount_Correctly()
        {
            // Arrange
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(1),
                Milestones = new(),
                Shipped = new() { new WorkItem { Id = "w1", Title = "Item 1" } },
                InProgress = new(),
                CarriedOver = new(),
                Metrics = new ProgressMetrics
                {
                    TotalPlanned = 3,
                    Completed = 1,
                    InFlight = 2,
                    HealthScore = 33m
                }
            };

            // Act
            var cut = RenderComponent<MetricsFooter>(parameters =>
                parameters.CascadingValue(dashboard));

            // Assert
            var completedBox = cut.FindAll(".metric-value")[1];
            Assert.Contains("1", completedBox.TextContent);
        }

        [Fact]
        public void Calculates_CompletionPercentage_Correctly()
        {
            // Arrange: 32/50 = 64%
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(1),
                Milestones = new(),
                Shipped = new(Enumerable.Range(1, 32)
                    .Select(i => new WorkItem { Id = $"w{i}", Title = $"Item {i}" }).ToList()),
                InProgress = new(Enumerable.Range(33, 18)
                    .Select(i => new WorkItem { Id = $"w{i}", Title = $"Item {i}" }).ToList()),
                CarriedOver = new(),
                Metrics = new ProgressMetrics
                {
                    TotalPlanned = 50,
                    Completed = 32,
                    InFlight = 18,
                    HealthScore = 64m
                }
            };

            // Act
            var cut = RenderComponent<MetricsFooter>(parameters =>
                parameters.CascadingValue(dashboard));

            // Assert
            var completionBox = cut.FindAll(".metric-value")[2];
            Assert.Contains("64%", completionBox.TextContent);
        }

        [Fact]
        public void Displays_HealthScore_Correctly()
        {
            // Arrange
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(1),
                Milestones = new(),
                Shipped = new() { new WorkItem { Id = "w1", Title = "Item 1" } },
                InProgress = new(),
                CarriedOver = new(),
                Metrics = new ProgressMetrics
                {
                    TotalPlanned = 1,
                    Completed = 1,
                    InFlight = 0,
                    HealthScore = 100m
                }
            };

            // Act
            var cut = RenderComponent<MetricsFooter>(parameters =>
                parameters.CascadingValue(dashboard));

            // Assert
            var healthBox = cut.FindAll(".metric-value")[3];
            Assert.Contains("100", healthBox.TextContent);
        }

        [Fact]
        public void AppliesGreenColor_WhenCompletionPercentageIsGreaterThanOrEqualTo75Percent()
        {
            // Arrange: 40/50 = 80%
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(1),
                Milestones = new(),
                Shipped = new(Enumerable.Range(1, 40)
                    .Select(i => new WorkItem { Id = $"w{i}", Title = $"Item {i}" }).ToList()),
                InProgress = new(Enumerable.Range(41, 10)
                    .Select(i => new WorkItem { Id = $"w{i}", Title = $"Item {i}" }).ToList()),
                CarriedOver = new(),
                Metrics = new ProgressMetrics
                {
                    TotalPlanned = 50,
                    Completed = 40,
                    InFlight = 10,
                    HealthScore = 80m
                }
            };

            // Act
            var cut = RenderComponent<MetricsFooter>(parameters =>
                parameters.CascadingValue(dashboard));

            // Assert
            var completionBox = cut.FindAll(".metric-value")[2];
            Assert.Contains("text-success", completionBox.GetAttribute("class"));
        }

        [Fact]
        public void AppliesOrangeColor_WhenCompletionPercentageIsBetween40And75Percent()
        {
            // Arrange: 25/50 = 50%
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(1),
                Milestones = new(),
                Shipped = new(Enumerable.Range(1, 25)
                    .Select(i => new WorkItem { Id = $"w{i}", Title = $"Item {i}" }).ToList()),
                InProgress = new(Enumerable.Range(26, 25)
                    .Select(i => new WorkItem { Id = $"w{i}", Title = $"Item {i}" }).ToList()),
                CarriedOver = new(),
                Metrics = new ProgressMetrics
                {
                    TotalPlanned = 50,
                    Completed = 25,
                    InFlight = 25,
                    HealthScore = 50m
                }
            };

            // Act
            var cut = RenderComponent<MetricsFooter>(parameters =>
                parameters.CascadingValue(dashboard));

            // Assert
            var completionBox = cut.FindAll(".metric-value")[2];
            Assert.Contains("text-warning", completionBox.GetAttribute("class"));
        }

        [Fact]
        public void AppliesRedColor_WhenCompletionPercentageIsLessThan40Percent()
        {
            // Arrange: 15/50 = 30%
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(1),
                Milestones = new(),
                Shipped = new(Enumerable.Range(1, 15)
                    .Select(i => new WorkItem { Id = $"w{i}", Title = $"Item {i}" }).ToList()),
                InProgress = new(Enumerable.Range(16, 35)
                    .Select(i => new WorkItem { Id = $"w{i}", Title = $"Item {i}" }).ToList()),
                CarriedOver = new(),
                Metrics = new ProgressMetrics
                {
                    TotalPlanned = 50,
                    Completed = 15,
                    InFlight = 35,
                    HealthScore = 30m
                }
            };

            // Act
            var cut = RenderComponent<MetricsFooter>(parameters =>
                parameters.CascadingValue(dashboard));

            // Assert
            var completionBox = cut.FindAll(".metric-value")[2];
            Assert.Contains("text-danger", completionBox.GetAttribute("class"));
        }

        [Fact]
        public void AppliesGreenColor_WhenHealthScoreIsGreaterThanOrEqualTo75()
        {
            // Arrange
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(1),
                Milestones = new(),
                Shipped = new() { new WorkItem { Id = "w1", Title = "Item 1" } },
                InProgress = new(),
                CarriedOver = new(),
                Metrics = new ProgressMetrics
                {
                    TotalPlanned = 1,
                    Completed = 1,
                    InFlight = 0,
                    HealthScore = 100m
                }
            };

            // Act
            var cut = RenderComponent<MetricsFooter>(parameters =>
                parameters.CascadingValue(dashboard));

            // Assert
            var healthBox = cut.FindAll(".metric-value")[3];
            Assert.Contains("text-success", healthBox.GetAttribute("class"));
        }

        [Fact]
        public void AppliesOrangeColor_WhenHealthScoreIsBetween40And75()
        {
            // Arrange
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(1),
                Milestones = new(),
                Shipped = new(Enumerable.Range(1, 5)
                    .Select(i => new WorkItem { Id = $"w{i}", Title = $"Item {i}" }).ToList()),
                InProgress = new(Enumerable.Range(6, 5)
                    .Select(i => new WorkItem { Id = $"w{i}", Title = $"Item {i}" }).ToList()),
                CarriedOver = new(),
                Metrics = new ProgressMetrics
                {
                    TotalPlanned = 10,
                    Completed = 5,
                    InFlight = 5,
                    HealthScore = 50m
                }
            };

            // Act
            var cut = RenderComponent<MetricsFooter>(parameters =>
                parameters.CascadingValue(dashboard));

            // Assert
            var healthBox = cut.FindAll(".metric-value")[3];
            Assert.Contains("text-warning", healthBox.GetAttribute("class"));
        }

        [Fact]
        public void AppliesRedColor_WhenHealthScoreIsLessThan40()
        {
            // Arrange
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(1),
                Milestones = new(),
                Shipped = new(Enumerable.Range(1, 3)
                    .Select(i => new WorkItem { Id = $"w{i}", Title = $"Item {i}" }).ToList()),
                InProgress = new(Enumerable.Range(4, 7)
                    .Select(i => new WorkItem { Id = $"w{i}", Title = $"Item {i}" }).ToList()),
                CarriedOver = new(),
                Metrics = new ProgressMetrics
                {
                    TotalPlanned = 10,
                    Completed = 3,
                    InFlight = 7,
                    HealthScore = 30m
                }
            };

            // Act
            var cut = RenderComponent<MetricsFooter>(parameters =>
                parameters.CascadingValue(dashboard));

            // Assert
            var healthBox = cut.FindAll(".metric-value")[3];
            Assert.Contains("text-danger", healthBox.GetAttribute("class"));
        }

        [Fact]
        public void Handles_NullDashboard_Gracefully()
        {
            // Act
            var cut = RenderComponent<MetricsFooter>(parameters =>
                parameters.CascadingValue<ProjectDashboard>(null!));

            // Assert
            var boxes = cut.FindAll(".metric-box");
            Assert.Equal(4, boxes.Count);
            var values = cut.FindAll(".metric-value");
            Assert.All(values, v => Assert.Contains("-", v.TextContent));
        }

        [Fact]
        public void Displays_Dash_WhenTotalPlannedIsZero()
        {
            // Arrange
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(1),
                Milestones = new(),
                Shipped = new(),
                InProgress = new(),
                CarriedOver = new(),
                Metrics = new ProgressMetrics
                {
                    TotalPlanned = 0,
                    Completed = 0,
                    InFlight = 0,
                    HealthScore = 0m
                }
            };

            // Act
            var cut = RenderComponent<MetricsFooter>(parameters =>
                parameters.CascadingValue(dashboard));

            // Assert
            var completionBox = cut.FindAll(".metric-value")[2];
            Assert.Contains("-", completionBox.TextContent);
        }

        [Fact]
        public void Handles_LargeNumbers_WithoutOverflow()
        {
            // Arrange
            var largeItems = Enumerable.Range(1, 1000)
                .Select(i => new WorkItem { Id = $"w{i}", Title = $"Item {i}" }).ToList();
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(1),
                Milestones = new(),
                Shipped = new(largeItems.Take(500).ToList()),
                InProgress = new(largeItems.Skip(500).Take(250).ToList()),
                CarriedOver = new(largeItems.Skip(750).ToList()),
                Metrics = new ProgressMetrics
                {
                    TotalPlanned = 1000,
                    Completed = 500,
                    InFlight = 500,
                    HealthScore = 50m
                }
            };

            // Act
            var cut = RenderComponent<MetricsFooter>(parameters =>
                parameters.CascadingValue(dashboard));

            // Assert
            var boxes = cut.FindAll(".metric-value");
            Assert.Equal(4, boxes.Count);
            Assert.Contains("1000", boxes[0].TextContent);
            Assert.Contains("500", boxes[1].TextContent);
            Assert.Contains("50%", boxes[2].TextContent);
        }

        [Fact]
        public void Rounds_CompletionPercentage_ToZeroDecimalPlaces()
        {
            // Arrange: 1/3 = 33.33% should round to 33%
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(1),
                Milestones = new(),
                Shipped = new() { new WorkItem { Id = "w1", Title = "Item 1" } },
                InProgress = new(Enumerable.Range(2, 2)
                    .Select(i => new WorkItem { Id = $"w{i}", Title = $"Item {i}" }).ToList()),
                CarriedOver = new(),
                Metrics = new ProgressMetrics
                {
                    TotalPlanned = 3,
                    Completed = 1,
                    InFlight = 2,
                    HealthScore = 33m
                }
            };

            // Act
            var cut = RenderComponent<MetricsFooter>(parameters =>
                parameters.CascadingValue(dashboard));

            // Assert
            var completionBox = cut.FindAll(".metric-value")[2];
            Assert.Contains("33%", completionBox.TextContent);
        }

        [Fact]
        public void Renders_MetricLabels_WithSemanticHTML()
        {
            // Arrange
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(1),
                Milestones = new(),
                Shipped = new() { new WorkItem { Id = "w1", Title = "Item 1" } },
                InProgress = new(),
                CarriedOver = new(),
                Metrics = new ProgressMetrics
                {
                    TotalPlanned = 1,
                    Completed = 1,
                    InFlight = 0,
                    HealthScore = 100m
                }
            };

            // Act
            var cut = RenderComponent<MetricsFooter>(parameters =>
                parameters.CascadingValue(dashboard));

            // Assert
            var labels = cut.FindAll("h6.metric-label");
            Assert.Equal(4, labels.Count);
            Assert.Contains("Total Planned", labels[0].TextContent);
            Assert.Contains("Completed", labels[1].TextContent);
            Assert.Contains("Completion %", labels[2].TextContent);
            Assert.Contains("Health Score", labels[3].TextContent);
        }

        [Fact]
        public void Metrics_Footer_ContainerHasBootstrapClasses()
        {
            // Arrange
            var dashboard = new ProjectDashboard
            {
                ProjectName = "Test",
                StartDate = DateTime.Now,
                PlannedCompletion = DateTime.Now.AddMonths(1),
                Milestones = new(),
                Shipped = new() { new WorkItem { Id = "w1", Title = "Item 1" } },
                InProgress = new(),
                CarriedOver = new(),
                Metrics = new ProgressMetrics
                {
                    TotalPlanned = 1,
                    Completed = 1,
                    InFlight = 0,
                    HealthScore = 100m
                }
            };

            // Act
            var cut = RenderComponent<MetricsFooter>(parameters =>
                parameters.CascadingValue(dashboard));

            // Assert
            var container = cut.Find(".container-fluid");
            Assert.NotNull(container);
        }
    }
}