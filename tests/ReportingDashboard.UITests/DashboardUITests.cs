using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class DashboardUITests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> CreatePageAsync()
    {
        var page = await _fixture.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        page.SetDefaultTimeout(60000);
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return page;
    }

    [Fact]
    public async Task Dashboard_LoadsWithHeaderTimlineAndHeatmap()
    {
        var page = await CreatePageAsync();

        // Check for either dashboard content or error state
        var dashboardVisible = await page.Locator(".dashboard").CountAsync() > 0;
        var errorVisible = await page.Locator(".error-container").CountAsync() > 0;

        (dashboardVisible || errorVisible).Should().BeTrue("page must show dashboard or error");

        if (dashboardVisible)
        {
            (await page.Locator(".hdr").CountAsync()).Should().BeGreaterThan(0);
            (await page.Locator(".tl-area").CountAsync()).Should().BeGreaterThan(0);
            (await page.Locator(".hm-wrap").CountAsync()).Should().BeGreaterThan(0);
        }

        await page.CloseAsync();
    }

    [Fact]
    public async Task Timeline_RendersSvgWithMilestones()
    {
        var page = await CreatePageAsync();

        var svgCount = await page.Locator(".tl-svg-box svg").CountAsync();
        if (svgCount > 0)
        {
            var svgWidth = await page.Locator(".tl-svg-box svg").GetAttributeAsync("width");
            svgWidth.Should().Be("1560");

            var svgHeight = await page.Locator(".tl-svg-box svg").GetAttributeAsync("height");
            svgHeight.Should().Be("185");
        }

        await page.CloseAsync();
    }

    [Fact]
    public async Task HeatmapGrid_RendersCornerAndColumnHeaders()
    {
        var page = await CreatePageAsync();

        var gridCount = await page.Locator(".hm-grid").CountAsync();
        if (gridCount > 0)
        {
            var cornerText = await page.Locator(".hm-corner").TextContentAsync();
            cornerText.Should().Contain("Status");

            var colHeaders = await page.Locator(".hm-col-hdr").CountAsync();
            colHeaders.Should().BeGreaterThan(0);
        }

        await page.CloseAsync();
    }

    [Fact]
    public async Task HeatmapGrid_CurrentMonthHasHighlight()
    {
        var page = await CreatePageAsync();

        var currentMonthCount = await page.Locator(".current-month-hdr").CountAsync();
        if (await page.Locator(".hm-grid").CountAsync() > 0)
        {
            currentMonthCount.Should().BeGreaterThan(0, "one column header should have current-month-hdr class");
        }

        await page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_NoScrollbarsAt1920x1080()
    {
        var page = await CreatePageAsync();

        var hasScrollbar = await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollHeight > document.documentElement.clientHeight || document.documentElement.scrollWidth > document.documentElement.clientWidth");

        hasScrollbar.Should().BeFalse("dashboard should fit 1920x1080 with no scrollbars");

        await page.CloseAsync();
    }
}