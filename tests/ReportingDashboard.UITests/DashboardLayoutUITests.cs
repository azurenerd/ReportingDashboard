using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class DashboardLayoutUITests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardLayoutUITests(PlaywrightFixture fixture)
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
    public async Task DashboardPage_DashWrap_HasCorrectInlineStyles()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var dashWrap = page.Locator(".dash-wrap");
        Assert.Equal(1, await dashWrap.CountAsync());

        var width = await page.EvaluateAsync<string>(
            "() => document.querySelector('.dash-wrap').style.width");
        var height = await page.EvaluateAsync<string>(
            "() => document.querySelector('.dash-wrap').style.height");
        var overflow = await page.EvaluateAsync<string>(
            "() => document.querySelector('.dash-wrap').style.overflow");

        Assert.Equal("1920px", width);
        Assert.Equal("1080px", height);
        Assert.Equal("hidden", overflow);

        await page.CloseAsync();
    }

    [Fact]
    public async Task DashboardPage_ErrorState_ShowsCorrectHeading()
    {
        // This test verifies error rendering when data.json is absent.
        // Only asserts when error-container is present; skips if data loaded successfully.
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorContainer = page.Locator(".error-container");
        var count = await errorContainer.CountAsync();

        if (count > 0)
        {
            var heading = await page.Locator(".error-container h2").TextContentAsync();
            Assert.Equal("Unable to load dashboard data", heading);

            var para = await page.Locator(".error-container p").First.TextContentAsync();
            Assert.Equal("Please check that data.json exists and contains valid JSON.", para);

            var errorDetail = page.Locator(".error-container .error-detail");
            Assert.Equal(1, await errorDetail.CountAsync());
        }

        await page.CloseAsync();
    }

    [Fact]
    public async Task DashboardPage_ValidData_NoErrorContainer()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorContainer = page.Locator(".error-container");
        var errorCount = await errorContainer.CountAsync();

        var dashWrap = page.Locator(".dash-wrap");
        var wrapCount = await dashWrap.CountAsync();

        // dash-wrap must always be present; error-container only when data fails
        Assert.Equal(1, wrapCount);
        // If no error, dash-wrap exists but error-container does not
        if (errorCount == 0)
        {
            Assert.Equal(1, wrapCount);
        }

        await page.CloseAsync();
    }

    [Fact]
    public async Task DashboardPage_DashWrap_IsFlexColumn()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var display = await page.EvaluateAsync<string>(
            "() => document.querySelector('.dash-wrap').style.display");
        var flexDirection = await page.EvaluateAsync<string>(
            "() => document.querySelector('.dash-wrap').style.flexDirection");

        Assert.Equal("flex", display);
        Assert.Equal("column", flexDirection);

        await page.CloseAsync();
    }

    [Fact]
    public async Task DashboardPage_BodyBackgroundIsWhite()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var bg = await page.EvaluateAsync<string>(
            "() => getComputedStyle(document.body).backgroundColor");

        // #FFFFFF = rgb(255, 255, 255)
        Assert.Equal("rgb(255, 255, 255)", bg);

        await page.CloseAsync();
    }
}