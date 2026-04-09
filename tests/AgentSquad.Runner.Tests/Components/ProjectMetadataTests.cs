using Bunit;
using FluentAssertions;
using Xunit;
using AgentSquad.Runner.Components;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Components
{
    public class ProjectMetadataTests : TestContext
    {
        [Fact]
        public void RenderProjectNameFromCascadingParameter()
        {
            // Arrange
            var component = RenderComponent<ProjectMetadata>(
                parameters => parameters
                    .Add(p => p.ProjectName, "Project Horizon")
                    .Add(p => p.ReportingPeriod, "2026-Q2")
                    .Add(p => p.Kpis, new Dictionary<string, int>())
            );

            // Act
            var h1 = component.Find("header h1");

            // Assert
            h1.TextContent.Should().Be("Project Horizon");
        }

        [Fact]
        public void RenderReportingPeriodFromCascadingParameter()
        {
            // Arrange
            var component = RenderComponent<ProjectMetadata>(
                parameters => parameters
                    .Add(p => p.ProjectName, "Project Horizon")
                    .Add(p => p.ReportingPeriod, "Q2 2026")
                    .Add(p => p.Kpis, new Dictionary<string, int>())
            );

            // Act
            var p = component.Find("header p.text-muted");

            // Assert
            p.TextContent.Should().Be("Q2 2026");
        }

        [Fact]
        public void RenderKpiGridWithMultipleMetrics()
        {
            // Arrange
            var kpis = new Dictionary<string, int>
            {
                { "On-Time Delivery", 85 },
                { "Team Capacity", 92 },
                { "Quality Score", 78 },
                { "Customer Satisfaction", 88 },
                { "Budget Utilization", 72 }
            };

            var component = RenderComponent<ProjectMetadata>(
                parameters => parameters
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.ReportingPeriod, "Test Period")
                    .Add(p => p.Kpis, kpis)
            );

            // Act
            var kpiCards = component.FindAll(".kpi-grid .card");

            // Assert
            kpiCards.Should().HaveCount(5);
        }

        [Fact]
        public void RenderKpiLabelsCorrectly()
        {
            // Arrange
            var kpis = new Dictionary<string, int>
            {
                { "On-Time Delivery", 85 },
                { "Team Capacity", 92 }
            };

            var component = RenderComponent<ProjectMetadata>(
                parameters => parameters
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.ReportingPeriod, "Test Period")
                    .Add(p => p.Kpis, kpis)
            );

            // Act
            var cardTexts = component.FindAll(".card-body .card-text");

            // Assert
            cardTexts[0].TextContent.Should().Contain("On-Time Delivery");
            cardTexts[1].TextContent.Should().Contain("Team Capacity");
        }

        [Fact]
        public void DisplayKpiValueAsPercentage()
        {
            // Arrange
            var kpis = new Dictionary<string, int>
            {
                { "On-Time Delivery", 85 }
            };

            var component = RenderComponent<ProjectMetadata>(
                parameters => parameters
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.ReportingPeriod, "Test Period")
                    .Add(p => p.Kpis, kpis)
            );

            // Act
            var kpiValue = component.Find(".kpi-value");

            // Assert
            kpiValue.TextContent.Should().Be("85%");
        }

        [Fact]
        public void RenderEmptyKpiGrid()
        {
            // Arrange
            var component = RenderComponent<ProjectMetadata>(
                parameters => parameters
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.ReportingPeriod, "Test Period")
                    .Add(p => p.Kpis, new Dictionary<string, int>())
            );

            // Act
            var kpiCards = component.FindAll(".kpi-grid .card");

            // Assert
            kpiCards.Should().HaveCount(0);
        }

        [Fact]
        public void RenderEmptyKpiGridWithNullDictionary()
        {
            // Arrange & Act
            var component = RenderComponent<ProjectMetadata>(
                parameters => parameters
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.ReportingPeriod, "Test Period")
                    .Add(p => p.Kpis, (Dictionary<string, int>)null!)
            );

            // Assert - Should render without errors
            var header = component.Find("header");
            header.Should().NotBeNull();
            var kpiCards = component.FindAll(".kpi-grid .card");
            kpiCards.Should().HaveCount(0);
        }

        [Fact]
        public void HandleNullProjectName()
        {
            // Arrange & Act
            var component = RenderComponent<ProjectMetadata>(
                parameters => parameters
                    .Add(p => p.ProjectName, (string)null!)
                    .Add(p => p.ReportingPeriod, "Test Period")
                    .Add(p => p.Kpis, new Dictionary<string, int>())
            );

            // Assert - Should render placeholder instead of null
            var h1 = component.Find("header h1");
            h1.Should().NotBeNull();
            h1.TextContent.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void HandleNullReportingPeriod()
        {
            // Arrange & Act
            var component = RenderComponent<ProjectMetadata>(
                parameters => parameters
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.ReportingPeriod, (string)null!)
                    .Add(p => p.Kpis, new Dictionary<string, int>())
            );

            // Assert - Should render placeholder instead of null
            var p = component.Find("header p.text-muted");
            p.Should().NotBeNull();
            p.TextContent.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void RenderResponsiveGridClasses()
        {
            // Arrange
            var kpis = new Dictionary<string, int>
            {
                { "Metric 1", 85 }
            };

            var component = RenderComponent<ProjectMetadata>(
                parameters => parameters
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.ReportingPeriod, "Test Period")
                    .Add(p => p.Kpis, kpis)
            );

            // Act
            var kpiColumn = component.Find(".kpi-grid .col-md-6");

            // Assert
            kpiColumn.ClassList.Should().Contain("col-md-6");
            kpiColumn.ClassList.Should().Contain("col-lg-3");
        }

        [Fact]
        public void UpdateOnCascadingParameterChange()
        {
            // Arrange
            var initialKpis = new Dictionary<string, int> { { "Metric 1", 85 } };
            var component = RenderComponent<ProjectMetadata>(
                parameters => parameters
                    .Add(p => p.ProjectName, "Project A")
                    .Add(p => p.ReportingPeriod, "Q2")
                    .Add(p => p.Kpis, initialKpis)
            );

            var initialH1 = component.Find("header h1");
            initialH1.TextContent.Should().Be("Project A");

            // Act - Update cascading parameter
            component.SetParametersAndRender(
                parameters => parameters
                    .Add(p => p.ProjectName, "Project B")
                    .Add(p => p.ReportingPeriod, "Q3")
                    .Add(p => p.Kpis, initialKpis)
            );

            // Assert - Component should re-render with new value
            var updatedH1 = component.Find("header h1");
            updatedH1.TextContent.Should().Be("Project B");
        }

        [Fact]
        public void UpdateKpiGridOnParameterChange()
        {
            // Arrange
            var initialKpis = new Dictionary<string, int> { { "Metric 1", 85 } };
            var component = RenderComponent<ProjectMetadata>(
                parameters => parameters
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.ReportingPeriod, "Test Period")
                    .Add(p => p.Kpis, initialKpis)
            );

            var initialCards = component.FindAll(".kpi-grid .card");
            initialCards.Should().HaveCount(1);

            // Act - Update KPI dictionary with more metrics
            var updatedKpis = new Dictionary<string, int>
            {
                { "Metric 1", 85 },
                { "Metric 2", 92 },
                { "Metric 3", 78 }
            };

            component.SetParametersAndRender(
                parameters => parameters
                    .Add(p => p.Kpis, updatedKpis)
            );

            // Assert - Grid should re-render with new KPI count
            var updatedCards = component.FindAll(".kpi-grid .card");
            updatedCards.Should().HaveCount(3);
        }

        [Fact]
        public void RenderBootstrapCardClasses()
        {
            // Arrange
            var kpis = new Dictionary<string, int> { { "Metric 1", 85 } };
            var component = RenderComponent<ProjectMetadata>(
                parameters => parameters
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.ReportingPeriod, "Test Period")
                    .Add(p => p.Kpis, kpis)
            );

            // Act
            var card = component.Find(".card");

            // Assert
            card.ClassList.Should().Contain("card");
            var cardBody = component.Find(".card-body");
            cardBody.ClassList.Should().Contain("card-body");
        }

        [Fact]
        public void HandleSpecialCharactersInKpiLabels()
        {
            // Arrange
            var kpis = new Dictionary<string, int>
            {
                { "On-Time & Quality", 85 }
            };

            var component = RenderComponent<ProjectMetadata>(
                parameters => parameters
                    .Add(p => p.ProjectName, "Test Project")
                    .Add(p => p.ReportingPeriod, "Test Period")
                    .Add(p => p.Kpis, kpis)
            );

            // Act
            var cardText = component.Find(".card-text");

            // Assert - Special characters should be properly encoded by Blazor
            cardText.TextContent.Should().Contain("On-Time");
            cardText.TextContent.Should().Contain("Quality");
        }
    }
}