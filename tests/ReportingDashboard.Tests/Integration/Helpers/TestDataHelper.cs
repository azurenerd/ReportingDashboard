using System.Text.Json;

namespace ReportingDashboard.Tests.Integration.Helpers;

/// <summary>
/// Shared helper for creating test JSON data across integration tests.
/// </summary>
public static class TestDataHelper
{
    public static string CreateValidDataJsonString()
    {
        return SerializeToJson(new
        {
            title = "Integration Test Dashboard",
            subtitle = "QA Team - April 2026",
            backlogLink = "https://dev.azure.com/test/backlog",
            currentMonth = "April",
            months = new[] { "January", "February", "March", "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[]
                {
                    new
                    {
                        name = "M1",
                        label = "Core Platform",
                        color = "#4285F4",
                        milestones = new[]
                        {
                            new { date = "2026-02-15", type = "poc", label = "Feb 15 PoC" },
                            new { date = "2026-05-01", type = "production", label = "May 1 GA" }
                        }
                    },
                    new
                    {
                        name = "M2",
                        label = "Data Pipeline",
                        color = "#00897B",
                        milestones = new[]
                        {
                            new { date = "2026-03-01", type = "checkpoint", label = "Mar 1 Check" }
                        }
                    }
                }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>
                {
                    ["jan"] = new[] { "Auth Module", "CI Pipeline" },
                    ["feb"] = new[] { "Search Feature" },
                    ["apr"] = new[] { "Dashboard v1" }
                },
                inProgress = new Dictionary<string, string[]>
                {
                    ["apr"] = new[] { "Analytics Engine", "Export API" }
                },
                carryover = new Dictionary<string, string[]>
                {
                    ["mar"] = new[] { "Legacy Migration" }
                },
                blockers = new Dictionary<string, string[]>
                {
                    ["feb"] = new[] { "Vendor SDK Delay" }
                }
            }
        });
    }

    public static string CreateMinimalValidJsonString()
    {
        return SerializeToJson(new
        {
            title = "Minimal Dashboard",
            subtitle = "Test",
            backlogLink = "",
            currentMonth = "January",
            months = new[] { "January" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-01-15",
                tracks = Array.Empty<object>()
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        });
    }

    public static string SerializeToJson(object data)
    {
        return JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }
}