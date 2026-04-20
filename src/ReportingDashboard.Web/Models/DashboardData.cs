namespace ReportingDashboard.Web.Models;

/// <summary>
/// Root document model bound from <c>wwwroot/data.json</c>. Fully describes a
/// single project's reporting dashboard (header, timeline, heatmap).
/// </summary>
public sealed class DashboardData
{
    /// <summary>Project header metadata (title, subtitle, backlog link).</summary>
    public required Project Project { get; init; }

    /// <summary>Milestone timeline definition: date range and swim lanes.</summary>
    public required Timeline Timeline { get; init; }

    /// <summary>Monthly execution heatmap: months, rows, per-cell items.</summary>
    public required Heatmap Heatmap { get; init; }

    /// <summary>Optional theme overrides. Reserved for future re-skin; unused in v1.</summary>
    public Theme? Theme { get; init; }
}