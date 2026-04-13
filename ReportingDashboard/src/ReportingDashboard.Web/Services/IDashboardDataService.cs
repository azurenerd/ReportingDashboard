using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public interface IDashboardDataService
{
    Task<DashboardData> GetDashboardDataAsync();
}