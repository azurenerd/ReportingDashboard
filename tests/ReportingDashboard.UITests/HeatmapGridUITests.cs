using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeatmapGridUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;

    public HeatmapGridUITests(PlaywrightFixture fixture)
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
    [Trait("Category", "UI")]
    public async Task HeatmapGrid_IsVisibleOnPage()
    {
        var heatmapWrap = _page.Locator(".hm-wrap");
        await heatmapWrap.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        (await heatmapWrap.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapGrid_DisplaysSectionTitle()
    {
        var title = _page.Locator(".hm-title");
        var text = await title.TextContentAsync();
        text.Should().NotBeNull();
        text!.ToUpperInvariant().Should().Contain("MONTHLY EXECUTION HEATMAP");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapGrid_HasFourStatusRows()
    {
        var rowHeaders = _page.Locator(".hm-row-hdr");
        var count = await rowHeaders.CountAsync();
        count.Should().Be(4);

        var first = await rowHeaders.Nth(0).TextContentAsync();
        first.Should().Contain("SHIPPED");

        var second = await rowHeaders.Nth(1).TextContentAsync();
        second.Should().Contain("IN PROGRESS");

        var third = await rowHeaders.Nth(2).TextContentAsync();
        third.Should().Contain("CARRYOVER");

        var fourth = await rowHeaders.Nth(3).TextContentAsync();
        fourth.Should().Contain("BLOCKERS");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapGrid_ShowsStatusCornerCell()
    {
        var corner = _page.Locator(".hm-corner");
        var text = await corner.TextContentAsync();
        text.Should().NotBeNull();
        text!.Trim().ToUpperInvariant().Should().Be("STATUS");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeatmapGrid_RendersMonthColumnHeaders()
    {
        var colHeaders = _page.Locator(".hm-col-hdr");
        var count = await colHeaders.CountAsync();
        count.Should().BeGreaterThan(0, "at least one month column header should render");
    }
}