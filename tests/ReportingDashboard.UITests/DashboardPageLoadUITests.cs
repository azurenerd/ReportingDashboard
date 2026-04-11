using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Fixtures;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardPageLoadUITests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardPageLoadUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dashboard_LoadsSuccessfully()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var response = await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            response.Should().NotBeNull();
            response!.Ok.Should().BeTrue("dashboard should return HTTP 200");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_LoadsSuccessfully));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_ContainsHeaderSection()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPage(page, _fixture.BaseUrl);
        await header.NavigateAsync();

        try
        {
            await Assertions.Expect(header.Header).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ContainsHeaderSection));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_NoJavaScriptErrors()
    {
        var page = await _fixture.NewPageAsync();
        var jsErrors = new List<string>();

        page.PageError += (_, error) => jsErrors.Add(error);

        await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });

        // Allow Blazor reconnection errors but no other JS errors
        var criticalErrors = jsErrors.Where(e => !e.Contains("reconnect")).ToList();
        criticalErrors.Should().BeEmpty("page should load without JavaScript errors");
    }

    [Fact]
    public async Task Dashboard_ViewportIs1920x1080()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        try
        {
            var viewportWidth = await page.EvaluateAsync<int>("() => window.innerWidth");
            var viewportHeight = await page.EvaluateAsync<int>("() => window.innerHeight");

            viewportWidth.Should().Be(1920);
            viewportHeight.Should().Be(1080);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ViewportIs1920x1080));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_UsesExpectedFontFamily()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        try
        {
            var fontFamily = await page.Locator("body").EvaluateAsync<string>(
                "el => window.getComputedStyle(el).fontFamily");

            // Should use Segoe UI or the fallback chain
            var normalizedFont = fontFamily.ToLowerInvariant();
            (normalizedFont.Contains("segoe ui") || normalizedFont.Contains("arial") || normalizedFont.Contains("sans-serif"))
                .Should().BeTrue($"body font-family should include 'Segoe UI', 'Arial', or 'sans-serif', got: {fontFamily}");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_UsesExpectedFontFamily));
            throw;
        }
    }
}