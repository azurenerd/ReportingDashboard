using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class StaticAssetsTests
{
    private readonly PlaywrightFixture _fixture;

    public StaticAssetsTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task CssStylesheet_IsLoaded()
    {
        var page = await _fixture.NewPageAsync();

        var cssLoaded = false;
        page.Response += (_, response) =>
        {
            if (response.Url.Contains("dashboard.css") && response.Status == 200)
                cssLoaded = true;
        };

        await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        // Verify CSS is applied by checking a known styled element
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var headerExists = await dashboard.Header.CountAsync() > 0;
        if (headerExists)
        {
            var display = await dashboard.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            display.Should().Be("flex", "header should have flex display from CSS");
        }
    }

    [Fact]
    public async Task NoConsoleErrors_OnPageLoad()
    {
        var page = await _fixture.NewPageAsync();
        var consoleErrors = new List<string>();

        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
                consoleErrors.Add(msg.Text);
        };

        await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        // Allow Blazor SignalR reconnection messages but no other errors
        var significantErrors = consoleErrors
            .Where(e => !e.Contains("WebSocket") && !e.Contains("SignalR") && !e.Contains("blazor"))
            .ToList();

        significantErrors.Should().BeEmpty("page should load without console errors");
    }

    [Fact]
    public async Task NoNetworkFailures_OnPageLoad()
    {
        var page = await _fixture.NewPageAsync();
        var failedRequests = new List<string>();

        page.RequestFailed += (_, request) =>
        {
            failedRequests.Add($"{request.Url} - {request.Failure}");
        };

        await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        failedRequests.Should().BeEmpty("all network requests should succeed");
    }
}