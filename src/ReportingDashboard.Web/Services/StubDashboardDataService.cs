using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

// TODO: Replace with DashboardDataService in T2
public class StubDashboardDataService : IDashboardDataService
{
    public Task<DashboardDataResult> LoadDashboardDataAsync()
    {
        return Task.FromResult(new DashboardDataResult
        {
            Success = false,
            ErrorMessage = "DashboardDataService not yet implemented. See T2."
        });
    }
}