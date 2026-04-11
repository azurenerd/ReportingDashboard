using FluentAssertions;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection(PlaywrightCollection.Name)]
public class DashboardHappyPathTests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardHappyPathTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Requires running server - run manually or in CI with server startup")]
    public async Task Dashboard_LoadsSuccessfully_WithValidData()
    {
        var page = await _fixture.CreatePageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);

        await dashboard.NavigateAsync();

        (await dashboard.HasDashboardContainerAsync()).Should().BeTrue();
        (await dashboard.HasErrorBannerAsync()).Should().BeFalse();
    }

    [Fact(Skip = "Requires running server")]
    public async Task Dashboard_ShowsProjectTitle()
    {
        var page = await _fixture.CreatePageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);

        await dashboard.NavigateAsync();

        var title = await dashboard.GetTitleAsync();
        title.Should().Contain("Dashboard");
    }

    [Fact(Skip = "Requires running server")]
    public async Task Dashboard_ShowsStatusBadge()
    {
        var page = await _fixture.CreatePageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);

        await dashboard.NavigateAsync();

        (await dashboard.StatusBadge.CountAsync()).Should().BeGreaterThan(0);
    }
}