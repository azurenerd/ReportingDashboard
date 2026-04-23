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
    public async Task Dashboard_LoadsAndRendersHeader()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var heading = page.Locator("h1");
        var text = await heading.TextContentAsync();
        text.Should().NotBeNullOrEmpty("The dashboard should display a project title in the header");

        await page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_RendersTimelineSvg()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var svg = page.Locator("svg");
        var svgCount = await svg.CountAsync();
        svgCount.Should().BeGreaterOrEqualTo(1, "The timeline should render at least one SVG element");

        await page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_RendersHeatmapGrid()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var heatmapTitle = page.GetByText("Monthly Execution Heatmap");
        (await heatmapTitle.CountAsync()).Should().BeGreaterOrEqualTo(1, "Heatmap title should be visible");

        await page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_LegendShowsFourItems()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var pocLegend = page.GetByText("PoC Milestone");
        (await pocLegend.CountAsync()).Should().BeGreaterOrEqualTo(1);

        var prodLegend = page.GetByText("Production Release");
        (await prodLegend.CountAsync()).Should().BeGreaterOrEqualTo(1);

        var checkpointLegend = page.GetByText("Checkpoint");
        (await checkpointLegend.CountAsync()).Should().BeGreaterOrEqualTo(1);

        await page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_PageFitsViewport_NoScrollbars()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var scrollHeight = await page.EvaluateAsync<int>("document.documentElement.scrollHeight");
        var clientHeight = await page.EvaluateAsync<int>("document.documentElement.clientHeight");

        // The page should fit within the viewport (allow small tolerance for rendering)
        scrollHeight.Should().BeLessOrEqualTo(clientHeight + 5,
            "The dashboard should fit within 1920x1080 without scrollbars");

        await page.CloseAsync();
    }
}