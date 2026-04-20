namespace ReportingDashboard.Web.Models;

public sealed record ProjectInfo
{
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public string? BacklogUrl { get; init; }
    public DateOnly AsOfDate { get; init; }
}