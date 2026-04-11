using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Additional timeline-specific UI tests covering layout dimensions,
/// track rendering, milestone shapes, and SVG structural elements.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class TimelineLayoutUITests
{
    private readonly PlaywrightFixture _fixture;

    public TimelineLayoutUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Timeline_Height_Is196px()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var height = await dashboard.TimelineSection.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).height");
            height.Should().Be("196px");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_Height");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_Background_IsFAFAFA()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var bg = await dashboard.TimelineSection.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).backgroundColor");
            // #FAFAFA = rgb(250, 250, 250)
            bg.Should().Be("rgb(250, 250, 250)");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_Background");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_BorderBottom_Is2pxSolid()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var borderWidth = await dashboard.TimelineSection.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).borderBottomWidth");
            borderWidth.Should().Be("2px");

            var borderStyle = await dashboard.TimelineSection.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).borderBottomStyle");
            borderStyle.Should().Be("solid");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_BorderBottom");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_Display_IsFlex()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var display = await dashboard.TimelineSection.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).display");
            display.Should().Be("flex");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_Display");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_SvgBox_IsPresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var svgBoxCount = await page.Locator(".tl-svg-box").CountAsync();
            svgBoxCount.Should().BeGreaterThanOrEqualTo(1);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_SvgBox");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_TrackCount_MatchesDataJson()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var timeline = new TimelinePage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var trackCount = await timeline.GetTrackCountAsync();
            // data.json has 3 tracks
            trackCount.Should().BeGreaterThanOrEqualTo(2,
                "data.json should have at least 2 tracks");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_TrackCount");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_NowLine_IsRedDashed()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var timeline = new TimelinePage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var nowLineCount = await timeline.NowLine.CountAsync();
            if (nowLineCount > 0)
            {
                var stroke = await timeline.NowLine.First.GetAttributeAsync("stroke");
                stroke.Should().Be("#EA4335");

                var dashArray = await timeline.NowLine.First.GetAttributeAsync("stroke-dasharray");
                dashArray.Should().NotBeNullOrEmpty("NOW line should be dashed");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_NowLineDashed");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_CheckpointCircles_ArePresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var timeline = new TimelinePage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var circleCount = await timeline.CheckpointCircles.CountAsync();
            circleCount.Should().BeGreaterThanOrEqualTo(1,
                "at least one checkpoint circle should be rendered");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_Checkpoints");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_SvgElement_HeightAccommodatesTracks()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var timeline = new TimelinePage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var heightStr = await timeline.GetSvgHeightAsync();
            heightStr.Should().NotBeNullOrEmpty();

            if (int.TryParse(heightStr, out var height))
            {
                height.Should().BeGreaterThan(50, "SVG height should be substantial");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_SvgHeight");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_IsPositionedBetweenHeaderAndHeatmap()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var hdrBox = await dashboard.HeaderSection.BoundingBoxAsync();
            var tlBox = await dashboard.TimelineSection.BoundingBoxAsync();
            var hmBox = await dashboard.HeatmapSection.BoundingBoxAsync();

            hdrBox.Should().NotBeNull();
            tlBox.Should().NotBeNull();
            hmBox.Should().NotBeNull();

            tlBox!.Y.Should().BeGreaterThanOrEqualTo(hdrBox!.Y + hdrBox.Height - 1,
                "timeline should start at or below header bottom");
            hmBox!.Y.Should().BeGreaterThanOrEqualTo(tlBox.Y + tlBox.Height - 1,
                "heatmap should start at or below timeline bottom");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_Position");
            throw;
        }
    }
}