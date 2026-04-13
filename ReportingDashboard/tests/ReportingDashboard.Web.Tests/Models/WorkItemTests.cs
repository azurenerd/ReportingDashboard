using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Tests.Models;

public class WorkItemTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void Deserialize_ShippedWorkItem_AllPropertiesMapped()
    {
        var json = """
        {
            "id": 101,
            "title": "REST API v2 Endpoint Migration",
            "description": "Migrated all endpoints.",
            "category": "Shipped",
            "milestoneId": 2,
            "owner": "Marcus Johnson",
            "priority": "High",
            "notes": "Zero-downtime migration",
            "statusIndicator": "Done"
        }
        """;

        var result = JsonSerializer.Deserialize<WorkItem>(json, Options);

        Assert.NotNull(result);
        Assert.Equal(101, result.Id);
        Assert.Equal("Shipped", result.Category);
        Assert.Equal(2, result.MilestoneId);
        Assert.Equal("Marcus Johnson", result.Owner);
        Assert.Equal("High", result.Priority);
        Assert.Equal("Zero-downtime migration", result.Notes);
        Assert.Equal("Done", result.StatusIndicator);
    }

    [Fact]
    public void Deserialize_WorkItem_NullNotes_IsAllowed()
    {
        var json = """
        {
            "id": 999,
            "title": "Standalone task",
            "description": "No notes.",
            "category": "InProgress",
            "milestoneId": 1,
            "owner": "Test User",
            "priority": "Low",
            "notes": null,
            "statusIndicator": null
        }
        """;

        var result = JsonSerializer.Deserialize<WorkItem>(json, Options);

        Assert.NotNull(result);
        Assert.Null(result.Notes);
        Assert.Null(result.StatusIndicator);
    }
}