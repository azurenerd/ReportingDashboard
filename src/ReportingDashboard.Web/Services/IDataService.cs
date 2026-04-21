using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public interface IDataService
{
    Task<DashboardData> LoadDashboardDataAsync();
}