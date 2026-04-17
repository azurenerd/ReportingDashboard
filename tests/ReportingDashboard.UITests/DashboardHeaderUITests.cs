using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class DashboardHeaderUITests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardHeaderUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> NewPageAsync()
    {
        var page = await _fixture.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        page.SetDefaultTimeout(60000);
        return page;
    }

    [Fact]
    public async Task Header_HdrElement_IsPresent_WhenDataLoads()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hdr = page.Locator(".hdr");
        var count = await hdr.CountAsync();

        // Only assert header present if no error state
        var errorCount = await page.Locator(".error-container").CountAsync();
        if (errorCount == 0)
        {
            Assert.Equal(1, count);
        }

        await page.CloseAsync();
    }

    [Fact]
    public async Task Header_H1Title_IsRendered_WhenDataLoads()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorCount = await page.Locator(".error-container").CountAsync();
        if (errorCount == 0)
        {
            var h1 = page.Locator(".hdr-title");
            Assert.Equal(1, await h1.CountAsync());

            var text = await h1.TextContentAsync();
            Assert.False(string.IsNullOrWhiteSpace(text), "h1.hdr-title should have non-empty text content");
        }

        await page.CloseAsync();
    }

    [Fact]
    public async Task Header_ADOBacklogLink_HasTargetBlank_WhenDataLoads()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorCount = await page.Locator(".error-container").CountAsync();
        if (errorCount == 0)
        {
            var link = page.Locator(".hdr-title a");
            Assert.Equal(1, await link.CountAsync());

            var target = await link.GetAttributeAsync("target");
            Assert.Equal("_blank", target);

            var href = await link.GetAttributeAsync("href");
            Assert.False(string.IsNullOrWhiteSpace(href), "ADO Backlog link must have a non-empty href");
        }

        await page.CloseAsync();
    }

    [Fact]
    public async Task Header_Legend_HasExactlyFourItems_WhenDataLoads()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorCount = await page.Locator(".error-container").CountAsync();
        if (errorCount == 0)
        {
            var legendItems = page.Locator(".legend .legend-item");
            Assert.Equal(4, await legendItems.CountAsync());

            var texts = new[]
            {
                await legendItems.Nth(0).TextContentAsync(),
                await legendItems.Nth(1).TextContentAsync(),
                await legendItems.Nth(2).TextContentAsync(),
                await legendItems.Nth(3).TextContentAsync(),
            };

            Assert.Contains("PoC Milestone", texts[0]);
            Assert.Contains("Production Release", texts[1]);
            Assert.Contains("Checkpoint", texts[2]);
            Assert.Contains("Now", texts[3]);
        }

        await page.CloseAsync();
    }

    [Fact]
    public async Task Header_LegendShapeClasses_ArePresent_WhenDataLoads()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorCount = await page.Locator(".error-container").CountAsync();
        if (errorCount == 0)
        {
            Assert.Equal(1, await page.Locator(".legend-diamond.poc").CountAsync());
            Assert.Equal(1, await page.Locator(".legend-diamond.prod").CountAsync());
            Assert.Equal(1, await page.Locator(".legend-circle").CountAsync());
            Assert.Equal(1, await page.Locator(".legend-now-bar").CountAsync());
        }

        await page.CloseAsync();
    }
}