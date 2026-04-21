using ReportingDashboard.Models;

namespace ReportingDashboard.Services;

public interface IDataService
{
    DashboardData? GetData();
    string? GetError();
}