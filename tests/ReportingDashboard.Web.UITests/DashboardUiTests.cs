using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.Web.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class DashboardUiTests
{
    private readonly PlaywrightFixture _fx;

    public DashboardUiTests(PlaywrightFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task Dashboard_LoadsAndRendersThreeLayoutBands()
    {
        var page = await _fx.NewPageAsync();
        var resp = await page.GotoAsync(_fx.BaseUrl + "/");
        resp!.Status.Should().Be(200);

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        (await page.Locator("div.hdr").CountAsync()).Should().BeGreaterThan(0);
        (await page.Locator("div.tl-area").CountAsync()).Should().BeGreaterThan(0);
        (await page.Locator("div.hm-wrap").CountAsync()).Should().BeGreaterThan(0);
        (await page.Locator("div.hm-grid").CountAsync()).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Dashboard_HeatmapTitle_IsVisible()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl + "/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var title = page.Locator("div.hm-title");
        (await title.CountAsync()).Should().Be(1);
        var text = await title.TextContentAsync();
        text.Should().Contain("Monthly Execution Heatmap");
    }

    [Fact]
    public async Task Dashboard_RowHeaders_RenderAllFourCategories()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl + "/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        (await page.Locator("div.hm-row-hdr.ship-hdr").TextContentAsync()).Should().Be("Shipped");
        (await page.Locator("div.hm-row-hdr.prog-hdr").TextContentAsync()).Should().Be("In Progress");
        (await page.Locator("div.hm-row-hdr.carry-hdr").TextContentAsync()).Should().Be("Carryover");
        (await page.Locator("div.hm-row-hdr.block-hdr").TextContentAsync()).Should().Be("Blockers");
    }

    [Fact]
    public async Task Dashboard_DoesNotLoadBlazorInteractiveRuntime()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl + "/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var html = await page.ContentAsync();
        html.Should().NotContain("blazor.server.js");
        html.Should().NotContain("blazor.web.js");
        html.Should().NotContain("components-reconnect-modal");
    }

    [Fact]
    public async Task Dashboard_BodyIsConstrainedTo1920x1080_NoScrollbars()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl + "/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var dims = await page.EvaluateAsync<int[]>(@"() => {
            const b = document.body;
            const cs = getComputedStyle(b);
            return [b.clientWidth, b.clientHeight, b.scrollWidth, b.scrollHeight];
        }");

        dims[0].Should().Be(1920);
        dims[1].Should().Be(1080);
        dims[2].Should().BeLessThanOrEqualTo(1920);
        dims[3].Should().BeLessThanOrEqualTo(1080);
    }
}