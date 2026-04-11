using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection(PlaywrightCollection.Name)]
public class LayoutAndCssTests
{
    private readonly PlaywrightFixture _fixture;

    public LayoutAndCssTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Requires running server")]
    public async Task Dashboard_HasFixedMaxWidth()
    {
        var page = await _fixture.CreatePageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        await dashboard.NavigateAsync();

        var container = dashboard.DashboardContainer;
        var box = await container.BoundingBoxAsync();

        box.Should().NotBeNull();
        box!.Width.Should().BeLessOrEqualTo(1200 + 48); // max-width + padding
    }

    [Fact(Skip = "Requires running server")]
    public async Task Dashboard_CssLoaded_HasCorrectBackground()
    {
        var page = await _fixture.CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var bgColor = await page.EvalOnSelectorAsync<string>("body", "el => getComputedStyle(el).backgroundColor");

        bgColor.Should().NotBeNullOrEmpty();
    }

    [Fact(Skip = "Requires running server")]
    public async Task Dashboard_ViewportIs1920x1080()
    {
        var page = await _fixture.CreatePageAsync();
        var viewport = page.ViewportSize;

        viewport.Should().NotBeNull();
        viewport!.Width.Should().Be(1920);
        viewport.Height.Should().Be(1080);
    }
}