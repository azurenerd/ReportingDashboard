using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CaptionPosition
{
    Above,
    Below
}