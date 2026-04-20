using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Services;

public sealed record DashboardLoadResult(
    DashboardData? Data,
    DashboardLoadError? Error,
    DateTimeOffset LoadedAt);