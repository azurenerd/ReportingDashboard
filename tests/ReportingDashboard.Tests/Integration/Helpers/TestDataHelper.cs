using System.Text.Json;

namespace ReportingDashboard.Tests.Integration.Helpers;

/// <summary>
/// Helper class for generating test JSON data for integration tests.
/// If this file already exists, this provides a known-good implementation.
/// </summary>
public static class TestDataHelper
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string CreateValidDataJsonString()
    {
        var data = new
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
                tracks = new object[]
                {
                    new
                    {
                        name = "M1",
                        label = "Core Platform",
                        color = "#4285F4",
                        milestones = new object[]
                        {
                            new { date = "2026-02-15", type = "poc", label = "Feb 15 PoC" },
                            new { date = "2026-05-01", type = "production", label = "May 1 GA" }
                        }
                    },
                    new
                    {
                        name = "M2",
                        label = "Data Pipeline",
                        color = "#EA4335",
                        milestones = new object[]
                        {
                            new { date = "2026-03-15", type = "checkpoint", label = "Mar 15 Check" }
                        }
                    }
                }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>
                {
                    ["jan"] = new[] { "Auth Module", "CI Pipeline" },
                    ["feb"] = new[] { "Dashboard v1" },
                    ["mar"] = new[] { "API Gateway", "Logging", "Monitoring" }
                },
                inProgress = new Dictionary<string, string[]>
                {
                    ["apr"] = new[] { "Notifications", "Reports Engine" }
                },
                carryover = new Dictionary<string, string[]>
                {
                    ["mar"] = new[] { "Legacy Migration" }
                },
                blockers = new Dictionary<string, string[]>
                {
                    ["apr"] = new[] { "Vendor License" }
                }
            }
        };

        return JsonSerializer.Serialize(data, SerializerOptions);
    }

    public static string SerializeToJson(object data)
    {
        return JsonSerializer.Serialize(data, SerializerOptions);
    }
}