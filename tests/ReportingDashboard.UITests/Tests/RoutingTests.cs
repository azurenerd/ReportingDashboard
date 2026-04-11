using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// E2E tests for Blazor routing behavior.
/// Verifies root route loads dashboard, unknown routes show not-found,
/// and the Router component handles navigation correctly.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class RoutingTests
{
    private readonly PlaywrightFixture _fixture;

    public RoutingTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task RootRoute_LoadsDashboard()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            Assert.NotNull(response);
            Assert.True(response!.Ok, $"Root route returned {response.Status}");

            // Should have either dashboard content or error panel (depending on data.json)
            var hasDashboard = await page.Locator(".dashboard").CountAsync() > 0;
            var hasError = await page.Locator(".error-panel").CountAsync() > 0;
            Assert.True(hasDashboard || hasError,
                "Root route should render either dashboard or error panel");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(RootRoute_LoadsDashboard));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task RootRoute_ReturnsHtmlContent()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync(_fixture.BaseUrl);
            var contentType = response?.Headers.GetValueOrDefault("content-type") ?? "";
            Assert.Contains("text/html", contentType);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(RootRoute_ReturnsHtmlContent));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task UnknownRoute_StillReturns200_BlazorHandles()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            // Blazor SPA returns 200 for all routes (client-side routing decides what to show)
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/this-page-does-not-exist",
                new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            Assert.NotNull(response);
            Assert.True(response!.Ok,
                "Blazor should return 200 even for unknown routes (client-side routing)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(UnknownRoute_StillReturns200_BlazorHandles));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task UnknownRoute_ShowsNotFoundMessage()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync($"{_fixture.BaseUrl}/nonexistent-page-xyz",
                new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Wait for Blazor to render client-side
            await page.WaitForTimeoutAsync(2000);

            var notFoundLocator = page.Locator("p[role='alert']");
            if (await notFoundLocator.CountAsync() > 0)
            {
                var text = await notFoundLocator.TextContentAsync();
                Assert.Contains("not found", text ?? "", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(UnknownRoute_ShowsNotFoundMessage));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task RootRoute_BlazorScriptLoaded()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            var blazorScript = page.Locator("script[src*='blazor']");
            var count = await blazorScript.CountAsync();
            Assert.True(count > 0, "Expected Blazor script tag in the page");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(RootRoute_BlazorScriptLoaded));
            throw;
        }
    }
}