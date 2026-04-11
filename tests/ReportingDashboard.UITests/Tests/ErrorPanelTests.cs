using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Tests for the error panel display. These tests require a server started with
/// an invalid or missing data.json to trigger the error state.
/// Set BASE_URL_ERROR environment variable to point to an error-state server,
/// or skip these tests if only a valid server is available.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class ErrorPanelTests
{
    private readonly PlaywrightFixture _fixture;

    public ErrorPanelTests(PlaywrightFixture fixture) => _fixture = fixture;

    private string ErrorBaseUrl =>
        Environment.GetEnvironmentVariable("BASE_URL_ERROR") ?? _fixture.BaseUrl;

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorPanel_IsDisplayed_WhenDataJsonInvalid()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, ErrorBaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            Assert.True(await dashboard.IsErrorVisibleAsync(),
                "Expected error panel to be visible");
            Assert.False(await dashboard.IsDashboardVisibleAsync(),
                "Expected dashboard-root to NOT be present");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_IsDisplayed_WhenDataJsonInvalid));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorPanel_ShowsTitle()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, ErrorBaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            await Assertions.Expect(dashboard.ErrorTitle).ToBeVisibleAsync();
            var text = await dashboard.ErrorTitle.TextContentAsync();
            Assert.Contains("Dashboard data could not be loaded", text ?? "");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_ShowsTitle));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorPanel_ShowsErrorDetails()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, ErrorBaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            await Assertions.Expect(dashboard.ErrorDetails).ToBeVisibleAsync();
            var text = await dashboard.ErrorDetails.TextContentAsync();
            Assert.False(string.IsNullOrWhiteSpace(text), "Error details should not be empty");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_ShowsErrorDetails));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorPanel_ShowsHelpText()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, ErrorBaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            await Assertions.Expect(dashboard.ErrorHelp).ToBeVisibleAsync();
            var text = await dashboard.ErrorHelp.TextContentAsync();
            Assert.Contains("Check data.json", text ?? "");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_ShowsHelpText));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorPanel_ShowsWarningIcon()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, ErrorBaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            await Assertions.Expect(dashboard.ErrorIcon).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_ShowsWarningIcon));
            throw;
        }
    }
}