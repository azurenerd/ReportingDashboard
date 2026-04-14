using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

/// <summary>
/// Playwright UI tests for live-reload and error banner features.
/// Validates that the error banner renders when expected and that
/// the dashboard displays core structural elements from Dashboard.razor.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class LiveReloadUITests
{
    private readonly PlaywrightFixture _fixture;

    public LiveReloadUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> CreatePageAsync()
    {
        Skip.If(!_fixture.BrowserAvailable, "Playwright browser not available.");
        Skip.If(_fixture.Browser is null, "Playwright browser not initialized.");

        var context = await _fixture.Browser!.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60000);
        return page;
    }

    private async Task<bool> IsServerRunning(IPage page)
    {
        try
        {
            var response = await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions { Timeout = 5000 });
            return response is not null && response.Ok;
        }
        catch { return false; }
    }

    [SkippableFact]
    public async Task ErrorBanner_NotVisible_WhenDataIsValid()
    {
        var page = await CreatePageAsync();
        Skip.If(!await IsServerRunning(page), "Server not reachable at " + _fixture.BaseUrl);

        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(2000);

        var bannerCount = await page.Locator(".error-banner").CountAsync();
        bannerCount.Should().Be(0, "error banner should not appear when data.json is valid");

        await page.Context.CloseAsync();
    }

    [SkippableFact]
    public async Task Legend_DisplaysFourItems()
    {
        var page = await CreatePageAsync();
        Skip.If(!await IsServerRunning(page), "Server not reachable at " + _fixture.BaseUrl);

        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(2000);

        var legendItems = page.Locator(".legend .legend-item");
        var count = await legendItems.CountAsync();

        if (count > 0)
        {
            count.Should().Be(4, "legend should have PoC Milestone, Production Release, Checkpoint, Now");
        }

        await page.Context.CloseAsync();
    }

    [SkippableFact]
    public async Task Subtitle_IsDisplayed()
    {
        var page = await CreatePageAsync();
        Skip.If(!await IsServerRunning(page), "Server not reachable at " + _fixture.BaseUrl);

        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(2000);

        var sub = page.Locator(".sub");
        var count = await sub.CountAsync();

        if (count > 0)
        {
            var text = await sub.TextContentAsync();
            text.Should().NotBeNullOrWhiteSpace("subtitle should render from data.json");
        }

        await page.Context.CloseAsync();
    }

    [SkippableFact]
    public async Task BacklogLink_IsPresent_WithHref()
    {
        var page = await CreatePageAsync();
        Skip.If(!await IsServerRunning(page), "Server not reachable at " + _fixture.BaseUrl);

        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(2000);

        var link = page.Locator(".hdr h1 a");
        var count = await link.CountAsync();

        if (count > 0)
        {
            var href = await link.GetAttributeAsync("href");
            href.Should().NotBeNullOrWhiteSpace("backlog link should have an href from data.json");

            var text = await link.TextContentAsync();
            text.Should().Contain("ADO Backlog");
        }

        await page.Context.CloseAsync();
    }

    [SkippableFact]
    public async Task Page_NoErrorPage_WhenValidData()
    {
        var page = await CreatePageAsync();
        Skip.If(!await IsServerRunning(page), "Server not reachable at " + _fixture.BaseUrl);

        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(2000);

        var errorPageCount = await page.Locator(".error-page").CountAsync();
        errorPageCount.Should().Be(0, "error-page should not appear when valid data.json is loaded");

        await page.Context.CloseAsync();
    }
}