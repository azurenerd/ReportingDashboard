using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.Web.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class HeatmapUiTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fx;

    public HeatmapUiTests(PlaywrightFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task Heatmap_WrapAndTitle_AreVisible()
    {
        var page = await _fx.NewPageAsync();
        (await page.Locator(".hm-wrap").CountAsync()).Should().Be(1);
        var titleText = await page.Locator(".hm-title").InnerTextAsync();
        titleText.Should().Contain("Monthly Execution Heatmap");
        titleText.Should().Contain("Shipped");
        titleText.Should().Contain("In Progress");
        titleText.Should().Contain("Carryover");
        titleText.Should().Contain("Blockers");
    }

    [Fact]
    public async Task HmGrid_Has25DirectChildren()
    {
        var page = await _fx.NewPageAsync();
        var count = await page.Locator(".hm-grid > *").CountAsync();
        count.Should().Be(25);
    }

    [Fact]
    public async Task HeaderRow_HasCornerAndFourMonthHeaders()
    {
        var page = await _fx.NewPageAsync();
        (await page.Locator(".hm-grid .hm-corner").CountAsync()).Should().Be(1);
        (await page.Locator(".hm-grid .hm-col-hdr").CountAsync()).Should().Be(4);
        (await page.Locator(".hm-corner").InnerTextAsync()).Trim().Should().Be("Status");
    }

    [Fact]
    public async Task AllFourCategoryRowHeaders_RenderInCanonicalOrder()
    {
        var page = await _fx.NewPageAsync();
        var rowHdrs = page.Locator(".hm-row-hdr");
        (await rowHdrs.CountAsync()).Should().Be(4);
        (await rowHdrs.Nth(0).GetAttributeAsync("class"))!.Should().Contain("ship-hdr");
        (await rowHdrs.Nth(1).GetAttributeAsync("class"))!.Should().Contain("prog-hdr");
        (await rowHdrs.Nth(2).GetAttributeAsync("class"))!.Should().Contain("carry-hdr");
        (await rowHdrs.Nth(3).GetAttributeAsync("class"))!.Should().Contain("block-hdr");
    }

    [Fact]
    public async Task NoInteractiveArtifacts_InHeatmap()
    {
        var page = await _fx.NewPageAsync();
        (await page.Locator(".hm-wrap script").CountAsync()).Should().Be(0);
        (await page.Locator(".hm-wrap button").CountAsync()).Should().Be(0);
        var html = await page.ContentAsync();
        html.Should().NotContain("blazor.server.js");
    }
}