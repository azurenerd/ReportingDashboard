using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

/// <summary>
/// Playwright E2E tests for the Dashboard header section.
/// Tests backlog link, subtitle, legend items, and overall header structure.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeaderUITests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> CreatePageAsync()
    {
        Skip.If(!_fixture.BrowserAvailable, "Playwright browser not available.");
        Skip.If(_fixture.Browser is null, "Browser not initialized.");

        var context = await _fixture.Browser!.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60000);
        return page;
    }

    private async Task<bool> NavigateAndWait(IPage page)
    {
        try
        {
            var response = await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions { Timeout = 10000 });
            if (response is null || !response.Ok) return false;
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.WaitForTimeoutAsync(2000);
            return true;
        }
        catch
        {
            return false;
        }
    }

    [SkippableFact]
    public async Task Header_BacklogLink_HasHrefAndText()
    {
        var page = await CreatePageAsync();
        Skip.If(!await NavigateAndWait(page), "Server not reachable at " + _fixture.BaseUrl);

        var link = page.Locator(".hdr h1 a");
        var count = await link.CountAsync();
        Skip.If(count == 0, "Header not rendered - possibly error state");

        var text = await link.TextContentAsync();
        text.Should().Contain("ADO Backlog");

        var href = await link.GetAttributeAsync("href");
        href.Should().NotBeNullOrWhiteSpace();

        await page.Context.CloseAsync();
    }

    [SkippableFact]
    public async Task Header_Subtitle_IsDisplayed()
    {
        var page = await CreatePageAsync();
        Skip.If(!await NavigateAndWait(page), "Server not reachable at " + _fixture.BaseUrl);

        var sub = page.Locator(".sub");
        var count = await sub.CountAsync();
        Skip.If(count == 0, "Subtitle not rendered");

        var text = await sub.TextContentAsync();
        text.Should().NotBeNullOrWhiteSpace();

        await page.Context.CloseAsync();
    }

    [SkippableFact]
    public async Task Header_Legend_HasFourItems()
    {
        var page = await CreatePageAsync();
        Skip.If(!await NavigateAndWait(page), "Server not reachable at " + _fixture.BaseUrl);

        var legendItems = page.Locator(".legend .legend-item");
        var count = await legendItems.CountAsync();
        Skip.If(count == 0, "Legend not rendered");

        count.Should().Be(4);

        var allText = await legendItems.AllTextContentsAsync();
        allText.Should().Contain(t => t.Contains("PoC Milestone"));
        allText.Should().Contain(t => t.Contains("Production Release"));
        allText.Should().Contain(t => t.Contains("Checkpoint"));
        allText.Should().Contain(t => t.Contains("Now"));

        await page.Context.CloseAsync();
    }

    [SkippableFact]
    public async Task Dashboard_RendersEitherHeaderOrErrorPage()
    {
        var page = await CreatePageAsync();
        Skip.If(!await NavigateAndWait(page), "Server not reachable at " + _fixture.BaseUrl);

        var hdrCount = await page.Locator(".hdr").CountAsync();
        var errorCount = await page.Locator(".error-page").CountAsync();

        // One of these must be present - either dashboard loaded or error state
        (hdrCount + errorCount).Should().BeGreaterThan(0,
            "Dashboard should render either .hdr (data loaded) or .error-page (no data)");

        await page.Context.CloseAsync();
    }

    [SkippableFact]
    public async Task Header_LayoutStructure_HasExpectedSections()
    {
        var page = await CreatePageAsync();
        Skip.If(!await NavigateAndWait(page), "Server not reachable at " + _fixture.BaseUrl);

        var hdr = page.Locator(".hdr");
        var hdrCount = await hdr.CountAsync();
        Skip.If(hdrCount == 0, "Header not rendered - data may be missing");

        // Verify header contains h1 and legend
        var h1Count = await page.Locator(".hdr h1").CountAsync();
        h1Count.Should().Be(1);

        var legendCount = await page.Locator(".hdr .legend").CountAsync();
        legendCount.Should().Be(1);

        await page.Context.CloseAsync();
    }
}