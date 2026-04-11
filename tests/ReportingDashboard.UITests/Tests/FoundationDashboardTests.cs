using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Tests for the Dashboard orchestrator page from PR #539.
/// Validates that with valid data.json the page renders Header + placeholders.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class FoundationDashboardTests
{
    private readonly PlaywrightFixture _fixture;

    public FoundationDashboardTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_WithValidData_ShowsDashboardContainer()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            Assert.True(await dashboard.IsDashboardVisibleAsync(),
                "Expected .dashboard container to be present");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_WithValidData_ShowsDashboardContainer));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_WithValidData_NoErrorPanel()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            Assert.False(await dashboard.IsErrorVisibleAsync(),
                "Expected no error panel when data is valid");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_WithValidData_NoErrorPanel));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_HasThreeSections_Header_Timeline_Heatmap()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            await Assertions.Expect(dashboard.Header).ToBeVisibleAsync();
            await Assertions.Expect(dashboard.TimelineArea).ToBeVisibleAsync();
            await Assertions.Expect(dashboard.HeatmapWrap).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_HasThreeSections_Header_Timeline_Heatmap));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_SectionsInCorrectOrder()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            // Header should be above timeline, timeline above heatmap
            var headerBox = await dashboard.Header.BoundingBoxAsync();
            var tlBox = await dashboard.TimelineArea.BoundingBoxAsync();
            var hmBox = await dashboard.HeatmapWrap.BoundingBoxAsync();

            Assert.NotNull(headerBox);
            Assert.NotNull(tlBox);
            Assert.NotNull(hmBox);
            Assert.True(headerBox!.Y < tlBox!.Y, "Header should be above Timeline");
            Assert.True(tlBox.Y < hmBox!.Y, "Timeline should be above Heatmap");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_SectionsInCorrectOrder));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_NoJavaScriptErrors()
    {
        var page = await _fixture.NewPageAsync();
        var errors = new List<string>();
        page.PageError += (_, error) => errors.Add(error);

        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            // Give Blazor time to initialize
            await page.WaitForTimeoutAsync(2000);
            Assert.Empty(errors);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_NoJavaScriptErrors));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_NoConsoleErrors()
    {
        var page = await _fixture.NewPageAsync();
        var consoleErrors = new List<string>();
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
                consoleErrors.Add(msg.Text);
        };

        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            await page.WaitForTimeoutAsync(2000);
            Assert.Empty(consoleErrors);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_NoConsoleErrors));
            throw;
        }
    }
}