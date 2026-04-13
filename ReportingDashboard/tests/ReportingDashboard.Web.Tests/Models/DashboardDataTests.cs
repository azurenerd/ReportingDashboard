using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Tests.Models;

public class DashboardDataTests
{
    [Fact]
    public void Deserialize_FullPayload_ReturnsAllSections()
    {
        var json = """
        {
            "projectInfo": {
                "projectName": "Test Project",
                "executiveSponsor": "Sponsor",
                "reportingPeriod": "Q1",
                "overallStatus": "AtRisk",
                "summary": "Some risk."
            },
            "milestones": [
                { "id": "M1", "title": "M1", "targetDate": "2026-01-01", "completionDate": null, "status": "Upcoming" }
            ],
            "workItems": [
                { "id": "W1", "title": "W1", "description": "Desc", "category": "Shipped", "milestoneId": "M1", "owner": "Owner", "priority": "High" },
                { "id": "W2", "title": "W2", "description": "Desc", "category": "InProgress", "milestoneId": null, "owner": "Owner", "priority": "Medium" }
            ]
        }
        """;

        var result = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(result);
        Assert.Equal("Test Project", result.ProjectInfo.ProjectName);
        Assert.Equal("AtRisk", result.ProjectInfo.OverallStatus);
        Assert.Single(result.Milestones);
        Assert.Equal(2, result.WorkItems.Count);
    }

    [Fact]
    public void Deserialize_EmptyCollections_DefaultsToEmptyLists()
    {
        var json = """
        {
            "projectInfo": { "projectName": "Empty" },
            "milestones": [],
            "workItems": []
        }
        """;

        var result = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(result);
        Assert.Empty(result.Milestones);
        Assert.Empty(result.WorkItems);
    }
}