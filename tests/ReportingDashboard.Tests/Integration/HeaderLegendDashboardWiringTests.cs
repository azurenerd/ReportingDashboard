using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests verifying the legend section of Header.razor is correctly
/// wired through Dashboard.razor with real DashboardDataService data.
/// Existing HeaderComponentIntegrationTests focus on title/subtitle/link data flow.
/// These tests specifically target legend rendering, CSS class structure, and
/// symbol count when rendered through the full Dashboard → Header component chain.
/// </summary>
[Trait("Category", "Integration")]
public class HeaderLegendDashboardWiringTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    public HeaderLegendDashboardWiringTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"LegendWire_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public new void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            base.Dispose();
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
    }

    private DashboardDataService CreateServiceWithData(string currentMonth = "April")
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "Legend Wiring Test",
            subtitle = "Team - Integration",
            backlogLink = "https://dev.azure.com/test",
            currentMonth,
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
                            new { date = "2026-02-15", type = "poc", label = "Feb PoC" },
                            new { date = "2026-04-01", type = "prod", label = "Apr Prod" }
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

        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        File.WriteAllText(path, json);
        var svc = new DashboardDataService(_logger);
        svc.LoadAsync(path).GetAwaiter().GetResult();
        return svc;
    }

    #region Legend Container Through Dashboard

    [Fact]
    public void Dashboard_RendersLegendContainer_WithCorrectClass()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var legend = cut.Find(".legend");
        Assert.NotNull(legend);
    }

    [Fact]
    public void Dashboard_LegendHasExactlyFourItems()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var items = cut.FindAll(".legend-item");
        Assert.Equal(4, items.Count);
    }

    #endregion

    #region Legend Label Text Through Dashboard

    [Fact]
    public void Dashboard_LegendContainsPocMilestoneLabel()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("PoC Milestone", cut.Markup);
    }

    [Fact]
    public void Dashboard_LegendContainsProductionReleaseLabel()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Production Release", cut.Markup);
    }

    [Fact]
    public void Dashboard_LegendContainsCheckpointLabel()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Checkpoint", cut.Markup);
    }

    [Fact]
    public void Dashboard_LegendNowLabel_IncludesCurrentMonthFromData()
    {
        var svc = CreateServiceWithData(currentMonth: "July");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Now (July 2026)", cut.Markup);
    }

    [Fact]
    public void Dashboard_LegendNowLabel_ChangesByMonth()
    {
        var svc = CreateServiceWithData(currentMonth: "November");
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Now (November 2026)", cut.Markup);
        Assert.DoesNotContain("Now (April", cut.Markup);
    }

    #endregion

    #region Legend Symbol CSS Classes Through Dashboard

    [Fact]
    public void Dashboard_LegendHasTwoDiamondSymbols()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var diamonds = cut.FindAll(".legend-diamond");
        Assert.Equal(2, diamonds.Count);
    }

    [Fact]
    public void Dashboard_LegendHasPocDiamondClass()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var poc = cut.Find(".legend-diamond.legend-poc");
        Assert.NotNull(poc);
    }

    [Fact]
    public void Dashboard_LegendHasProdDiamondClass()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var prod = cut.Find(".legend-diamond.legend-prod");
        Assert.NotNull(prod);
    }

    [Fact]
    public void Dashboard_LegendHasCircleSymbol()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var circle = cut.Find(".legend-circle");
        Assert.NotNull(circle);
    }

    [Fact]
    public void Dashboard_LegendHasNowLineSymbol()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var nowLine = cut.Find(".legend-now-line");
        Assert.NotNull(nowLine);
    }

    #endregion

    #region Legend Items Are Span Elements

    [Fact]
    public void Dashboard_LegendItems_AreSpanElements()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var items = cut.FindAll(".legend-item");
        foreach (var item in items)
        {
            Assert.Equal("SPAN", item.TagName);
        }
    }

    [Fact]
    public void Dashboard_LegendSymbols_AreNestedSpanElements()
    {
        var svc = CreateServiceWithData();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var items = cut.FindAll(".legend-item");
        foreach (var item in items)
        {
            var innerSpan = item.QuerySelector("span");
            Assert.NotNull(innerSpan);
        }
    }

    #endregion
}