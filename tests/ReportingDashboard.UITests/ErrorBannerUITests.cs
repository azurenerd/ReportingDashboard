using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class ErrorBannerUITests
{
    private readonly PlaywrightFixture _fixture;

    public ErrorBannerUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ValidDataJson_RendersNoErrorBanner()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorBanner = page.Locator(".error-banner");
        var count = await errorBanner.CountAsync();
        count.Should().Be(0, "no error banner should appear when valid data.json loads");

        await page.Context.DisposeAsync();
    }

    [Fact]
    public async Task MissingFile_ShowsErrorBannerWithMessage()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/?data=nonexistent.json");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorBanner = page.Locator(".error-banner");
        await errorBanner.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var bannerText = await errorBanner.TextContentAsync();
        bannerText.Should().Contain("Dashboard Error");
        bannerText.Should().Contain("Could not load data file: nonexistent.json");

        await page.Context.DisposeAsync();
    }

    [Fact]
    public async Task PathTraversal_ShowsInvalidFilenameError()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/?data=../../etc/passwd");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorBanner = page.Locator(".error-banner");
        await errorBanner.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var bannerText = await errorBanner.TextContentAsync();
        bannerText.Should().Contain("Invalid filename:");

        await page.Context.DisposeAsync();
    }

    [Fact]
    public async Task ErrorBanner_ContainsBoldHeading()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/?data=nonexistent.json");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var heading = page.Locator(".error-banner strong");
        await heading.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var headingText = await heading.TextContentAsync();
        headingText.Should().Be("Dashboard Error");

        await page.Context.DisposeAsync();
    }

    [Fact]
    public async Task ErrorBanner_SuppressesDashboardSections()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync($"{_fixture.BaseUrl}/?data=nonexistent.json");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorBanner = page.Locator(".error-banner");
        (await errorBanner.CountAsync()).Should().Be(1, "error banner should be present");

        var header = page.Locator(".hdr");
        (await header.CountAsync()).Should().Be(0, "header section must not render when error is shown");

        var timeline = page.Locator(".tl-area");
        (await timeline.CountAsync()).Should().Be(0, "timeline section must not render when error is shown");

        var heatmap = page.Locator(".hm-wrap");
        (await heatmap.CountAsync()).Should().Be(0, "heatmap section must not render when error is shown");

        await page.Context.DisposeAsync();
    }
}