using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HeatmapCategory
{
    Shipped,
    InProgress,
    Carryover,
    Blockers
}