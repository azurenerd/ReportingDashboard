using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class TimelineVisualFidelityUITests
{
    private readonly PlaywrightFixture _fixture;

    public TimelineVisualFidelityUITests(PlaywrightFixture fixture)
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
    public async Task Timeline_SidebarAndSvgRenderSideBySide()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var labelsBox = await timeline.TimelineLabelsContainer.BoundingBoxAsync();
            var svgBox = await timeline.SvgBox.BoundingBoxAsync();

            labelsBox.Should().NotBeNull("sidebar should be visible");
            svgBox.Should().NotBeNull("SVG box should be visible");

            // Sidebar should be to the left of SVG
            labelsBox!.X.Should().BeLessThan(svgBox!.X,
                "labels sidebar should be to the left of SVG area");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_SideBySide");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_SvgBoxFillsRemainingWidth()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var svgBoxFlex = await timeline.SvgBox.EvaluateAsync<string>(
                "el => getComputedStyle(el).flexGrow");

            svgBoxFlex.Should().Be("1", ".tl-svg-box should have flex: 1");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_SvgBoxFlex");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_TrackCountMatchesSidebarLabels()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var labelCount = await timeline.GetTrackLabelCountAsync();
            var trackLineCount = await timeline.TrackLines.CountAsync();

            trackLineCount.Should().Be(labelCount,
                "number of SVG track lines should match sidebar label count");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_TrackCountMatch");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_GridLinesHaveCorrectOpacity()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var gridLineCount = await timeline.GridLines.CountAsync();
            if (gridLineCount > 0)
            {
                var opacity = await timeline.GridLines.First.GetAttributeAsync("stroke-opacity");
                opacity.Should().Be("0.4");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_GridOpacity");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_AllDiamondPolygonsHaveShadowFilter()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var polygonCount = await timeline.SvgPolygons.CountAsync();
            for (int i = 0; i < polygonCount; i++)
            {
                var filter = await timeline.SvgPolygons.Nth(i).GetAttributeAsync("filter");
                filter.Should().Contain("shadow",
                    $"polygon {i} should reference shadow filter");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_DiamondFilters");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_MilestoneTextLabelsUseMiddleAnchor()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var textElements = page.Locator(".tl-svg-box svg text[text-anchor='middle']");
            var count = await textElements.CountAsync();

            count.Should().BeGreaterOrEqualTo(1,
                "milestone labels should use text-anchor: middle");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_TextAnchor");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_NowLabelIsRedBold()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var nowText = page.Locator(
                ".tl-svg-box svg text[fill='#EA4335'][font-weight='700']");
            var count = await nowText.CountAsync();

            count.Should().BeGreaterOrEqualTo(1,
                "NOW label should be red (#EA4335) and bold (700)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_NowLabelStyle");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_MonthLabelsHaveCorrectFontProperties()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var monthTexts = page.Locator(
                ".tl-svg-box svg text[font-size='11'][font-weight='600'][fill='#666']");
            var count = await monthTexts.CountAsync();

            count.Should().BeGreaterOrEqualTo(6,
                "month labels should have font-size 11, weight 600, fill #666");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_MonthFontProps");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_SvgNamespaceIsCorrect()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var xmlns = await timeline.SvgElement.GetAttributeAsync("xmlns");
            xmlns.Should().Be("http://www.w3.org/2000/svg");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_SvgNamespace");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_NoConsoleErrorsOnLoad()
    {
        var page = await _fixture.NewPageAsync();
        var errors = new List<string>();
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
                errors.Add(msg.Text);
        };

        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            // Allow a brief moment for any async errors
            await page.WaitForTimeoutAsync(1000);

            errors.Should().BeEmpty("dashboard should load without console errors");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_ConsoleErrors");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_MilestonePositions_AreWithinSvgBounds()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var svgWidth = 1560;

            // Check polygon positions
            var polygonCount = await timeline.SvgPolygons.CountAsync();
            for (int i = 0; i < polygonCount; i++)
            {
                var points = await timeline.SvgPolygons.Nth(i).GetAttributeAsync("points");
                if (!string.IsNullOrEmpty(points))
                {
                    var vertices = points.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var firstVertex = vertices[0].Split(',');
                    var cx = double.Parse(firstVertex[0]);

                    // Allow some tolerance for edge milestones
                    cx.Should().BeInRange(-50, svgWidth + 50,
                        $"polygon {i} center X should be near SVG bounds");
                }
            }

            // Check circle positions
            var circleCount = await timeline.SvgCircles.CountAsync();
            for (int i = 0; i < circleCount; i++)
            {
                var cxStr = await timeline.SvgCircles.Nth(i).GetAttributeAsync("cx");
                if (!string.IsNullOrEmpty(cxStr))
                {
                    var cx = double.Parse(cxStr);
                    cx.Should().BeInRange(-50, svgWidth + 50,
                        $"circle {i} cx should be near SVG bounds");
                }
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_MarkerBounds");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_TrackLinesHaveDistinctColors()
    {
        var (page, timeline) = await SetupAsync();
        try
        {
            var trackLineCount = await timeline.TrackLines.CountAsync();
            var colors = new HashSet<string>();

            for (int i = 0; i < trackLineCount; i++)
            {
                var stroke = await timeline.TrackLines.Nth(i).GetAttributeAsync("stroke");
                if (!string.IsNullOrEmpty(stroke))
                    colors.Add(stroke);
            }

            if (trackLineCount > 1)
            {
                colors.Count.Should().Be(trackLineCount,
                    "each track should have a distinct color");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_DistinctColors");
            throw;
        }
    }
}