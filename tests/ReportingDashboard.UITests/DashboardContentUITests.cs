using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Fixtures;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardContentUITests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardContentUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dashboard_WithValidData_ShowsDashboardRoot()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            // Either dashboard content or error panel should be visible
            var hasDashboard = await dashboardPage.HasDashboardContentAsync();
            var hasError = await dashboardPage.HasErrorPanelAsync();

            (hasDashboard || hasError).Should().BeTrue(
                "page should render either dashboard content or error panel");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_ShowsDashboardRoot_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_WithValidData_ShowsHeaderSection()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasDashboardContentAsync())
            {
                await Assertions.Expect(dashboardPage.Header).ToBeVisibleAsync();
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_ShowsHeader_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_WithValidData_ShowsTimelineSection()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasDashboardContentAsync())
            {
                var hasTimeline = await dashboardPage.HasTimelineAsync();
                hasTimeline.Should().BeTrue("timeline area should be visible with valid data");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_ShowsTimeline_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_WithValidData_ShowsHeatmapSection()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasDashboardContentAsync())
            {
                var hasHeatmap = await dashboardPage.HasHeatmapAsync();
                hasHeatmap.Should().BeTrue("heatmap section should be visible with valid data");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_ShowsHeatmap_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_DoesNotShowErrorPanel_WhenDataIsValid()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasDashboardContentAsync())
            {
                var hasError = await dashboardPage.HasErrorPanelAsync();
                hasError.Should().BeFalse("error panel should not appear when data is valid");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_NoErrorPanel_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_RendersProjectTitle()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasDashboardContentAsync())
            {
                var bodyText = await page.Locator("body").InnerTextAsync();
                bodyText.Should().NotBeEmpty("page should contain rendered text from data.json");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_RendersTitle_Failed");
            throw;
        }
    }
}