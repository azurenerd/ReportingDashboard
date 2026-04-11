using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Fixtures;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class LayoutUITests
{
    private readonly PlaywrightFixture _fixture;

    public LayoutUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Layout_HasNoBlazerDefaultChrome()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            var sidebarCount = await page.Locator(".sidebar").CountAsync();
            var navMenuCount = await page.Locator(".nav-menu").CountAsync();
            var topRowCount = await page.Locator(".top-row").CountAsync();

            sidebarCount.Should().Be(0);
            navMenuCount.Should().Be(0);
            topRowCount.Should().Be(0);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Layout_NoChrome_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Layout_RendersOnlyBodyContent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            // Body should have direct content without wrapping chrome
            var bodyChildren = await page.Locator("body > *").CountAsync();
            bodyChildren.Should().BeGreaterThan(0, "body should have child elements");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Layout_OnlyBody_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Layout_HasHtmlLangAttribute()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            var lang = await page.Locator("html").GetAttributeAsync("lang");
            lang.Should().Be("en");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Layout_HtmlLang_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Layout_HasCharsetMeta()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            var charset = page.Locator("meta[charset='utf-8']");
            var count = await charset.CountAsync();
            count.Should().BeGreaterThan(0, "page should have utf-8 charset meta tag");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Layout_Charset_Failed");
            throw;
        }
    }

    [Fact]
    public async Task NonExistentRoute_ShowsNotFoundText()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync($"{_fixture.BaseUrl}/nonexistent-page", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            var bodyText = await page.Locator("body").InnerTextAsync();
            bodyText.Should().Contain("not found", "non-existent routes should show 'not found' text");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "NonExistentRoute_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_SectionsAreInCorrectOrder()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.HasDashboardContentAsync())
            {
                // Header should come before Timeline
                var headerBox = await dashboardPage.Header.BoundingBoxAsync();
                var timelineBox = await dashboardPage.TimelineArea.BoundingBoxAsync();

                if (headerBox != null && timelineBox != null)
                {
                    headerBox.Y.Should().BeLessThan(timelineBox.Y,
                        "header should be above timeline");
                }

                // Timeline should come before Heatmap (if both exist)
                if (await dashboardPage.HasHeatmapAsync() && timelineBox != null)
                {
                    var heatmapBox = await dashboardPage.HeatmapWrap.BoundingBoxAsync();
                    if (heatmapBox != null)
                    {
                        timelineBox.Y.Should().BeLessThan(heatmapBox.Y,
                            "timeline should be above heatmap");
                    }
                }
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_SectionOrder_Failed");
            throw;
        }
    }
}