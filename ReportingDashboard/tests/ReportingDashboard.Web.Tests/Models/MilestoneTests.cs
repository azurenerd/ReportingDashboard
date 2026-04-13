using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Tests.Models;

public class MilestoneTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void Deserialize_CompletedMilestone_IncludesCompletionDate()
    {
        var json = """
        {
            "id": 1,
            "title": "Architecture Design Complete",
            "targetDate": "2026-01-15",
            "completionDate": "2026-01-14",
            "status": "Completed",
            "description": "Architecture review board sign-off"
        }
        """;

        var result = JsonSerializer.Deserialize<Milestone>(json, Options);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("2026-01-14", result.CompletionDate);
        Assert.Equal("Completed", result.Status);
    }

    [Fact]
    public void Deserialize_UpcomingMilestone_CompletionDateIsNull()
    {
        var json = """
        {
            "id": 7,
            "title": "Production Rollout",
            "targetDate": "2026-05-01",
            "completionDate": null,
            "status": "Upcoming",
            "description": "GA release"
        }
        """;

        var result = JsonSerializer.Deserialize<Milestone>(json, Options);

        Assert.NotNull(result);
        Assert.Null(result.CompletionDate);
        Assert.Equal("Upcoming", result.Status);
    }
}