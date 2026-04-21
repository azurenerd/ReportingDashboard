using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Data;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class DashboardIntegrationTests : IDisposable
{
    private readonly string _tempDir;

    public DashboardIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard_int_tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string WriteTempJson(string json)
    {
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, json);
        return path;
    }

    private static string BuildValidJson() => JsonSerializer.Serialize(new
    {
        project = new { title = "Test Project", subtitle = "Test Sub", backlogUrl = "http://example.com" },
        timeline = new
        {
            startDate = "2026-01-01",
            endDate = "2026-06-30",
            tracks = new[]
            {
                new
                {
                    id = "M1", name = "Track 1", color = "#0078D4",
                    milestones = new[] { new { date = "2026-03-01", type = "start", label = "Start" } }
                }
            }
        },
        heatmap = new
        {
            columns = new[] { "Jan", "Feb" },
            currentColumn = "Feb",
            rows = new[]
            {
                new { category = "Shipped", items = new Dictionary<string, List<string>> { ["Jan"] = new() { "Item1" }, ["Feb"] = new() } }
            }
        }
    });

    [Fact]
    public void DashboardDataService_LoadsValidJson_HasDataTrue()
    {
        var path = WriteTempJson(BuildValidJson());
        var svc = new DashboardDataService(path);

        svc.HasData.Should().BeTrue();
        svc.Data.Should().NotBeNull();
        svc.Data!.Project.Title.Should().Be("Test Project");
    }

    [Fact]
    public void DashboardDataService_IsRegisteredCorrectly_WhenResolvedTwice_SameInstance()
    {
        var path = WriteTempJson(BuildValidJson());
        var services = new ServiceCollection();
        services.AddSingleton<DashboardDataService>(_ => new DashboardDataService(path));
        var provider = services.BuildServiceProvider();

        var svc1 = provider.GetRequiredService<DashboardDataService>();
        var svc2 = provider.GetRequiredService<DashboardDataService>();

        svc1.Should().BeSameAs(svc2);
    }

    [Fact]
    public void DashboardDataService_Reload_RefreshesData()
    {
        var path = WriteTempJson(BuildValidJson());
        var svc = new DashboardDataService(path);
        svc.HasData.Should().BeTrue();

        // Delete file and reload
        File.Delete(path);
        svc.Reload();

        svc.HasData.Should().BeFalse();
        svc.ErrorMessage.Should().Contain("Dashboard data not found");
    }

    [Fact]
    public void DashboardDataService_OnDataChanged_FiresOnReload()
    {
        var path = WriteTempJson(BuildValidJson());
        var svc = new DashboardDataService(path);

        var fired = false;
        svc.OnDataChanged += () => fired = true;
        svc.Reload();

        fired.Should().BeTrue();
    }

    [Fact]
    public void DashboardDataService_InvalidMilestoneType_FailsValidation()
    {
        var json = JsonSerializer.Serialize(new
        {
            project = new { title = "T", subtitle = "S", backlogUrl = "http://x.com" },
            timeline = new
            {
                startDate = "2026-01-01", endDate = "2026-06-30",
                tracks = new[]
                {
                    new
                    {
                        id = "M1", name = "T1", color = "#000",
                        milestones = new[] { new { date = "2026-02-01", type = "invalid_type" } }
                    }
                }
            },
            heatmap = new
            {
                columns = new[] { "Jan" }, currentColumn = "Jan",
                rows = new[] { new { category = "Shipped", items = new Dictionary<string, List<string>> { ["Jan"] = new() } } }
            }
        });
        var path = WriteTempJson(json);
        var svc = new DashboardDataService(path);

        svc.HasData.Should().BeFalse();
        svc.ErrorMessage.Should().Contain("milestone type");
        svc.ErrorMessage.Should().Contain("invalid_type");
    }
}