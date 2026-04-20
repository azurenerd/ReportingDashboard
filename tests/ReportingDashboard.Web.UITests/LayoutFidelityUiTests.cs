using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.Web.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class LayoutFidelityUiTests
{
    private readonly PlaywrightFixture _fx;

    public LayoutFidelityUiTests(PlaywrightFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task Dashboard_TimelineArea_Is196PxTall()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl + "/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var height = await page.Locator("div.tl-area").First.EvaluateAsync<int>("el => el.clientHeight");
        height.Should().Be(196);
    }

    [Fact]
    public async Task Dashboard_HeatmapGrid_HasCorrectTemplateColumns()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl + "/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var display = await page.Locator("div.hm-grid").First
            .EvaluateAsync<string>("el => getComputedStyle(el).display");
        display.Should().Be("grid");
    }

    [Fact]
    public async Task Dashboard_HmCornerShowsStatusText()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl + "/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var text = await page.Locator("div.hm-corner").TextContentAsync();
        text.Should().Be("Status");
    }

    [Fact]
    public async Task Dashboard_HasExactly25HeatmapGridChildren()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl + "/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var count = await page.Locator("div.hm-grid > div").CountAsync();
        count.Should().Be(25);
    }

    [Fact]
    public async Task Dashboard_NoLegacyAprClassInRenderedDom()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl + "/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var html = await page.ContentAsync();
        html.Should().NotContain("apr-hdr");
        html.Should().NotContain("\"apr\"");
    }
}