using System.Text.Json;

namespace ReportingDashboard.Tests.Integration.Helpers;

public static class TestDataHelper
{
    public static string BuildValidJsonString(
        string title = "Test Project",
        string subtitle = "Test Subtitle",
        string backlogUrl = "https://example.com",
        string currentMonth = "Apr",
        int monthCount = 6,
        int categoryCount = 4)
    {
        var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        var selectedMonths = months.Take(monthCount).ToList();

        var categories = new List<object>();
        var categoryDefs = new (string name, string css, string emoji)[]
        {
            ("Shipped", "ship", "✅"),
            ("In Progress", "prog", "🔄"),
            ("Carryover", "carry", "📦"),
            ("Blockers", "block", "🚫")
        };

        for (int i = 0; i < Math.Min(categoryCount, categoryDefs.Length); i++)
        {
            var def = categoryDefs[i];
            categories.Add(new
            {
                name = def.name,
                cssClass = def.css,
                emoji = def.emoji,
                items = new Dictionary<string, List<string>>
                {
                    { selectedMonths[0], new List<string> { $"{def.name} Item 1" } }
                }
            });
        }

        var data = new
        {
            project = new
            {
                title,
                subtitle,
                backlogUrl,
                currentMonth
            },
            timeline = new
            {
                months = selectedMonths,
                nowPosition = 0.5
            },
            tracks = new[]
            {
                new
                {
                    id = "m1",
                    label = "Track 1",
                    color = "#0078D4",
                    milestones = new[]
                    {
                        new { date = "2024-03-15", type = "checkpoint", position = 0.4, label = "CP1" }
                    }
                }
            },
            heatmap = new
            {
                months = selectedMonths,
                categories
            }
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    }
}