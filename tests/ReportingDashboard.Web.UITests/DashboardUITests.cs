using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.Web.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class DashboardUITests
{
    private readonly PlaywrightFixture _fx;

    public DashboardUITests(PlaywrightFixture fx) => _fx = fx;

    [Fact]
    public async Task Dashboard_Root_RendersPageTitle()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var title = await page.TitleAsync();
        title.Should().Be("Reporting Dashboard");
    }

    [Fact]
    public async Task Dashboard_Root_HasNoBlazorServerScript()
    {
        var page = await _fx.NewPageAsync();
        var response = await page.GotoAsync(_fx.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        response.Should().NotBeNull();
        var html = await response!.TextAsync();
        html.Should().NotContain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task Dashboard_RendersAt1920x1080_WithoutScrollbars()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hasVerticalScroll = await page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollHeight > window.innerHeight");
        hasVerticalScroll.Should().BeFalse("1920x1080 layout should fit viewport without vertical scrollbar");
    }

    [Fact]
    public async Task Dashboard_RendersChildSections_OrErrorBanner()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var bannerCount = await page.Locator(".error-banner").CountAsync();
        if (bannerCount > 0)
        {
            var strongText = await page.Locator(".error-banner strong").First.InnerTextAsync();
            strongText.Should().ContainAny("data.json", "Failed to");
        }
        else
        {
            var svgCount = await page.Locator("svg").CountAsync();
            svgCount.Should().BeGreaterThan(0, "timeline SVG should render in happy path");
        }
    }

    [Fact]
    public async Task Dashboard_RespondsWithHttp200()
    {
        var page = await _fx.NewPageAsync();
        var response = await page.GotoAsync(_fx.BaseUrl);

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
    }
}