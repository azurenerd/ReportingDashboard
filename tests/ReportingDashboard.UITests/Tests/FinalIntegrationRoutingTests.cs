using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Routing and navigation tests for the Final Integration PR.
/// Verifies Routes.razor correctly maps "/" to Dashboard and handles not-found.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class FinalIntegrationRoutingTests
{
    private readonly PlaywrightFixture _fixture;

    public FinalIntegrationRoutingTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Root_Route_LoadsDashboard()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30_000
            });

            // Should either show dashboard or error panel
            var hasDashboard = await page.Locator(".dashboard").CountAsync() > 0;
            var hasError = await page.Locator(".error-panel").CountAsync() > 0;

            Assert.True(hasDashboard || hasError,
                "Root route should render either dashboard or error panel");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Root_Route_LoadsDashboard));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task NotFound_Route_ShowsNotFoundMessage()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/nonexistent-page-xyz", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30_000
            });

            var bodyText = await page.TextContentAsync("body") ?? "";
            Assert.Contains("Not found", bodyText, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(NotFound_Route_ShowsNotFoundMessage));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task StaticFiles_CssLoads_Successfully()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            Assert.NotNull(response);
            Assert.True(response!.Ok, "dashboard.css should return 200 OK");

            var contentType = response.Headers["content-type"];
            Assert.Contains("css", contentType);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(StaticFiles_CssLoads_Successfully));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task StaticFiles_DataJson_IsAccessible()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/data.json");
            Assert.NotNull(response);
            Assert.True(response!.Ok, "data.json should return 200 OK");

            var body = await response.TextAsync();
            Assert.Contains("title", body);
            Assert.Contains("timeline", body);
            Assert.Contains("heatmap", body);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(StaticFiles_DataJson_IsAccessible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task BlazorJs_IsServed_Successfully()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30_000
            });

            var hasBlazorScript = await page.EvaluateAsync<bool>(
                "() => Array.from(document.scripts).some(s => s.src && s.src.includes('blazor.web.js'))");
            Assert.True(hasBlazorScript, "blazor.web.js script should be loaded");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(BlazorJs_IsServed_Successfully));
            throw;
        }
    }
}