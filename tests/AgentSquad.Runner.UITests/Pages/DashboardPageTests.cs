using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace AgentSquad.Runner.UITests.Pages;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class DashboardPageTests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardPageTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dashboard_Loads_Successfully()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var response = _fixture.Page.Url;
        response.Should().StartWith(_fixture.BaseUrl);
    }

    [Fact]
    public async Task Dashboard_DisplaysProjectName()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = await _fixture.Page.QuerySelectorAsync("h1");
        var title = await header?.TextContentAsync();

        title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Dashboard_DisplaysDescription()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var description = await _fixture.Page.QuerySelectorAsync(".sub");
        var text = await description?.TextContentAsync();

        text.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Dashboard_DisplaysLegend()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var legendItems = await _fixture.Page.QuerySelectorAllAsync("[style*='display:flex'][style*='gap:']");

        legendItems.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Dashboard_DisplaysHeatmapGrid()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var grid = await _fixture.Page.QuerySelectorAsync(".hm-grid");

        grid.Should().NotBeNull();
    }

    [Fact]
    public async Task Dashboard_DisplaysTimelineArea()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var timeline = await _fixture.Page.QuerySelectorAsync(".tl-area");

        timeline.Should().NotBeNull();
    }

    [Fact]
    public async Task Dashboard_DisplaysSvgElements()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var svgElements = await _fixture.Page.QuerySelectorAllAsync("svg");

        svgElements.Count.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task Dashboard_HeaderContainsAdoBacklogLink()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var link = await _fixture.Page.QuerySelectorAsync("a");
        var href = await link?.GetAttributeAsync("href");

        href.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Dashboard_HeatmapContainsStatusRows()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var rowHeaders = await _fixture.Page.QuerySelectorAllAsync(".hm-row-hdr");

        rowHeaders.Count.Should().BeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public async Task Dashboard_HeatmapContainsDataCells()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var cells = await _fixture.Page.QuerySelectorAllAsync(".hm-cell");

        cells.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Dashboard_HeaderHasCorrectHeight()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = await _fixture.Page.QuerySelectorAsync(".hdr");
        var boundingBox = await header?.BoundingBoxAsync();

        boundingBox.Should().NotBeNull();
        boundingBox!.Height.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Dashboard_NoConsoleErrors()
    {
        var consoleMessages = new List<string>();
        _fixture.Page!.Console += (sender, msg) =>
        {
            if (msg.Type == "error")
            {
                consoleMessages.Add(msg.Text);
            }
        };

        await _fixture.Page.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _fixture.Page.WaitForTimeoutAsync(1000);

        consoleMessages.Should().BeEmpty("There should be no console errors");
    }

    [Fact]
    public async Task Dashboard_StylesAreApplied()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var body = await _fixture.Page.QuerySelectorAsync("body");
        var fontSize = await body?.EvaluateAsync<string>("el => window.getComputedStyle(el).fontSize");

        fontSize.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Dashboard_RespondsToRefresh()
    {
        await _fixture.Page!.GotoAsync(_fixture.BaseUrl);
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var initialTitle = await _fixture.Page.QuerySelectorAsync("h1");
        var initialText = await initialTitle?.TextContentAsync();

        await _fixture.Page.ReloadAsync();
        await _fixture.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var reloadedTitle = await _fixture.Page.QuerySelectorAsync("h1");
        var reloadedText = await reloadedTitle?.TextContentAsync();

        reloadedText.Should().Be(initialText);
    }
}