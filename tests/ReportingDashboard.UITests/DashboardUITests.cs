using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;

    public DashboardUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _page = await _fixture.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        _page.SetDefaultTimeout(60000);
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
    }

    [Fact]
    public async Task DashboardLoads_ShowsHeaderWithTitle()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var h1 = _page.Locator("h1");
        var text = await h1.TextContentAsync();
        text.Should().NotBeNullOrWhiteSpace("The dashboard should display a project title in the header");
    }

    [Fact]
    public async Task Header_DisplaysLegendItems()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var pocText = await _page.GetByText("PoC Milestone").CountAsync();
        pocText.Should().BeGreaterThan(0);

        var prodText = await _page.GetByText("Production Release").CountAsync();
        prodText.Should().BeGreaterThan(0);

        var checkpointText = await _page.GetByText("Checkpoint").CountAsync();
        checkpointText.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Timeline_RendersSvgElement()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var svg = _page.Locator(".tl-svg-box svg");
        var count = await svg.CountAsync();
        count.Should().BeGreaterThan(0, "Timeline should render an SVG element");

        var width = await svg.First.GetAttributeAsync("width");
        width.Should().Be("1560");

        var height = await svg.First.GetAttributeAsync("height");
        height.Should().Be("185");
    }

    [Fact]
    public async Task Heatmap_RendersGridWithStatusCorner()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var corner = _page.Locator(".hm-corner");
        var cornerText = await corner.TextContentAsync();
        cornerText.Should().Contain("Status");

        var colHeaders = _page.Locator(".hm-col-hdr");
        var headerCount = await colHeaders.CountAsync();
        headerCount.Should().BeGreaterOrEqualTo(2, "Heatmap should have at least 2 month columns");
    }

    [Fact]
    public async Task Heatmap_HighlightedMonth_HasSpecialStyling()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var highlightHeader = _page.Locator(".hm-col-hdr.apr-hdr");
        var count = await highlightHeader.CountAsync();
        count.Should().BeGreaterThan(0, "There should be a highlighted month column with apr-hdr class");

        var text = await highlightHeader.First.TextContentAsync();
        text.Should().Contain("Now", "Highlighted month header should contain 'Now' indicator");
    }
}