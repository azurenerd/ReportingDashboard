using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Components.Pages;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests verifying the full DOM structure of Header.razor when
/// rendered through Dashboard.razor with real service data.
/// Ensures the component hierarchy (div.hdr > h1.hdr-title + div.sub + div.legend)
/// is correctly produced end-to-end.
/// </summary>
[Trait("Category", "Integration")]
public class HeaderStructureDashboardIntegrationTests : TestContext, IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;
    private bool _disposed;

    public HeaderStructureDashboardIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"HdrStruct_{Guid.NewGuid():N}");
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

    private DashboardDataService CreateService()
    {
        var json = System.Text.Json.JsonSerializer.Serialize(new
        {
            title = "Structure Test",
            subtitle = "Team Structure - April 2026",
            backlogLink = "https://dev.azure.com/org/project",
            currentMonth = "April",
            months = new[] { "January", "February", "March", "April" },
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-01",
                nowDate = "2026-04-10",
                tracks = new[]
                {
                    new { name = "M1", label = "Core", color = "#4285F4", milestones = Array.Empty<object>() }
                }
            },
            heatmap = new
            {
                shipped = new Dictionary<string, string[]>(),
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

    #region Header Container Structure

    [Fact]
    public void Dashboard_HasHdrContainer()
    {
        var svc = CreateService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var hdr = cut.Find(".hdr");
        Assert.NotNull(hdr);
        Assert.Equal("DIV", hdr.TagName);
    }

    [Fact]
    public void Dashboard_HdrContainsH1WithHdrTitleClass()
    {
        var svc = CreateService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var h1 = cut.Find("h1.hdr-title");
        Assert.NotNull(h1);
    }

    [Fact]
    public void Dashboard_HdrContainsSubDiv()
    {
        var svc = CreateService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var hdr = cut.Find(".hdr");
        var sub = hdr.QuerySelector(".sub");
        Assert.NotNull(sub);
        Assert.Equal("DIV", sub!.TagName);
    }

    [Fact]
    public void Dashboard_HdrContainsLegendDiv()
    {
        var svc = CreateService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var hdr = cut.Find(".hdr");
        var legend = hdr.QuerySelector(".legend");
        Assert.NotNull(legend);
        Assert.Equal("DIV", legend!.TagName);
    }

    #endregion

    #region Full Structure Consistency

    [Fact]
    public void Dashboard_HeaderStructure_TitleSubtitleLegend_AllPresent()
    {
        var svc = CreateService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        // All three child elements of .hdr should be present
        Assert.NotNull(cut.Find(".hdr h1.hdr-title"));
        Assert.NotNull(cut.Find(".hdr .sub"));
        Assert.NotNull(cut.Find(".hdr .legend"));
    }

    [Fact]
    public void Dashboard_Header_TitleContainsCorrectText()
    {
        var svc = CreateService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var h1 = cut.Find("h1.hdr-title");
        Assert.Contains("Structure Test", h1.TextContent);
    }

    [Fact]
    public void Dashboard_Header_SubtitleContainsCorrectText()
    {
        var svc = CreateService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var sub = cut.Find(".sub");
        Assert.Equal("Team Structure - April 2026", sub.TextContent);
    }

    [Fact]
    public void Dashboard_Header_LinkIsInsideH1()
    {
        var svc = CreateService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        var h1 = cut.Find("h1.hdr-title");
        var link = h1.QuerySelector("a.hdr-link");
        Assert.NotNull(link);
        Assert.Equal("https://dev.azure.com/org/project", link!.GetAttribute("href"));
    }

    [Fact]
    public void Dashboard_Header_NowLabelIncludesYearFromNowDate()
    {
        var svc = CreateService();
        Services.AddSingleton(svc);

        var cut = RenderComponent<Dashboard>();

        Assert.Contains("Now (April 2026)", cut.Markup);
    }

    #endregion

    #region Multiple Renders Produce Consistent Structure

    [Fact]
    public void Dashboard_MultipleRenders_ProduceSameStructure()
    {
        var svc = CreateService();
        Services.AddSingleton(svc);

        var cut1 = RenderComponent<Dashboard>();
        var markup1 = cut1.Markup;

        var cut2 = RenderComponent<Dashboard>();
        var markup2 = cut2.Markup;

        Assert.Equal(markup1, markup2);
    }

    #endregion
}