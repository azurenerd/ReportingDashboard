using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Tests for the Timeline area styling and structure from PR #539.
/// Validates CSS properties match the OriginalDesignConcept.html spec.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class FoundationTimelineAreaTests
{
    private readonly PlaywrightFixture _fixture;

    public FoundationTimelineAreaTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TimelineArea_IsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            await Assertions.Expect(dashboard.TimelineArea).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(TimelineArea_IsVisible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TimelineArea_HasHeight196px()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var height = await dashboard.TimelineArea.EvaluateAsync<string>(
                "el => getComputedStyle(el).height");
            Assert.Equal("196px", height);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(TimelineArea_HasHeight196px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TimelineArea_HasFAFAFABackground()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var bgColor = await dashboard.TimelineArea.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // #FAFAFA = rgb(250, 250, 250)
            Assert.Contains("250, 250, 250", bgColor);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(TimelineArea_HasFAFAFABackground));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TimelineArea_HasFlexDisplay()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var display = await dashboard.TimelineArea.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            Assert.Equal("flex", display);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(TimelineArea_HasFlexDisplay));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TimelineArea_HasBottomBorder()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var borderWidth = await dashboard.TimelineArea.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomWidth");
            Assert.Equal("2px", borderWidth);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(TimelineArea_HasBottomBorder));
            throw;
        }
    }
}