namespace ReportingDashboard.Web.Models;

public sealed record TimelineConfig
{
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public DateOnly NowDate { get; init; }
    public IReadOnlyList<string>? MonthLabels { get; init; }
    public IReadOnlyList<Swimlane> Swimlanes { get; init; } = Array.Empty<Swimlane>();
}