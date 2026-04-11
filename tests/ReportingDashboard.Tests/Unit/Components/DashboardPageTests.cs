using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class DashboardPageTests : TestContext
{
    private DashboardDataService CreateLoadedService(string tempDir)
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
        var svc = new DashboardDataService(logger);

        var validJson = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "Test Dashboard",
            subtitle = "Team X - April",
            backlogLink = "https://ado.example.com",
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
                        label = "Core",
                        color = "#4285F4",
                        milestones = new[]
                        {
                            new { date = "2026-03-01", type = "poc", label = "PoC" }
                        }
                    }
                }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]> { ["jan"] = new[] { "Item A" } },
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        var path = System.IO.Path.Combine(tempDir, "data.json");
        System.IO.File.WriteAllText(path, validJson);
        svc.LoadAsync(path).GetAwaiter().GetResult();

        return svc;
    }

    private DashboardDataService CreateErrorService()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
        var svc = new DashboardDataService(logger);
        svc.LoadAsync("nonexistent/path.json").GetAwaiter().GetResult();
        return svc;
    }

    [Fact]
    public void Dashboard_WhenServiceHasError_ShowsErrorPanel()
    {
        var svc = CreateErrorService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("Dashboard data could not be loaded", cut.Markup);
    }

    [Fact]
    public void Dashboard_WhenServiceHasError_DoesNotShowDashboardRoot()
    {
        var svc = CreateErrorService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("dashboard-root", cut.Markup);
    }

    [Fact]
    public void Dashboard_WhenServiceHasData_ShowsDashboardRoot()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"DashPageTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var svc = CreateLoadedService(tempDir);
            Services.AddSingleton(svc);

            var cut = RenderComponent<Dashboard>();

            Assert.Contains("dashboard-root", cut.Markup);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Dashboard_WhenServiceHasData_ShowsHeader()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"DashPageTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var svc = CreateLoadedService(tempDir);
            Services.AddSingleton(svc);

            var cut = RenderComponent<Dashboard>();

            Assert.Contains("hdr", cut.Markup);
            Assert.Contains("Test Dashboard", cut.Markup);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Dashboard_WhenServiceHasData_ShowsTimeline()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"DashPageTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var svc = CreateLoadedService(tempDir);
            Services.AddSingleton(svc);

            var cut = RenderComponent<Dashboard>();

            Assert.Contains("tl-area", cut.Markup);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Dashboard_WhenServiceHasData_ShowsHeatmap()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"DashPageTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var svc = CreateLoadedService(tempDir);
            Services.AddSingleton(svc);

            var cut = RenderComponent<Dashboard>();

            Assert.Contains("hm-wrap", cut.Markup);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Dashboard_WhenServiceHasError_ErrorMessagePassedToPanel()
    {
        var svc = CreateErrorService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("not found", cut.Markup);
    }
}