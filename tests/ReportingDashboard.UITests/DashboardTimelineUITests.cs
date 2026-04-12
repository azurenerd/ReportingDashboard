using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

/// <summary>
/// Tests for the Timeline region of Dashboard.razor.
/// Covers: SVG dimensions, milestone tracks, sidebar labels, NOW line, event markers.
/// Improved: uses selectors matching actual Dashboard.razor markup (.tl-ms, .tl-ms-lbl, .tl-ms-desc).
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardTimelineUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;

    public DashboardTimelineUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _page = await _fixture.CreatePageAsync();
        await _page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });
        await _page.WaitForSelectorAsync("svg", new PageWaitForSelectorOptions { Timeout = 30000 });
    }

    public async Task DisposeAsync()
    {
        await _page.Context.DisposeAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Timeline_SvgElement_RendersWithCorrectDimensions()
    {
        var svg = _page.Locator(".tl-svg-box svg").First;
        await svg.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });

        var width = await svg.GetAttributeAsync("width");
        var height = await svg.GetAttributeAsync("height");

        width.Should().Be("1560", "SVG width must be 1560px per spec");
        height.Should().Be("185", "SVG height must be 185px per spec");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Timeline_Sidebar_DisplaysMilestoneLabelsAndDescriptions()
    {
        var sidebar = _page.Locator(".tl-sidebar").First;
        await sidebar.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });

        // Dashboard.razor renders each milestone as .tl-ms with .tl-ms-lbl and .tl-ms-desc
        var milestoneEntries = _page.Locator(".tl-sidebar .tl-ms");
        var count = await milestoneEntries.CountAsync();
        count.Should().BeGreaterThan(0, "at least one milestone entry should appear in the sidebar");

        var firstLabel = _page.Locator(".tl-sidebar .tl-ms-lbl").First;
        var labelText = await firstLabel.TextContentAsync();
        labelText.Should().NotBeNullOrWhiteSpace("milestone label (e.g., 'M1') should be visible");

        var firstDesc = _page.Locator(".tl-sidebar .tl-ms-desc").First;
        var descText = await firstDesc.TextContentAsync();
        descText.Should().NotBeNullOrWhiteSpace("milestone description should be visible");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Timeline_MilestoneTrackLines_RenderForEachMilestone()
    {
        // Milestone tracks are SVG lines with stroke-width="3"
        var trackLines = _page.Locator(".tl-svg-box svg line[stroke-width='3']");
        var count = await trackLines.CountAsync();
        count.Should().BeGreaterThan(0, "at least one milestone track line should render in the SVG");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Timeline_NowLine_HasDashedRedStroke()
    {
        var nowLines = _page.Locator(".tl-svg-box svg line[stroke='#EA4335']");
        var count = await nowLines.CountAsync();

        if (count > 0)
        {
            var strokeWidth = await nowLines.First.GetAttributeAsync("stroke-width");
            strokeWidth.Should().Be("2", "NOW line stroke-width should be 2");

            var dashArray = await nowLines.First.GetAttributeAsync("stroke-dasharray");
            dashArray.Should().Be("5,3", "NOW line should have dashed stroke pattern 5,3");

            var nowText = _page.GetByText("NOW");
            (await nowText.CountAsync()).Should().BeGreaterThan(0, "NOW text label should be visible");
        }
        // If NOW line is absent, today's date is outside the timeline range — valid scenario
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Timeline_EventMarkers_RenderDiamondsAndCircles()
    {
        // PoC events = gold diamond polygons, Production = green diamond, Checkpoint = circles
        var polygons = _page.Locator(".tl-svg-box svg polygon");
        var circles = _page.Locator(".tl-svg-box svg circle");

        var polygonCount = await polygons.CountAsync();
        var circleCount = await circles.CountAsync();

        // At least some event markers should exist if milestones have events in data.json
        (polygonCount + circleCount).Should().BeGreaterThan(0,
            "timeline should render at least one event marker (diamond polygon or checkpoint circle)");

        // If polygons exist, verify they use the expected fill colors
        if (polygonCount > 0)
        {
            var firstFill = await polygons.First.GetAttributeAsync("fill");
            firstFill.Should().NotBeNullOrWhiteSpace("diamond markers should have a fill color");
            // Valid fills: #F4B400 (PoC gold) or #34A853 (Production green)
            firstFill.Should().BeOneOf("#F4B400", "#34A853",
                "diamond fill should be gold (PoC) or green (Production)");
        }
    }
}