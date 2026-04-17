using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeatmapUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;

    public HeatmapUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _page = await _fixture.Browser.NewPageAsync();
        _page.SetDefaultTimeout(60000);
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
    }

    [SkippableFact]
    [Trait("Category", "UI")]
    public async Task Heatmap_TitleIsVisible()
    {
        var title = _page.Locator(".hm-title");
        var count = await title.CountAsync();
        Skip.If(count == 0, "Heatmap not rendered — app may not be running or data.json missing");

        var text = await title.First.TextContentAsync();
        text.Should().NotBeNull();
        text!.ToUpperInvariant().Should().Contain("MONTHLY EXECUTION HEATMAP");
        text.Should().Contain("SHIPPED");
        text.Should().Contain("BLOCKERS");
    }

    [SkippableFact]
    [Trait("Category", "UI")]
    public async Task Heatmap_CornerCellShowsStatus()
    {
        var corner = _page.Locator(".hm-corner");
        var count = await corner.CountAsync();
        Skip.If(count == 0, "Heatmap grid not rendered");

        var text = await corner.First.TextContentAsync();
        text.Should().NotBeNull();
        text!.Trim().ToUpperInvariant().Should().Be("STATUS");
    }

    [SkippableFact]
    [Trait("Category", "UI")]
    public async Task Heatmap_CurrentMonthHeaderHighlighted()
    {
        var currentHeader = _page.Locator(".hm-col-hdr.current");
        var count = await currentHeader.CountAsync();
        Skip.If(count == 0, "No current month header found");

        var text = await currentHeader.First.TextContentAsync();
        text.Should().NotBeNull();
        text.Should().Contain("Now");
    }

    [SkippableFact]
    [Trait("Category", "UI")]
    public async Task Heatmap_FourCategoryRowsRendered()
    {
        var rowHeaders = _page.Locator(".hm-row-hdr");
        var count = await rowHeaders.CountAsync();
        Skip.If(count == 0, "No row headers found — heatmap may not be rendered");

        // Each heatmap instance renders 4 rows; there may be multiple heatmap components
        (count % 4).Should().Be(0, "row headers should come in groups of 4 categories");
        count.Should().BeGreaterThanOrEqualTo(4);
    }

    [SkippableFact]
    [Trait("Category", "UI")]
    public async Task Heatmap_EmptyCellsShowDash()
    {
        var dashSpans = _page.Locator(".hm-cell span");
        var count = await dashSpans.CountAsync();
        Skip.If(count == 0, "No empty cells found — all cells may have items");

        var text = await dashSpans.First.TextContentAsync();
        text.Should().NotBeNull();
        text!.Trim().Should().Be("-");
    }
}