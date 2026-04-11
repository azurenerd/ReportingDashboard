using FluentAssertions;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection(PlaywrightCollection.Name)]
public class NavigationTests
{
    private readonly PlaywrightFixture _fixture;

    public NavigationTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Requires running server")]
    public async Task RootUrl_NavigatesToDashboard()
    {
        var page = await _fixture.CreatePageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);

        await dashboard.NavigateAsync();

        page.Url.Should().Contain(_fixture.BaseUrl);
        (await dashboard.HasDashboardContainerAsync()).Should().BeTrue();
    }

    [Fact(Skip = "Requires running server")]
    public async Task UnknownRoute_DoesNotCrash()
    {
        var page = await _fixture.CreatePageAsync();

        var response = await page.GotoAsync($"{_fixture.BaseUrl}/nonexistent-page");

        response.Should().NotBeNull();
        // Blazor may return 200 with a "not found" page or 404
        response!.Status.Should().BeOneOf(200, 404);
    }

    [Fact(Skip = "Requires running server")]
    public async Task PageTitle_ContainsDashboard()
    {
        var page = await _fixture.CreatePageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        await dashboard.NavigateAsync();

        var title = await page.TitleAsync();

        title.Should().Contain("Dashboard");
    }
}