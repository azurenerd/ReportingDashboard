using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardHeaderUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;

    public DashboardHeaderUITests(PlaywrightFixture fixture)
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
        await _page.Context.DisposeAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeaderRendersWithTitleAndSubtitle()
    {
        var header = _page.Locator("div.hdr");
        await Expect(header).ToBeVisibleAsync();

        var h1 = header.Locator("h1");
        await Expect(h1).ToBeVisibleAsync();
        var h1Text = await h1.TextContentAsync();
        Assert.NotNull(h1Text);
        Assert.False(string.IsNullOrWhiteSpace(h1Text), "H1 title should contain data-driven text");

        var subtitle = header.Locator("div.sub");
        await Expect(subtitle).ToBeVisibleAsync();
        var subText = await subtitle.TextContentAsync();
        Assert.NotNull(subText);
        Assert.False(string.IsNullOrWhiteSpace(subText), "Subtitle should contain data-driven text");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task AdoBacklogLinkIsPresent()
    {
        var link = _page.Locator("div.hdr h1 a");
        await Expect(link).ToBeVisibleAsync();

        var href = await link.GetAttributeAsync("href");
        Assert.NotNull(href);
        Assert.False(string.IsNullOrWhiteSpace(href), "ADO Backlog link should have a non-empty href");

        var linkText = await link.TextContentAsync();
        Assert.Contains("ADO Backlog", linkText);

        var style = await link.GetAttributeAsync("style");
        Assert.NotNull(style);
        Assert.Contains("color:#0078D4", style!.Replace(" ", ""));
        Assert.Contains("text-decoration:none", style!.Replace(" ", ""));
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task LegendDisplaysFourItems()
    {
        var header = _page.Locator("div.hdr");
        await Expect(header).ToBeVisibleAsync();

        await Expect(header.GetByText("PoC Milestone")).ToBeVisibleAsync();
        await Expect(header.GetByText("Production Release")).ToBeVisibleAsync();
        await Expect(header.GetByText("Checkpoint")).ToBeVisibleAsync();
        await Expect(header.GetByText("Now (", new() { Exact = false })).ToBeVisibleAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task HeaderLayoutAt1920x1080_NoOverflow()
    {
        var header = _page.Locator("div.hdr");
        var box = await header.BoundingBoxAsync();
        Assert.NotNull(box);

        // Header should span close to full width and not overflow vertically
        Assert.True(box!.Width > 1800, $"Header width should be near 1920px, was {box.Width}");
        Assert.True(box.Height < 120, $"Header height should be compact, was {box.Height}");

        // Verify no horizontal scrollbar on body
        var hasHorizontalScroll = await _page.EvaluateAsync<bool>(
            "() => document.body.scrollWidth > document.body.clientWidth");
        Assert.False(hasHorizontalScroll, "Page should not have horizontal scrollbar at 1920x1080");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task LegendShapesHaveCorrectColors()
    {
        var header = _page.Locator("div.hdr");

        // PoC Milestone diamond: amber #F4B400, 12x12, rotated
        var pocShape = header.Locator("span[style*='background:#F4B400']");
        await Expect(pocShape).ToBeVisibleAsync();
        var pocStyle = await pocShape.GetAttributeAsync("style");
        Assert.Contains("rotate(45deg)", pocStyle!);
        Assert.Contains("width:12px", pocStyle!.Replace(" ", ""));

        // Production Release diamond: green #34A853, 12x12, rotated
        var prodShape = header.Locator("span[style*='background:#34A853']");
        await Expect(prodShape).ToBeVisibleAsync();
        var prodStyle = await prodShape.GetAttributeAsync("style");
        Assert.Contains("rotate(45deg)", prodStyle!);

        // Checkpoint circle: gray #999, 8x8, border-radius 50%
        var cpShape = header.Locator("span[style*='background:#999']");
        await Expect(cpShape).ToBeVisibleAsync();
        var cpStyle = await cpShape.GetAttributeAsync("style");
        Assert.Contains("border-radius:50%", cpStyle!.Replace(" ", ""));
        Assert.Contains("width:8px", cpStyle!.Replace(" ", ""));

        // Now bar: red #EA4335, 2x14
        var nowShape = header.Locator("span[style*='background:#EA4335']");
        await Expect(nowShape).ToBeVisibleAsync();
        var nowStyle = await nowShape.GetAttributeAsync("style");
        Assert.Contains("width:2px", nowStyle!.Replace(" ", ""));
        Assert.Contains("height:14px", nowStyle!.Replace(" ", ""));
    }

    private static ILocatorAssertions Expect(ILocator locator) =>
        Assertions.Expect(locator);
}