using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Tests.Models;

public class DashboardDataTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void Deserialize_FullPayload_ReturnsAllSections()
    {
        var json = """
        {
            "project": {
                "projectName": "Test Project",
                "executiveSponsor": "Sponsor",
                "reportingPeriod": "Q1",
                "lastUpdated": "2026-03-28",
                "overallStatus": "AtRisk",
                "summary": "Some risk."
            },
            "milestones": [
                { "id": 1, "title": "M1", "targetDate": "2026-01-01", "completionDate": null, "status": "Upcoming", "description": "Desc" }
            ],
            "workItems": [
                { "id": 101, "title": "W1", "description": "Desc", "category": "Shipped", "milestoneId": 1, "owner": "Owner", "priority": "High", "notes": null, "statusIndicator": "Done" },
                { "id": 102, "title": "W2", "description": "Desc", "category": "InProgress", "milestoneId": 1, "owner": "Owner", "priority": "Medium", "notes": null, "statusIndicator": "50%" }
            ]
        }
        """;

        var result = JsonSerializer.Deserialize<DashboardData>(json, Options);

        Assert.NotNull(result);
        Assert.Equal("Test Project", result.Project.ProjectName);
        Assert.Equal("AtRisk", result.Project.OverallStatus);
        Assert.Single(result.Milestones);
        Assert.Equal(2, result.WorkItems.Count);
    }

    [Fact]
    public void Deserialize_EmptyCollections_DefaultsToEmptyLists()
    {
        var json = """
        {
            "project": { "projectName": "Empty" },
            "milestones": [],
            "workItems": []
        }
        """;

        var result = JsonSerializer.Deserialize<DashboardData>(json, Options);

        Assert.NotNull(result);
        Assert.Empty(result.Milestones);
        Assert.Empty(result.WorkItems);
    }
}