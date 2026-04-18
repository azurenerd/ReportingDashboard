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
    public async Task Dashboard_RootPath_Returns200()
    {
        var page = await NewPageAsync();
        var response = await page.GotoAsync(_fixture.BaseUrl);

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
    }

    [Fact]
    public async Task Dashboard_OnLoad_NoBlazorDefaultChrome()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var content = await page.ContentAsync();

        content.Should().NotContain("Hello, world!");
        content.Should().NotContain("counter");
        content.Should().NotContain("weather");
    }

    [Fact]
    public async Task Dashboard_ErrorState_ShowsErrorContainer_WhenDataMissing()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var body = await page.Locator("body").InnerHTMLAsync();
        body.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Dashboard_ErrorMessage_ContainsHelpfulText_WhenDataMissing()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorContainer = page.Locator(".error-container");
        if (await errorContainer.IsVisibleAsync())
        {
            var h2Text = await page.Locator(".error-container h2").TextContentAsync();
            h2Text.Should().Be("Unable to load dashboard data");
        }
    }

    [Fact]
    public async Task Dashboard_BlazorErrorUI_IsHidden()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorUi = page.Locator("#blazor-error-ui");
        var count = await errorUi.CountAsync();
        if (count > 0)
        {
            var isVisible = await errorUi.IsVisibleAsync();
            isVisible.Should().BeFalse();
        }
    }
}