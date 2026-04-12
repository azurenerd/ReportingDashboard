using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace AgentSquad.Runner.UITests.Pages;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class TimelineChartComponentTests
{
    private readonly PlaywrightFixture _fixture;

    public TimelineChartComponentTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TimelineChart_RendersSvgElement()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var timelineArea = await _fixture.Page.QuerySelectorAsync(".tl-area");
        timelineArea.Should().NotBeNull();

        var svg = await timelineArea!.QuerySelectorAsync("svg");
        svg.Should().NotBeNull();
    }

    [Fact]
    public async Task TimelineChart_HasCorrectSvgDimensions()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var svg = await _fixture.Page.QuerySelectorAsync(".tl-svg-box svg");
        
        var width = await svg!.GetAttributeAsync("width");
        var height = await svg.GetAttributeAsync("height");

        width.Should().NotBeNullOrEmpty();
        height.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TimelineChart_DisplaysSvgLines()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var svg = await _fixture.Page.QuerySelectorAsync(".tl-svg-box svg");
        var lines = await svg!.QuerySelectorAllAsync("line");

        lines.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task TimelineChart_DisplaysMilestoneLabels()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tlLabels = await _fixture.Page.QuerySelectorAsync(".tl-labels");
        tlLabels.Should().NotBeNull();

        var text = await tlLabels!.TextContentAsync();
        text.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TimelineChart_ContainsMilestoneIndicators()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var svg = await _fixture.Page.QuerySelectorAsync(".tl-svg-box svg");
        
        // Check for graphical elements
        var elements = await svg!.ContentAsync();
        elements.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TimelineChart_HasProperNesting()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tlArea = await _fixture.Page.QuerySelectorAsync(".tl-area");
        var tlSvgBox = await tlArea!.QuerySelectorAsync(".tl-svg-box");
        
        tlSvgBox.Should().NotBeNull();
    }
}