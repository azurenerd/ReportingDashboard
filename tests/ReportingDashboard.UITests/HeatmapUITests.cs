using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class HeatmapUITests
{
    private readonly PlaywrightFixture _fx;

    public HeatmapUITests(PlaywrightFixture fx) => _fx = fx;

    private async Task<IPage> GotoDashboardAsync()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        return page;
    }

    [Fact]
    public async Task Heatmap_Wrap_And_Title_Are_Rendered()
    {
        var page = await GotoDashboardAsync();

        (await page.Locator(".hm-wrap").CountAsync()).Should().BeGreaterThan(0);

        var title = page.Locator(".hm-title");
        (await title.CountAsync()).Should().BeGreaterThan(0);
        var titleText = await title.First.InnerTextAsync();
        titleText.Should().Contain("MONTHLY EXECUTION HEATMAP");
        titleText.Should().Contain("SHIPPED");
        titleText.Should().Contain("IN PROGRESS");
        titleText.Should().Contain("CARRYOVER");
        titleText.Should().Contain("BLOCKERS");
    }

    [Fact]
    public async Task Heatmap_Grid_Has_Corner_And_Four_Row_Headers()
    {
        var page = await GotoDashboardAsync();

        var grid = page.Locator(".hm-grid").First;
        await grid.WaitForAsync();

        var corner = grid.Locator(".hm-corner");
        (await corner.CountAsync()).Should().Be(1);
        (await corner.InnerTextAsync()).Trim().Should().Be("Status");

        var rowHeaders = grid.Locator(".hm-row-hdr");
        (await rowHeaders.CountAsync()).Should().Be(4);

        var labels = new[]
        {
            await rowHeaders.Nth(0).InnerTextAsync(),
            await rowHeaders.Nth(1).InnerTextAsync(),
            await rowHeaders.Nth(2).InnerTextAsync(),
            await rowHeaders.Nth(3).InnerTextAsync(),
        };
        labels[0].Trim().Should().Be("SHIPPED");
        labels[1].Trim().Should().Be("IN PROGRESS");
        labels[2].Trim().Should().Be("CARRYOVER");
        labels[3].Trim().Should().Be("BLOCKERS");
    }

    [Fact]
    public async Task Heatmap_Category_Cells_Are_Rendered_For_Each_Row()
    {
        var page = await GotoDashboardAsync();

        var grid = page.Locator(".hm-grid").First;
        await grid.WaitForAsync();

        (await grid.Locator(".ship-cell").CountAsync()).Should().BeGreaterThan(0);
        (await grid.Locator(".prog-cell").CountAsync()).Should().BeGreaterThan(0);
        (await grid.Locator(".carry-cell").CountAsync()).Should().BeGreaterThan(0);
        (await grid.Locator(".block-cell").CountAsync()).Should().BeGreaterThan(0);
    }
}