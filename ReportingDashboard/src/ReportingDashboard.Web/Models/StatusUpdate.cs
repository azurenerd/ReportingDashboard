using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

public class StatusUpdate
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("risks")]
    public List<string> Risks { get; set; } = new();

    [JsonPropertyName("nextSteps")]
    public List<string> NextSteps { get; set; } = new();
}