using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.Web.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class AppShellUiTests
{
    private readonly PlaywrightFixture _fx;

    public AppShellUiTests(PlaywrightFixture fx)
    {
        _fx = fx;
    }

    [Fact]
    public async Task AppShell_TitleIsReportingDashboard()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl + "/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        (await page.TitleAsync()).Should().Be("Reporting Dashboard");
    }

    [Fact]
    public async Task AppShell_AppCssIsLinkedAndLoads200()
    {
        var page = await _fx.NewPageAsync();
        var resp = await page.GotoAsync(_fx.BaseUrl + "/app.css");
        resp!.Status.Should().Be(200);

        var body = await resp.TextAsync();
        body.Should().Contain("1920px");
        body.Should().Contain("1080px");
    }

    [Fact]
    public async Task AppShell_ScopedStylesBundleLoads200()
    {
        var page = await _fx.NewPageAsync();
        var resp = await page.GotoAsync(_fx.BaseUrl + "/ReportingDashboard.Web.styles.css");
        resp!.Status.Should().Be(200);
    }

    [Fact]
    public async Task AppShell_H1HeaderAndSubtitleAreVisible()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl + "/");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var h1 = page.Locator("div.hdr h1");
        (await h1.TextContentAsync()).Should().Be("Reporting Dashboard");

        var sub = page.Locator("div.sub");
        (await sub.TextContentAsync()).Should().Contain("Static SSR shell");
    }
}