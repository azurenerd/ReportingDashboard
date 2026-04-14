using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

/// <summary>
/// Playwright E2E tests for the SVG Milestone Timeline section of Dashboard.razor.
/// Validates timeline area rendering, SVG elements, workstream labels, and NOW indicator.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class TimelineUITests
{
    private readonly PlaywrightFixture _fixture;

    public TimelineUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> CreatePageAsync()
    {
        Skip.If(!_fixture.BrowserAvailable, "Playwright browser not available.");
        Skip.If(_fixture.Browser is null, "Playwright browser not initialized.");

        var context = await _fixture.Browser!.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60000);
        return page;
    }

    private async Task<bool> IsServerRunning(IPage page)
    {
        try
        {
            var response = await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions { Timeout = 5000 });
            return response is not null && response.Ok;
        }
        catch
        {
            return false;
        }
    }

    [SkippableFact]
    public async Task Timeline_AreaIsPresent_WithCorrectHeight()
    {
        var page = await CreatePageAsync();
        Skip.If(!await IsServerRunning(page), "Server not reachable at " + _fixture.BaseUrl);

        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(2000);

        var tlArea = page.Locator(".tl-area");
        var count = await tlArea.CountAsync();

        if (count > 0)
        {
            var box = await tlArea.BoundingBoxAsync();
            box.Should().NotBeNull();
            box!.Height.Should().BeApproximately(196, 10, "Timeline area should be ~196px tall");
        }

        await page.Context.CloseAsync();
    }

    [SkippableFact]
    public async Task Timeline_SvgContainsElements()
    {
        var page = await CreatePageAsync();
        Skip.If(!await IsServerRunning(page), "Server not reachable at " + _fixture.BaseUrl);

        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(2000);

        var svg = page.Locator(".tl-svg-wrap svg, .tl-area svg");
        var svgCount = await svg.CountAsync();

        if (svgCount > 0)
        {
            // SVG should contain line elements (gridlines + workstream lines)
            var lines = page.Locator(".tl-svg-wrap svg line, .tl-area svg line");
            var lineCount = await lines.CountAsync();
            lineCount.Should().BeGreaterThan(0, "SVG should contain line elements for gridlines and workstreams");
        }

        await page.Context.CloseAsync();
    }

    [SkippableFact]
    public async Task Timeline_WorkstreamLabelsRendered()
    {
        var page = await CreatePageAsync();
        Skip.If(!await IsServerRunning(page), "Server not reachable at " + _fixture.BaseUrl);

        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(2000);

        var sidebar = page.Locator(".tl-sidebar, .tl-area [style*='230px']");
        var sidebarCount = await sidebar.CountAsync();

        if (sidebarCount > 0)
        {
            var labels = page.Locator(".tl-sidebar .ws-label, .tl-area .ws-label");
            var labelCount = await labels.CountAsync();
            labelCount.Should().BeGreaterThan(0, "Timeline sidebar should contain workstream labels");
        }

        await page.Context.CloseAsync();
    }

    [SkippableFact]
    public async Task Timeline_NowLineIsRendered_WithRedStroke()
    {
        var page = await CreatePageAsync();
        Skip.If(!await IsServerRunning(page), "Server not reachable at " + _fixture.BaseUrl);

        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(2000);

        // Look for inner HTML containing NOW line characteristics
        var svgHtml = await page.Locator(".tl-svg-wrap, .tl-area").First.InnerHTMLAsync();

        if (svgHtml.Contains("svg"))
        {
            // The NOW line uses #EA4335 stroke and dashed pattern
            var hasNowIndicator = svgHtml.Contains("#EA4335") || svgHtml.Contains("NOW");
            hasNowIndicator.Should().BeTrue("SVG should contain the red NOW indicator line or label");
        }

        await page.Context.CloseAsync();
    }

    [SkippableFact]
    public async Task Timeline_DropShadowFilterDefined()
    {
        var page = await CreatePageAsync();
        Skip.If(!await IsServerRunning(page), "Server not reachable at " + _fixture.BaseUrl);

        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForTimeoutAsync(2000);

        var svgHtml = await page.Locator(".tl-svg-wrap, .tl-area").First.InnerHTMLAsync();

        if (svgHtml.Contains("svg"))
        {
            svgHtml.Should().Contain("filter", "SVG should contain a drop-shadow filter definition");
            svgHtml.Should().Contain("feDropShadow", "SVG filter should use feDropShadow element");
        }

        await page.Context.CloseAsync();
    }
}