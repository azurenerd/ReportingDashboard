namespace ReportingDashboard.Web.Models;

public sealed record DashboardLoadResult(
    DashboardData? Data,
    DashboardLoadError? Error,
    DateTimeOffset LoadedAt);

public sealed record DashboardLoadError(
    string FilePath,
    string Message,
    int? Line,
    int? Column,
    string Kind);