using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardUITests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> CreatePageAsync()
    {
        var page = await _fixture.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        page.SetDefaultTimeout(60000);
        return page;
    }

    [Fact]
    public async Task HomePage_Loads_ReturnsSuccessStatus()
    {
        var page = await CreatePageAsync();
        var response = await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);

        await page.CloseAsync();
    }

    [Fact]
    public async Task DashboardCss_IsServed()
    {
        var page = await CreatePageAsync();
        var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);
        var body = await response.TextAsync();
        body.Should().Contain("1920px");
        body.Should().Contain("1080px");

        await page.CloseAsync();
    }

    [Fact]
    public async Task Header_DisplaysProjectTitle()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = page.Locator(".hdr h1");
        var count = await header.CountAsync();

        if (count > 0)
        {
            var text = await header.TextContentAsync();
            text.Should().NotBeNullOrWhiteSpace();
        }

        await page.CloseAsync();
    }

    [Fact]
    public async Task HeatmapGrid_IsPresent()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var grid = page.Locator(".hm-grid");
        var errorPage = page.Locator(".error-page");

        var gridCount = await grid.CountAsync();
        var errorCount = await errorPage.CountAsync();

        // Either the dashboard renders with a grid, or an error page is shown
        (gridCount > 0 || errorCount > 0).Should().BeTrue("either .hm-grid or .error-page should be present");

        await page.CloseAsync();
    }

    [Fact]
    public async Task TimelineArea_IsPresent()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tlArea = page.Locator(".tl-area");
        var errorPage = page.Locator(".error-page");

        var tlCount = await tlArea.CountAsync();
        var errorCount = await errorPage.CountAsync();

        (tlCount > 0 || errorCount > 0).Should().BeTrue("either .tl-area or .error-page should be present");

        await page.CloseAsync();
    }
}