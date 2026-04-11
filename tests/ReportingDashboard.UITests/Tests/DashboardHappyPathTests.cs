using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardHappyPathTests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardHappyPathTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_LoadsSuccessfully()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await Assertions.Expect(page.Locator("body")).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_LoadsSuccessfully));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_HasTimelineSection()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await Assertions.Expect(page.Locator(".tl-area")).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_HasTimelineSection));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_HasHeaderSection()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await Assertions.Expect(page.Locator(".hdr")).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_HasHeaderSection));
            throw;
        }
    }
}