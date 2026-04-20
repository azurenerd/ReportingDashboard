using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

/// <summary>
/// Stub. Downstream task T2 will fill in validation rules.
/// </summary>
public static class DashboardDataValidator
{
    public static IReadOnlyList<string> Validate(DashboardData data)
        => Array.Empty<string>();
}