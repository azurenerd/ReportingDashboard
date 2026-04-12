using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace AgentSquad.Runner.UITests.Pages;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class DashboardPageTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage? _page;

    public DashboardPageTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _page = await _fixture.Page!.Context!.NewPageAsync();
        await _page.GotoAsync($"{_fixture.BaseUrl}/");
    }

    public async Task DisposeAsync()
    {
        if (_page != null)
        {
            await _page.CloseAsync();
        }
    }

    [Fact]
    public async Task DashboardPage_LoadsSuccessfully()
    {
        _page!.Url.Should().Contain(_fixture.BaseUrl);
    }

    [Fact]
    public async Task DashboardPage_DisplaysPageTitle()
    {
        var title = await _page!.TitleAsync();

        title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DashboardPage_ContainsHeaderSection()
    {
        var header = await _page!.QuerySelectorAsync(".hdr");

        header.Should().NotBeNull();
    }

    [Fact]
    public async Task DashboardPage_ContainsTimelineSection()
    {
        var timeline = await _page!.QuerySelectorAsync(".tl-area");

        timeline.Should().NotBeNull();
    }

    [Fact]
    public async Task DashboardPage_ContainsHeatmapSection()
    {
        var heatmap = await _page!.QuerySelectorAsync(".hm-wrap");

        heatmap.Should().NotBeNull();
    }

    [Fact]
    public async Task DashboardPage_HeaderContainsProjectName()
    {
        var projectNameElement = await _page!.QuerySelectorAsync(".hdr h1");

        var text = await projectNameElement!.TextContentAsync();
        text.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DashboardPage_HeaderContainsDescription()
    {
        var descriptionElement = await _page!.QuerySelectorAsync(".sub");

        descriptionElement.Should().NotBeNull();
    }

    [Fact]
    public async Task DashboardPage_LegendDisplaysFourItems()
    {
        var legendItems = await _page!.QuerySelectorAllAsync(".legend-item");

        legendItems.Should().HaveCount(4);
    }

    [Fact]
    public async Task DashboardPage_HeatmapDisplaysShippedRow()
    {
        var shippedRow = await _page!.QuerySelectorAsync(".ship-hdr");

        shippedRow.Should().NotBeNull();
    }

    [Fact]
    public async Task DashboardPage_HeatmapDisplaysInProgressRow()
    {
        var progressRow = await _page!.QuerySelectorAsync(".prog-hdr");

        progressRow.Should().NotBeNull();
    }

    [Fact]
    public async Task DashboardPage_HeatmapDisplaysCarryoverRow()
    {
        var carryoverRow = await _page!.QuerySelectorAsync(".carry-hdr");

        carryoverRow.Should().NotBeNull();
    }

    [Fact]
    public async Task DashboardPage_HeatmapDisplaysBlockersRow()
    {
        var blockersRow = await _page!.QuerySelectorAsync(".block-hdr");

        blockersRow.Should().NotBeNull();
    }

    [Fact]
    public async Task DashboardPage_HeatmapGridRendersWithoutErrors()
    {
        var gridContainer = await _page!.QuerySelectorAsync(".hm-grid");

        gridContainer.Should().NotBeNull();
    }

    [Fact]
    public async Task DashboardPage_TimelineSvgRenders()
    {
        var svg = await _page!.QuerySelectorAsync("svg");

        svg.Should().NotBeNull();
    }

    [Fact]
    public async Task DashboardPage_NoConsoleErrors()
    {
        var errors = new List<string>();
        _page!.Console += (_, msg) =>
        {
            if (msg.Type == "error")
            {
                errors.Add(msg.Text);
            }
        };

        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task DashboardPage_CssLoaded()
    {
        var link = await _page!.QuerySelectorAsync("link[rel='stylesheet']");

        link.Should().NotBeNull();
    }

    [Fact]
    public async Task DashboardPage_HeaderLayoutIsFlexible()
    {
        var header = await _page!.QuerySelectorAsync(".hdr");

        var display = await header!.EvaluateAsync<string>("el => window.getComputedStyle(el).display");

        display.Should().Be("flex");
    }

    [Fact]
    public async Task DashboardPage_HeatmapGridLayoutIsGridBased()
    {
        var grid = await _page!.QuerySelectorAsync(".hm-grid");

        var display = await grid!.EvaluateAsync<string>("el => window.getComputedStyle(el).display");

        display.Should().Be("grid");
    }

    [Fact]
    public async Task DashboardPage_LegendItemsVisible()
    {
        var legendItems = await _page!.QuerySelectorAllAsync(".legend-item");

        var allVisible = true;
        foreach (var item in legendItems)
        {
            var isVisible = await item.IsVisibleAsync();
            if (!isVisible) allVisible = false;
        }

        allVisible.Should().BeTrue();
    }

    [Fact]
    public async Task DashboardPage_ADOBacklogLinkPresent()
    {
        var link = await _page!.QuerySelectorAsync(".hdr a");

        link.Should().NotBeNull();
    }

    [Fact]
    public async Task DashboardPage_LastUpdatedTimestampDisplays()
    {
        var lastUpdated = await _page!.QuerySelectorAsync(".last-updated");

        var text = await lastUpdated!.TextContentAsync();
        text.Should().Contain("Last Updated");
    }
}