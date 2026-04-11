using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// UI tests verifying Timeline milestone rendering from PR #555.
/// Covers PoC diamonds (#F4B400), Production diamonds (#34A853),
/// Checkpoint circles, NOW marker (#EA4335), and SVG structure.
/// Tests the Components/Timeline.razor inline SVG rendering.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class TimelineMilestoneTests
{
    private readonly PlaywrightFixture _fixture;

    public TimelineMilestoneTests(PlaywrightFixture fixture) => _fixture = fixture;

    #region Milestone Shape Rendering

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_PocMilestones_AreGoldDiamonds()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var fillColors = await timeline.GetPolygonFillColorsAsync();
            Assert.Contains("#F4B400", fillColors);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_PocMilestones_AreGoldDiamonds));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_ProductionMilestones_AreGreenDiamonds()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var fillColors = await timeline.GetPolygonFillColorsAsync();
            Assert.Contains("#34A853", fillColors);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_ProductionMilestones_AreGreenDiamonds));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_CheckpointMilestones_AreCircles()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var circleCount = await timeline.GetCircleCountAsync();
            Assert.True(circleCount > 0, "Expected at least one checkpoint circle");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_CheckpointMilestones_AreCircles));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_CheckpointCircles_HaveWhiteFill()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var circles = timeline.AllCircles;
            var count = await circles.CountAsync();
            for (int i = 0; i < count; i++)
            {
                var fill = await circles.Nth(i).GetAttributeAsync("fill");
                Assert.Equal("white", fill);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_CheckpointCircles_HaveWhiteFill));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_Diamonds_HaveDropShadowFilter()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var polygons = timeline.AllPolygons;
            var count = await polygons.CountAsync();
            Assert.True(count > 0, "Expected at least one diamond polygon");

            for (int i = 0; i < count; i++)
            {
                var filter = await polygons.Nth(i).GetAttributeAsync("filter");
                Assert.Contains("url(#sh)", filter ?? "");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_Diamonds_HaveDropShadowFilter));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_DropShadowFilter_ExistsInDefs()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var filterCount = await timeline.DropShadowFilter.CountAsync();
            Assert.True(filterCount > 0, "Expected drop shadow filter in SVG defs");

            var dropShadow = page.Locator(".tl-svg-box svg defs filter feDropShadow");
            Assert.True(await dropShadow.CountAsync() > 0, "Expected feDropShadow element");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_DropShadowFilter_ExistsInDefs));
            throw;
        }
    }

    #endregion

    #region NOW Marker

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_NowMarker_HasDashedStroke()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var dashedLines = timeline.DashedLines;
            var count = await dashedLines.CountAsync();
            Assert.True(count > 0, "Expected at least one dashed line (NOW marker)");

            for (int i = 0; i < count; i++)
            {
                var dashArray = await dashedLines.Nth(i).GetAttributeAsync("stroke-dasharray");
                Assert.Equal("5,3", dashArray);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_NowMarker_HasDashedStroke));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_NowMarker_HasRedColor()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var dashedLines = timeline.DashedLines;
            var count = await dashedLines.CountAsync();
            Assert.True(count > 0);

            var stroke = await dashedLines.First.GetAttributeAsync("stroke");
            Assert.Equal("#EA4335", stroke);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_NowMarker_HasRedColor));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_NowLabel_HasBoldRedText()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var nowText = timeline.NowText;
            Assert.True(await nowText.CountAsync() > 0, "Expected NOW text label");

            var fill = await nowText.GetAttributeAsync("fill");
            Assert.Equal("#EA4335", fill);

            var fontWeight = await nowText.GetAttributeAsync("font-weight");
            Assert.Equal("700", fontWeight);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_NowLabel_HasBoldRedText));
            throw;
        }
    }

    #endregion

    #region SVG Structure

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_SvgWidth_Is1560()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var width = await timeline.GetSvgWidthAsync();
            Assert.Equal("1560", width);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_SvgWidth_Is1560));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_SvgHeight_IsAtLeast185()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var heightStr = await timeline.GetSvgHeightAsync();
            Assert.NotNull(heightStr);
            Assert.True(double.TryParse(heightStr, out var height));
            Assert.True(height >= 185, $"SVG height {height} should be at least 185");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_SvgHeight_IsAtLeast185));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_MonthGridLines_Present()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var lineCount = await timeline.GetLineCountAsync();
            // At minimum: grid lines + track lines + NOW line
            Assert.True(lineCount >= 3, $"Expected at least 3 SVG lines, got {lineCount}");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_MonthGridLines_Present));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_MonthLabels_VisibleInSvg()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var textElements = timeline.AllText;
            var count = await textElements.CountAsync();
            Assert.True(count > 0, "Expected text elements in SVG");

            var allText = "";
            for (int i = 0; i < count; i++)
            {
                allText += await textElements.Nth(i).TextContentAsync() + " ";
            }

            Assert.Contains("Jan", allText);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_MonthLabels_VisibleInSvg));
            throw;
        }
    }

    #endregion

    #region Milestone Tooltips

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_Milestones_HaveTooltips()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var tooltips = await timeline.GetMilestoneTooltipsAsync();
            Assert.True(tooltips.Count > 0, "Expected milestone tooltips (title elements)");
            Assert.All(tooltips, t => Assert.False(string.IsNullOrWhiteSpace(t),
                "Tooltip text should not be empty"));
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_Milestones_HaveTooltips));
            throw;
        }
    }

    #endregion

    #region Timeline Area CSS

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TimelineArea_HasFAFAFABackground()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var bgColor = await timeline.TimelineArea.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // #FAFAFA = rgb(250, 250, 250)
            Assert.Contains("250", bgColor);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(TimelineArea_HasFAFAFABackground));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TimelineArea_Has196pxHeight()
    {
        var page = await _fixture.NewPageAsync();
        var timeline = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await timeline.NavigateAsync();

            var height = await timeline.TimelineArea.EvaluateAsync<string>(
                "el => getComputedStyle(el).height");
            Assert.Equal("196px", height);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(TimelineArea_Has196pxHeight));
            throw;
        }
    }

    #endregion
}