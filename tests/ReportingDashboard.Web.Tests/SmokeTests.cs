using ReportingDashboard.Web.Services;
using ReportingDashboard.Web.ViewModels;

namespace ReportingDashboard.Web.Tests;

public class SmokeTests
{
    [Fact]
    public void DashboardDataService_Stub_ReturnsEmptyResult()
    {
        IDashboardDataService svc = new DashboardDataService();
        var result = svc.GetCurrent();

        result.Should().NotBeNull();
        result.Data.Should().BeNull();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void ViewModels_Empty_AreNotNull()
    {
        TimelineViewModel.Empty.Should().NotBeNull();
        TimelineViewModel.Empty.Lanes.Should().BeEmpty();
        HeatmapViewModel.Empty.Should().NotBeNull();
        HeatmapViewModel.Empty.Rows.Should().BeEmpty();
    }
}