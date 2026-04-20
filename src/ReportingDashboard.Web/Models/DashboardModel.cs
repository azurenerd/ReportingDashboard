namespace ReportingDashboard.Web.Models;

public sealed record DashboardModel(
    string Title,
    string Subtitle,
    string BacklogUrl,
    DateOnly CurrentDate,
    TimelineModel Timeline,
    HeatmapModel Heatmap)
{
    public static DashboardModel Empty { get; } = new(
        Title: string.Empty,
        Subtitle: string.Empty,
        BacklogUrl: string.Empty,
        CurrentDate: DateOnly.FromDateTime(DateTime.Today),
        Timeline: TimelineModel.Empty,
        Heatmap: HeatmapModel.Empty);
}