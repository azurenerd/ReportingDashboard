using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MilestoneType
{
    Checkpoint,
    Poc,
    Prod
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CaptionPosition
{
    Above,
    Below
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HeatmapCategory
{
    Shipped,
    InProgress,
    Carryover,
    Blockers
}