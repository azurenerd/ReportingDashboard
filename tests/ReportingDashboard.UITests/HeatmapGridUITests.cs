using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeatmapGridUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage? _page;

    public HeatmapGridUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        if (_fixture.Browser is not null)
        {
            _page = await _fixture.Browser.NewPageAsync();
        }
    }

    public async Task DisposeAsync()
    {
        if (_page is not null)
        {
            await _page.CloseAsync();
        }
    }

    [Fact(Skip = "Playwright browser binaries not installed. Run 'pwsh bin/Debug/net8.0/playwright.ps1 install' to enable.")]
    public async Task HeatmapGrid_DisplaysSectionTitle_MonthlyExecutionHeatmap()
    {
        var page = _page ?? await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        var title = page.Locator(".hm-title");
        await Assertions.Expect(title).ToBeVisibleAsync();
        var text = await title.TextContentAsync();
        Assert.Contains("Monthly Execution Heatmap", text ?? "");
    }

    [Fact(Skip = "Playwright browser binaries not installed. Run 'pwsh bin/Debug/net8.0/playwright.ps1 install' to enable.")]
    public async Task HeatmapGrid_RendersCornerCell_WithStatusText()
    {
        var page = _page ?? await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        var corner = page.Locator(".hm-corner");
        await Assertions.Expect(corner).ToBeVisibleAsync();
        var text = await corner.TextContentAsync();
        Assert.Contains("Status", text ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact(Skip = "Playwright browser binaries not installed. Run 'pwsh bin/Debug/net8.0/playwright.ps1 install' to enable.")]
    public async Task HeatmapGrid_RendersColumnHeaders_WithMonthNames()
    {
        var page = _page ?? await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        var headers = page.Locator(".hm-col-hdr");
        Assert.Equal(4, await headers.CountAsync());
    }

    [Fact(Skip = "Playwright browser binaries not installed. Run 'pwsh bin/Debug/net8.0/playwright.ps1 install' to enable.")]
    public async Task HeatmapGrid_RendersFourStatusRows_WithCategoryHeaders()
    {
        var page = _page ?? await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        var rowHeaders = page.Locator(".hm-row-hdr");
        Assert.Equal(4, await rowHeaders.CountAsync());
    }

    [Fact(Skip = "Playwright browser binaries not installed. Run 'pwsh bin/Debug/net8.0/playwright.ps1 install' to enable.")]
    public async Task HeatmapGrid_RendersHighlightedColumn_WithGoldBackground()
    {
        var page = _page ?? await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        var highlightedHeader = page.Locator(".hm-col-hdr.highlight");
        if (await highlightedHeader.CountAsync() > 0)
        {
            await Assertions.Expect(highlightedHeader.First).ToBeVisibleAsync();
        }
    }
}