using FluentAssertions;
using ReportingDashboard.UITests.Pages;
using Xunit;

namespace ReportingDashboard.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class DashboardUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private DashboardPageObject? _page;

    public DashboardUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.InitializeAsync();
        _page = new DashboardPageObject(_fixture.Page!);
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task Dashboard_WithValidData_LoadsSuccessfully()
    {
        await _page!.NavigateAsync(_fixture.BaseUrl);

        var headerTitle = await _page.GetHeaderTitleAsync();
        headerTitle.Should().NotBeEmpty();
        headerTitle.Should().Contain("ADO Backlog");
    }

    [Fact]
    public async Task Dashboard_HeaderSection_DisplaysProjectTitle()
    {
        await _page!.NavigateAsync(_fixture.BaseUrl);

        var headerTitle = await _page.GetHeaderTitleAsync();
        headerTitle.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Dashboard_TimelineSection_IsVisible()
    {
        await _page!.NavigateAsync(_fixture.BaseUrl);

        var timelineVisible = await _page.TimelineAreaVisibleAsync();
        timelineVisible.Should().BeTrue();
    }

    [Fact]
    public async Task Dashboard_HeatmapSection_IsVisible()
    {
        await _page!.NavigateAsync(_fixture.BaseUrl);

        var heatmapVisible = await _page.HeatmapAreaVisibleAsync();
        heatmapVisible.Should().BeTrue();

        var heatmapTitle = await _page.GetHeatmapTitleAsync();
        heatmapTitle.Should().Contain("Monthly Execution Heatmap");
    }

    [Fact]
    public async Task Dashboard_BacklogLink_IsClickable()
    {
        await _page!.NavigateAsync(_fixture.BaseUrl);

        var backlogHref = await _page.GetBacklogLinkHrefAsync();
        backlogHref.Should().NotBeEmpty();
        backlogHref.Should().NotBe("#");
    }
}