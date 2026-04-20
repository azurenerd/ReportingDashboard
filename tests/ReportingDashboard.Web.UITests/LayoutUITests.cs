using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.Web.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class LayoutUITests
{
    private readonly PlaywrightFixture _fx;

    public LayoutUITests(PlaywrightFixture fx) => _fx = fx;

    [Fact]
    public async Task HomePage_LoadsWithoutBlazorJsOrScrollbars()
    {
        await using var context = await _fx.NewContextAsync();
        var page = await context.NewPageAsync();

        bool sawBlazorJs = false;
        page.Request += (_, req) =>
        {
            if (req.Url.Contains("blazor.server.js") ||
                req.Url.Contains("blazor.web.js") ||
                req.Url.Contains("blazor.webassembly.js"))
            {
                sawBlazorJs = true;
            }
        };

        var response = await page.GotoAsync(_fx.BaseUrl);
        response.Should().NotBeNull();
        response!.Status.Should().BeLessThan(500);

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        sawBlazorJs.Should().BeFalse("static SSR must not load any blazor.*.js runtime");

        var title = await page.TitleAsync();
        title.Should().Be("Reporting Dashboard");

        var html = await page.ContentAsync();
        html.Should().NotContain("id=\"blazor-error-ui\"");
        html.Should().NotContain("components-reconnect-modal");

        var bodyWidth = await page.EvaluateAsync<int>("() => document.body.clientWidth");
        var bodyHeight = await page.EvaluateAsync<int>("() => document.body.clientHeight");
        bodyWidth.Should().Be(1920);
        bodyHeight.Should().Be(1080);
    }

    [Fact]
    public async Task HomePage_LinksAppCssAndScopedBundle()
    {
        await using var context = await _fx.NewContextAsync();
        var page = await context.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var appCssCount = await page.Locator("link[href='app.css']").CountAsync();
        appCssCount.Should().BeGreaterThan(0, "app.css stylesheet must be referenced");

        var scopedCount = await page.Locator("link[href='ReportingDashboard.Web.styles.css']").CountAsync();
        scopedCount.Should().BeGreaterThan(0, "scoped CSS bundle must be referenced");
    }
}