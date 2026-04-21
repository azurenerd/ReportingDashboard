using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardPageTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;

    public DashboardPageTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _page = await _fixture.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        _page.SetDefaultTimeout(60000);
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DashboardLoads_ShowsHeader()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var h1 = _page.Locator("h1");
        var text = await h1.TextContentAsync();
        text.Should().NotBeNullOrWhiteSpace();
        text.Should().Contain("ADO Backlog");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DashboardLoads_ShowsLegend()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var legend = _page.Locator(".legend");
        (await legend.IsVisibleAsync()).Should().BeTrue();

        var pocText = _page.GetByText("PoC Milestone");
        (await pocText.CountAsync()).Should().BeGreaterThan(0);

        var prodText = _page.GetByText("Production Release");
        (await prodText.CountAsync()).Should().BeGreaterThan(0);

        var checkpointText = _page.GetByText("Checkpoint");
        (await checkpointText.CountAsync()).Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DashboardLoads_ShowsTimelineSvg()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var svg = _page.Locator(".tl-svg-box svg");
        (await svg.IsVisibleAsync()).Should().BeTrue();

        var width = await svg.GetAttributeAsync("width");
        width.Should().Be("1560");

        var height = await svg.GetAttributeAsync("height");
        height.Should().Be("185");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DashboardLoads_ShowsHeatmapGrid()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var heatmapTitle = _page.Locator(".hm-title");
        (await heatmapTitle.IsVisibleAsync()).Should().BeTrue();
        var titleText = await heatmapTitle.TextContentAsync();
        titleText.Should().Contain("Heatmap");

        var grid = _page.Locator(".hm-grid");
        (await grid.IsVisibleAsync()).Should().BeTrue();

        var corner = _page.Locator(".hm-corner");
        var cornerText = await corner.TextContentAsync();
        cornerText.Should().Contain("Status");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DashboardLoads_PageFitsViewport_NoScrollbars()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var scrollWidth = await _page.EvaluateAsync<int>("document.documentElement.scrollWidth");
        var scrollHeight = await _page.EvaluateAsync<int>("document.documentElement.scrollHeight");

        scrollWidth.Should().BeLessOrEqualTo(1920);
        scrollHeight.Should().BeLessOrEqualTo(1080);
    }
}