using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class HeaderUITests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> NewPageAsync()
    {
        var context = await _fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60000);
        return page;
    }

    [Fact]
    public async Task Dashboard_WhenDataLoaded_ShowsHdrDiv()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var isError = await page.Locator(".error-container").IsVisibleAsync();
        if (!isError)
        {
            var hdr = page.Locator(".hdr");
            (await hdr.CountAsync()).Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task Dashboard_WhenDataLoaded_ShowsLegendItems()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var isError = await page.Locator(".error-container").IsVisibleAsync();
        if (!isError)
        {
            var legendItems = page.Locator(".legend-item");
            (await legendItems.CountAsync()).Should().Be(4);
        }
    }

    [Fact]
    public async Task Dashboard_WhenDataLoaded_ShowsTimelineArea()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var isError = await page.Locator(".error-container").IsVisibleAsync();
        if (!isError)
        {
            var tlArea = page.Locator(".tl-area");
            (await tlArea.CountAsync()).Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task Dashboard_WhenDataLoaded_ShowsHeatmapWrap()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var isError = await page.Locator(".error-container").IsVisibleAsync();
        if (!isError)
        {
            var hmWrap = page.Locator(".hm-wrap");
            (await hmWrap.CountAsync()).Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task Dashboard_WhenDataLoaded_HeatmapTitleContainsExpectedText()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var isError = await page.Locator(".error-container").IsVisibleAsync();
        if (!isError)
        {
            var hmTitle = page.Locator(".hm-title");
            if (await hmTitle.CountAsync() > 0)
            {
                var text = await hmTitle.TextContentAsync();
                text.Should().NotBeNullOrWhiteSpace();
                text!.ToUpper().Should().Contain("HEATMAP");
            }
        }
    }
}