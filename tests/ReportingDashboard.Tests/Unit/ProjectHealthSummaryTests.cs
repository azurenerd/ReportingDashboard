using Bunit;
using FluentAssertions;
using ReportingDashboard.Components;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

public class ProjectHealthSummaryTests : TestContext
{
    private static DashboardData CreateTestData(
        string name = "Test Project",
        string period = "April 2026",
        string ragStatus = "Green",
        string summary = "Project is on track.")
    {
        return new DashboardData
        {
            Project = new ProjectInfo
            {
                Name = name,
                ReportingPeriod = period,
                RagStatus = ragStatus,
                Summary = summary
            }
        };
    }

    [Fact]
    public void Renders_ProjectName_AsH1()
    {
        var data = CreateTestData(name: "Privacy Automation Release Roadmap");

        var cut = RenderComponent<ProjectHealthSummary>(p =>
            p.Add(c => c.Data, data));

        var h1 = cut.Find("h1.health-project-name");
        h1.TextContent.Should().Be("Privacy Automation Release Roadmap");
    }

    [Fact]
    public void Renders_ReportingPeriod_Subtitle()
    {
        var data = CreateTestData(period: "Q2 2026 · Sprint 14");

        var cut = RenderComponent<ProjectHealthSummary>(p =>
            p.Add(c => c.Data, data));

        var period = cut.Find(".health-reporting-period");
        period.TextContent.Should().Be("Q2 2026 · Sprint 14");
    }

    [Theory]
    [InlineData("Green", "rag-status-green", "GREEN")]
    [InlineData("Amber", "rag-status-amber", "AMBER")]
    [InlineData("Red", "rag-status-red", "RED")]
    [InlineData("Yellow", "rag-status-amber", "YELLOW")]
    public void Renders_RagStatus_WithCorrectColorClass(string ragStatus, string expectedClass, string expectedLabel)
    {
        var data = CreateTestData(ragStatus: ragStatus);

        var cut = RenderComponent<ProjectHealthSummary>(p =>
            p.Add(c => c.Data, data));

        var indicator = cut.Find(".health-rag-indicator");
        indicator.ClassList.Should().Contain(expectedClass);
        indicator.TextContent.Trim().Should().Be(expectedLabel);
    }

    [Fact]
    public void Renders_RagDot_InsideIndicator()
    {
        var data = CreateTestData(ragStatus: "Red");

        var cut = RenderComponent<ProjectHealthSummary>(p =>
            p.Add(c => c.Data, data));

        var dot = cut.Find(".health-rag-indicator .health-rag-dot");
        dot.Should().NotBeNull();
    }

    [Fact]
    public void Renders_SummaryText()
    {
        var data = CreateTestData(summary: "All milestones on track for Q2 delivery.");

        var cut = RenderComponent<ProjectHealthSummary>(p =>
            p.Add(c => c.Data, data));

        var summaryEl = cut.Find(".health-summary-text");
        summaryEl.TextContent.Should().Be("All milestones on track for Q2 delivery.");
    }

    [Fact]
    public void Hides_SummaryText_WhenEmpty()
    {
        var data = CreateTestData(summary: "");

        var cut = RenderComponent<ProjectHealthSummary>(p =>
            p.Add(c => c.Data, data));

        cut.FindAll(".health-summary-text").Should().BeEmpty();
    }

    [Fact]
    public void Hides_SummaryText_WhenWhitespace()
    {
        var data = CreateTestData(summary: "   ");

        var cut = RenderComponent<ProjectHealthSummary>(p =>
            p.Add(c => c.Data, data));

        cut.FindAll(".health-summary-text").Should().BeEmpty();
    }

    [Fact]
    public void Renders_HeaderElement()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ProjectHealthSummary>(p =>
            p.Add(c => c.Data, data));

        var header = cut.Find("header.health-header");
        header.Should().NotBeNull();
    }

    [Fact]
    public void Renders_BorderBottom_SeparatorViaClass()
    {
        var data = CreateTestData();

        var cut = RenderComponent<ProjectHealthSummary>(p =>
            p.Add(c => c.Data, data));

        // The .health-header class carries the border-bottom style from scoped CSS
        var header = cut.Find(".health-header");
        header.Should().NotBeNull();
    }

    [Fact]
    public void Defaults_UnknownRagStatus_ToGreen()
    {
        var data = CreateTestData(ragStatus: "Unknown");

        var cut = RenderComponent<ProjectHealthSummary>(p =>
            p.Add(c => c.Data, data));

        var indicator = cut.Find(".health-rag-indicator");
        indicator.ClassList.Should().Contain("rag-status-green");
    }

    [Fact]
    public void Handles_CaseInsensitive_RagStatus()
    {
        var data = CreateTestData(ragStatus: "GREEN");

        var cut = RenderComponent<ProjectHealthSummary>(p =>
            p.Add(c => c.Data, data));

        var indicator = cut.Find(".health-rag-indicator");
        indicator.ClassList.Should().Contain("rag-status-green");
    }

    [Fact]
    public void Component_StructuralLayout_MatchesDesign()
    {
        var data = CreateTestData(
            name: "My Project",
            period: "March 2026",
            ragStatus: "Amber",
            summary: "Some items at risk.");

        var cut = RenderComponent<ProjectHealthSummary>(p =>
            p.Add(c => c.Data, data));

        // Verify structural hierarchy: header > top row (left + right) + summary
        cut.Find("header.health-header .health-header-top .health-header-left .health-project-name")
            .Should().NotBeNull();
        cut.Find("header.health-header .health-header-top .health-header-left .health-reporting-period")
            .Should().NotBeNull();
        cut.Find("header.health-header .health-header-top .health-header-right .health-rag-indicator")
            .Should().NotBeNull();
        cut.Find("header.health-header .health-summary-text")
            .Should().NotBeNull();
    }
}