using System.Collections.Generic;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests;

internal sealed class FakeDashboardDataService : IDashboardDataService
{
    private readonly DashboardLoadResult _result;
    public FakeDashboardDataService(DashboardLoadResult result) => _result = result;
    public DashboardLoadResult GetCurrent() => _result;
    public event EventHandler? DataChanged { add { } remove { } }

    public static DashboardData SampleData() => new()
    {
        Project = new Project
        {
            Title = "Sample Project",
            Subtitle = "Org - Workstream - Apr 2026",
            BacklogUrl = "https://example.com/backlog"
        },
        Timeline = new Timeline
        {
            Start = new DateOnly(2026, 1, 1),
            End = new DateOnly(2026, 6, 30),
            Lanes = new List<TimelineLane>
            {
                new() { Id = "M1", Label = "Lane One", Color = "#0078D4", Milestones = Array.Empty<Milestone>() },
                new() { Id = "M2", Label = "Lane Two", Color = "#00897B", Milestones = Array.Empty<Milestone>() },
                new() { Id = "M3", Label = "Lane Three", Color = "#546E7A", Milestones = Array.Empty<Milestone>() }
            }
        },
        Heatmap = new Heatmap
        {
            Months = new[] { "Jan", "Feb", "Mar", "Apr" },
            CurrentMonthIndex = null,
            MaxItemsPerCell = 4,
            Rows = new List<HeatmapRow>
            {
                new() { Category = HeatmapCategory.Shipped,    Cells = Empty4() },
                new() { Category = HeatmapCategory.InProgress, Cells = Empty4() },
                new() { Category = HeatmapCategory.Carryover,  Cells = Empty4() },
                new() { Category = HeatmapCategory.Blockers,   Cells = Empty4() }
            }
        }
    };

    private static IReadOnlyList<IReadOnlyList<string>> Empty4() => new IReadOnlyList<string>[]
    {
        Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()
    };

    public static FakeDashboardDataService WithSample() => new(new DashboardLoadResult(
        SampleData(), Error: null, LoadedAt: DateTimeOffset.UtcNow));

    public static FakeDashboardDataService WithError(DashboardLoadError error) => new(new DashboardLoadResult(
        Data: null, Error: error, LoadedAt: DateTimeOffset.UtcNow));
}
