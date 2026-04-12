using Microsoft.Playwright;
using ReportingDashboard.UITests.Pages;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;
    private DashboardPage _dashboard = null!;

    public DashboardUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _page = await _fixture.CreatePageAsync();
        _dashboard = new DashboardPage(_page, _fixture.BaseUrl);
    }

    public async Task DisposeAsync()
    {
        await _page.Context.DisposeAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Dashboard_LoadsWithCorrectViewportDimensions()
    {
        await _dashboard.NavigateAsync();
        await _dashboard.WaitForDashboardAsync();

        var root = _dashboard.DashboardRoot;
        var box = await root.BoundingBoxAsync();

        Assert.NotNull(box);
        Assert.Equal(1920, box.Width, 2);
        Assert.Equal(1080, box.Height, 2);
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Dashboard_DisplaysHeaderWithProjectNameAndBacklogLink()
    {
        await _dashboard.NavigateAsync();
        await _dashboard.WaitForDashboardAsync();

        var headerText = await _dashboard.HeaderTitle.InnerTextAsync();
        Assert.Contains("Privacy Automation Release Roadmap", headerText);

        var linkVisible = await _dashboard.BacklogLink.IsVisibleAsync();
        Assert.True(linkVisible, "ADO Backlog link should be visible");

        var href = await _dashboard.BacklogLink.GetAttributeAsync("href");
        Assert.NotNull(href);
        Assert.StartsWith("http", href);
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Dashboard_DisplaysTimelineWithMilestonesAndNowLine()
    {
        await _dashboard.NavigateAsync();
        await _dashboard.WaitForDashboardAsync();

        var timelineVisible = await _dashboard.TimelineArea.IsVisibleAsync();
        Assert.True(timelineVisible, "Timeline area should be visible");

        var milestoneCount = await _dashboard.TimelineLabels.CountAsync();
        Assert.True(milestoneCount >= 2, $"Expected at least 2 milestones, got {milestoneCount}");

        var svgVisible = await _dashboard.TimelineSvg.IsVisibleAsync();
        Assert.True(svgVisible, "Timeline SVG should be rendered");

        var nowVisible = await _dashboard.NowLine.IsVisibleAsync();
        Assert.True(nowVisible, "NOW line indicator should be visible");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Dashboard_DisplaysHeatmapWithAllSections()
    {
        await _dashboard.NavigateAsync();
        await _dashboard.WaitForDashboardAsync();

        var heatmapVisible = await _dashboard.HeatmapWrap.IsVisibleAsync();
        Assert.True(heatmapVisible, "Heatmap section should be visible");

        var titleText = await _dashboard.HeatmapTitle.InnerTextAsync();
        Assert.Contains("HEATMAP", titleText.ToUpperInvariant());

        var colHeaderCount = await _dashboard.HeatmapColumnHeaders.CountAsync();
        Assert.True(colHeaderCount >= 4, $"Expected at least 4 month columns, got {colHeaderCount}");

        var currentColVisible = await _dashboard.CurrentColumnHeader.IsVisibleAsync();
        Assert.True(currentColVisible, "Current month column should be highlighted");

        var rowHeaderCount = await _dashboard.HeatmapRowHeaders.CountAsync();
        Assert.Equal(4, rowHeaderCount); // Shipped, In Progress, Carryover, Blockers
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Dashboard_HasNoHorizontalOrVerticalScrollbars()
    {
        await _dashboard.NavigateAsync();
        await _dashboard.WaitForDashboardAsync();

        var overflow = await _dashboard.DashboardRoot.EvaluateAsync<string>(
            "el => window.getComputedStyle(el).overflow");
        Assert.Equal("hidden", overflow);

        var hasHScroll = await _page.EvaluateAsync<bool>(
            "() => document.documentElement.scrollWidth > document.documentElement.clientWidth");
        Assert.False(hasHScroll, "Page should not have horizontal scrollbar");
    }
}