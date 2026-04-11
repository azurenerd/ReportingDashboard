using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Tests for the ErrorPanel component from PR #539.
/// Requires a server started with missing or invalid data.json.
/// Set BASE_URL_ERROR env var to point to an error-state server.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class FoundationErrorPanelTests
{
    private readonly PlaywrightFixture _fixture;

    public FoundationErrorPanelTests(PlaywrightFixture fixture) => _fixture = fixture;

    private string ErrorBaseUrl =>
        Environment.GetEnvironmentVariable("BASE_URL_ERROR") ?? _fixture.BaseUrl;

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_IsDisplayed_WhenDataMissing()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, ErrorBaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            Assert.True(await dashboard.IsErrorVisibleAsync(),
                "Expected .error-panel to be visible");
            Assert.False(await dashboard.IsDashboardVisibleAsync(),
                "Expected no .dashboard container when in error state");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_IsDisplayed_WhenDataMissing));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_ShowsStaticTitle()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, ErrorBaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var panelText = await dashboard.ErrorPanel.TextContentAsync() ?? "";
            Assert.Contains("Dashboard data could not be loaded", panelText);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_ShowsStaticTitle));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_ShowsErrorMessage()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, ErrorBaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var errorContent = await dashboard.ErrorContent.TextContentAsync() ?? "";
            // Should contain either "not found" or "parse" depending on error type
            Assert.True(
                errorContent.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
                errorContent.Contains("parse", StringComparison.OrdinalIgnoreCase) ||
                errorContent.Contains("error", StringComparison.OrdinalIgnoreCase),
                $"Error panel should show an error message, got: {errorContent}");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_ShowsErrorMessage));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_ShowsHelpText()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, ErrorBaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var text = await dashboard.ErrorPanel.TextContentAsync() ?? "";
            Assert.Contains("Check data.json", text);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_ShowsHelpText));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_ShowsWarningIndicator()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, ErrorBaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            // The warning indicator is "?" in 48px red (#EA4335)
            var text = await dashboard.ErrorPanel.TextContentAsync() ?? "";
            Assert.Contains("?", text);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_ShowsWarningIndicator));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_IsCenteredOnPage()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, ErrorBaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var display = await dashboard.ErrorPanel.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            var justifyContent = await dashboard.ErrorPanel.EvaluateAsync<string>(
                "el => getComputedStyle(el).justifyContent");
            var alignItems = await dashboard.ErrorPanel.EvaluateAsync<string>(
                "el => getComputedStyle(el).alignItems");

            Assert.Equal("flex", display);
            Assert.Equal("center", justifyContent);
            Assert.Equal("center", alignItems);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_IsCenteredOnPage));
            throw;
        }
    }
}