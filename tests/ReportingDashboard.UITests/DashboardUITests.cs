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
    public async Task Dashboard_LoadsWithHeaderTitleAndBacklogLink()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = page.Locator(".hdr");
        await header.WaitForAsync();

        var h1 = page.Locator(".hdr h1");
        var titleText = await h1.TextContentAsync();
        titleText.Should().NotBeNullOrEmpty();

        var backlogLink = page.Locator(".hdr h1 a");
        var href = await backlogLink.GetAttributeAsync("href");
        href.Should().NotBeNullOrEmpty();

        await page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_RendersTimelineSectionWithStreams()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var timelineArea = page.Locator(".tl-area");
        await timelineArea.WaitForAsync();
        (await timelineArea.IsVisibleAsync()).Should().BeTrue();

        var svg = page.Locator(".tl-svg-box svg");
        (await svg.IsVisibleAsync()).Should().BeTrue();

        // Verify NOW line exists (red dashed line)
        var nowLine = page.Locator("line[stroke='#EA4335']");
        (await nowLine.CountAsync()).Should().BeGreaterThan(0);

        await page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_RendersHeatmapGridWithCategories()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var heatmapWrap = page.Locator(".hm-wrap");
        await heatmapWrap.WaitForAsync();

        var title = page.Locator(".hm-title");
        var titleText = await title.TextContentAsync();
        titleText.Should().Contain("Monthly Execution Heatmap");

        var grid = page.Locator(".hm-grid");
        (await grid.IsVisibleAsync()).Should().BeTrue();

        // Verify corner cell says "Status"
        var corner = page.Locator(".hm-corner");
        var cornerText = await corner.TextContentAsync();
        cornerText.Should().Contain("Status");

        await page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_CurrentMonthColumnIsHighlighted()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Check for highlighted current month header
        var currentMonthHeader = page.Locator(".hm-col-hdr.apr-hdr");
        (await currentMonthHeader.CountAsync()).Should().BeGreaterThan(0);

        var headerText = await currentMonthHeader.TextContentAsync();
        headerText.Should().Contain("Now");

        await page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_NoScrollbarsAt1920x1080()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Check that page doesn't have scroll by evaluating overflow
        var hasScrollbar = await page.EvaluateAsync<bool>("""
            () => {
                return document.documentElement.scrollHeight > document.documentElement.clientHeight ||
                       document.documentElement.scrollWidth > document.documentElement.clientWidth;
            }
        """);

        hasScrollbar.Should().BeFalse("Dashboard should render at 1920x1080 without scrollbars");

        await page.CloseAsync();
    }
}