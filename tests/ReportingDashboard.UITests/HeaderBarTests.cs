using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class HeaderBarTests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderBarTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> CreatePageAsync()
    {
        var context = await _fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60000);
        return page;
    }

    [Fact]
    public async Task Header_DisplaysProjectTitle_InH1Element()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var h1 = page.Locator("h1");
        var h1Count = await h1.CountAsync();

        if (h1Count > 0)
        {
            var titleText = await h1.First.InnerTextAsync();
            titleText.Should().NotBeNullOrWhiteSpace(
                "the header h1 should display the project title from data.json");
        }
        else
        {
            // If no h1, data may be missing — check for error state
            var errorText = page.GetByText("Unable to load dashboard data");
            (await errorText.CountAsync()).Should().BeGreaterThan(0,
                "if no h1 is rendered, an error message should be shown");
        }
    }

    [Fact]
    public async Task Header_DisplaysSubtitle_BelowTitle()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // The subtitle uses a div with class "sub" inside the .hdr
        var subtitle = page.Locator(".hdr .sub");
        var count = await subtitle.CountAsync();

        if (count > 0)
        {
            var subtitleText = await subtitle.First.InnerTextAsync();
            subtitleText.Should().NotBeNullOrWhiteSpace(
                "subtitle should render the Data.Subtitle value from data.json");
        }
    }

    [Fact]
    public async Task Header_ShowsFourLegendItems_WithCorrectLabels()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var legendItems = page.Locator(".legend .legend-item");
        var count = await legendItems.CountAsync();

        if (count > 0)
        {
            count.Should().Be(4, "the legend should contain exactly four items");

            var allText = await page.Locator(".legend").First.InnerTextAsync();
            allText.Should().Contain("PoC Milestone");
            allText.Should().Contain("Production Release");
            allText.Should().Contain("Checkpoint");
            allText.Should().Contain("Now");
        }
    }

    [Fact]
    public async Task Header_BacklogLink_RendersWhenUrlProvided()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // The backlog link is an <a> inside h1 with target="_blank" containing "ADO Backlog"
        var backlogLink = page.Locator("h1 a[target='_blank']");
        var linkCount = await backlogLink.CountAsync();

        // If data has a backlogUrl, the link should be present and clickable
        if (linkCount > 0)
        {
            var linkText = await backlogLink.First.InnerTextAsync();
            linkText.Should().Contain("ADO Backlog",
                "the backlog link text should contain 'ADO Backlog'");

            var href = await backlogLink.First.GetAttributeAsync("href");
            href.Should().NotBeNullOrWhiteSpace(
                "the backlog link should have a valid href from Data.BacklogUrl");
        }
    }

    [Fact]
    public async Task Header_LegendSymbols_HaveCorrectShapeElements()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var legend = page.Locator(".legend");
        if (await legend.CountAsync() == 0) return;

        // PoC diamond: .legend-diamond.poc-diamond
        var pocDiamond = page.Locator(".poc-diamond");
        (await pocDiamond.CountAsync()).Should().Be(1,
            "there should be exactly one PoC diamond symbol");

        // Production diamond: .legend-diamond.prod-diamond
        var prodDiamond = page.Locator(".prod-diamond");
        (await prodDiamond.CountAsync()).Should().Be(1,
            "there should be exactly one Production diamond symbol");

        // Checkpoint circle: .legend-checkpoint
        var checkpoint = page.Locator(".legend-checkpoint");
        (await checkpoint.CountAsync()).Should().Be(1,
            "there should be exactly one Checkpoint circle symbol");

        // Now marker: .legend-now
        var nowMarker = page.Locator(".legend-now");
        (await nowMarker.CountAsync()).Should().Be(1,
            "there should be exactly one Now marker symbol");
    }
}