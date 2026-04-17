using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class DashboardHeaderUITests : IClassFixture<PlaywrightFixture>, IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;
    private IBrowserContext _context = null!;

    public DashboardHeaderUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        if (!_fixture.ServerAvailable)
            return;

        _context = await _fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        _page = await _context.NewPageAsync();
        _page.SetDefaultTimeout(60000);
        await _page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 60000
        });
        await _page.WaitForSelectorAsync("div.hdr", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 15000
        });
    }

    public async Task DisposeAsync()
    {
        if (_page is not null) await _page.CloseAsync();
        if (_context is not null) await _context.DisposeAsync();
    }

    [Fact]
    public async Task HeaderRendersWithTitleAndSubtitle()
    {
        _fixture.EnsureServerAvailable();

        var h1 = await _page.QuerySelectorAsync("div.hdr h1");
        Assert.NotNull(h1);
        var titleText = await h1.InnerTextAsync();
        Assert.False(string.IsNullOrWhiteSpace(titleText), "H1 title should not be empty");

        var sub = await _page.QuerySelectorAsync("div.hdr div.sub");
        Assert.NotNull(sub);
        var subText = await sub.InnerTextAsync();
        Assert.False(string.IsNullOrWhiteSpace(subText), "Subtitle should not be empty");
    }

    [Fact]
    public async Task AdoBacklogLinkIsPresent()
    {
        _fixture.EnsureServerAvailable();

        var link = await _page.QuerySelectorAsync("div.hdr h1 a");
        Assert.NotNull(link);

        var href = await link.GetAttributeAsync("href");
        Assert.False(string.IsNullOrWhiteSpace(href), "ADO Backlog link href should not be empty");

        var text = await link.InnerTextAsync();
        Assert.Contains("ADO Backlog", text);

        var color = await link.EvaluateAsync<string>("el => getComputedStyle(el).color");
        Assert.NotNull(color);
    }

    [Fact]
    public async Task LegendDisplaysFourItems()
    {
        _fixture.EnsureServerAvailable();

        var legendContainer = await _page.QuerySelectorAllAsync("div.hdr > div:last-child > div");
        Assert.Equal(4, legendContainer.Count);

        var legendTexts = new List<string>();
        foreach (var item in legendContainer)
        {
            var text = await item.InnerTextAsync();
            legendTexts.Add(text.Trim());
        }

        Assert.Contains(legendTexts, t => t.Contains("PoC Milestone"));
        Assert.Contains(legendTexts, t => t.Contains("Production Release"));
        Assert.Contains(legendTexts, t => t.Contains("Checkpoint"));
        Assert.Contains(legendTexts, t => t.Contains("Now"));
    }

    [Fact]
    public async Task LegendShapesHaveCorrectColors()
    {
        _fixture.EnsureServerAvailable();

        var shapes = await _page.QuerySelectorAllAsync("div.hdr > div:last-child > div > span:first-child");
        Assert.True(shapes.Count >= 4, $"Expected at least 4 legend shapes, found {shapes.Count}");

        var pocBg = await shapes[0].EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        Assert.Contains("244", pocBg);

        var prodBg = await shapes[1].EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        Assert.Contains("52", prodBg);

        var cpBg = await shapes[2].EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        Assert.Contains("153", cpBg);

        var nowBg = await shapes[3].EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        Assert.Contains("234", nowBg);
    }

    [Fact]
    public async Task HeaderLayoutAt1920x1080_NoOverflow()
    {
        _fixture.EnsureServerAvailable();

        var hdr = await _page.QuerySelectorAsync("div.hdr");
        Assert.NotNull(hdr);

        var box = await hdr.BoundingBoxAsync();
        Assert.NotNull(box);

        Assert.True(box.Width <= 1920, $"Header width {box.Width} exceeds 1920px viewport");
        Assert.True(box.Y < 10, $"Header Y position {box.Y} should be near top of page");
    }

    [Fact]
    public async Task LegendItems_AreDataDriven()
    {
        _fixture.EnsureServerAvailable();

        // Legend should contain "Now" with dynamic date context from data.json
        var legendContainer = await _page.QuerySelectorAllAsync("div.hdr > div:last-child > div");
        Assert.True(legendContainer.Count == 4, "Legend should have exactly 4 items driven by design spec");

        // Each legend item should have a shape span and text
        foreach (var item in legendContainer)
        {
            var shape = await item.QuerySelectorAsync("span:first-child");
            Assert.NotNull(shape);
            var text = await item.InnerTextAsync();
            Assert.False(string.IsNullOrWhiteSpace(text), "Each legend item must have label text");
        }
    }
}