using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public interface IDashboardDataService
{
    DashboardLoadResult GetCurrent();

    event EventHandler? DataChanged;
}
