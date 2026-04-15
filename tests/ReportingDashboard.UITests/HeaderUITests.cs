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
        _page = await _fixture.CreatePageAsync();
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
    }

    [Fact]
    public async Task Header_DisplaysTitleInH1()
    {
        var h1 = _page.Locator(".hdr h1");
        await h1.WaitForAsync();
        var text = await h1.TextContentAsync();
        text.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Header_DisplaysSubtitle()
    {
        var sub = _page.Locator(".hdr .sub");
        await sub.WaitForAsync();
        var text = await sub.TextContentAsync();
        text.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Header_DisplaysFourLegendItems()
    {
        var legendItems = _page.Locator(".hdr-legend .legend-item");
        var count = await legendItems.CountAsync();
        count.Should().Be(4);

        var labels = _page.Locator(".hdr-legend .legend-label");
        (await labels.Nth(0).TextContentAsync()).Should().Be("PoC Milestone");
        (await labels.Nth(1).TextContentAsync()).Should().Be("Production Release");
        (await labels.Nth(2).TextContentAsync()).Should().Be("Checkpoint");
        (await labels.Nth(3).TextContentAsync()).Should().Be("Now");
    }

    [Fact]
    public async Task Header_HasHdrContainerWithFlexLayout()
    {
        var hdr = _page.Locator(".hdr");
        await hdr.WaitForAsync();
        var display = await hdr.EvaluateAsync<string>("el => getComputedStyle(el).display");
        display.Should().Be("flex");
    }

    [Fact]
    public async Task Header_BacklogLinkOpensInNewTab_WhenPresent()
    {
        var link = _page.Locator(".hdr h1 a");
        var count = await link.CountAsync();
        if (count > 0)
        {
            var target = await link.GetAttributeAsync("target");
            target.Should().Be("_blank");
            var href = await link.GetAttributeAsync("href");
            href.Should().NotBeNullOrWhiteSpace();
        }
    }
}