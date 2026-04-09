using System.ComponentModel.DataAnnotations;

namespace AgentSquad.Runner.Models;

public class DashboardSummary
{
    [Range(0, 10000, ErrorMessage = "Shipped count must be between 0 and 10000")]
    public int ShippedCount { get; set; }

    [Range(0, 10000, ErrorMessage = "In progress count must be between 0 and 10000")]
    public int InProgressCount { get; set; }

    [Range(0, 10000, ErrorMessage = "Carryover count must be between 0 and 10000")]
    public int CarryoverCount { get; set; }

    [Range(0, 100, ErrorMessage = "Overall percent complete must be between 0 and 100")]
    public int OverallPercentComplete { get; set; }

    public DateTime LastUpdated { get; set; }
}