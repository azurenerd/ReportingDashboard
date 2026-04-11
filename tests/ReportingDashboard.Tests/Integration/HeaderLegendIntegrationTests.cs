using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using ReportingDashboard.Tests.Integration.Helpers;
using System.Text.Json;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests verifying that the Header component's timeline legend
/// renders correctly when wired up with DashboardDataService (real file → service → component).
/// </summary>
[Trait("Category", "Integration")]
public class HeaderLegendIntegrationTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    public HeaderLegendIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"HeaderLegendInteg_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public new void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
            base.Dispose();
        }
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private string WriteDataJson(object data)
    {
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, JsonSerializer.Serialize(data, JsonOpts));
        return path;
    }

    private object CreateFullData(string currentMonth = "Apr 2026") => new
    {
        title = "Integration Dashboard",
        subtitle = "QA Team - April 2026",
        backlogLink = "https://dev.azure.com/test/backlog",
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
                    label = "Core Platform",
                    color = "#4285F4",
                    milestones = new[]
                    {
                        new { date = "2026-02-15", type = "poc", label = "Feb 15" },
                        new { date = "2026-05-01", type = "production", label = "May 1" }
                    }
                },
                new
                {
                    name = "M2",
                    label = "Data Pipeline",
                    color = "#EA4335",
                    milestones = new[]
                    {
                        new { date = "2026-03-01", type = "checkpoint", label = "Mar 1" }
                    }
                }
            }
        },
        heatmap = new
        {
            shipped = new Dictionary<string, string[]>
            {
                ["jan"] = new[] { "Feature A", "Feature B" },
                ["feb"] = new[] { "Feature C" }
            },
            inProgress = new Dictionary<string, string[]>
            {
                ["apr"] = new[] { "Feature D", "Feature E" }
            },
            carryover = new Dictionary<string, string[]>
            {
                ["mar"] = new[] { "Legacy Item" }
            },
            blockers = new Dictionary<string, string[]>
            {
                ["apr"] = new[] { "Dependency X" }
            }
        }
    };

    #region End-to-End: File → Service → Header Component (inline-styled variant)

    [Fact]
    public async Task HeaderLegend_FromRealDataFile_RendersAllFourLegendLabels()
    {
        var path = WriteDataJson(CreateFullData("Apr 2026"));
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        svc.IsError.Should().BeFalse();
        svc.Data.Should().NotBeNull();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, svc.Data!));

        cut.Markup.Should().Contain("PoC Milestone");
        cut.Markup.Should().Contain("Production Release");
        cut.Markup.Should().Contain("Checkpoint");
        cut.Markup.Should().Contain("Now (Apr 2026)");
    }

    [Fact]
    public async Task HeaderLegend_FromRealDataFile_RendersAllFourSymbolColors()
    {
        var path = WriteDataJson(CreateFullData());
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, svc.Data!));

        cut.Markup.Should().Contain("#F4B400", "gold PoC diamond");
        cut.Markup.Should().Contain("#34A853", "green production diamond");
        cut.Markup.Should().Contain("#999", "gray checkpoint circle");
        cut.Markup.Should().Contain("#EA4335", "red now bar");
    }

    [Fact]
    public async Task HeaderLegend_FromRealDataFile_TitleAndLegendCoexist()
    {
        var path = WriteDataJson(CreateFullData());
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, svc.Data!));

        // Title side
        cut.Find("h1").TextContent.Should().Contain("Integration Dashboard");
        cut.Find(".sub").TextContent.Should().Contain("QA Team - April 2026");

        // Legend side
        cut.Markup.Should().Contain("PoC Milestone");
        cut.Markup.Should().Contain("Now (Apr 2026)");
    }

    #endregion

    #region CurrentMonth data-driven binding through the full pipeline

    [Theory]
    [InlineData("Jan 2026")]
    [InlineData("Jun 2026")]
    [InlineData("September 2026")]
    [InlineData("December 2027")]
    public async Task HeaderLegend_DifferentCurrentMonths_NowLabelReflectsValue(string currentMonth)
    {
        var path = WriteDataJson(CreateFullData(currentMonth));
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, svc.Data!));

        cut.Markup.Should().Contain($"Now ({currentMonth})");
    }

    [Fact]
    public async Task HeaderLegend_EmptyCurrentMonth_NowLabelShowsEmptyParens()
    {
        var data = CreateFullData("");
        var path = WriteDataJson(data);
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        // Service may fail validation for empty currentMonth; if it succeeds, verify rendering
        if (!svc.IsError && svc.Data != null)
        {
            var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
                p.Add(x => x.Data, svc.Data));

            cut.Markup.Should().Contain("Now ()");
        }
    }

    #endregion

    #region File update → re-load → re-render simulating data.json change

    [Fact]
    public async Task HeaderLegend_AfterFileUpdate_NewCurrentMonthReflected()
    {
        // Initial load
        var path = WriteDataJson(CreateFullData("Mar 2026"));
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, svc.Data!));
        cut.Markup.Should().Contain("Now (Mar 2026)");

        // Simulate data.json edit
        WriteDataJson(CreateFullData("Jul 2026"));
        await svc.LoadAsync(path);

        svc.IsError.Should().BeFalse();

        // Re-render with updated data
        cut.SetParametersAndRender(p => p.Add(x => x.Data, svc.Data!));

        cut.Markup.Should().Contain("Now (Jul 2026)");
        cut.Markup.Should().NotContain("Now (Mar 2026)");
    }

    [Fact]
    public async Task HeaderLegend_AfterFileUpdate_TitleChangesReflected()
    {
        var path = WriteDataJson(CreateFullData());
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, svc.Data!));
        cut.Find("h1").TextContent.Should().Contain("Integration Dashboard");

        // Update with different title
        var updatedData = new
        {
            title = "Updated Dashboard Title",
            subtitle = "New Team - May 2026",
            backlogLink = "https://dev.azure.com/new",
            currentMonth = "May 2026",
            months = new[] { "January", "February", "March", "April", "May" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-08-01",
                nowDate = "2026-05-10",
                tracks = new[]
                {
                    new
                    {
                        name = "M1",
                        label = "Core",
                        color = "#4285F4",
                        milestones = new[] { new { date = "2026-02-15", type = "poc", label = "Feb 15" } }
                    }
                }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        };
        File.WriteAllText(path, JsonSerializer.Serialize(updatedData, JsonOpts));
        await svc.LoadAsync(path);

        cut.SetParametersAndRender(p => p.Add(x => x.Data, svc.Data!));

        cut.Find("h1").TextContent.Should().Contain("Updated Dashboard Title");
        cut.Markup.Should().Contain("Now (May 2026)");
    }

    #endregion

    #region Header + Legend structural integration (inline-style variant)

    [Fact]
    public async Task HeaderLegend_FromRealData_LegendHasFlexLayout()
    {
        var path = WriteDataJson(CreateFullData());
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, svc.Data!));

        var hdr = cut.Find(".hdr");
        var childDivs = hdr.Children.Where(c => c.TagName == "DIV").ToList();
        childDivs.Should().HaveCount(2, "header has title div and legend div");

        var legendDiv = childDivs.Last();
        var style = legendDiv.GetAttribute("style") ?? "";
        style.Should().Contain("display:flex");
        style.Should().Contain("gap:22px");
        style.Should().Contain("font-size:12px");
    }

    [Fact]
    public async Task HeaderLegend_FromRealData_HasExactlyFourLegendItems()
    {
        var path = WriteDataJson(CreateFullData());
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, svc.Data!));

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var legendSpans = legendDiv.Children.Where(c => c.TagName == "SPAN").ToList();
        legendSpans.Should().HaveCount(4);
    }

    [Fact]
    public async Task HeaderLegend_FromRealData_DiamondSymbolsAreRotated()
    {
        var path = WriteDataJson(CreateFullData());
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, svc.Data!));

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var items = legendDiv.Children.Where(c => c.TagName == "SPAN").ToList();

        // PoC diamond
        var pocSymbol = items[0].Children.First();
        pocSymbol.GetAttribute("style").Should().Contain("rotate(45deg)");
        pocSymbol.GetAttribute("style").Should().Contain("#F4B400");

        // Production diamond
        var prodSymbol = items[1].Children.First();
        prodSymbol.GetAttribute("style").Should().Contain("rotate(45deg)");
        prodSymbol.GetAttribute("style").Should().Contain("#34A853");
    }

    [Fact]
    public async Task HeaderLegend_FromRealData_CheckpointIsCircle()
    {
        var path = WriteDataJson(CreateFullData());
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, svc.Data!));

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var items = legendDiv.Children.Where(c => c.TagName == "SPAN").ToList();

        var circleSymbol = items[2].Children.First();
        var style = circleSymbol.GetAttribute("style") ?? "";
        style.Should().Contain("border-radius:50%");
        style.Should().Contain("width:8px");
        style.Should().Contain("height:8px");
        style.Should().NotContain("rotate");
    }

    [Fact]
    public async Task HeaderLegend_FromRealData_NowBarCorrectDimensions()
    {
        var path = WriteDataJson(CreateFullData());
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, svc.Data!));

        var hdr = cut.Find(".hdr");
        var legendDiv = hdr.Children.Where(c => c.TagName == "DIV").Last();
        var items = legendDiv.Children.Where(c => c.TagName == "SPAN").ToList();

        var barSymbol = items[3].Children.First();
        var style = barSymbol.GetAttribute("style") ?? "";
        style.Should().Contain("width:2px");
        style.Should().Contain("height:14px");
        style.Should().Contain("#EA4335");
    }

    #endregion

    #region Backlog link integration with legend

    [Fact]
    public async Task Header_BacklogLinkAndLegend_BothRenderFromSameDataSource()
    {
        var path = WriteDataJson(CreateFullData());
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, svc.Data!));

        // Backlog link from data
        var link = cut.Find("a");
        link.GetAttribute("href").Should().Be("https://dev.azure.com/test/backlog");
        link.GetAttribute("target").Should().Be("_blank");
        link.TextContent.Should().Contain("ADO Backlog");

        // Legend also present
        cut.Markup.Should().Contain("PoC Milestone");
        cut.Markup.Should().Contain("Now (Apr 2026)");
    }

    #endregion

    #region Data model integrity through service pipeline to component

    [Fact]
    public async Task HeaderLegend_FullPipeline_CurrentMonthMatchesDataJsonValue()
    {
        var expectedMonth = "February 2027";
        var path = WriteDataJson(CreateFullData(expectedMonth));
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        // Verify service level
        svc.Data!.CurrentMonth.Should().Be(expectedMonth);

        // Verify component level
        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, svc.Data));

        cut.Markup.Should().Contain($"Now ({expectedMonth})");
    }

    [Fact]
    public async Task HeaderLegend_FullPipeline_AllHeaderFieldsFromSameLoad()
    {
        var path = WriteDataJson(CreateFullData("Apr 2026"));
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        var data = svc.Data!;
        data.Title.Should().Be("Integration Dashboard");
        data.Subtitle.Should().Be("QA Team - April 2026");
        data.BacklogLink.Should().Be("https://dev.azure.com/test/backlog");
        data.CurrentMonth.Should().Be("Apr 2026");

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, data));

        cut.Find("h1").TextContent.Should().Contain("Integration Dashboard");
        cut.Find(".sub").TextContent.Should().Contain("QA Team - April 2026");
        cut.Find("a").GetAttribute("href").Should().Be("https://dev.azure.com/test/backlog");
        cut.Markup.Should().Contain("Now (Apr 2026)");
    }

    #endregion

    #region Error recovery: service error → valid load → legend renders

    [Fact]
    public async Task HeaderLegend_AfterServiceRecoveryFromError_RendersCorrectly()
    {
        var svc = new DashboardDataService(_logger);

        // First: load from nonexistent file → error
        await svc.LoadAsync(Path.Combine(_tempDir, "nonexistent.json"));
        svc.IsError.Should().BeTrue();

        // Second: load valid file → recovery
        var path = WriteDataJson(CreateFullData("Oct 2026"));
        await svc.LoadAsync(path);
        svc.IsError.Should().BeFalse();
        svc.Data.Should().NotBeNull();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, svc.Data!));

        cut.Markup.Should().Contain("PoC Milestone");
        cut.Markup.Should().Contain("Production Release");
        cut.Markup.Should().Contain("Checkpoint");
        cut.Markup.Should().Contain("Now (Oct 2026)");
        cut.Find("h1").TextContent.Should().Contain("Integration Dashboard");
    }

    #endregion

    #region Multiple sequential loads with different currentMonth values

    [Fact]
    public async Task HeaderLegend_ThreeSequentialLoads_AlwaysReflectsLatestMonth()
    {
        var svc = new DashboardDataService(_logger);
        var months = new[] { "Jan 2026", "May 2026", "Dec 2026" };

        foreach (var month in months)
        {
            var path = WriteDataJson(CreateFullData(month));
            await svc.LoadAsync(path);
            svc.IsError.Should().BeFalse($"should load successfully for month '{month}'");

            var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
                p.Add(x => x.Data, svc.Data!));

            cut.Markup.Should().Contain($"Now ({month})");

            // Other legend items remain stable
            cut.Markup.Should().Contain("PoC Milestone");
            cut.Markup.Should().Contain("Production Release");
            cut.Markup.Should().Contain("Checkpoint");
        }
    }

    #endregion

    #region Special characters in data flowing through the pipeline

    [Fact]
    public async Task HeaderLegend_SpecialCharsInTitle_RendersSafelyAlongsideLegend()
    {
        var data = new
        {
            title = "Project <Alpha> & \"Beta\"",
            subtitle = "Team — Q2 2026",
            backlogLink = "https://dev.azure.com/test?foo=bar&baz=1",
            currentMonth = "Apr 2026",
            months = new[] { "April" },
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
                        milestones = new[] { new { date = "2026-02-15", type = "poc", label = "Feb" } }
                    }
                }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
                inProgress = new Dictionary<string, string[]>(),
                carryover = new Dictionary<string, string[]>(),
                blockers = new Dictionary<string, string[]>()
            }
        };

        var path = WriteDataJson(data);
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);
        svc.IsError.Should().BeFalse();

        var cut = RenderComponent<ReportingDashboard.Components.Header>(p =>
            p.Add(x => x.Data, svc.Data!));

        // Title renders with special chars (HTML-encoded by Blazor)
        cut.Find("h1").TextContent.Should().Contain("Project <Alpha> & \"Beta\"");

        // Legend still intact
        cut.Markup.Should().Contain("PoC Milestone");
        cut.Markup.Should().Contain("Now (Apr 2026)");
    }

    #endregion
}