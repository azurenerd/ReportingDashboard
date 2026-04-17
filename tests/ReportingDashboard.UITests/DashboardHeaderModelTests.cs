using Xunit;

namespace ReportingDashboard.UITests;

/// <summary>
/// Pure reflection and file-system tests that do NOT need a running server.
/// These were previously inside DashboardHeaderUITests and failed with
/// ERR_CONNECTION_REFUSED because InitializeAsync navigated to localhost.
/// </summary>
[Trait("Category", "Unit")]
public class DashboardHeaderModelTests
{
    [Fact]
    public void DashboardHeaderComponent_ExistsInProject()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && dir.GetFiles("*.sln").Length == 0)
        {
            dir = dir.Parent;
        }
        Assert.NotNull(dir);

        var componentPath = Path.Combine(dir.FullName, "ReportingDashboard", "Components", "DashboardHeader.razor");
        Assert.True(File.Exists(componentPath), $"DashboardHeader.razor should exist at {componentPath}");
    }

    [Fact]
    public void DashboardData_ModelHasRequiredProperties()
    {
        var type = typeof(ReportingDashboard.Models.DashboardData);
        Assert.NotNull(type.GetProperty("Title"));
        Assert.NotNull(type.GetProperty("Subtitle"));
        Assert.NotNull(type.GetProperty("BacklogUrl"));
        Assert.NotNull(type.GetProperty("Heatmap"));
    }

    [Fact]
    public void HeatmapData_HasCurrentMonthProperty()
    {
        var type = typeof(ReportingDashboard.Models.HeatmapData);
        var prop = type.GetProperty("CurrentMonth");
        Assert.NotNull(prop);
        Assert.Equal(typeof(string), prop.PropertyType);
    }

    [Fact]
    public void DashboardDataService_IsRegistered()
    {
        // Validate the service type exists and has expected members
        var type = typeof(ReportingDashboard.Services.DashboardDataService);
        Assert.NotNull(type);
        Assert.NotNull(type.GetProperty("Data"));
        Assert.NotNull(type.GetProperty("HasError"));
        Assert.NotNull(type.GetProperty("ErrorMessage"));
    }

    [Fact]
    public void ComponentParameter_AcceptsDashboardData()
    {
        // Verify DashboardData record can be constructed (parameter compatibility)
        var heatmap = new ReportingDashboard.Models.HeatmapData(
            "Test", new List<string> { "Jan" }, "Jan",
            new List<ReportingDashboard.Models.HeatmapRow>());

        var range = new ReportingDashboard.Models.TimelineRange(
            "2026-01-01", "2026-06-30",
            new List<ReportingDashboard.Models.MonthGridline>());

        var data = new ReportingDashboard.Models.DashboardData(
            Title: "Test",
            Subtitle: "Sub",
            BacklogUrl: "https://example.com",
            ReportDate: "2026-04-15",
            TimelineRange: range,
            Workstreams: new List<ReportingDashboard.Models.Workstream>(),
            Heatmap: heatmap);

        Assert.Equal("Test", data.Title);
        Assert.Equal("Sub", data.Subtitle);
        Assert.NotNull(data.Heatmap);
    }

    [Fact]
    public void DashboardHeaderRazor_IsNotModifyingOtherFiles()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && dir.GetFiles("*.sln").Length == 0)
        {
            dir = dir.Parent;
        }
        Assert.NotNull(dir);

        // Verify key sibling components still exist (header change didn't delete them)
        var timelinePath = Path.Combine(dir.FullName, "ReportingDashboard", "Components", "TimelineSection.razor");
        Assert.True(File.Exists(timelinePath), "TimelineSection.razor should still exist");

        var heatmapPath = Path.Combine(dir.FullName, "ReportingDashboard", "Components", "HeatmapGrid.razor");
        Assert.True(File.Exists(heatmapPath), "HeatmapGrid.razor should still exist");

        var dashboardPath = Path.Combine(dir.FullName, "ReportingDashboard", "Components", "Pages", "Dashboard.razor");
        Assert.True(File.Exists(dashboardPath), "Dashboard.razor should still exist");
    }
}