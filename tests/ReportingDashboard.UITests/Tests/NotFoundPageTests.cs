using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class NotFoundPageTests
{
    private readonly PlaywrightFixture _fixture;

    public NotFoundPageTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task UnknownRoute_RendersPageNotFound()
    {
        var page = await _fixture.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/this-page-does-not-exist", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });

        var html = await page.ContentAsync();

        // The Blazor Router should show the NotFound template
        // which renders "Page Not Found"
        html.Should().Contain("Page Not Found");
    }

    [Fact]
    public async Task UnknownRoute_UsesMainLayout()
    {
        var page = await _fixture.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/nonexistent-route", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });

        // Main layout container should still be present
        var container = page.Locator("div[style*='width:1920px'][style*='height:1080px']");
        var count = await container.CountAsync();
        count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UnknownRoute_ShowsErrorStyling()
    {
        var page = await _fixture.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/xyz-missing", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });

        var errorDiv = page.Locator(".dashboard-error");
        var count = await errorDiv.CountAsync();
        count.Should().BeGreaterThan(0);
    }
}