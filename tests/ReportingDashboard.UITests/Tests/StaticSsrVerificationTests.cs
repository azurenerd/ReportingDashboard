using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class StaticSsrVerificationTests
{
    private readonly PlaywrightFixture _fixture;

    public StaticSsrVerificationTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task StaticSSR_NoWebSocketConnection()
    {
        var page = await _fixture.NewPageAsync();

        bool webSocketOpened = false;
        page.WebSocket += (_, _) => webSocketOpened = true;

        await page.GotoAsync($"{_fixture.BaseUrl}/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });

        // Wait a bit to ensure no late WebSocket connections
        await page.WaitForTimeoutAsync(2000);

        webSocketOpened.Should().BeFalse("Static SSR should not open WebSocket/SignalR connections");
    }

    [Fact]
    public async Task StaticSSR_NoBlazorServerJs()
    {
        var page = await _fixture.NewPageAsync();

        bool blazorJsRequested = false;
        page.Request += (_, r) =>
        {
            if (r.Url.Contains("blazor.server.js") || r.Url.Contains("blazor.web.js"))
                blazorJsRequested = true;
        };

        await page.GotoAsync($"{_fixture.BaseUrl}/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });

        blazorJsRequested.Should().BeFalse("Static SSR should not request Blazor JS files");
    }

    [Fact]
    public async Task StaticSSR_HtmlDoesNotContainBlazorScriptTag()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        var html = await dp.GetPageHtmlAsync();
        html.Should().NotContain("blazor.server.js");
        html.Should().NotContain("blazor.web.js");
        html.Should().NotContain("_framework/blazor");
    }

    [Fact]
    public async Task StaticSSR_NoReconnectModalInDOM()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        var hasModal = await dp.HasBlazorReconnectModalAsync();
        hasModal.Should().BeFalse();
    }

    [Fact]
    public async Task StaticSSR_PageRendersWithinSingleRequest()
    {
        var page = await _fixture.NewPageAsync();

        int requestCount = 0;
        page.Request += (_, r) =>
        {
            if (r.Url.TrimEnd('/') == _fixture.BaseUrl.TrimEnd('/') ||
                r.Url == _fixture.BaseUrl + "/")
                requestCount++;
        };

        await page.GotoAsync($"{_fixture.BaseUrl}/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });

        requestCount.Should().Be(1, "Static SSR should render in a single request");
    }

    [Fact]
    public async Task StaticSSR_NoInteractiveElements()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();

        // No forms, buttons, or inputs should be present (it's a static display page)
        var formCount = await page.Locator("form").CountAsync();
        var buttonCount = await page.Locator("button").CountAsync();
        var inputCount = await page.Locator("input").CountAsync();

        formCount.Should().Be(0, "static dashboard should have no forms");
        buttonCount.Should().Be(0, "static dashboard should have no buttons");
        inputCount.Should().Be(0, "static dashboard should have no inputs");
    }
}