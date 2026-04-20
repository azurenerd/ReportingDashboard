using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

// STUB — T3 will implement the full rule set. Returning empty keeps downstream engines unblocked.
public static class DashboardDataValidator
{
    public static IReadOnlyList<string> Validate(DashboardData data) => Array.Empty<string>();
}