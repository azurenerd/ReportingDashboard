using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Tests.Models;

public class WorkItemTests
{
    [Fact]
    public void Deserialize_ShippedWorkItem_AllPropertiesMapped()
    {
        var json = """
        {
            "id": "WI-101",
            "title": "REST API v2 Endpoint Migration",
            "description": "Migrated all endpoints.",
            "category": "Shipped",
            "milestoneId": "MS-2",
            "owner": "Marcus Johnson",
            "priority": "High"
        }
        """;

        var result = JsonSerializer.Deserialize<WorkItem>(json);

        Assert.NotNull(result);
        Assert.Equal("WI-101", result.Id);
        Assert.Equal("Shipped", result.Category);
        Assert.Equal("MS-2", result.MilestoneId);
        Assert.Equal("Marcus Johnson", result.Owner);
        Assert.Equal("High", result.Priority);
    }

    [Fact]
    public void Deserialize_WorkItem_NullMilestoneId_IsAllowed()
    {
        var json = """
        {
            "id": "WI-999",
            "title": "Standalone task",
            "description": "No milestone.",
            "category": "InProgress",
            "milestoneId": null,
            "owner": "Test User",
            "priority": "Low"
        }
        """;

        var result = JsonSerializer.Deserialize<WorkItem>(json);

        Assert.NotNull(result);
        Assert.Null(result.MilestoneId);
    }
}