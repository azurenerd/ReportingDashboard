using Microsoft.Playwright;

namespace ReportingDashboard.Web.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardShellUITests
{
    private readonly PlaywrightFixture _fx;

    public DashboardShellUITests(PlaywrightFixture fx) => _fx = fx;

    [Fact]
    public async Task Root_Returns_200_And_NonEmpty_Html()
    {
        var page = await _fx.NewPageAsync();
        var resp = await page.GotoAsync(_fx.BaseUrl);
        resp.Should().NotBeNull();
        resp!.Status.Should().Be(200);

        var content = await page.ContentAsync();
        content.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Page_Does_Not_Contain_SignalR_Script()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var html = await page.ContentAsync();
        html.Should().NotContain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task Renders_Header_Timeline_And_Heatmap_Regions()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        (await page.Locator(".hdr").CountAsync()).Should().BeGreaterThan(0);
        (await page.Locator(".tl-area").CountAsync()).Should().BeGreaterThan(0);
        (await page.Locator(".hm-wrap").CountAsync()).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Body_Has_Fixed_1920x1080_Dimensions()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var width = await page.EvaluateAsync<string>(
            "() => getComputedStyle(document.body).width");
        var height = await page.EvaluateAsync<string>(
            "() => getComputedStyle(document.body).height");
        var overflow = await page.EvaluateAsync<string>(
            "() => getComputedStyle(document.body).overflow");

        width.Should().Be("1920px");
        height.Should().Be("1080px");
        overflow.Should().Contain("hidden");
    }

    [Fact]
    public async Task Heatmap_Grid_Uses_Expected_Column_Template()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var cols = await page.EvaluateAsync<string>(@"() => {
            const el = document.querySelector('.hm-grid');
            return el ? getComputedStyle(el).gridTemplateColumns : '';
        }");

        cols.Should().StartWith("160px");
    }
}