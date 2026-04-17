using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
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

    [Fact]
    [Trait("Category", "UI")]
    public async Task Heatmap_GridIsVisibleOnPage()
    {
        var heatmapWrap = _page.Locator(".hm-wrap");
        await heatmapWrap.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        (await heatmapWrap.First.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Heatmap_CornerCellDisplaysSTATUS()
    {
        var corner = _page.Locator(".hm-corner").First;
        var text = await corner.TextContentAsync();
        text.Should().Be("STATUS");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Heatmap_CurrentMonthHeaderShowsNowIndicator()
    {
        var currentHeader = _page.Locator(".hm-col-hdr.current").First;
        await currentHeader.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var text = await currentHeader.TextContentAsync();
        text.Should().Contain("Now");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Heatmap_FourCategoryRowHeadersExist()
    {
        var rowHeaders = _page.Locator(".hm-row-hdr");
        // There may be multiple heatmap instances (Heatmap + HeatmapSection), check at least 4
        var count = await rowHeaders.CountAsync();
        count.Should().BeGreaterThanOrEqualTo(4);
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Heatmap_TitleContainsExpectedText()
    {
        var title = _page.Locator(".hm-title").First;
        var text = await title.TextContentAsync();
        text.Should().NotBeNull();
        text!.Should().Contain("MONTHLY EXECUTION HEATMAP");
        text.Should().Contain("SHIPPED");
        text.Should().Contain("BLOCKERS");
    }
}