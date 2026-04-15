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
    public async Task TimelineArea_IsVisible_WithSvgAndSidebar()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tlArea = page.Locator(".tl-area");
        await tlArea.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 60000 });

        (await tlArea.IsVisibleAsync()).Should().BeTrue();

        var sidebar = tlArea.Locator(".tl-sidebar");
        (await sidebar.IsVisibleAsync()).Should().BeTrue();

        var svg = tlArea.Locator(".tl-svg-box svg");
        (await svg.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task MilestoneMarkers_AreRendered_InSvg()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var svg = page.Locator(".tl-svg-box svg");
        await svg.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 60000 });

        // Check that at least one milestone marker (polygon or circle) exists
        var polygons = svg.Locator("polygon");
        var circles = svg.Locator("circle");

        var polygonCount = await polygons.CountAsync();
        var circleCount = await circles.CountAsync();

        (polygonCount + circleCount).Should().BeGreaterThan(0, "at least one milestone marker should be rendered");
    }

    [Fact]
    public async Task DropShadowFilter_ExistsInSvgDefs()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var svg = page.Locator(".tl-svg-box svg");
        await svg.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 60000 });

        var filter = svg.Locator("defs filter#sh");
        (await filter.CountAsync()).Should().Be(1, "SVG should contain a <filter id='sh'> in <defs>");

        var feDropShadow = filter.Locator("feDropShadow");
        (await feDropShadow.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task NowIndicator_RendersRedDashedLine()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var svg = page.Locator(".tl-svg-box svg");
        await svg.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 60000 });

        // Find the NOW text label
        var nowLabel = svg.GetByText("NOW");
        (await nowLabel.CountAsync()).Should().BeGreaterOrEqualTo(1, "NOW label should be rendered in SVG");

        // Find the red dashed line (stroke="#EA4335" with stroke-dasharray)
        var redLines = svg.Locator("line[stroke='#EA4335']");
        (await redLines.CountAsync()).Should().BeGreaterOrEqualTo(1, "Red dashed NOW indicator line should exist");
    }

    [Fact]
    public async Task MilestoneLabels_AreRendered_WithCorrectStyling()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var svg = page.Locator(".tl-svg-box svg");
        await svg.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 60000 });

        // Milestone labels are <text> elements with font-size="10" and fill="#666"
        var milestoneLabels = svg.Locator("text[font-size='10'][fill='#666']");
        var count = await milestoneLabels.CountAsync();
        count.Should().BeGreaterThan(0, "milestone text labels should be rendered");

        // Verify text-anchor is middle
        var firstLabel = milestoneLabels.First;
        var textAnchor = await firstLabel.GetAttributeAsync("text-anchor");
        textAnchor.Should().Be("middle");
    }
}