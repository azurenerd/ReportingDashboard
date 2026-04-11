using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeatmapCellStructureTests
{
    private readonly PlaywrightFixture _fixture;

    public HeatmapCellStructureTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task HeatmapCells_HaveCorrectPrefixClasses()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            Assert.True(await page.Locator(".ship-cell").CountAsync() > 0, "Expected ship-cell classes");
            Assert.True(await page.Locator(".prog-cell").CountAsync() > 0, "Expected prog-cell classes");
            Assert.True(await page.Locator(".carry-cell").CountAsync() > 0, "Expected carry-cell classes");
            Assert.True(await page.Locator(".block-cell").CountAsync() > 0, "Expected block-cell classes");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(HeatmapCells_HaveCorrectPrefixClasses));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task HeatmapCells_CurrentMonthHasAprClass()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var aprCells = page.Locator(".hm-cell.apr");
            var count = await aprCells.CountAsync();
            Assert.Equal(4, count); // one per row
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(HeatmapCells_CurrentMonthHasAprClass));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task HeatmapCells_EmptyCellsShowDash()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var empties = page.Locator(".hm-empty");
            var count = await empties.CountAsync();
            Assert.True(count > 0, "Expected at least one empty cell with dash");

            var firstText = await empties.First.TextContentAsync();
            Assert.Equal("-", firstText?.Trim());
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(HeatmapCells_EmptyCellsShowDash));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task HeatmapCells_ItemsHaveItClass()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var items = page.Locator(".hm-cell .it");
            var count = await items.CountAsync();
            Assert.True(count > 0, "Expected at least one .it item in heatmap");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(HeatmapCells_ItemsHaveItClass));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task HeatmapGrid_HasCorrectCssGridLayout()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var grid = page.Locator(".hm-grid");
            var style = await grid.GetAttributeAsync("style") ?? "";
            Assert.Contains("160px", style);
            Assert.Contains("repeat(", style);
            Assert.Contains("1fr", style);
            Assert.Contains("36px", style);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(HeatmapGrid_HasCorrectCssGridLayout));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task HeatmapColumnHeader_CurrentMonth_HasAprHdrClass()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var aprHdr = page.Locator(".hm-col-hdr.apr-hdr");
            Assert.Equal(1, await aprHdr.CountAsync());
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(HeatmapColumnHeader_CurrentMonth_HasAprHdrClass));
            throw;
        }
    }
}