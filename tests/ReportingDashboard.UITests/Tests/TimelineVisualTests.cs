using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class TimelineVisualTests
{
    private readonly PlaywrightFixture _fixture;

    public TimelineVisualTests(PlaywrightFixture fixture) => _fixture = fixture;

    private async Task<(IPage page, TimelinePageObject tl)> SetupAsync()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);
        await tl.NavigateAsync();
        return (page, tl);
    }

    #region Container Layout & Styling

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TimelineArea_IsVisible_OnDashboard()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            await Assertions.Expect(tl.TimelineArea).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(TimelineArea_IsVisible_OnDashboard));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TimelineArea_HasCorrectBackgroundColor()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var bg = await tl.GetTimelineAreaBackgroundAsync();
            // #FAFAFA = rgb(250, 250, 250)
            bg.Should().Contain("250, 250, 250", "Background should be #FAFAFA");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(TimelineArea_HasCorrectBackgroundColor));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TimelineArea_HasHeight196px()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var height = await tl.GetTimelineAreaHeightAsync();
            // Parse pixel value
            var px = double.Parse(height.Replace("px", ""));
            px.Should().BeApproximately(196, 2, "Timeline area height should be 196px");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(TimelineArea_HasHeight196px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TimelineArea_HasBottomBorder()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var border = await tl.GetTimelineAreaBorderBottomAsync();
            border.Should().Contain("2px", "Border should be 2px");
            border.Should().Contain("solid", "Border should be solid");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(TimelineArea_HasBottomBorder));
            throw;
        }
    }

    #endregion

    #region Sidebar Layout

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Sidebar_IsVisible()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            await Assertions.Expect(tl.Sidebar).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Sidebar_IsVisible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Sidebar_HasFixedWidth230px()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var width = await tl.GetSidebarComputedWidthAsync();
            width.Should().BeApproximately(230, 5, "Sidebar should be ~230px wide");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Sidebar_HasFixedWidth230px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Sidebar_HasAtLeastOneTrack()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var count = await tl.GetTrackCountAsync();
            count.Should().BeGreaterThan(0, "Dashboard should have at least one timeline track");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Sidebar_HasAtLeastOneTrack));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Sidebar_TrackLabelsHaveIdAndName()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var count = await tl.GetTrackCountAsync();
            for (int i = 0; i < count; i++)
            {
                var id = await tl.GetTrackIdTextAsync(i);
                var name = await tl.GetTrackNameTextAsync(i);
                id.Should().NotBeNullOrWhiteSpace($"Track {i} ID should not be empty");
                name.Should().NotBeNullOrWhiteSpace($"Track {i} name should not be empty");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Sidebar_TrackLabelsHaveIdAndName));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Sidebar_TrackIdsHaveDistinctColors()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var count = await tl.TrackIds.CountAsync();
            if (count > 1)
            {
                var colors = new HashSet<string>();
                for (int i = 0; i < count; i++)
                {
                    var color = await tl.GetTrackIdColorAsync(i);
                    colors.Add(color);
                }
                colors.Count.Should().BeGreaterThan(1, "Track IDs should have distinct colors");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Sidebar_TrackIdsHaveDistinctColors));
            throw;
        }
    }

    #endregion

    #region SVG Container

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Svg_IsVisible()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            await Assertions.Expect(tl.Svg).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Svg_IsVisible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Svg_HasWidth1560()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var width = await tl.GetSvgWidthAsync();
            width.Should().Be("1560");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Svg_HasWidth1560));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Svg_HasHeight185()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var height = await tl.GetSvgHeightAsync();
            height.Should().Be("185");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Svg_HasHeight185));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Svg_HasOverflowVisible()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var overflow = await tl.Svg.GetAttributeAsync("overflow");
            overflow.Should().Be("visible");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Svg_HasOverflowVisible));
            throw;
        }
    }

    #endregion

    #region Month Grid

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task MonthGrid_HasVerticalGridLines()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var count = await tl.MonthGridLines.CountAsync();
            count.Should().BeGreaterThan(0, "Should have month grid lines");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(MonthGrid_HasVerticalGridLines));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task MonthGrid_GridLinesHaveCorrectOpacity()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var firstLine = tl.MonthGridLines.First;
            var opacity = await firstLine.GetAttributeAsync("stroke-opacity");
            opacity.Should().Be("0.4");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(MonthGrid_GridLinesHaveCorrectOpacity));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task MonthGrid_HasMonthTextLabels()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var count = await tl.MonthLabels.CountAsync();
            count.Should().BeGreaterThan(0, "Should have month text labels");

            // Verify at least some expected month abbreviations
            var svgHtml = await tl.Svg.InnerHTMLAsync();
            var hasAnyMonth = svgHtml.Contains("Jan") || svgHtml.Contains("Feb") ||
                              svgHtml.Contains("Mar") || svgHtml.Contains("Apr");
            hasAnyMonth.Should().BeTrue("SVG should contain month abbreviations");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(MonthGrid_HasMonthTextLabels));
            throw;
        }
    }

    #endregion

    #region Track Lines

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TrackLines_RenderedForEachTrack()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var trackCount = await tl.GetTrackCountAsync();
            var lineCount = await tl.TrackLines.CountAsync();
            lineCount.Should().BeGreaterOrEqualTo(trackCount,
                "Should have at least one horizontal line per track");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(TrackLines_RenderedForEachTrack));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TrackLines_SpanFullSvgWidth()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var count = await tl.TrackLines.CountAsync();
            for (int i = 0; i < Math.Min(count, 3); i++)
            {
                var line = tl.TrackLines.Nth(i);
                var x1 = await line.GetAttributeAsync("x1");
                var x2 = await line.GetAttributeAsync("x2");
                x1.Should().Be("0", $"Track line {i} should start at x=0");
                x2.Should().Be("1560", $"Track line {i} should end at x=1560");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(TrackLines_SpanFullSvgWidth));
            throw;
        }
    }

    #endregion

    #region NOW Line

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task NowLine_IsPresent()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var count = await tl.NowLine.CountAsync();
            count.Should().Be(1, "Should have exactly one NOW dashed line");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(NowLine_IsPresent));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task NowLine_HasCorrectStrokeAttributes()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var line = tl.NowLine;
            var stroke = await line.GetAttributeAsync("stroke");
            var strokeWidth = await line.GetAttributeAsync("stroke-width");
            var dashArray = await line.GetAttributeAsync("stroke-dasharray");

            stroke.Should().Be("#EA4335");
            strokeWidth.Should().Be("2");
            dashArray.Should().Be("5,3");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(NowLine_HasCorrectStrokeAttributes));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task NowLine_SpansFullSvgHeight()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var line = tl.NowLine;
            var y1 = await line.GetAttributeAsync("y1");
            var y2 = await line.GetAttributeAsync("y2");

            y1.Should().Be("0");
            y2.Should().Be("185");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(NowLine_SpansFullSvgHeight));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task NowLabel_IsVisible()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var count = await tl.NowLabel.CountAsync();
            count.Should().BeGreaterThan(0, "NOW label should be present");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(NowLabel_IsVisible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task NowLabel_HasBoldRedStyling()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var label = tl.NowLabel.First;
            var fill = await label.GetAttributeAsync("fill");
            var fontWeight = await label.GetAttributeAsync("font-weight");
            var fontSize = await label.GetAttributeAsync("font-size");

            fill.Should().Be("#EA4335");
            fontWeight.Should().Be("700");
            fontSize.Should().Be("10");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(NowLabel_HasBoldRedStyling));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task NowLabel_AlignedWithNowLine()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var lineX = await tl.NowLine.GetAttributeAsync("x1");
            var labelX = await tl.NowLabel.First.GetAttributeAsync("x");

            lineX.Should().Be(labelX, "NOW label X should match NOW line X position");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(NowLabel_AlignedWithNowLine));
            throw;
        }
    }

    #endregion

    #region Milestone Markers

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task PocDiamonds_ArePresent()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var count = await tl.PocDiamonds.CountAsync();
            count.Should().BeGreaterThan(0, "Should have at least one PoC diamond");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(PocDiamonds_ArePresent));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task PocDiamonds_HaveDropShadowFilter()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var count = await tl.PocDiamonds.CountAsync();
            for (int i = 0; i < count; i++)
            {
                var filter = await tl.PocDiamonds.Nth(i).GetAttributeAsync("filter");
                filter.Should().Be("url(#sh)", $"PoC diamond {i} should have drop shadow filter");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(PocDiamonds_HaveDropShadowFilter));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task ProductionDiamonds_ArePresent()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var count = await tl.ProductionDiamonds.CountAsync();
            count.Should().BeGreaterThan(0, "Should have at least one production diamond");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ProductionDiamonds_ArePresent));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task ProductionDiamonds_HaveDropShadowFilter()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var count = await tl.ProductionDiamonds.CountAsync();
            for (int i = 0; i < count; i++)
            {
                var filter = await tl.ProductionDiamonds.Nth(i).GetAttributeAsync("filter");
                filter.Should().Be("url(#sh)", $"Production diamond {i} should have drop shadow filter");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ProductionDiamonds_HaveDropShadowFilter));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task CheckpointCircles_HaveCorrectAttributes()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var count = await tl.CheckpointCircles.CountAsync();
            if (count > 0)
            {
                var first = tl.CheckpointCircles.First;
                var fill = await first.GetAttributeAsync("fill");
                var strokeWidth = await first.GetAttributeAsync("stroke-width");
                var r = await first.GetAttributeAsync("r");

                fill.Should().Be("white");
                strokeWidth.Should().Be("2.5");
                r.Should().Be("7");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(CheckpointCircles_HaveCorrectAttributes));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DotCircles_HaveCorrectAttributes()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var count = await tl.DotCircles.CountAsync();
            if (count > 0)
            {
                var first = tl.DotCircles.First;
                var fill = await first.GetAttributeAsync("fill");
                var r = await first.GetAttributeAsync("r");

                fill.Should().Be("#999");
                r.Should().Be("4");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DotCircles_HaveCorrectAttributes));
            throw;
        }
    }

    #endregion

    #region Tooltips

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Milestones_HaveTitleElements_ForTooltips()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            // Each polygon and circle milestone should have a <title> child
            var polygonTitles = await tl.Svg.Locator("polygon > title").CountAsync();
            var circleTitles = await tl.Svg.Locator("circle > title").CountAsync();

            var totalPolygons = await tl.AllPolygons.CountAsync();
            var totalMilestoneCircles = (await tl.CheckpointCircles.CountAsync()) +
                                         (await tl.DotCircles.CountAsync());

            polygonTitles.Should().Be(totalPolygons, "Every polygon should have a <title> tooltip");
            circleTitles.Should().Be(totalMilestoneCircles, "Every milestone circle should have a <title> tooltip");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Milestones_HaveTitleElements_ForTooltips));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Tooltip_ContainsLabelAndDate()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            // Check first polygon tooltip contains " - " separator (label - date format)
            var polygonCount = await tl.AllPolygons.CountAsync();
            if (polygonCount > 0)
            {
                var titleText = await tl.AllPolygons.First.Locator("title").TextContentAsync();
                titleText.Should().Contain(" - ", "Tooltip should contain 'label - date' format");
                titleText.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}",
                    "Tooltip should contain an ISO date");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Tooltip_ContainsLabelAndDate));
            throw;
        }
    }

    #endregion

    #region Drop Shadow Filter

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DropShadowFilter_ExistsInDefs()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var filterCount = await tl.DropShadowFilter.CountAsync();
            filterCount.Should().Be(1, "Should have exactly one #sh filter");

            var feCount = await tl.FeDropShadow.CountAsync();
            feCount.Should().Be(1, "Should have exactly one feDropShadow element");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DropShadowFilter_ExistsInDefs));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DropShadowFilter_HasCorrectAttributes()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var fe = tl.FeDropShadow;
            var dx = await fe.GetAttributeAsync("dx");
            var dy = await fe.GetAttributeAsync("dy");
            var stdDev = await fe.GetAttributeAsync("stdDeviation");
            var floodOpacity = await fe.GetAttributeAsync("flood-opacity");

            dx.Should().Be("0");
            dy.Should().Be("1");
            stdDev.Should().Be("1.5");
            floodOpacity.Should().Be("0.3");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DropShadowFilter_HasCorrectAttributes));
            throw;
        }
    }

    #endregion

    #region Milestone Date Labels

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task MilestoneDateLabels_ArePresent()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var count = await tl.MilestoneDateLabels.CountAsync();
            // Exclude the NOW label; milestone labels have fill=#666
            var milestoneLabels = tl.Svg.Locator("text[font-size='10'][fill='#666']");
            var mlCount = await milestoneLabels.CountAsync();
            mlCount.Should().BeGreaterThan(0, "Should have milestone date labels");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(MilestoneDateLabels_ArePresent));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task MilestoneDateLabels_HaveTextAnchorMiddle()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var labels = tl.Svg.Locator("text[font-size='10'][fill='#666']");
            var count = await labels.CountAsync();
            for (int i = 0; i < Math.Min(count, 5); i++)
            {
                var anchor = await labels.Nth(i).GetAttributeAsync("text-anchor");
                anchor.Should().Be("middle", $"Milestone label {i} should have text-anchor middle");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(MilestoneDateLabels_HaveTextAnchorMiddle));
            throw;
        }
    }

    #endregion

    #region Screenshot Fidelity

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_ScreenshotCapture_At1920x1080()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            // Verify the page is at the correct viewport
            var viewport = page.ViewportSize;
            viewport!.Width.Should().Be(1920);
            viewport.Height.Should().Be(1080);

            // Take a screenshot of just the timeline area for visual inspection
            var dir = Path.Combine(AppContext.BaseDirectory, "screenshots");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, $"timeline_fidelity_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");

            await tl.TimelineArea.ScreenshotAsync(new LocatorScreenshotOptions { Path = path });

            File.Exists(path).Should().BeTrue("Screenshot should be captured");
            new FileInfo(path).Length.Should().BeGreaterThan(0, "Screenshot should not be empty");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_ScreenshotCapture_At1920x1080));
            throw;
        }
    }

    #endregion

    #region Sidebar and SVG Relative Positioning

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task SidebarAndSvg_AreHorizontallyAdjacent()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var sidebarBox = await tl.Sidebar.EvaluateAsync<Dictionary<string, double>>(
                "el => { var r = el.getBoundingClientRect(); return { left: r.left, right: r.right, top: r.top }; }");
            var svgBoxBox = await tl.SvgBox.EvaluateAsync<Dictionary<string, double>>(
                "el => { var r = el.getBoundingClientRect(); return { left: r.left, right: r.right, top: r.top }; }");

            // SVG box should start at or near where sidebar ends
            svgBoxBox["left"].Should().BeGreaterOrEqualTo(sidebarBox["right"] - 2,
                "SVG box should be to the right of the sidebar");

            // Both should be at approximately the same vertical position
            Math.Abs(sidebarBox["top"] - svgBoxBox["top"]).Should().BeLessThan(20,
                "Sidebar and SVG box should be vertically aligned");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(SidebarAndSvg_AreHorizontallyAdjacent));
            throw;
        }
    }

    #endregion

    #region Track Count Matches Between Sidebar and SVG

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TrackCount_SidebarMatchesSvgLines()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var sidebarTracks = await tl.GetTrackCountAsync();

            // Count horizontal track lines (x1=0, x2=1560, stroke-width=3)
            var svgTrackLines = await tl.Svg
                .Locator("line[x1='0'][x2='1560'][stroke-width='3']")
                .CountAsync();

            svgTrackLines.Should().Be(sidebarTracks,
                "Number of SVG track lines should match sidebar track count");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(TrackCount_SidebarMatchesSvgLines));
            throw;
        }
    }

    #endregion

    #region SVG Font Family

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Svg_HasSegoeUIFontFamily()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var style = await tl.Svg.GetAttributeAsync("style") ?? "";
            style.Should().Contain("Segoe UI", "SVG should use Segoe UI font family");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Svg_HasSegoeUIFontFamily));
            throw;
        }
    }

    #endregion

    #region NowLine Horizontal Position Sanity

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task NowLine_IsWithinSvgBounds()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var x1Str = await tl.NowLine.GetAttributeAsync("x1");
            x1Str.Should().NotBeNull();

            var x1 = double.Parse(x1Str!);
            x1.Should().BeGreaterOrEqualTo(0, "NOW line should not be before SVG start");
            x1.Should().BeLessOrEqualTo(1560, "NOW line should not exceed SVG width");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(NowLine_IsWithinSvgBounds));
            throw;
        }
    }

    #endregion

    #region Polygon Points Validity

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task AllPolygons_HaveValidFourPointDiamondShape()
    {
        var (page, tl) = await SetupAsync();
        try
        {
            var count = await tl.AllPolygons.CountAsync();
            for (int i = 0; i < count; i++)
            {
                var points = await tl.AllPolygons.Nth(i).GetAttributeAsync("points");
                points.Should().NotBeNullOrWhiteSpace($"Polygon {i} should have points attribute");

                var pointPairs = points!.Trim().Split(' ');
                pointPairs.Length.Should().Be(4, $"Polygon {i} should have exactly 4 points (diamond)");

                foreach (var pair in pointPairs)
                {
                    var coords = pair.Split(',');
                    coords.Length.Should().Be(2, $"Each point should have x,y coordinates");
                    double.TryParse(coords[0], out _).Should().BeTrue("X should be numeric");
                    double.TryParse(coords[1], out _).Should().BeTrue("Y should be numeric");
                }
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(AllPolygons_HaveValidFourPointDiamondShape));
            throw;
        }
    }

    #endregion
}