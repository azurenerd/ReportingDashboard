using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using System.Text.Json;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests for the Dashboard page component rendering pipeline:
/// DashboardDataService → Dashboard.razor → child components (ErrorPanel, Header, stubs).
/// Tests the full render tree with real service instances.
/// </summary>
[Trait("Category", "Integration")]
public class DashboardRenderingIntegrationTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public DashboardRenderingIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashRenderInteg_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public new void Dispose()
    {
        if (!_disposed)
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
            _disposed = true;
        }
        base.Dispose();
    }

    private DashboardDataService LoadService(string json)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        return svc;
    }

    private static string CreateValidJson(
        string title = "Integration Dashboard",
        int trackCount = 2,
        int monthCount = 4)
    {
        var months = new[] { "January", "February", "March", "April", "May", "June" }
            .Take(monthCount).ToArray();

        var tracks = Enumerable.Range(1, trackCount).Select(i => new
        {
            name = $"M{i}",
            label = $"Workstream {i}",
            color = $"#{i * 444444 % 0xFFFFFF:X6}",
            milestones = new[]
            {
                new { date = "2026-02-15", type = "poc", label = $"M{i} PoC" },
                new { date = "2026-05-01", type = "production", label = $"M{i} GA" }
            }
        }).ToArray();

        return JsonSerializer.Serialize(new
        {
            title,
            subtitle = "QA Team - April 2026",
            backlogLink = "https://dev.azure.com/test/backlog",
            currentMonth = "April",
            months,
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>
                {
                    ["jan"] = new[] { "Shipped Item A", "Shipped Item B" },
                    ["feb"] = new[] { "Shipped Item C" }
                },
                inProgress = new Dictionary<string, string[]>
                {
                    ["apr"] = new[] { "WIP Item D" }
                },
                carryover = new Dictionary<string, string[]>
                {
                    ["mar"] = new[] { "Carried Over E" }
                },
                blockers = new Dictionary<string, string[]>
                {
                    ["apr"] = new[] { "Blocker F" }
                }
            }
        }, JsonOpts);
    }

    #region ErrorPanel ↔ Dashboard integration

    [Fact]
    public void Dashboard_FileNotFound_ErrorPanelShowsPathInfo()
    {
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(Path.Combine(_tempDir, "totally_missing.json")).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("not found", cut.Markup);
        Assert.Contains("totally_missing.json", cut.Markup);
    }

    [Fact]
    public void Dashboard_MalformedJson_ErrorPanelShowsParseError()
    {
        var path = Path.Combine(_tempDir, "bad.json");
        File.WriteAllText(path, "{{{ invalid json");
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("parse", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Dashboard_ValidationFailed_ErrorPanelShowsFieldNames()
    {
        var path = Path.Combine(_tempDir, "invalid.json");
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            title = "",
            subtitle = "",
            months = Array.Empty<string>()
        }, JsonOpts));

        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("title", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Dashboard_UninitializedService_ShowsFallbackError()
    {
        var svc = new DashboardDataService(_logger);
        // Never call LoadAsync
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("error-panel", cut.Markup);
        Assert.Contains("Dashboard data is not available", cut.Markup);
    }

    #endregion

    #region Dashboard ↔ Header integration

    [Fact]
    public void Dashboard_ValidData_HeaderReceivesAllFields()
    {
        var svc = LoadService(CreateValidJson("Full Title Test"));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Full Title Test", cut.Markup);
        Assert.Contains("QA Team - April 2026", cut.Markup);
        Assert.Contains("https://dev.azure.com/test/backlog", cut.Markup);
    }

    [Fact]
    public void Dashboard_ValidData_HeaderShowsBacklogLink()
    {
        var svc = LoadService(CreateValidJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();
        var links = cut.FindAll("a[href='https://dev.azure.com/test/backlog']");

        Assert.NotEmpty(links);
    }

    #endregion

    #region Dashboard ↔ Section stubs integration

    [Fact]
    public void Dashboard_ValidData_TimelineAreaPresent()
    {
        var svc = LoadService(CreateValidJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("tl-area", cut.Markup);
    }

    [Fact]
    public void Dashboard_ValidData_HeatmapWrapPresent()
    {
        var svc = LoadService(CreateValidJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("hm-wrap", cut.Markup);
    }

    [Fact]
    public void Dashboard_ValidData_DoesNotShowErrorPanel()
    {
        var svc = LoadService(CreateValidJson());
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("error-panel", cut.Markup);
    }

    #endregion

    #region Data variations through full render

    [Fact]
    public void Dashboard_SingleTrack_RendersCorrectly()
    {
        var svc = LoadService(CreateValidJson(trackCount: 1));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("error-panel", cut.Markup);
        Assert.Contains("tl-area", cut.Markup);
    }

    [Fact]
    public void Dashboard_ManyTracks_RendersCorrectly()
    {
        var svc = LoadService(CreateValidJson(trackCount: 10));
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.DoesNotContain("error-panel", cut.Markup);
        Assert.Contains("tl-area", cut.Markup);
    }

    [Fact]
    public void Dashboard_SpecialCharactersInData_RendersEncoded()
    {
        var json = JsonSerializer.Serialize(new
        {
            title = "Privacy <Automation> & Release",
            subtitle = "Team \"X\" — April 2026",
            backlogLink = "https://link?a=1&b=2",
            currentMonth = "April",
            months = new[] { "January" },
            timeline = new
            {
                startDate = "2026-01-01", endDate = "2026-07-01", nowDate = "2026-04-10",
                tracks = new[] { new { name = "M1", label = "Core <Platform>", color = "#000", milestones = Array.Empty<object>() } }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        }, JsonOpts);

        var svc = LoadService(json);
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // HTML encoding should prevent raw < > in rendered output
        Assert.DoesNotContain("<Automation>", cut.Markup);
        Assert.Contains("&lt;Automation&gt;", cut.Markup);
        Assert.DoesNotContain("error-panel", cut.Markup);
    }

    #endregion
}