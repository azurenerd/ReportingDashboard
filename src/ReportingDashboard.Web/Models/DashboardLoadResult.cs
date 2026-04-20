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
    string Kind) // one of DashboardLoadErrorKind.*
{
}

public static class DashboardLoadErrorKind
{
    public const string NotFound = "NotFound";
    public const string ParseError = "ParseError";
    public const string ValidationError = "ValidationError";
}