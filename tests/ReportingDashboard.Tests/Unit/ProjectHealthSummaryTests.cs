using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class ProjectHealthSummaryTests : TestContext
{
    private static DashboardData CreateDashboardData(
        string projectName = "Test Project",
        string reportingPeriod = "Q1 2026",
        string ragStatus = "Green",
        string? summary = "All systems operational.")
    {
        return new DashboardData
        {
            Project = new ProjectInfo
            {
                Name = projectName,
                ReportingPeriod = reportingPeriod,
                RagStatus = ragStatus,
                Summary = summary
            }
        };
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RendersAllDataPoints_WhenFullyPopulated()
    {
        // Arrange
        var data = CreateDashboardData(
            projectName: "Alpha Release",
            reportingPeriod: "March 2026",
            ragStatus: "Green",
            summary: "On track for delivery.");

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.ProjectHealthSummary>(
            p => p.Add(x => x.Data, data));

        // Assert
        cut.Find("h1.health-project-name").TextContent.Should().Be("Alpha Release");
        cut.Find(".health-reporting-period").TextContent.Should().Be("March 2026");
        cut.Find(".health-rag-indicator").TextContent.Trim().Should().Contain("GREEN");
        cut.Find("p.health-summary-text").TextContent.Should().Be("On track for delivery.");
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("Green", "rag-status-green")]
    [InlineData("Amber", "rag-status-amber")]
    [InlineData("Red", "rag-status-red")]
    [InlineData("yellow", "rag-status-amber")]
    [InlineData("GREEN", "rag-status-green")]
    public void RagIndicator_AppliesCorrectCssClass(string ragStatus, string expectedClass)
    {
        // Arrange
        var data = CreateDashboardData(ragStatus: ragStatus);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.ProjectHealthSummary>(
            p => p.Add(x => x.Data, data));

        // Assert
        var indicator = cut.Find(".health-rag-indicator");
        indicator.ClassList.Should().Contain(expectedClass);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RagIndicator_DefaultsToGreen_WhenStatusUnknown()
    {
        // Arrange
        var data = CreateDashboardData(ragStatus: "Purple");

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.ProjectHealthSummary>(
            p => p.Add(x => x.Data, data));

        // Assert
        var indicator = cut.Find(".health-rag-indicator");
        indicator.ClassList.Should().Contain("rag-status-green");
        indicator.TextContent.Trim().Should().Contain("PURPLE");
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HidesSummaryText_WhenNullOrWhitespace(string? summary)
    {
        // Arrange
        var data = CreateDashboardData(summary: summary);

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.ProjectHealthSummary>(
            p => p.Add(x => x.Data, data));

        // Assert
        cut.FindAll("p.health-summary-text").Should().BeEmpty();
        // Header and other elements still render
        cut.Find("h1.health-project-name").Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RagStatusText_IsUppercased()
    {
        // Arrange
        var data = CreateDashboardData(ragStatus: "amber");

        // Act
        var cut = RenderComponent<ReportingDashboard.Components.ProjectHealthSummary>(
            p => p.Add(x => x.Data, data));

        // Assert
        var indicator = cut.Find(".health-rag-indicator");
        indicator.TextContent.Trim().Should().Contain("AMBER");
        indicator.ClassList.Should().Contain("rag-status-amber");
    }
}