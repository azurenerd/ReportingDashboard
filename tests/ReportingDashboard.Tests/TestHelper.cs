using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using ReportingDashboard.Services;

namespace ReportingDashboard.Tests;

internal static class TestHelper
{
    public static DashboardDataService CreateService(string contentRoot)
    {
        var env = new TestHostEnvironment { ContentRootPath = contentRoot };
        var loggerFactory = LoggerFactory.Create(builder => { });
        var logger = loggerFactory.CreateLogger<DashboardDataService>();
        return new DashboardDataService(env, logger);
    }

    public static string CreateTempDir(Dictionary<string, string>? files = null)
    {
        var dir = Path.Combine(Path.GetTempPath(), $"dashboard-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);

        if (files != null)
        {
            foreach (var (name, content) in files)
            {
                var filePath = Path.Combine(dir, name);
                var parentDir = Path.GetDirectoryName(filePath)!;
                Directory.CreateDirectory(parentDir);
                File.WriteAllText(filePath, content);
            }
        }

        return dir;
    }

    public const string MinimalValidJson = """
    {
        "title": "Test Dashboard",
        "subtitle": "Test Subtitle",
        "backlogUrl": "https://example.com",
        "currentDate": "2026-04-01",
        "months": ["Apr"],
        "currentMonthIndex": 0,
        "timelineStart": "2026-04-01",
        "timelineEnd": "2026-04-30",
        "milestones": [
            {
                "id": "M1",
                "label": "M1",
                "description": "Test Milestone",
                "color": "#0078D4",
                "markers": []
            }
        ],
        "categories": [
            { "name": "Shipped", "key": "shipped", "items": {} },
            { "name": "In Progress", "key": "inProgress", "items": {} },
            { "name": "Carryover", "key": "carryover", "items": {} },
            { "name": "Blockers", "key": "blockers", "items": {} }
        ]
    }
    """;

    public const string PhoenixValidJson = """
    {
        "title": "Project Phoenix Release Roadmap",
        "subtitle": "Cloud Engineering \u2013 Phoenix Migration Workstream \u2013 April 2026",
        "backlogUrl": "https://dev.azure.com/contoso/Phoenix/_backlogs",
        "currentDate": "2026-04-14",
        "months": ["Feb", "Mar", "Apr", "May", "Jun", "Jul"],
        "currentMonthIndex": 2,
        "timelineStart": "2026-02-01",
        "timelineEnd": "2026-07-31",
        "milestones": [
            {
                "id": "P1",
                "label": "P1",
                "description": "Core Migration",
                "color": "#D32F2F",
                "markers": [
                    { "date": "2026-02-15", "type": "checkpoint", "label": "Feb 15" },
                    { "date": "2026-04-01", "type": "poc", "label": "Apr 1 PoC" },
                    { "date": "2026-06-15", "type": "production", "label": "Jun 15 GA" }
                ]
            },
            {
                "id": "P2",
                "label": "P2",
                "description": "Data Platform",
                "color": "#7B1FA2",
                "markers": [
                    { "date": "2026-03-01", "type": "checkpoint", "label": "Mar 1" },
                    { "date": "2026-04-15", "type": "checkpoint", "label": "Apr 15" },
                    { "date": "2026-07-01", "type": "production", "label": "Jul 1 GA" }
                ]
            }
        ],
        "categories": [
            {
                "name": "Shipped",
                "key": "shipped",
                "items": {
                    "Feb": ["VM fleet migrated to Gen3"],
                    "Mar": ["Cosmos DB throughput tuned"],
                    "Apr": ["Service mesh rollout phase 1"],
                    "May": [], "Jun": [], "Jul": []
                }
            },
            {
                "name": "In Progress",
                "key": "inProgress",
                "items": {
                    "Feb": ["Database schema migration planning"],
                    "Mar": ["Kubernetes cluster provisioning"],
                    "Apr": ["Data pipeline modernization"],
                    "May": [], "Jun": [], "Jul": []
                }
            },
            {
                "name": "Carryover",
                "key": "carryover",
                "items": {
                    "Feb": [], "Mar": ["DNS cutover delayed from Feb"],
                    "Apr": ["Storage migration backlog"],
                    "May": [], "Jun": [], "Jul": []
                }
            },
            {
                "name": "Blockers",
                "key": "blockers",
                "items": {
                    "Feb": [], "Mar": [],
                    "Apr": ["Legacy API sunset blocked by compliance"],
                    "May": [], "Jun": [], "Jul": []
                }
            }
        ]
    }
    """;
}

internal class TestHostEnvironment : IWebHostEnvironment
{
    public string WebRootPath { get; set; } = string.Empty;
    public IFileProvider WebRootFileProvider { get; set; } = null!;
    public string ApplicationName { get; set; } = "ReportingDashboard.Tests";
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
    public string ContentRootPath { get; set; } = string.Empty;
    public string EnvironmentName { get; set; } = "Testing";
}