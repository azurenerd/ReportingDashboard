#nullable enable

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
    public async Task Dashboard_LoadsAndRendersTimelineAndHeatmapSuccessfully()
    {
        var page = _fixture.Page!;

        await page.GotoAsync($"{_fixture.BaseUrl}/dashboard");

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var timelineContainer = await page.QuerySelectorAsync(".timeline-container");
        timelineContainer.Should().NotBeNull("Timeline container should be rendered");

        var heatmapGrid = await page.QuerySelectorAsync(".hm-grid");
        heatmapGrid.Should().NotBeNull("Heatmap grid should be rendered");

        var headerText = await page.TextContentAsync("h1");
        headerText.Should().NotBeNullOrEmpty("Header should display project name");
    }
}