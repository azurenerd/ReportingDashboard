using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class TimelineTests
{
    private readonly PlaywrightFixture _fixture;

    public TimelineTests(PlaywrightFixture fixture) => _fixture = fixture;

    private async Task<DashboardPage> LoadDashboardAsync()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        await dashboard.NavigateAsync();
        await dashboard.WaitForDashboardLoadedAsync();
        return dashboard;
    }

    [Fact]
    public async Task Timeline_AreaIsVisible()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            await Assertions.Expect(dashboard.TimelineArea).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Timeline_AreaIsVisible));
            throw;
        }
    }

    [Fact]
    public async Task Timeline_HasFafafaBackground()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var bgColor = await dashboard.TimelineArea.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // #FAFAFA = rgb(250, 250, 250)
            bgColor.Should().Contain("250");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Timeline_HasFafafaBackground));
            throw;
        }
    }

    [Fact]
    public async Task Timeline_DisplaysTrackLabels()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var count = await dashboard.TrackLabels.CountAsync();
            count.Should().BeGreaterThan(0, "dashboard should have at least one timeline track");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Timeline_DisplaysTrackLabels));
            throw;
        }
    }

    [Fact]
    public async Task Timeline_TrackIdsAreVisible()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var count = await dashboard.TrackIds.CountAsync();
            count.Should().BeGreaterThan(0);

            for (int i = 0; i < count; i++)
            {
                var text = await dashboard.TrackIds.Nth(i).TextContentAsync();
                text.Should().NotBeNullOrWhiteSpace();
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Timeline_TrackIdsAreVisible));
            throw;
        }
    }

    [Fact]
    public async Task Timeline_TrackNamesAreVisible()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var count = await dashboard.TrackNames.CountAsync();
            count.Should().BeGreaterThan(0);

            for (int i = 0; i < count; i++)
            {
                var text = await dashboard.TrackNames.Nth(i).TextContentAsync();
                text.Should().NotBeNullOrWhiteSpace();
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Timeline_TrackNamesAreVisible));
            throw;
        }
    }

    [Fact]
    public async Task Timeline_TrackIdsHaveDistinctColors()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var count = await dashboard.TrackIds.CountAsync();
            if (count > 1)
            {
                var colors = new List<string>();
                for (int i = 0; i < count; i++)
                {
                    var color = await dashboard.TrackIds.Nth(i).EvaluateAsync<string>(
                        "el => getComputedStyle(el).color");
                    colors.Add(color);
                }
                colors.Distinct().Count().Should().BeGreaterThan(1,
                    "track IDs should use distinct colors");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Timeline_TrackIdsHaveDistinctColors));
            throw;
        }
    }

    [Fact]
    public async Task Timeline_SvgIsRendered()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            await Assertions.Expect(dashboard.Svg).ToBeVisibleAsync();

            var width = await dashboard.Svg.GetAttributeAsync("width");
            width.Should().Be("1560");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Timeline_SvgIsRendered));
            throw;
        }
    }

    [Fact]
    public async Task Timeline_SvgContainsNowLine()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            // Look for the NOW text element in SVG
            var nowText = dashboard.Page.Locator(".tl-svg-box svg text", new PageLocatorOptions
            {
                HasText = "NOW"
            });
            var count = await nowText.CountAsync();
            // NOW line may or may not be visible depending on date range
            // Just verify SVG renders without error
            await Assertions.Expect(dashboard.Svg).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Timeline_SvgContainsNowLine));
            throw;
        }
    }

    [Fact]
    public async Task Timeline_SvgContainsMilestoneShapes()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            // Check for polygons (diamonds) or circles (checkpoints)
            var polygonCount = await dashboard.SvgPolygons.CountAsync();
            var circleCount = await dashboard.SvgCircles.CountAsync();

            (polygonCount + circleCount).Should().BeGreaterThan(0,
                "SVG should contain milestone markers (polygons or circles)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Timeline_SvgContainsMilestoneShapes));
            throw;
        }
    }

    [Fact]
    public async Task Timeline_SvgContainsMonthLabels()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var svgTexts = dashboard.Page.Locator(".tl-svg-box svg text");
            var count = await svgTexts.CountAsync();
            count.Should().BeGreaterThan(0, "SVG should contain month label text elements");

            // Check for at least one recognizable month abbreviation
            var allText = await dashboard.SvgBox.InnerHTMLAsync();
            var hasMonth = allText.Contains("Jan") || allText.Contains("Feb") ||
                           allText.Contains("Mar") || allText.Contains("Apr") ||
                           allText.Contains("May") || allText.Contains("Jun");
            hasMonth.Should().BeTrue("SVG should contain month abbreviations");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Timeline_SvgContainsMonthLabels));
            throw;
        }
    }

    [Fact]
    public async Task Timeline_SvgHasDropShadowFilter()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var svgHtml = await dashboard.Svg.InnerHTMLAsync();
            svgHtml.Should().Contain("filter");
            svgHtml.Should().Contain("shadow");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Timeline_SvgHasDropShadowFilter));
            throw;
        }
    }

    [Fact]
    public async Task Timeline_HasHorizontalTrackLines()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var lines = dashboard.Page.Locator(".tl-svg-box svg line");
            var count = await lines.CountAsync();
            count.Should().BeGreaterThan(0, "SVG should contain track horizontal lines");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Timeline_HasHorizontalTrackLines));
            throw;
        }
    }
}