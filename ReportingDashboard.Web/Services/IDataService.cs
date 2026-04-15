using ReportingDashboard.Models;

namespace ReportingDashboard.Web.Services;

public interface IDataService
{
    DashboardData? GetData();
    string? GetError();
    event Action? OnDataChanged;
}