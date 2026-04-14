using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class TimelineSectionUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;

    public TimelineSectionUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _page = await _fixture.Browser.NewPageAsync();
        _page.SetDefaultTimeout(60000);
        await _page.SetViewportSizeAsync(1920, 1080);
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
    }

    [Fact]
    public async Task TimelineSection_IsVisible_WithCorrectLayout()
    {
        var tlArea = _page.Locator(".tl-area");
        await tlArea.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        var box = await tlArea.BoundingBoxAsync();
        box.Should().NotBeNull();
        // Timeline area should have approximately 196px height per spec
        box!.Height.Should().BeGreaterThan(150).And.BeLessThan(250);
    }

    [Fact]
    public async Task Sidebar_DisplaysTrackLabels()
    {
        var sidebar = _page.Locator(".tl-sidebar");
        await sidebar.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        var trackLabels = sidebar.Locator(".tl-track-label");
        var count = await trackLabels.CountAsync();
        count.Should().BeGreaterOrEqualTo(1, "at least one track label should be present");

        // First track label should have text content
        var firstLabel = trackLabels.First;
        var text = await firstLabel.TextContentAsync();
        text.Should().NotBeNullOrWhiteSpace("track label should have text");
    }

    [Fact]
    public async Task Svg_ContainsMonthGridlines()
    {
        var svg = _page.Locator(".tl-svg-box svg");
        await svg.WaitForAsync(new() { State = WaitForSelectorState.Attached });

        // Month gridlines are <line> elements with stroke="#bbb" and opacity="0.4"
        var gridlines = svg.Locator("line[stroke='#bbb']");
        var count = await gridlines.CountAsync();
        count.Should().BeGreaterOrEqualTo(1, "month gridlines should be rendered");
    }

    [Fact]
    public async Task NowIndicator_IsDisplayed()
    {
        var svg = _page.Locator(".tl-svg-box svg");
        await svg.WaitForAsync(new() { State = WaitForSelectorState.Attached });

        // NOW line: dashed red line with stroke="#EA4335" and stroke-dasharray="5,3"
        var nowLine = svg.Locator("line[stroke='#EA4335'][stroke-dasharray='5,3']");
        var count = await nowLine.CountAsync();
        count.Should().Be(1, "exactly one NOW indicator line should be present");

        // NOW text label
        var nowText = _page.GetByText("NOW");
        (await nowText.CountAsync()).Should().BeGreaterOrEqualTo(1, "NOW label text should be visible");
    }

    [Fact]
    public async Task Svg_ContainsMilestoneMarkers()
    {
        var svg = _page.Locator(".tl-svg-box svg");
        await svg.WaitForAsync(new() { State = WaitForSelectorState.Attached });

        // Track lines: <line> elements with stroke-width="3"
        var trackLines = svg.Locator("line[stroke-width='3']");
        var trackCount = await trackLines.CountAsync();
        trackCount.Should().BeGreaterOrEqualTo(1, "at least one track line should be rendered");

        // Milestone markers: circles (checkpoint) and/or polygons (PoC/Production)
        var circles = svg.Locator("circle");
        var polygons = svg.Locator("polygon");
        var circleCount = await circles.CountAsync();
        var polygonCount = await polygons.CountAsync();
        (circleCount + polygonCount).Should().BeGreaterOrEqualTo(1, "at least one milestone marker should exist");
    }
}