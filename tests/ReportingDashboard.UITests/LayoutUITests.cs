using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class LayoutUITests
{
    private readonly PlaywrightFixture _fixture;

    public LayoutUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> NewPageAsync()
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
    public async Task Page_Title_IsExecutiveReportingDashboard()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var title = await page.TitleAsync();
        title.Should().Be("Executive Reporting Dashboard");
    }

    [Fact]
    public async Task Page_HasNoSignalRWebSocketConnections()
    {
        var page = await NewPageAsync();
        var wsConnections = new List<string>();
        page.WebSocket += (_, ws) => wsConnections.Add(ws.Url);

        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        wsConnections.Should().BeEmpty("Static SSR should not establish WebSocket connections");
    }

    [Fact]
    public async Task Page_HtmlLang_IsEn()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var lang = await page.Locator("html").GetAttributeAsync("lang");
        lang.Should().Be("en");
    }

    [Fact]
    public async Task Page_Viewport_MetaTagWidth1920()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var content = await page.ContentAsync();
        content.Should().Contain("width=1920");
    }

    [Fact]
    public async Task Page_BlazorWebJs_ScriptPresent()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var content = await page.ContentAsync();
        content.Should().Contain("_framework/blazor.web.js");
    }
}