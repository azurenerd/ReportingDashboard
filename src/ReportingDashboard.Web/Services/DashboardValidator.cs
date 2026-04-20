using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

// Stub — downstream task T4 will implement full validation rules.
public static class DashboardValidator
{
    public static (DashboardModel Model, ParseError? Error) Validate(DashboardModel model)
    {
        return (model, null);
    }
}