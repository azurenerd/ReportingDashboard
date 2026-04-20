using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.Web.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class TimelineSvgUiTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fx;
    public TimelineSvgUiTests(PlaywrightFixture fx) => _fx = fx;

    [Fact]
    public async Task Timeline_area_is_rendered_with_svg_canvas()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tlArea = page.Locator(".tl-area");
        (await tlArea.CountAsync()).Should().BeGreaterThan(0);

        var svg = page.Locator(".tl-svg-box svg").First;
        await svg.WaitForAsync(new LocatorWaitForOptions { Timeout = 60000 });
        (await svg.GetAttributeAsync("width")).Should().Be("1560");
        (await svg.GetAttributeAsync("height")).Should().Be("185");
    }

    [Fact]
    public async Task Timeline_defs_contains_dropshadow_filter()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var filter = page.Locator(".tl-svg-box svg defs filter#sh");
        (await filter.CountAsync()).Should().BeGreaterThan(0);
        var drop = page.Locator(".tl-svg-box svg defs filter#sh feDropShadow");
        (await drop.CountAsync()).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Timeline_lane_labels_column_is_present()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var labels = page.Locator(".tl-area .tl-labels");
        (await labels.CountAsync()).Should().BeGreaterThan(0);

        var laneDivs = page.Locator(".tl-area .tl-labels .tl-lane-label");
        (await laneDivs.CountAsync()).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Timeline_has_no_blazor_interactive_script_markers()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var html = await page.ContentAsync();
        html.Should().NotContain("blazor.server.js");
        html.Should().NotContain("_framework/blazor.");
    }
}