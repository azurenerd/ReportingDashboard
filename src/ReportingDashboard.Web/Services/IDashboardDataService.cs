using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public interface IDashboardDataService
{
    Task<DashboardDataResult> LoadDashboardDataAsync();
}