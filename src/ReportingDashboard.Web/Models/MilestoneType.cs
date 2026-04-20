using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MilestoneType
{
    Checkpoint,
    Poc,
    Prod
}