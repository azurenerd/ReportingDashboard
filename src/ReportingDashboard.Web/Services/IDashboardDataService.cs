using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public interface IDashboardDataService
{
    DashboardState Current { get; }
    event Action OnChanged;
    void Reload();
}