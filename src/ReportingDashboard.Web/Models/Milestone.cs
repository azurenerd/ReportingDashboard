using System.Text.Json.Serialization;

namespace ReportingDashboard.Web.Models;

public class Milestone
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";

    [JsonPropertyName("label")]
    public string Label { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "checkpoint";
}