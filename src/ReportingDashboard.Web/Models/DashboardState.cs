namespace ReportingDashboard.Web.Models;

public sealed record DashboardState(
    DashboardModel Model,
    ParseError? Error,
    DateTime LoadedAtUtc);