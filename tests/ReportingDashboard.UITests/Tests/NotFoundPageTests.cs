using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class NotFoundPageTests
{
    private readonly PlaywrightFixture _fixture;

    public NotFoundPageTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task NotFoundRoute_StillReturns200()
    {
        var page = await _fixture.NewPageAsync();

        var response = await page.GotoAsync($"{_fixture.BaseUrl}/nonexistent-page", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });

        // Blazor router handles 404 internally
        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
    }

    [Fact]
    public async Task NotFoundRoute_ShowsNotFoundMessage()
    {
        var page = await _fixture.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/nonexistent-page", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });

        var html = await page.ContentAsync();
        html.Should().Contain("Page not found");
    }

    [Fact]
    public async Task NotFoundRoute_StillHas1920x1080Container()
    {
        var page = await _fixture.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/nonexistent-page", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });

        var container = page.Locator("div[style*='width:1920px'][style*='height:1080px']");
        var count = await container.CountAsync();
        count.Should().BeGreaterThan(0, "MainLayout 1920x1080 container should still render for 404");
    }

    [Fact]
    public async Task NotFoundRoute_DoesNotShowDashboardSections()
    {
        var page = await _fixture.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/nonexistent-page", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });

        var hdrCount = await page.Locator(".hdr").CountAsync();
        var tlCount = await page.Locator(".tl-area").CountAsync();
        var hmCount = await page.Locator(".hm-wrap").CountAsync();

        hdrCount.Should().Be(0);
        tlCount.Should().Be(0);
        hmCount.Should().Be(0);
    }

    [Fact]
    public async Task NotFoundRoute_StillHasPageTitle()
    {
        var page = await _fixture.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/nonexistent-page", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });

        var title = await page.TitleAsync();
        title.Should().Contain("Executive Reporting Dashboard");
    }

    [Fact]
    public async Task DeepNotFoundRoute_AlsoShowsNotFound()
    {
        var page = await _fixture.NewPageAsync();

        await page.GotoAsync($"{_fixture.BaseUrl}/some/deep/path/that/doesnt/exist", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });

        var html = await page.ContentAsync();
        html.Should().Contain("Page not found");
    }
}