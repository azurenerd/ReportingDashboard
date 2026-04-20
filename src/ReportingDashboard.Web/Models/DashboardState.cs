namespace ReportingDashboard.Web.Models;

public sealed record DashboardState(
    DashboardModel Model,
    ParseError? Error,
    DateTime LoadedAtUtc)
{
    public static DashboardState Empty { get; } = new(
        Model: DashboardModel.Empty,
        Error: null,
        LoadedAtUtc: DateTime.UtcNow);
}