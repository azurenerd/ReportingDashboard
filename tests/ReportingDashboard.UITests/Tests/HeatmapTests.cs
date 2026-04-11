using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeatmapTests
{
    private readonly PlaywrightFixture _fixture;

    public HeatmapTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_IsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            await Assertions.Expect(dashboard.HeatmapWrap).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_IsVisible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_TitleContainsExpectedText()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var titleText = await dashboard.HeatmapTitle.TextContentAsync() ?? "";
            Assert.Contains("MONTHLY EXECUTION HEATMAP", titleText.ToUpperInvariant());
            Assert.Contains("SHIPPED", titleText.ToUpperInvariant());
            Assert.Contains("IN PROGRESS", titleText.ToUpperInvariant());
            Assert.Contains("CARRYOVER", titleText.ToUpperInvariant());
            Assert.Contains("BLOCKERS", titleText.ToUpperInvariant());
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_TitleContainsExpectedText));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_HasStatusCornerCell()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            await Assertions.Expect(dashboard.HeatmapCorner).ToBeVisibleAsync();
            var text = await dashboard.HeatmapCorner.TextContentAsync();
            Assert.Equal("STATUS", text?.Trim());
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_HasStatusCornerCell));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_HasMonthColumnHeaders()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var colCount = await dashboard.GetMonthColumnCountAsync();
            Assert.True(colCount > 0, "Expected at least one month column header");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_HasMonthColumnHeaders));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_CurrentMonthIsHighlighted()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var curHdrCount = await dashboard.HeatmapCurrentMonthHeader.CountAsync();
            Assert.Equal(1, curHdrCount);

            var text = await dashboard.HeatmapCurrentMonthHeader.TextContentAsync() ?? "";
            Assert.Contains("Now", text);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_CurrentMonthIsHighlighted));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_HasFourCategoryRows()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var rowCount = await dashboard.GetHeatmapRowCountAsync();
            Assert.Equal(4, rowCount);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_HasFourCategoryRows));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_RowHeaders_InCorrectOrder()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var headers = dashboard.HeatmapRowHeaders;
            Assert.Equal("SHIPPED", (await headers.Nth(0).TextContentAsync())?.Trim());
            Assert.Equal("IN PROGRESS", (await headers.Nth(1).TextContentAsync())?.Trim());
            Assert.Equal("CARRYOVER", (await headers.Nth(2).TextContentAsync())?.Trim());
            Assert.Equal("BLOCKERS", (await headers.Nth(3).TextContentAsync())?.Trim());
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_RowHeaders_InCorrectOrder));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_CategoryHeaders_HaveCorrectCssClasses()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            Assert.Equal(1, await dashboard.ShippedHeader.CountAsync());
            Assert.Equal(1, await dashboard.InProgressHeader.CountAsync());
            Assert.Equal(1, await dashboard.CarryoverHeader.CountAsync());
            Assert.Equal(1, await dashboard.BlockersHeader.CountAsync());
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_CategoryHeaders_HaveCorrectCssClasses));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_DataCells_HaveCategorySpecificClasses()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            Assert.True(await dashboard.ShippedCells.CountAsync() > 0, "Expected ship-cell elements");
            Assert.True(await dashboard.InProgressCells.CountAsync() > 0, "Expected prog-cell elements");
            Assert.True(await dashboard.CarryoverCells.CountAsync() > 0, "Expected carry-cell elements");
            Assert.True(await dashboard.BlockerCells.CountAsync() > 0, "Expected block-cell elements");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_DataCells_HaveCategorySpecificClasses));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_CurrentMonthCells_HaveCurClass()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var curCellCount = await dashboard.HeatmapCurrentCells.CountAsync();
            // 4 categories, each with one current-month cell
            Assert.Equal(4, curCellCount);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_CurrentMonthCells_HaveCurClass));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_CellsWithData_ShowItems()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var itemCount = await dashboard.HeatmapItems.CountAsync();
            Assert.True(itemCount > 0, "Expected at least one work item in heatmap cells");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_CellsWithData_ShowItems));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_EmptyCells_ShowDash()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var emptyCount = await dashboard.HeatmapEmptyCells.CountAsync();
            Assert.True(emptyCount > 0, "Expected at least one empty cell with dash");

            var firstEmptyText = await dashboard.HeatmapEmptyCells.First.TextContentAsync();
            Assert.Equal("-", firstEmptyText?.Trim());
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_EmptyCells_ShowDash));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_GridUsesCorrectColumnTemplate()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var style = await dashboard.HeatmapGrid.GetAttributeAsync("style") ?? "";
            Assert.Contains("160px", style);
            Assert.Contains("repeat(", style);
            Assert.Contains("1fr", style);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_GridUsesCorrectColumnTemplate));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_ShippedHeader_HasGreenTheme()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var bg = await dashboard.ShippedHeader.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // #E8F5E9 = rgb(232, 245, 233)
            Assert.Contains("232", bg);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_ShippedHeader_HasGreenTheme));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_BlockersHeader_HasRedTheme()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var bg = await dashboard.BlockersHeader.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // #FEF2F2 = rgb(254, 242, 242)
            Assert.Contains("254", bg);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_BlockersHeader_HasRedTheme));
            throw;
        }
    }
}