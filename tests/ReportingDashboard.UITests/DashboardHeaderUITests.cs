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
    public async Task Header_RendersProjectTitleAndSubtitle()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = page.Locator(".hdr");
        await Expect(header).ToBeVisibleAsync();

        var title = page.Locator(".hdr-title");
        await Expect(title).ToBeVisibleAsync();
        var titleText = await title.TextContentAsync();
        Assert.False(string.IsNullOrWhiteSpace(titleText), "Header title should contain text from data.json");

        var subtitle = page.Locator(".sub");
        await Expect(subtitle).ToBeVisibleAsync();
        var subtitleText = await subtitle.TextContentAsync();
        Assert.False(string.IsNullOrWhiteSpace(subtitleText), "Subtitle should contain text from data.json");
    }

    [Fact]
    public async Task Header_RendersAdoBacklogLink_WithTargetBlank()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var backlogLink = page.Locator(".hdr-backlog-link");
        var count = await backlogLink.CountAsync();

        if (count > 0)
        {
            await Expect(backlogLink).ToBeVisibleAsync();
            var target = await backlogLink.GetAttributeAsync("target");
            Assert.Equal("_blank", target);

            var rel = await backlogLink.GetAttributeAsync("rel");
            Assert.Contains("noopener", rel ?? "");

            var linkText = await backlogLink.TextContentAsync();
            Assert.Contains("ADO Backlog", linkText ?? "");

            var href = await backlogLink.GetAttributeAsync("href");
            Assert.False(string.IsNullOrEmpty(href), "Backlog link href should not be empty");
        }
        else
        {
            // BacklogUrl is empty/null in data.json — link should not render per source code
            Assert.Equal(0, count);
        }
    }

    [Fact]
    public async Task Header_RendersFourLegendItems()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var legend = page.Locator(".hdr-legend");
        await Expect(legend).ToBeVisibleAsync();

        var legendItems = page.Locator(".legend-item");
        var itemCount = await legendItems.CountAsync();
        Assert.Equal(4, itemCount);

        var labels = page.Locator(".legend-label");
        var labelTexts = new List<string>();
        for (int i = 0; i < await labels.CountAsync(); i++)
        {
            var text = await labels.Nth(i).TextContentAsync();
            labelTexts.Add(text ?? "");
        }

        Assert.Contains(labelTexts, t => t.Contains("PoC Milestone"));
        Assert.Contains(labelTexts, t => t.Contains("Production Release"));
        Assert.Contains(labelTexts, t => t.Contains("Checkpoint"));
        Assert.Contains(labelTexts, t => t.StartsWith("Now"));
    }

    [Fact]
    public async Task Header_LegendHasCorrectIndicatorElements()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var pocDiamond = page.Locator(".legend-diamond--poc");
        await Expect(pocDiamond).ToBeVisibleAsync();

        var prodDiamond = page.Locator(".legend-diamond--prod");
        await Expect(prodDiamond).ToBeVisibleAsync();

        var circle = page.Locator(".legend-circle");
        await Expect(circle).ToBeVisibleAsync();

        var nowBar = page.Locator(".legend-now-bar");
        await Expect(nowBar).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Header_FitsWithinViewportWithNoOverflow()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = page.Locator(".hdr");
        var box = await header.BoundingBoxAsync();

        Assert.NotNull(box);
        Assert.True(box.Width <= 1920, $"Header width {box.Width}px should not exceed 1920px viewport");
        Assert.True(box.X >= 0, "Header should not overflow to the left");

        // Verify no horizontal scrollbar by checking document scroll width
        var scrollWidth = await page.EvaluateAsync<int>("document.documentElement.scrollWidth");
        Assert.True(scrollWidth <= 1920, $"Page scroll width {scrollWidth}px should not exceed viewport width");
    }

    private static ILocatorAssertions Expect(ILocator locator)
    {
        return Assertions.Expect(locator);
    }
}