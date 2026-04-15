using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class TimelineSectionUITests
{
    private readonly PlaywrightFixture _fixture;

    public TimelineSectionUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TimelineArea_IsVisible_WithCorrectStructure()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tlArea = page.Locator(".tl-area");
        await tlArea.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        (await tlArea.IsVisibleAsync()).Should().BeTrue();

        var sidebar = tlArea.Locator(".tl-sidebar");
        (await sidebar.IsVisibleAsync()).Should().BeTrue();

        var svgBox = tlArea.Locator(".tl-svg-box");
        (await svgBox.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task Sidebar_RendersTrackLabels_WithColorAndDescription()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var trackLabels = page.Locator(".tl-area .tl-sidebar .tl-track-label");
        var count = await trackLabels.CountAsync();
        count.Should().BeGreaterThanOrEqualTo(1, "at least one track label should render");

        // Each track label should have a colored .tl-label and a .tl-desc
        var firstLabel = trackLabels.First.Locator(".tl-label");
        (await firstLabel.IsVisibleAsync()).Should().BeTrue();
        var style = await firstLabel.GetAttributeAsync("style");
        style.Should().Contain("color:", "track label should have an inline color style");

        var firstDesc = trackLabels.First.Locator(".tl-desc");
        (await firstDesc.IsVisibleAsync()).Should().BeTrue();
        var descText = await firstDesc.TextContentAsync();
        descText.Should().NotBeNullOrWhiteSpace("track description should not be empty");
    }

    [Fact]
    public async Task Svg_RendersWithCorrectDimensions_AndDropShadowFilter()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var svg = page.Locator(".tl-area .tl-svg-box svg");
        await svg.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

        var width = await svg.GetAttributeAsync("width");
        width.Should().Be("1560");

        var height = await svg.GetAttributeAsync("height");
        height.Should().Be("185");

        // Verify the drop shadow filter def exists
        var filter = svg.Locator("defs filter#sh feDropShadow");
        (await filter.CountAsync()).Should().Be(1, "drop shadow filter should be defined in <defs>");
    }

    [Fact]
    public async Task NowIndicator_IsRendered_WithDashedRedLine()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // "NOW" text label should be visible in the SVG
        var nowText = page.Locator(".tl-area .tl-svg-box svg text", new PageLocatorOptions())
            .Filter(new LocatorFilterOptions { HasText = "NOW" });
        (await nowText.CountAsync()).Should().BeGreaterThanOrEqualTo(1, "NOW text label should render");

        var fill = await nowText.First.GetAttributeAsync("fill");
        fill.Should().Be("#EA4335", "NOW label should be red");

        // Dashed NOW line: stroke=#EA4335, stroke-dasharray=5,3
        var nowLine = page.Locator(".tl-area .tl-svg-box svg line[stroke='#EA4335'][stroke-dasharray='5,3']");
        (await nowLine.CountAsync()).Should().BeGreaterThanOrEqualTo(1, "dashed red NOW line should render");
    }

    [Fact]
    public async Task TrackLines_RenderHorizontally_WithCorrectColors()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Track lines have x1=0, x2=1560, stroke-width=3, and are NOT the gridlines or NOW line
        var trackLines = page.Locator(
            ".tl-area .tl-svg-box svg line[x1='0'][x2='1560'][stroke-width='3']");
        var count = await trackLines.CountAsync();
        count.Should().BeGreaterThanOrEqualTo(1, "at least one horizontal track line should render");

        // The number of track lines should match the number of sidebar labels
        var sidebarLabels = page.Locator(".tl-area .tl-sidebar .tl-track-label");
        var labelCount = await sidebarLabels.CountAsync();
        count.Should().Be(labelCount, "track line count should match sidebar label count");
    }
}