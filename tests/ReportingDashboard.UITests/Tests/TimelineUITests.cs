using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class TimelineUITests
{
    private readonly PlaywrightFixture _fixture;

    public TimelineUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<(IPage page, TimelinePage timeline)> SetupAsync()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePage(page, _fixture.BaseUrl);
        await timeline.NavigateAsync();
        return (page, timeline);
    }

    [Fact]
    public async Task TimelineSection_IsVisibleOnDashboard()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            await Assertions.Expect(timeline.TimelineArea).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSection_Visibility");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSidebar_DisplaysTrackLabels()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            await Assertions.Expect(timeline.TimelineLabelsContainer).ToBeVisibleAsync();

            var count = await timeline.GetTrackLabelCountAsync();
            count.Should().BeGreaterOrEqualTo(1, "at least one track label should render");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSidebar_Labels");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSidebar_TrackIdsAreVisible()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var count = await timeline.TimelineTrackIds.CountAsync();
            count.Should().BeGreaterOrEqualTo(1);

            var firstId = await timeline.GetTrackIdTextAsync(0);
            firstId.Should().NotBeNullOrWhiteSpace();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSidebar_TrackIds");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSidebar_TrackNamesAreVisible()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var count = await timeline.TimelineTrackNames.CountAsync();
            count.Should().BeGreaterOrEqualTo(1);

            var firstName = await timeline.GetTrackNameTextAsync(0);
            firstName.Should().NotBeNullOrWhiteSpace();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSidebar_TrackNames");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSidebar_TrackIdsHaveColorStyling()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var style = await timeline.GetTrackIdColorAsync(0);
            style.Should().NotBeNullOrEmpty();
            style.Should().Contain("color:");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSidebar_TrackColors");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_ElementExists()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            await Assertions.Expect(timeline.SvgElement).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_Exists");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_HasCorrectWidth()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var width = await timeline.GetSvgWidthAsync();
            width.Should().Be("1560");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_Width");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_HasOverflowVisible()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var overflow = await timeline.GetSvgOverflowAsync();
            overflow.Should().Be("visible");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_Overflow");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_HasDynamicHeight()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var heightStr = await timeline.GetSvgHeightAsync();
            heightStr.Should().NotBeNullOrEmpty();

            int.TryParse(heightStr, out var height).Should().BeTrue();
            height.Should().BeGreaterThan(0);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_Height");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_ContainsDropShadowFilter()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var filterCount = await timeline.SvgFilter.CountAsync();
            filterCount.Should().BeGreaterOrEqualTo(1, "SVG should contain a drop shadow filter");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_Filter");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_MonthGridLinesExist()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var gridLineCount = await timeline.GetGridLineCountAsync();
            gridLineCount.Should().BeGreaterOrEqualTo(6,
                "should have at least 6 month grid lines for Jan-Jun range");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_GridLines");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_MonthLabelsArePresent()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var hasJan = await timeline.PageContainsTextAsync("Jan");
            var hasFeb = await timeline.PageContainsTextAsync("Feb");
            var hasMar = await timeline.PageContainsTextAsync("Mar");

            hasJan.Should().BeTrue("Jan month label should be present");
            hasFeb.Should().BeTrue("Feb month label should be present");
            hasMar.Should().BeTrue("Mar month label should be present");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_MonthLabels");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_NowLineIsPresent()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var hasNowLine = await timeline.IsNowLineVisibleAsync();
            hasNowLine.Should().BeTrue("NOW dashed line should be rendered");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_NowLine");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_NowLineHasDashedStyle()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var dasharray = await timeline.NowLine.GetAttributeAsync("stroke-dasharray");
            dasharray.Should().Be("5,3");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_NowLineDash");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_NowLineHasCorrectStrokeWidth()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var strokeWidth = await timeline.NowLine.GetAttributeAsync("stroke-width");
            strokeWidth.Should().Be("2");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_NowLineStroke");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_NowTextLabelExists()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var hasNow = await timeline.PageContainsTextAsync("NOW");
            hasNow.Should().BeTrue("NOW text label should appear in SVG");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_NowLabel");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_TrackHorizontalLinesExist()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var trackLineCount = await timeline.TrackLines.CountAsync();
            trackLineCount.Should().BeGreaterOrEqualTo(1,
                "at least one track horizontal line should render");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_TrackLines");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_MilestoneMarkersExist()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var polygonCount = await timeline.GetPolygonCountAsync();
            var circleCount = await timeline.GetCircleCountAsync();

            var totalMarkers = polygonCount + circleCount;
            totalMarkers.Should().BeGreaterOrEqualTo(1,
                "at least one milestone marker (diamond or circle) should render");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_Markers");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_PocDiamondsAreGold()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var goldPolygons = page.Locator(".tl-svg-box svg polygon[fill='#F4B400']");
            var count = await goldPolygons.CountAsync();

            // May or may not have PoC milestones in sample data, but verify styling if present
            if (count > 0)
            {
                var filter = await goldPolygons.First.GetAttributeAsync("filter");
                filter.Should().Contain("shadow", "PoC diamonds should have shadow filter");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_PocDiamonds");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_ProductionDiamondsAreGreen()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var greenPolygons = page.Locator(".tl-svg-box svg polygon[fill='#34A853']");
            var count = await greenPolygons.CountAsync();

            if (count > 0)
            {
                var filter = await greenPolygons.First.GetAttributeAsync("filter");
                filter.Should().Contain("shadow", "Production diamonds should have shadow filter");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_ProdDiamonds");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_CheckpointCirclesHaveWhiteFill()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var whiteCircles = page.Locator(".tl-svg-box svg circle[fill='white']");
            var count = await whiteCircles.CountAsync();

            if (count > 0)
            {
                var strokeWidth = await whiteCircles.First.GetAttributeAsync("stroke-width");
                strokeWidth.Should().Be("2.5");

                var r = await whiteCircles.First.GetAttributeAsync("r");
                r.Should().Be("5");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_Checkpoints");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_MilestonesHaveTooltips()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var titleCount = await timeline.SvgTitles.CountAsync();
            titleCount.Should().BeGreaterOrEqualTo(1,
                "milestone markers should have <title> elements for tooltips");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_Tooltips");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_TooltipsContainMilestoneLabels()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var titleCount = await timeline.SvgTitles.CountAsync();
            if (titleCount > 0)
            {
                var firstTitle = await timeline.SvgTitles.First.TextContentAsync();
                firstTitle.Should().NotBeNullOrWhiteSpace(
                    "tooltip title should contain milestone label text");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_TooltipContent");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_NowLineSpansFullHeight()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var y1 = await timeline.NowLine.GetAttributeAsync("y1");
            y1.Should().Be("0", "NOW line should start at top of SVG");

            var y2 = await timeline.NowLine.GetAttributeAsync("y2");
            var svgHeight = await timeline.GetSvgHeightAsync();
            y2.Should().Be(svgHeight, "NOW line should span full SVG height");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_NowLineHeight");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_GridLinesSpanFullHeight()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var gridLineCount = await timeline.GridLines.CountAsync();
            if (gridLineCount > 0)
            {
                var y1 = await timeline.GridLines.First.GetAttributeAsync("y1");
                y1.Should().Be("0");

                var y2 = await timeline.GridLines.First.GetAttributeAsync("y2");
                var svgHeight = await timeline.GetSvgHeightAsync();
                y2.Should().Be(svgHeight);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_GridLineHeight");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_PolygonPointsHaveFourVertices()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var polygonCount = await timeline.SvgPolygons.CountAsync();
            if (polygonCount > 0)
            {
                var points = await timeline.SvgPolygons.First.GetAttributeAsync("points");
                points.Should().NotBeNullOrEmpty();

                var vertices = points!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                vertices.Should().HaveCount(4, "diamond polygon should have 4 vertices");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_DiamondVertices");
            throw;
        }
    }

    [Fact]
    public async Task TimelineSvg_TrackLinesSpanFullWidth()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var trackLineCount = await timeline.TrackLines.CountAsync();
            if (trackLineCount > 0)
            {
                var x1 = await timeline.TrackLines.First.GetAttributeAsync("x1");
                x1.Should().Be("0", "track line should start at x=0");

                var x2 = await timeline.TrackLines.First.GetAttributeAsync("x2");
                x2.Should().Be("1560", "track line should end at x=1560");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "TimelineSvg_TrackLineWidth");
            throw;
        }
    }
}