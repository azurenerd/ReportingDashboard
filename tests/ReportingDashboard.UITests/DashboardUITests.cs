using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;

    public DashboardUITests(PlaywrightFixture fixture)
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
    public async Task Header_DisplaysTitleAndLegend()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = _page.Locator(".hdr");
        await header.WaitForAsync();

        var h1 = _page.Locator("h1");
        var h1Text = await h1.TextContentAsync();
        h1Text.Should().NotBeNullOrWhiteSpace();

        var legendItems = _page.Locator(".legend-item");
        var count = await legendItems.CountAsync();
        count.Should().Be(4, "header should have 4 legend items (PoC, Production, Checkpoint, Now)");
    }

    [Fact]
    public async Task Timeline_RendersWithSvgAndTracks()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tlArea = _page.Locator(".tl-area");
        await tlArea.WaitForAsync();

        var svg = _page.Locator(".tl-svg-box svg");
        (await svg.CountAsync()).Should().BeGreaterThan(0, "timeline should contain an SVG element");

        var trackLabels = _page.Locator(".tl-track-label");
        (await trackLabels.CountAsync()).Should().BeGreaterThan(0, "timeline should display track labels");
    }

    [Fact]
    public async Task Heatmap_RendersGridWithMonthColumns()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hmGrid = _page.Locator(".hm-grid");
        await hmGrid.WaitForAsync();

        var title = _page.Locator(".hm-title");
        var titleText = await title.TextContentAsync();
        titleText.Should().NotBeNull();
        titleText!.ToUpperInvariant().Should().Contain("HEATMAP");

        var colHeaders = _page.Locator(".hm-col-hdr");
        (await colHeaders.CountAsync()).Should().BeGreaterOrEqualTo(3, "heatmap should have at least 3 month columns");
    }

    [Fact]
    public async Task Heatmap_HighlightedMonthHasSpecialStyle()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var highlighted = _page.Locator(".hm-col-hdr.apr-hdr");
        var highlightCount = await highlighted.CountAsync();
        highlightCount.Should().BeGreaterOrEqualTo(1, "at least one month column should be highlighted");

        var text = await highlighted.First.TextContentAsync();
        text.Should().Contain("Now", "highlighted month should show 'Now' indicator");
    }

    [Fact]
    public async Task Dashboard_PageRendersWithoutErrors()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify no error panel is shown when data loads correctly
        var errorPanels = _page.Locator(".error-panel");
        (await errorPanels.CountAsync()).Should().Be(0, "dashboard should not show error panel with valid data");

        // Verify all three major sections are present
        (await _page.Locator(".hdr").CountAsync()).Should().Be(1);
        (await _page.Locator(".tl-area").CountAsync()).Should().Be(1);
        (await _page.Locator(".hm-wrap").CountAsync()).Should().Be(1);
    }
}