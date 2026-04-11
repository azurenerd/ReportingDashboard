using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// CSS-focused design verification tests for the ErrorPanel component.
/// Validates that the CSS styles in dashboard.css correctly apply to the error panel
/// per the acceptance criteria: centering, font families, spacing, and color values.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class ErrorPanelCssDesignTests
{
    private readonly PlaywrightFixture _fixture;

    public ErrorPanelCssDesignTests(PlaywrightFixture fixture) => _fixture = fixture;

    private string ErrorBaseUrl =>
        Environment.GetEnvironmentVariable("BASE_URL_ERROR") ?? _fixture.BaseUrl;

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_FontFamily_IsSegoeUI()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var fontFamily = await errorPage.ErrorPanel.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontFamily");

            Assert.True(
                fontFamily.Contains("Segoe UI", StringComparison.OrdinalIgnoreCase) ||
                fontFamily.Contains("Arial", StringComparison.OrdinalIgnoreCase) ||
                fontFamily.Contains("sans-serif", StringComparison.OrdinalIgnoreCase),
                $"Expected dashboard font-family, got: {fontFamily}");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_FontFamily_IsSegoeUI));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorIndicator_HasMarginBottom16px()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var marginBottom = await errorPage.ErrorIndicator.EvaluateAsync<string>(
                "el => getComputedStyle(el).marginBottom");

            Assert.Equal("16px", marginBottom);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorIndicator_HasMarginBottom16px));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorTitle_HasMarginBottom12px()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var marginBottom = await errorPage.ErrorTitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).marginBottom");

            Assert.Equal("12px", marginBottom);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorTitle_HasMarginBottom12px));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorMessage_HasMarginBottom16px()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var marginBottom = await errorPage.ErrorMessage.EvaluateAsync<string>(
                "el => getComputedStyle(el).marginBottom");

            Assert.Equal("16px", marginBottom);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorMessage_HasMarginBottom16px));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorMessage_HasMaxWidth600px()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var maxWidth = await errorPage.ErrorMessage.EvaluateAsync<string>(
                "el => getComputedStyle(el).maxWidth");

            Assert.Equal("600px", maxWidth);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorMessage_HasMaxWidth600px));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorMessage_HasWordBreak()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var wordBreak = await errorPage.ErrorMessage.EvaluateAsync<string>(
                "el => getComputedStyle(el).wordBreak");

            Assert.True(
                wordBreak == "break-word" || wordBreak == "break-all",
                $"Expected word-break to allow long paths to wrap, got: {wordBreak}");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorMessage_HasWordBreak));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task DashboardCss_IsLoaded()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(ErrorBaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30_000
            });

            // Verify that the CSS stylesheet is loaded
            var stylesheetCount = await page.EvaluateAsync<int>(
                @"() => {
                    let count = 0;
                    for (let sheet of document.styleSheets) {
                        if (sheet.href && sheet.href.includes('dashboard.css')) count++;
                    }
                    return count;
                }");

            Assert.True(stylesheetCount >= 1,
                "dashboard.css stylesheet should be loaded");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DashboardCss_IsLoaded));
            throw;
        }
    }
}