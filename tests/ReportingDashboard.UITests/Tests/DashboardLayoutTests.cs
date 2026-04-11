using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardLayoutTests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardLayoutTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_PageLoads_NoErrors()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync(_fixture.BaseUrl);
            Assert.NotNull(response);
            Assert.True(response!.Ok, $"Expected 200 OK, got {response.Status}");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_PageLoads_NoErrors));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_HasAllMajorSections()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            // Header
            await Assertions.Expect(page.Locator(".hdr")).ToBeVisibleAsync();
            // Timeline
            await Assertions.Expect(page.Locator(".tl-area")).ToBeVisibleAsync();
            // Heatmap
            await Assertions.Expect(page.Locator(".hm-wrap")).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_HasAllMajorSections));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_FixedViewport_1920x1080()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.SetViewportSizeAsync(1920, 1080);
            await page.GotoAsync(_fixture.BaseUrl);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var bodyWidth = await page.EvaluateAsync<int>("() => document.body.scrollWidth");
            Assert.Equal(1920, bodyWidth);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_FixedViewport_1920x1080));
            throw;
        }
    }
}