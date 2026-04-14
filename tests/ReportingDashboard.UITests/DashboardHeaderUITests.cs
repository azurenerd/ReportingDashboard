using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardHeaderUITests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardHeaderUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Header_DisplaysProjectTitle()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var h1 = page.Locator("header.hdr h1");
        await h1.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var text = await h1.TextContentAsync();

        Assert.NotNull(text);
        Assert.False(string.IsNullOrWhiteSpace(text));
        // Title is data-driven, just verify h1 has content
        Assert.Contains("ADO Backlog", text);
    }

    [Fact]
    public async Task Header_DisplaysBacklogLink_WithTargetBlank()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var link = page.Locator("header.hdr h1 a");
        await link.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var target = await link.GetAttributeAsync("target");
        Assert.Equal("_blank", target);

        var href = await link.GetAttributeAsync("href");
        Assert.NotNull(href);
        Assert.NotEmpty(href);
    }

    [Fact]
    public async Task Header_DisplaysSubtitle()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var sub = page.Locator("header.hdr .sub");
        await sub.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        var text = await sub.TextContentAsync();

        Assert.NotNull(text);
        Assert.False(string.IsNullOrWhiteSpace(text));
    }

    [Fact]
    public async Task Header_DisplaysFourLegendItems()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var items = page.Locator("header.hdr .legend .leg-item");
        await items.First.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        Assert.Equal(4, await items.CountAsync());
        Assert.Contains("PoC Milestone", await items.Nth(0).TextContentAsync());
        Assert.Contains("Production Release", await items.Nth(1).TextContentAsync());
        Assert.Contains("Checkpoint", await items.Nth(2).TextContentAsync());
        Assert.Contains("Now", await items.Nth(3).TextContentAsync());
    }

    [Fact]
    public async Task Header_HasCorrectLayoutStructure()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = page.Locator("header.hdr");
        await header.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        // Verify left and right sections exist
        var hdrLeft = page.Locator("header.hdr .hdr-left");
        Assert.Equal(1, await hdrLeft.CountAsync());

        var legend = page.Locator("header.hdr .legend");
        Assert.Equal(1, await legend.CountAsync());

        // Verify legend marker shape elements exist
        Assert.Equal(1, await page.Locator("header.hdr .diamond.poc").CountAsync());
        Assert.Equal(1, await page.Locator("header.hdr .diamond.prod").CountAsync());
        Assert.Equal(1, await page.Locator("header.hdr .dot.chk").CountAsync());
        Assert.Equal(1, await page.Locator("header.hdr .bar.now").CountAsync());
    }
}