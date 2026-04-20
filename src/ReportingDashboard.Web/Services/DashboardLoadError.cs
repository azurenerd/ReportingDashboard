namespace ReportingDashboard.Web.Services;

public sealed record DashboardLoadError(
    string FilePath,
    string Message,
    int? Line,
    int? Column,
    string Kind);