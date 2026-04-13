using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Web.Tests.Unit;

[Trait("Category", "Unit")]
public class HeaderSectionComponentTests : TestContext
{
    private readonly Mock<IDashboardDataService> _mockService;

    public HeaderSectionComponentTests()
    {
        _mockService = new Mock<IDashboardDataService>();
    }

    private DashboardData CreateTestData(
        string projectName = "Test Project",
        string overallStatus = "OnTrack",
        string reportingPeriod = "Q1 2026",
        string lastUpdated = "2026-03-28",
        string summary = "Project is on track.")
    {
        return new DashboardData
        {
            Project = new ProjectInfo
            {
                ProjectName = projectName,
                OverallStatus = overallStatus,
                ReportingPeriod = reportingPeriod,
                LastUpdated = lastUpdated,
                Summary = summary
            },
            Milestones = new List<Milestone>(),
            WorkItems = new List<WorkItem>()
        };
    }

    private void RegisterService(DashboardData data)
    {
        _mockService.Setup(s => s.GetDashboardDataAsync())
            .ReturnsAsync(data);
        Services.AddSingleton<IDashboardDataService>(_mockService.Object);
    }

    [Fact]
    public void HeaderSection_RendersProjectNameAsHeading()
    {
        var data = CreateTestData(projectName: "Project Atlas");
        RegisterService(data);

        var cut = RenderComponent<ReportingDashboard.Web.Components.Sections.HeaderSection>();

        var heading = cut.Find("h1");
        Assert.Contains("Project Atlas", heading.TextContent);
    }

    [Fact]
    public void HeaderSection_RendersReportingPeriodAndLastUpdated()
    {
        var data = CreateTestData(reportingPeriod: "Q1 2026", lastUpdated: "2026-03-28");
        RegisterService(data);

        var cut = RenderComponent<ReportingDashboard.Web.Components.Sections.HeaderSection>();

        Assert.Contains("Q1 2026", cut.Markup);
        Assert.Contains("2026-03-28", cut.Markup);
    }

    [Theory]
    [InlineData("OnTrack", "badge-green")]
    [InlineData("AtRisk", "badge-amber")]
    [InlineData("Blocked", "badge-red")]
    public void HeaderSection_RendersCorrectBadgeClass_ForStatus(string status, string expectedClass)
    {
        var data = CreateTestData(overallStatus: status);
        RegisterService(data);

        var cut = RenderComponent<ReportingDashboard.Web.Components.Sections.HeaderSection>();

        Assert.Contains(expectedClass, cut.Markup);
    }

    [Fact]
    public void HeaderSection_RendersHealthSummaryText()
    {
        var data = CreateTestData(summary: "All milestones on track for Q1 delivery.");
        RegisterService(data);

        var cut = RenderComponent<ReportingDashboard.Web.Components.Sections.HeaderSection>();

        Assert.Contains("All milestones on track for Q1 delivery.", cut.Markup);
    }
}