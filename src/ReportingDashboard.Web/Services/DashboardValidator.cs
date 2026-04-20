using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public static class DashboardValidator
{
    public static ParseError? Validate(DashboardModel model)
    {
        // T4 fills in validation rules. Stub returns null (valid).
        _ = model;
        return null;
    }
}