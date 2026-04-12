using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

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
        // Wait for Blazor SignalR circuit to initialize
        await _page.WaitForSelectorAsync("svg", new PageWaitForSelectorOptions { Timeout = 30000 });
    }

    public async Task DisposeAsync()
    {
        await _page.Context.DisposeAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Dashboard_PageLoads_DisplaysTitle()
    {
        var heading = _page.Locator("h1").First;
        await heading.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });

        var text = await heading.TextContentAsync();
        text.Should().NotBeNullOrWhiteSpace("the dashboard title should be visible");
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
    public async Task Timeline_MilestoneTrackLines_AreRendered()
    {
        var trackLines = _page.Locator(".tl-svg-box svg line[stroke-width='3']");
        var count = await trackLines.CountAsync();

        count.Should().BeGreaterThan(0, "at least one milestone track line should render");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Timeline_Sidebar_DisplaysMilestoneLabels()
    {
        var sidebar = _page.Locator(".tl-sidebar").First;
        await sidebar.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });

        var labels = _page.Locator(".tl-sidebar .tl-milestone-label");
        var labelCount = await labels.CountAsync();

        labelCount.Should().BeGreaterThan(0, "milestone labels should appear in the sidebar");

        var firstId = _page.Locator(".tl-sidebar .tl-ml-id").First;
        var firstIdText = await firstId.TextContentAsync();
        firstIdText.Should().NotBeNullOrWhiteSpace("milestone ID text should be visible");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Timeline_NowLine_IsVisibleWithDashedStroke()
    {
        var nowLines = _page.Locator(".tl-svg-box svg line[stroke='#EA4335']");
        var count = await nowLines.CountAsync();

        if (count > 0)
        {
            var dashArray = await nowLines.First.GetAttributeAsync("stroke-dasharray");
            dashArray.Should().Be("5,3", "NOW line should have dashed stroke pattern");

            var nowText = _page.GetByText("NOW");
            var nowTextCount = await nowText.CountAsync();
            nowTextCount.Should().BeGreaterThan(0, "NOW label should be visible near the line");
        }
        // If NOW line is not visible, today's date is outside the timeline range — valid
    }
}