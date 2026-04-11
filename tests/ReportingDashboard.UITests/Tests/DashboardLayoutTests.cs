using FluentAssertions;
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

    private async Task<DashboardPage> CreatePageAsync()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        return dashboard;
    }

    [Fact]
    public async Task Dashboard_LoadsSuccessfully_Returns200()
    {
        var dashboard = await CreatePageAsync();
        var response = await dashboard.Page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
    }

    [Fact]
    public async Task Dashboard_HasThreeMainSections()
    {
        var dashboard = await CreatePageAsync();
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            await Assertions.Expect(dashboard.Header).ToBeVisibleAsync();
            await Assertions.Expect(dashboard.TimelineArea).ToBeVisibleAsync();
            await Assertions.Expect(dashboard.HeatmapWrap).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Dashboard_HasThreeMainSections));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_ViewportIs1920x1080()
    {
        var dashboard = await CreatePageAsync();
        await dashboard.NavigateAsync();

        var viewportSize = dashboard.Page.ViewportSize;
        viewportSize.Should().NotBeNull();
        viewportSize!.Width.Should().Be(1920);
        viewportSize.Height.Should().Be(1080);
    }

    [Fact]
    public async Task Dashboard_NoHorizontalScrollbar()
    {
        var dashboard = await CreatePageAsync();
        await dashboard.NavigateAsync();
        await dashboard.WaitForDashboardLoadedAsync();

        var scrollWidth = await dashboard.Page.EvaluateAsync<int>("document.documentElement.scrollWidth");
        var clientWidth = await dashboard.Page.EvaluateAsync<int>("document.documentElement.clientWidth");

        scrollWidth.Should().BeLessThanOrEqualTo(clientWidth + 5); // small tolerance
    }

    [Fact]
    public async Task Dashboard_ErrorPanel_NotVisible_WhenDataLoaded()
    {
        var dashboard = await CreatePageAsync();
        await dashboard.NavigateAsync();
        await dashboard.WaitForDashboardLoadedAsync();

        // If dashboard loaded with data, error panel should not exist
        var headerVisible = await dashboard.Header.IsVisibleAsync();
        if (headerVisible)
        {
            await Assertions.Expect(dashboard.ErrorPanel).Not.ToBeVisibleAsync();
        }
    }
}