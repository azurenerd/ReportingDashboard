using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class ErrorStateTests
{
    private readonly PlaywrightFixture _fixture;

    public ErrorStateTests(PlaywrightFixture fixture) => _fixture = fixture;

    private string ErrorBaseUrl =>
        Environment.GetEnvironmentVariable("BASE_URL_ERROR") ?? _fixture.BaseUrl;

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorPanel_IsDisplayed_WhenDataJsonInvalid()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, ErrorBaseUrl);

        await dashboard.NavigateAsync();

        Assert.True(await dashboard.IsErrorVisibleAsync());
        Assert.False(await dashboard.IsDashboardVisibleAsync());
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorPanel_ShowsTitle()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, ErrorBaseUrl);

        await dashboard.NavigateAsync();

        await Assertions.Expect(dashboard.ErrorTitle).ToBeVisibleAsync();
        var text = await dashboard.ErrorTitle.TextContentAsync();
        Assert.Contains("Dashboard data could not be loaded", text ?? "");
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorPanel_ShowsHelpText()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, ErrorBaseUrl);

        await dashboard.NavigateAsync();

        await Assertions.Expect(dashboard.ErrorHelp).ToBeVisibleAsync();
        var text = await dashboard.ErrorHelp.TextContentAsync();
        Assert.Contains("Check data.json", text ?? "");
    }
}