using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.Web.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class DashboardHeaderUiTests
{
    private readonly PlaywrightFixture _fx;

    public DashboardHeaderUiTests(PlaywrightFixture fx) => _fx = fx;

    private static string BaseUrl =>
        Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5080";

    [Fact]
    public async Task Header_Renders_With_Title_And_Subtitle()
    {
        var page = await _fx.Browser.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = page.Locator("header.hdr");
        await header.WaitForAsync(new() { Timeout = 60000 });
        (await header.CountAsync()).Should().Be(1);

        (await header.Locator("h1").CountAsync()).Should().Be(1);
        (await header.Locator(".sub").CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Header_Has_Four_Legend_Items()
    {
        var page = await _fx.Browser.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var items = page.Locator("header.hdr .legend-item");
        await items.First.WaitForAsync(new() { Timeout = 60000 });
        (await items.CountAsync()).Should().Be(4);
    }

    [Fact]
    public async Task Header_Legend_Labels_Are_Present()
    {
        var page = await _fx.Browser.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GetByText("PoC Milestone").First.WaitForAsync(new() { Timeout = 60000 });
        (await page.GetByText("PoC Milestone").CountAsync()).Should().BeGreaterThan(0);
        (await page.GetByText("Production Release").CountAsync()).Should().BeGreaterThan(0);
        (await page.GetByText("Checkpoint").CountAsync()).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Header_Backlog_Link_Has_No_Script_Scheme()
    {
        var page = await _fx.Browser.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var links = page.Locator("header.hdr a.backlog-link");
        var count = await links.CountAsync();
        for (int i = 0; i < count; i++)
        {
            var href = await links.Nth(i).GetAttributeAsync("href");
            href.Should().NotBeNull();
            (href!.StartsWith("http://") || href.StartsWith("https://")).Should().BeTrue();
        }
    }
}