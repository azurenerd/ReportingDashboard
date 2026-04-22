using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public interface IDataService
{
    DashboardData? GetData();
    string? GetError();
}
