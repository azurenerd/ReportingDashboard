namespace ReportingDashboard.Web.Models;

public sealed record HeatmapRows
{
    public IReadOnlyList<IReadOnlyList<string>> Shipped { get; init; } = Array.Empty<IReadOnlyList<string>>();
    public IReadOnlyList<IReadOnlyList<string>> InProgress { get; init; } = Array.Empty<IReadOnlyList<string>>();
    public IReadOnlyList<IReadOnlyList<string>> Carryover { get; init; } = Array.Empty<IReadOnlyList<string>>();
    public IReadOnlyList<IReadOnlyList<string>> Blockers { get; init; } = Array.Empty<IReadOnlyList<string>>();
}