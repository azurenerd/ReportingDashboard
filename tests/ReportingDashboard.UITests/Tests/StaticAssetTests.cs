using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class StaticAssetTests
{
    private readonly PlaywrightFixture _fixture;

    public StaticAssetTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task CssFile_IsServed()
    {
        var page = await _fixture.NewPageAsync();
        var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");

        Assert.NotNull(response);
        Assert.True(response!.Ok, "dashboard.css should be served as a static file");
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_IsServed()
    {
        var page = await _fixture.NewPageAsync();
        var response = await page.GotoAsync($"{_fixture.BaseUrl}/data.json");

        Assert.NotNull(response);
        Assert.True(response!.Ok, "data.json should be served as a static file");
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task NoConsoleErrors_OnDashboardLoad()
    {
        var page = await _fixture.NewPageAsync();
        var consoleErrors = new List<string>();

        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
                consoleErrors.Add(msg.Text);
        };

        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);
        await dashboard.NavigateAsync();
        await page.WaitForTimeoutAsync(2000);

        var realErrors = consoleErrors
            .Where(e => !e.Contains("WebSocket") && !e.Contains("negotiate"))
            .ToList();

        Assert.Empty(realErrors);
    }
}