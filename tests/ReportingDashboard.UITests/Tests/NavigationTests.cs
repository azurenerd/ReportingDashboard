using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class NavigationTests
{
    private readonly PlaywrightFixture _fixture;

    public NavigationTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task BacklogLink_HasTargetBlank()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        await dashboard.NavigateAsync();

        var linkCount = await dashboard.BacklogLink.CountAsync();
        if (linkCount > 0)
        {
            var target = await dashboard.BacklogLink.GetAttributeAsync("target");
            Assert.Equal("_blank", target);
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task BacklogLink_HasNoopener()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        await dashboard.NavigateAsync();

        var linkCount = await dashboard.BacklogLink.CountAsync();
        if (linkCount > 0)
        {
            var rel = await dashboard.BacklogLink.GetAttributeAsync("rel");
            Assert.Contains("noopener", rel ?? "");
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task PageTitle_IsNotEmpty()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        await dashboard.NavigateAsync();

        var title = await page.TitleAsync();
        Assert.False(string.IsNullOrWhiteSpace(title));
    }
}