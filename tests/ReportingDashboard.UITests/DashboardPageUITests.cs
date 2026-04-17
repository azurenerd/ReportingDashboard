using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class DashboardPageUITests : IClassFixture<PlaywrightFixture>, IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;
    private IBrowserContext _context = null!;

    public DashboardPageUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        if (!_fixture.ServerAvailable)
            return;

        _context = await _fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        _page = await _context.NewPageAsync();
        _page.SetDefaultTimeout(60000);
        await _page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 60000
        });
    }

    public async Task DisposeAsync()
    {
        if (_page is not null) await _page.CloseAsync();
        if (_context is not null) await _context.DisposeAsync();
    }

    [Fact]
    public async Task DashboardRendersAllThreeSections()
    {
        _fixture.EnsureServerAvailable();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hdr = await _page.QuerySelectorAsync("div.hdr");
        Assert.NotNull(hdr);

        var tlArea = await _page.QuerySelectorAsync("div.tl-area");
        Assert.NotNull(tlArea);

        var hmWrap = await _page.QuerySelectorAsync("div.hm-wrap");
        Assert.NotNull(hmWrap);
    }

    [Fact]
    public async Task TimelineSectionHasSvgWithCorrectDimensions()
    {
        _fixture.EnsureServerAvailable();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var svg = await _page.QuerySelectorAsync("div.tl-area svg");
        Assert.NotNull(svg);

        var width = await svg.GetAttributeAsync("width");
        var height = await svg.GetAttributeAsync("height");
        Assert.Equal("1560", width);
        Assert.Equal("185", height);
    }

    [Fact]
    public async Task HeatmapGridRendersWithRowsAndColumns()
    {
        _fixture.EnsureServerAvailable();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var corner = await _page.QuerySelectorAsync("div.hm-grid div.hm-corner");
        Assert.NotNull(corner);

        var colHeaders = await _page.QuerySelectorAllAsync("div.hm-grid div.hm-col-hdr");
        Assert.True(colHeaders.Count >= 1, "Heatmap should have at least one month column");

        var rowHeaders = await _page.QuerySelectorAllAsync("div.hm-grid div.hm-row-hdr");
        Assert.True(rowHeaders.Count >= 1, "Heatmap should have at least one status row");
    }

    [Fact]
    public async Task FullPageFitsWithin1920x1080_NoScrollbars()
    {
        _fixture.EnsureServerAvailable();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hasOverflow = await _page.EvaluateAsync<bool>(
            "() => document.body.scrollWidth > 1920 || document.body.scrollHeight > 1080");
        Assert.False(hasOverflow, "Page should fit within 1920x1080 with no scrollbars");
    }

    [Fact]
    public async Task NoErrorStateDisplayedWithValidData()
    {
        _fixture.EnsureServerAvailable();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorDiv = await _page.QuerySelectorAsync("div.error-state");
        Assert.Null(errorDiv);

        var hdr = await _page.QuerySelectorAsync("div.hdr");
        Assert.NotNull(hdr);
    }
}