using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Tests.Models;

public class MilestoneTests
{
    [Fact]
    public void Deserialize_CompletedMilestone_IncludesCompletionDate()
    {
        var json = """
        {
            "id": "MS-1",
            "title": "Architecture Design Complete",
            "targetDate": "2026-01-15",
            "completionDate": "2026-01-14",
            "status": "Completed"
        }
        """;

        var result = JsonSerializer.Deserialize<Milestone>(json);

        Assert.NotNull(result);
        Assert.Equal("MS-1", result.Id);
        Assert.Equal("2026-01-14", result.CompletionDate);
        Assert.Equal("Completed", result.Status);
    }

    [Fact]
    public void Deserialize_UpcomingMilestone_CompletionDateIsNull()
    {
        var json = """
        {
            "id": "MS-7",
            "title": "Production Rollout",
            "targetDate": "2026-05-01",
            "completionDate": null,
            "status": "Upcoming"
        }
        """;

        var result = JsonSerializer.Deserialize<Milestone>(json);

        Assert.NotNull(result);
        Assert.Null(result.CompletionDate);
        Assert.Equal("Upcoming", result.Status);
    }
}