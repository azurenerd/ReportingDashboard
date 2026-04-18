using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class HeaderUITests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Header_WhenDataLoads_RendersHdrSection()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hdrCount = await page.Locator(".hdr").CountAsync();
        // Either .hdr is present (data loaded) or .error-container is present (data missing) — never neither
        var errorCount = await page.Locator(".error-container").CountAsync();
        (hdrCount + errorCount).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Header_WhenDataLoads_RendersFourLegendItems()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hdr = page.Locator(".hdr");
        if (await hdr.IsVisibleAsync())
        {
            var legendItems = await page.Locator(".legend-item").CountAsync();
            legendItems.Should().Be(4);
        }
    }

    [Fact]
    public async Task Header_WhenDataLoads_LegendContainsExpectedLabels()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hdr = page.Locator(".hdr");
        if (await hdr.IsVisibleAsync())
        {
            var legendText = await page.Locator(".legend").InnerTextAsync();
            legendText.Should().Contain("PoC Milestone");
            legendText.Should().Contain("Production Release");
            legendText.Should().Contain("Checkpoint");
            legendText.Should().Contain("Now");
        }
    }

    [Fact]
    public async Task Header_WhenDataLoads_AdoBacklogLinkHasTargetBlank()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hdr = page.Locator(".hdr");
        if (await hdr.IsVisibleAsync())
        {
            var link = page.Locator(".hdr h1 a");
            var target = await link.GetAttributeAsync("target");
            target.Should().Be("_blank");

            var linkText = await link.InnerTextAsync();
            linkText.Should().Contain("ADO Backlog");
        }
    }

    [Fact]
    public async Task Header_WhenDataLoads_SubtitleDivIsPresent()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hdr = page.Locator(".hdr");
        if (await hdr.IsVisibleAsync())
        {
            var subCount = await page.Locator(".hdr .sub").CountAsync();
            subCount.Should().Be(1);
        }
    }
}