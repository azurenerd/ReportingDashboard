using System.Text.Json;

namespace ReportingDashboard.Tests.Helpers;

internal static class TestDataHelper
{
    public static string GenerateValidJson(string title = "Test Project")
    {
        var data = new
        {
            title,
            subtitle = "Test Org \u00b7 Test Workstream \u00b7 March 2026",
            backlogUrl = "https://dev.azure.com/test",
            currentDate = "2026-03-15",
            months = new[] { "Jan", "Feb", "Mar" },
            currentMonthIndex = 2,
            timelineStart = "2026-01-01",
            timelineEnd = "2026-06-30",
            milestones = new[]
            {
                new
                {
                    id = "M1",
                    label = "M1",
                    description = "Test Milestone",
                    color = "#0078D4",
                    markers = new[]
                    {
                        new { date = "2026-02-15", type = "checkpoint", label = "Feb 15" }
                    }
                }
            },
            categories = new object[]
            {
                new { name = "Shipped", key = "shipped", items = new Dictionary<string, string[]> { { "Jan", new[] { "Item 1" } } } },
                new { name = "In Progress", key = "inProgress", items = new Dictionary<string, string[]>() },
                new { name = "Carryover", key = "carryover", items = new Dictionary<string, string[]>() },
                new { name = "Blockers", key = "blockers", items = new Dictionary<string, string[]>() }
            }
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    }

    public static string GenerateValidJsonWithTitle(string title)
    {
        return GenerateValidJson(title);
    }
}