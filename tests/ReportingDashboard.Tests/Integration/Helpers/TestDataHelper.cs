using System.Text.Json;

namespace ReportingDashboard.Tests.Integration.Helpers;

public static class TestDataHelper
{
    public static string BuildValidJsonString(
        string title = "Test Project",
        string subtitle = "Test Org · Test Workstream · Apr 2024",
        string backlogUrl = "https://dev.azure.com/test",
        string currentMonth = "Apr",
        double nowPosition = 0.55)
    {
        var data = new
        {
            project = new { title, subtitle, backlogUrl, currentMonth },
            timeline = new
            {
                months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
                nowPosition
            },
            tracks = new[]
            {
                new
                {
                    id = "m1",
                    label = "M1 - Feature Alpha",
                    color = "#4285F4",
                    milestones = new[]
                    {
                        new { date = "2024-02-15", type = "checkpoint", position = 0.25, label = (string?)null },
                        new { date = "2024-04-01", type = "poc", position = 0.55, label = "PoC" },
                        new { date = "2024-06-01", type = "production", position = 0.92, label = "GA" }
                    }
                }
            },
            heatmap = new
            {
                months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
                categories = new[]
                {
                    new
                    {
                        name = "Shipped",
                        cssClass = "ship",
                        emoji = "✅",
                        items = new Dictionary<string, string[]>
                        {
                            { "Jan", new[] { "Auth module", "CI pipeline" } },
                            { "Feb", new[] { "Search API" } }
                        }
                    },
                    new
                    {
                        name = "In Progress",
                        cssClass = "prog",
                        emoji = "🔄",
                        items = new Dictionary<string, string[]>
                        {
                            { "Apr", new[] { "Dashboard UI", "Data layer" } }
                        }
                    },
                    new
                    {
                        name = "Carryover",
                        cssClass = "carry",
                        emoji = "📦",
                        items = new Dictionary<string, string[]>
                        {
                            { "Mar", new[] { "Legacy migration" } }
                        }
                    },
                    new
                    {
                        name = "Blockers",
                        cssClass = "block",
                        emoji = "🚫",
                        items = new Dictionary<string, string[]>
                        {
                            { "Apr", new[] { "Vendor dependency" } }
                        }
                    }
                }
            }
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
    }
}