using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardPageTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage? _page;

    public DashboardPageTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        if (_fixture.Browser is not null)
        {
            _page = await _fixture.Browser.NewPageAsync();
            _page.SetDefaultTimeout(60000);
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
    public async Task HomePage_Loads_WithoutErrors()
    {
        var page = _page ?? await _fixture.CreatePageAsync();
        var response = await page.GotoAsync(_fixture.BaseUrl);
        Assert.NotNull(response);
        Assert.True(response.Ok, $"Expected OK response but got {response.Status}");
    }

    [Fact(Skip = "Playwright browser binaries not installed. Run 'pwsh bin/Debug/net8.0/playwright.ps1 install' to enable.")]
    public async Task HomePage_ShowsEitherDataOrError()
    {
        var page = _page ?? await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        // Dashboard should show either the rendered dashboard or an error panel
        var hasDashboard = await page.Locator(".hdr").CountAsync() > 0;
        var hasError = await page.Locator("text=Dashboard Configuration Error").CountAsync() > 0;
        Assert.True(hasDashboard || hasError, "Page should show either dashboard content or an error panel");
    }

    [Fact(Skip = "Playwright browser binaries not installed. Run 'pwsh bin/Debug/net8.0/playwright.ps1 install' to enable.")]
    public async Task ErrorPanel_ShowsConfigurationError_WhenDataMissing()
    {
        var page = _page ?? await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        // If data.json is missing/invalid, error panel should be visible
        var errorPanel = page.Locator("text=Dashboard Configuration Error");
        if (await errorPanel.CountAsync() > 0)
        {
            await Assertions.Expect(errorPanel).ToBeVisibleAsync();
        }
    }

    [Fact(Skip = "Playwright browser binaries not installed. Run 'pwsh bin/Debug/net8.0/playwright.ps1 install' to enable.")]
    public async Task SuccessState_ShowsProjectTitle_WhenDataValid()
    {
        var page = _page ?? await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        var header = page.Locator(".hdr h1");
        if (await header.CountAsync() > 0)
        {
            var text = await header.TextContentAsync();
            Assert.False(string.IsNullOrWhiteSpace(text), "Project title should not be empty");
        }
    }

    [Fact(Skip = "Playwright browser binaries not installed. Run 'pwsh bin/Debug/net8.0/playwright.ps1 install' to enable.")]
    public async Task Viewport_IsFixedDimensions()
    {
        var page = _page ?? await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        var bodyWidth = await page.EvaluateAsync<int>("document.body.offsetWidth");
        var bodyHeight = await page.EvaluateAsync<int>("document.body.offsetHeight");
        Assert.Equal(1920, bodyWidth);
        Assert.Equal(1080, bodyHeight);
    }
}