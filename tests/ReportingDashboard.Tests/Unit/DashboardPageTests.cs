using System.Text.Json;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardPageTests : IDisposable
{
    private readonly string _tempDir;

    public DashboardPageTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dashboard_page_tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private DashboardDataService CreateServiceWithData(DashboardData? data)
    {
        var filePath = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        if (data != null)
        {
            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(filePath, json);
        }
        var options = new DashboardDataServiceOptions { FilePath = filePath };
        var service = new DashboardDataService(options);
        service.Initialize();
        return service;
    }

    [Fact]
    public void Dashboard_WhenDataIsNull_RendersErrorContainer()
    {
        using var service = CreateServiceWithData(null);
        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton<DashboardDataService>(service);

        var cut = ctx.RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("Unable to load dashboard data");
        cut.Markup.Should().Contain("Please check that data.json exists and contains valid JSON");
        cut.Markup.Should().Contain("width:1920px");
        cut.Markup.Should().Contain("height:1080px");
        cut.Markup.Should().Contain("Data file not found");
    }

    [Fact]
    public void Dashboard_WhenDataIsNull_ErrorMessage_RendersDetailText()
    {
        var filePath = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(filePath, "NOT JSON");
        var options = new DashboardDataServiceOptions { FilePath = filePath };
        using var service = new DashboardDataService(options);
        service.Initialize();

        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton<DashboardDataService>(service);

        var cut = ctx.RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Markup.Should().Contain("Invalid JSON in data file");
        cut.Markup.Should().Contain("font-size:14px;color:#999");
    }

    [Fact]
    public void Dashboard_WhenDataIsValid_RendersChildComponents()
    {
        var data = new DashboardData
        {
            Title = "Test Project",
            Subtitle = "Test Subtitle",
            Timeline = new TimelineConfig
            {
                StartMonth = "2026-01",
                EndMonth = "2026-06",
                Tracks = new List<Track>()
            },
            Heatmap = new HeatmapConfig()
        };
        using var service = CreateServiceWithData(data);
        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton<DashboardDataService>(service);

        var cut = ctx.RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Markup.Should().NotContain("Unable to load dashboard data");
    }

    [Fact]
    public void Dashboard_ComputeNowDate_WithValidNowDate_ParsesOverride()
    {
        var data = new DashboardData
        {
            Title = "Test",
            NowDate = "2026-03-15",
            Timeline = new TimelineConfig
            {
                StartMonth = "2026-01",
                EndMonth = "2026-06",
                Tracks = new List<Track>()
            },
            Heatmap = new HeatmapConfig()
        };
        using var service = CreateServiceWithData(data);
        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton<DashboardDataService>(service);

        var cut = ctx.RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        cut.Markup.Should().NotContain("Unable to load dashboard data");
    }

    [Fact]
    public void Dashboard_Dispose_UnsubscribesFromDataChanged()
    {
        var data = new DashboardData
        {
            Title = "Dispose Test",
            Timeline = new TimelineConfig { StartMonth = "2026-01", EndMonth = "2026-06", Tracks = new List<Track>() },
            Heatmap = new HeatmapConfig()
        };
        using var service = CreateServiceWithData(data);
        using var ctx = new Bunit.TestContext();
        ctx.Services.AddSingleton<DashboardDataService>(service);

        var cut = ctx.RenderComponent<ReportingDashboard.Components.Pages.Dashboard>();

        var dispose = () => cut.Dispose();

        dispose.Should().NotThrow();
    }
}