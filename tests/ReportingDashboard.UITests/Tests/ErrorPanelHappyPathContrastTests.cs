using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Tests that verify the dashboard renders correctly with valid data and does NOT show
/// any error panel elements. This is the contrast/happy-path verification for the
/// ErrorPanel feature — confirming the error panel is conditionally hidden.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class ErrorPanelHappyPathContrastTests
{
    private readonly PlaywrightFixture _fixture;

    public ErrorPanelHappyPathContrastTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server with valid data.json")]
    public async Task Dashboard_ValidData_NoErrorPanelRendered()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            Assert.False(await dashboard.IsErrorVisibleAsync(),
                "Error panel should NOT be visible when data.json is valid");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ValidData_NoErrorPanelRendered));
            throw;
        }
    }

    [Fact(Skip = "Requires running server with valid data.json")]
    public async Task Dashboard_ValidData_NoErrorTitleText()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var bodyText = await page.TextContentAsync("body") ?? "";
            Assert.DoesNotContain("Dashboard data could not be loaded", bodyText);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ValidData_NoErrorTitleText));
            throw;
        }
    }

    [Fact(Skip = "Requires running server with valid data.json")]
    public async Task Dashboard_ValidData_NoHelpText()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var bodyText = await page.TextContentAsync("body") ?? "";
            Assert.DoesNotContain("Check data.json for errors and restart the application", bodyText);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ValidData_NoHelpText));
            throw;
        }
    }

    [Fact(Skip = "Requires running server with valid data.json")]
    public async Task Dashboard_ValidData_ShowsDashboardContent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            Assert.True(await dashboard.IsDashboardVisibleAsync(),
                "Dashboard root should be visible with valid data");
            await Assertions.Expect(dashboard.Header).ToBeVisibleAsync();
            await Assertions.Expect(dashboard.TimelineArea).ToBeVisibleAsync();
            await Assertions.Expect(dashboard.HeatmapWrap).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ValidData_ShowsDashboardContent));
            throw;
        }
    }

    [Fact(Skip = "Requires running server with valid data.json")]
    public async Task Dashboard_ValidData_PageLoadsWithHttp200()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30_000
            });

            Assert.NotNull(response);
            Assert.Equal(200, response!.Status);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ValidData_PageLoadsWithHttp200));
            throw;
        }
    }
}