using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public sealed record DashboardLoadResult(
    DashboardData? Data,
    DashboardLoadError? Error,
    DateTimeOffset LoadedAt)
{
    public static DashboardLoadResult Empty { get; } = new(null, null, DateTimeOffset.MinValue);
}

public sealed record DashboardLoadError(
    string FilePath,
    string Message,
    int? Line,
    int? Column,
    string Kind);