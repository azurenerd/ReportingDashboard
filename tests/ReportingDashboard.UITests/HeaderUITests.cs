using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeaderUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;

    public HeaderUITests(PlaywrightFixture fixture)
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
    [Trait("Category", "UI")]
    public async Task Header_DisplaysTitleAndSubtitle()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = _page.Locator("div.hdr");
        await header.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var title = _page.Locator("span.title");
        var titleText = await title.TextContentAsync();
        titleText.Should().NotBeNullOrEmpty();

        var subtitle = _page.Locator("div.sub");
        var subtitleText = await subtitle.TextContentAsync();
        subtitleText.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Header_DisplaysLegendWithFourItems()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var legendItems = _page.Locator("div.legend-item");
        var count = await legendItems.CountAsync();
        count.Should().Be(4);

        (await _page.Locator("div.legend-item").Nth(0).TextContentAsync()).Should().Contain("PoC Milestone");
        (await _page.Locator("div.legend-item").Nth(1).TextContentAsync()).Should().Contain("Production Release");
        (await _page.Locator("div.legend-item").Nth(2).TextContentAsync()).Should().Contain("Checkpoint");
        (await _page.Locator("div.legend-item").Nth(3).TextContentAsync()).Should().Contain("Now");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Header_BacklogLinkOpensInNewTab()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var link = _page.Locator("a.ado-link");
        var linkCount = await link.CountAsync();

        if (linkCount > 0)
        {
            var target = await link.GetAttributeAsync("target");
            target.Should().Be("_blank");

            var rel = await link.GetAttributeAsync("rel");
            rel.Should().Be("noopener noreferrer");
        }
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Header_HasCorrectLayoutStructure()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = _page.Locator("div.hdr");
        await header.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var hdrLeft = _page.Locator("div.hdr-left");
        (await hdrLeft.CountAsync()).Should().Be(1);

        var legend = _page.Locator("div.legend");
        (await legend.CountAsync()).Should().Be(1);
    }
}