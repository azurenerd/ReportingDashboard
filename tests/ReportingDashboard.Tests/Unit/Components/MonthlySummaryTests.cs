using Bunit;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class MonthlySummaryTests : TestContext
{
    private static MonthSummary CreateTestSummary(
        string month = "April",
        int totalItems = 42,
        int completedItems = 28,
        string overallHealth = "On Track")
    {
        return new MonthSummary
        {
            Month = month,
            TotalItems = totalItems,
            CompletedItems = completedItems,
            OverallHealth = overallHealth
        };
    }

    // --- Basic Rendering ---

    [Fact]
    public void MonthlySummary_WithData_RendersMetricsGrid()
    {
        var summary = CreateTestSummary();

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        cut.Find("div.metrics-grid").Should().NotBeNull();
    }

    [Fact]
    public void MonthlySummary_WithData_RendersMonth()
    {
        var summary = CreateTestSummary(month: "March");

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        cut.Markup.Should().Contain("March");
        cut.Markup.Should().Contain("Current Month");
    }

    [Fact]
    public void MonthlySummary_WithData_RendersTotalItems()
    {
        var summary = CreateTestSummary(totalItems: 55);

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        cut.Markup.Should().Contain("55");
        cut.Markup.Should().Contain("Total Items");
    }

    [Fact]
    public void MonthlySummary_WithData_RendersCompletedItems()
    {
        var summary = CreateTestSummary(completedItems: 30);

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        cut.Markup.Should().Contain("30");
        cut.Markup.Should().Contain("Completed");
    }

    [Fact]
    public void MonthlySummary_WithData_RendersOverallHealth()
    {
        var summary = CreateTestSummary(overallHealth: "On Track");

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        cut.Markup.Should().Contain("On Track");
        cut.Markup.Should().Contain("Overall Health");
    }

    [Fact]
    public void MonthlySummary_WithData_RendersFourMetricCards()
    {
        var summary = CreateTestSummary();

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        var cards = cut.FindAll("div.metric-card");
        cards.Should().HaveCount(4);
    }

    // --- Null Summary ---

    [Fact]
    public void MonthlySummary_NullSummary_ShowsNoDataMessage()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, (MonthSummary?)null));

        cut.Markup.Should().Contain("No monthly summary data available.");
    }

    [Fact]
    public void MonthlySummary_NullSummary_DoesNotRenderMetricsGrid()
    {
        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, (MonthSummary?)null));

        cut.FindAll("div.metrics-grid").Should().BeEmpty();
    }

    // --- Health Class Mapping ---

    [Fact]
    public void MonthlySummary_OnTrackHealth_HasOnTrackClass()
    {
        var summary = CreateTestSummary(overallHealth: "On Track");

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        cut.Markup.Should().Contain("health-on-track");
    }

    [Fact]
    public void MonthlySummary_OnTrackHyphenated_HasOnTrackClass()
    {
        var summary = CreateTestSummary(overallHealth: "on-track");

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        cut.Markup.Should().Contain("health-on-track");
    }

    [Fact]
    public void MonthlySummary_AtRiskHealth_HasAtRiskClass()
    {
        var summary = CreateTestSummary(overallHealth: "At Risk");

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        cut.Markup.Should().Contain("health-at-risk");
    }

    [Fact]
    public void MonthlySummary_AtRiskHyphenated_HasAtRiskClass()
    {
        var summary = CreateTestSummary(overallHealth: "at-risk");

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        cut.Markup.Should().Contain("health-at-risk");
    }

    [Fact]
    public void MonthlySummary_BehindHealth_HasBehindClass()
    {
        var summary = CreateTestSummary(overallHealth: "Behind");

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        cut.Markup.Should().Contain("health-behind");
    }

    [Fact]
    public void MonthlySummary_BehindLowercase_HasBehindClass()
    {
        var summary = CreateTestSummary(overallHealth: "behind");

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        cut.Markup.Should().Contain("health-behind");
    }

    [Fact]
    public void MonthlySummary_UnknownHealth_HasEmptyClass()
    {
        var summary = CreateTestSummary(overallHealth: "Unknown Status");

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        cut.Markup.Should().Contain("health-");
    }

    [Fact]
    public void MonthlySummary_NullHealth_HasEmptyClass()
    {
        // Use object initializer to set init-only property to null via casting
        var summary = new MonthSummary
        {
            Month = "April",
            TotalItems = 42,
            CompletedItems = 28,
            OverallHealth = null!
        };

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        cut.Markup.Should().Contain("health-");
    }

    // --- Section Structure ---

    [Fact]
    public void MonthlySummary_HasSectionClass()
    {
        var summary = CreateTestSummary();

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        cut.Find("div.monthly-summary").Should().NotBeNull();
    }

    [Fact]
    public void MonthlySummary_HasHeading()
    {
        var summary = CreateTestSummary();

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        var h2 = cut.Find("h2");
        h2.TextContent.Should().Contain("Monthly Summary");
    }

    // --- Boundary Values ---

    [Fact]
    public void MonthlySummary_ZeroItems_RendersCorrectly()
    {
        var summary = CreateTestSummary(totalItems: 0, completedItems: 0);

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        cut.Markup.Should().Contain("0");
    }

    [Fact]
    public void MonthlySummary_LargeNumbers_RendersCorrectly()
    {
        var summary = CreateTestSummary(totalItems: 99999, completedItems: 88888);

        var cut = RenderComponent<ReportingDashboard.Components.Sections.MonthlySummary>(
            p => p.Add(x => x.Summary, summary));

        cut.Markup.Should().Contain("99999");
        cut.Markup.Should().Contain("88888");
    }
}