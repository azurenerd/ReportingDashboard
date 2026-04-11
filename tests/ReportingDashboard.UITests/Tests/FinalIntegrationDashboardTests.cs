using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// End-to-end UI tests verifying the Final Integration PR (#521).
/// Tests all three major sections (Header, Timeline, Heatmap) rendered together
/// in a single page load using the root Components/ versions with inline styles.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class FinalIntegrationDashboardTests
{
    private readonly PlaywrightFixture _fixture;

    public FinalIntegrationDashboardTests(PlaywrightFixture fixture) => _fixture = fixture;

    #region Full Dashboard Loads With All Sections

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_AllThreeSectionsPresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            await Assertions.Expect(dashboard.Header).ToBeVisibleAsync();
            await Assertions.Expect(dashboard.TimelineArea).ToBeVisibleAsync();
            await Assertions.Expect(dashboard.HeatmapWrap).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_AllThreeSectionsPresent));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_SectionsInCorrectVerticalOrder()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var headerBox = await dashboard.Header.BoundingBoxAsync();
            var tlBox = await dashboard.TimelineArea.BoundingBoxAsync();
            var hmBox = await dashboard.HeatmapWrap.BoundingBoxAsync();

            Assert.NotNull(headerBox);
            Assert.NotNull(tlBox);
            Assert.NotNull(hmBox);

            Assert.True(headerBox!.Y < tlBox!.Y,
                "Header should be above Timeline");
            Assert.True(tlBox.Y < hmBox!.Y,
                "Timeline should be above Heatmap");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_SectionsInCorrectVerticalOrder));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_FitsWithin1920x1080_NoScrollbars()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var hasScrollbar = await page.EvaluateAsync<bool>(
                "() => document.documentElement.scrollHeight > document.documentElement.clientHeight || " +
                "document.documentElement.scrollWidth > document.documentElement.clientWidth");
            Assert.False(hasScrollbar, "Dashboard should not have scrollbars at 1920x1080");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_FitsWithin1920x1080_NoScrollbars));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_PageTitle_IsExecutiveReportingDashboard()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var title = await page.TitleAsync();
            Assert.Contains("Executive Reporting Dashboard", title);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_PageTitle_IsExecutiveReportingDashboard));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_NoErrorPanel_WhenDataLoaded()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            Assert.False(await dashboard.IsErrorVisibleAsync(),
                "Error panel should not be visible when data is loaded");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_NoErrorPanel_WhenDataLoaded));
            throw;
        }
    }

    #endregion

    #region Header Integration

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_TitleAndLegend_BothVisible()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            Assert.True(await header.IsHeaderVisibleAsync());
            var titleText = await header.GetTitleTextAsync();
            Assert.False(string.IsNullOrWhiteSpace(titleText));

            // Legend items (inline-styled spans)
            var legendCount = await header.GetLegendItemCountAsync();
            Assert.Equal(4, legendCount);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_TitleAndLegend_BothVisible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_InlineLegend_HasCorrectLabels()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            Assert.True(await header.PocMilestoneLegend.CountAsync() > 0, "PoC Milestone label missing");
            Assert.True(await header.ProductionReleaseLegend.CountAsync() > 0, "Production Release label missing");
            Assert.True(await header.CheckpointLegend.CountAsync() > 0, "Checkpoint label missing");
            Assert.True(await header.NowLegend.CountAsync() > 0, "Now label missing");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_InlineLegend_HasCorrectLabels));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_InlineLegend_SymbolsHaveCorrectColors()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            // PoC diamond: #F4B400 with rotate(45deg)
            var pocSpan = header.PocMilestoneLegend.Locator("span").First;
            var pocBg = await pocSpan.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
            Assert.Contains("244", pocBg); // rgb(244, 180, 0) => F4B400

            // Production diamond: #34A853
            var prodSpan = header.ProductionReleaseLegend.Locator("span").First;
            var prodBg = await prodSpan.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
            Assert.Contains("52", prodBg); // rgb(52, 168, 83) => 34A853

            // Checkpoint circle: #999
            var checkSpan = header.CheckpointLegend.Locator("span").First;
            var checkBg = await checkSpan.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
            Assert.Contains("153", checkBg); // rgb(153, 153, 153) => #999

            // Now bar: #EA4335
            var nowSpan = header.NowLegend.Locator("span").First;
            var nowBg = await nowSpan.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
            Assert.Contains("234", nowBg); // rgb(234, 67, 53) => EA4335
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_InlineLegend_SymbolsHaveCorrectColors));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_InlineLegend_Gap22pxBetweenItems()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var gap = await header.LegendContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).gap");
            Assert.Equal("22px", gap);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_InlineLegend_Gap22pxBetweenItems));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_OpensInNewTab()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            if (await header.HasBacklogLinkAsync())
            {
                var target = await header.GetBacklogTargetAsync();
                Assert.Equal("_blank", target);

                var rel = await header.GetBacklogRelAsync();
                Assert.Contains("noopener", rel ?? "");
                Assert.Contains("noreferrer", rel ?? "");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_OpensInNewTab));
            throw;
        }
    }

    #endregion

    #region Timeline Integration

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_InlineSvg_IsRendered()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await tl.NavigateAsync();

            Assert.True(await tl.IsTimelineVisibleAsync(), "Timeline area should be visible");
            Assert.True(await tl.HasSvgAsync(), "SVG element should be present");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_InlineSvg_IsRendered));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_SvgWidth_Is1560()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await tl.NavigateAsync();

            var width = await tl.GetSvgWidthAsync();
            Assert.Equal("1560", width);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_SvgWidth_Is1560));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_HasTrackLabels_InSidebar()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await tl.NavigateAsync();

            var trackCount = await tl.GetTrackCountAsync();
            Assert.True(trackCount > 0, "Expected at least one track label in sidebar");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_HasTrackLabels_InSidebar));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_SidebarWidth_Is230px()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await tl.NavigateAsync();

            var width = await tl.SidebarContainer.EvaluateAsync<double>(
                "el => el.getBoundingClientRect().width");
            Assert.Equal(230, width, 2);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_SidebarWidth_Is230px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_Sidebar_HasBorderRight()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await tl.NavigateAsync();

            var borderStyle = await tl.SidebarContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderRightStyle");
            Assert.Equal("solid", borderStyle);

            var borderColor = await tl.SidebarContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderRightColor");
            Assert.Contains("224", borderColor); // #E0E0E0 = rgb(224,224,224)
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_Sidebar_HasBorderRight));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_HasPocDiamonds()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await tl.NavigateAsync();

            var count = await tl.GetPocDiamondCountAsync();
            Assert.True(count > 0, "Expected at least one PoC diamond (#F4B400)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_HasPocDiamonds));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_HasProductionDiamonds()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await tl.NavigateAsync();

            var count = await tl.GetProductionDiamondCountAsync();
            Assert.True(count > 0, "Expected at least one Production diamond (#34A853)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_HasProductionDiamonds));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_HasCheckpointCircles()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await tl.NavigateAsync();

            var count = await tl.GetCheckpointCircleCountAsync();
            Assert.True(count > 0, "Expected at least one checkpoint circle");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_HasCheckpointCircles));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_NowMarker_IsPresent()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await tl.NavigateAsync();

            Assert.True(await tl.HasNowMarkerAsync(), "Expected NOW text marker in SVG");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_NowMarker_IsPresent));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_NowLine_IsDashed_Red()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await tl.NavigateAsync();

            var nowLineCount = await tl.NowLine.CountAsync();
            Assert.True(nowLineCount > 0, "Expected red NOW line");

            var dasharray = await tl.NowLine.First.GetAttributeAsync("stroke-dasharray");
            Assert.Equal("5,3", dasharray);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_NowLine_IsDashed_Red));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_DropShadowFilter_Defined()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await tl.NavigateAsync();

            var filterCount = await tl.DropShadowFilter.CountAsync();
            Assert.True(filterCount > 0, "Expected drop shadow filter with id='sh'");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_DropShadowFilter_Defined));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_MonthLabels_PresentInSvg()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await tl.NavigateAsync();

            var textCount = await tl.SvgTexts.CountAsync();
            Assert.True(textCount > 0, "Expected month label text elements in SVG");

            // At least Jan, Feb, Mar should appear
            var svgContent = await tl.Svg.InnerHTMLAsync();
            Assert.Contains("Jan", svgContent);
            Assert.Contains("Feb", svgContent);
            Assert.Contains("Mar", svgContent);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_MonthLabels_PresentInSvg));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_TrackLines_HaveDistinctColors()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await tl.NavigateAsync();

            // Get all horizontal track lines (stroke-width=3)
            var trackLines = _page_Locator(page, ".tl-svg-box svg line[stroke-width='3']");
            var count = await trackLines.CountAsync();
            if (count > 1)
            {
                var colors = new HashSet<string>();
                for (int i = 0; i < count; i++)
                {
                    var stroke = await trackLines.Nth(i).GetAttributeAsync("stroke");
                    if (stroke != null) colors.Add(stroke);
                }
                Assert.True(colors.Count > 1, "Expected track lines to have distinct colors");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_TrackLines_HaveDistinctColors));
            throw;
        }
    }

    private static ILocator _page_Locator(IPage page, string selector) => page.Locator(selector);

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_AreaHeight_Is196px()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await tl.NavigateAsync();

            var height = await tl.TimelineArea.EvaluateAsync<double>(
                "el => el.getBoundingClientRect().height");
            Assert.Equal(196, height, 2);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_AreaHeight_Is196px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_Background_IsFAFAFA()
    {
        var page = await _fixture.NewPageAsync();
        var tl = new TimelinePageObject(page, _fixture.BaseUrl);

        try
        {
            await tl.NavigateAsync();

            var bg = await tl.TimelineArea.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            Assert.Contains("250", bg); // #FAFAFA = rgb(250,250,250)
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_Background_IsFAFAFA));
            throw;
        }
    }

    #endregion

    #region Heatmap Integration

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_IsVisible_WithTitle()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            Assert.True(await hm.IsHeatmapVisibleAsync(), "Heatmap should be visible");

            var titleText = await hm.HeatmapTitle.TextContentAsync() ?? "";
            Assert.Contains("MONTHLY EXECUTION HEATMAP", titleText.ToUpperInvariant());
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_IsVisible_WithTitle));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_HasStatusCornerCell()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            await Assertions.Expect(hm.CornerCell).ToBeVisibleAsync();
            var text = await hm.CornerCell.TextContentAsync();
            Assert.Equal("STATUS", text?.Trim());
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_HasStatusCornerCell));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_HasMonthColumnHeaders()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            var colCount = await hm.GetMonthColumnCountAsync();
            Assert.True(colCount >= 1, "Expected at least one month column header");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_HasMonthColumnHeaders));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_CurrentMonth_HighlightedWithAprHdr()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            Assert.True(await hm.HasCurrentMonthHighlightAsync(),
                "Expected current month header to have apr-hdr class");

            var bg = await hm.CurrentMonthHeader.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // #FFF0D0 = rgb(255, 240, 208)
            Assert.Contains("255", bg);
            Assert.Contains("240", bg);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_CurrentMonth_HighlightedWithAprHdr));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_FourCategoryRows_InCorrectOrder()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            var rowCount = await hm.GetRowCountAsync();
            Assert.Equal(4, rowCount);

            Assert.Equal(1, await hm.ShippedHeader.CountAsync());
            Assert.Equal(1, await hm.InProgressHeader.CountAsync());
            Assert.Equal(1, await hm.CarryoverHeader.CountAsync());
            Assert.Equal(1, await hm.BlockersHeader.CountAsync());

            // Verify vertical order
            var shipBox = await hm.ShippedHeader.BoundingBoxAsync();
            var progBox = await hm.InProgressHeader.BoundingBoxAsync();
            var carryBox = await hm.CarryoverHeader.BoundingBoxAsync();
            var blockBox = await hm.BlockersHeader.BoundingBoxAsync();

            Assert.NotNull(shipBox);
            Assert.NotNull(progBox);
            Assert.NotNull(carryBox);
            Assert.NotNull(blockBox);

            Assert.True(shipBox!.Y < progBox!.Y, "Shipped above In Progress");
            Assert.True(progBox.Y < carryBox!.Y, "In Progress above Carryover");
            Assert.True(carryBox.Y < blockBox!.Y, "Carryover above Blockers");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_FourCategoryRows_InCorrectOrder));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_DataCells_ContainItems()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            var itemCount = await hm.GetTotalItemCountAsync();
            Assert.True(itemCount > 0, "Expected at least one work item in the heatmap");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_DataCells_ContainItems));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_EmptyCells_ShowDash()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            var emptyCount = await hm.EmptyCells.CountAsync();
            Assert.True(emptyCount > 0, "Expected at least one empty cell with dash");

            var firstEmpty = hm.EmptyCells.First;
            var text = await firstEmpty.TextContentAsync();
            Assert.Equal("-", text?.Trim());
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_EmptyCells_ShowDash));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_CurrentMonthCells_HaveAprClass()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            var aprCellCount = await hm.GetCurrentMonthCellCountAsync();
            // 4 categories × 1 current month = 4 cells
            Assert.Equal(4, aprCellCount);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_CurrentMonthCells_HaveAprClass));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_GridTemplate_MatchesMonthCount()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            var colCount = await hm.GetMonthColumnCountAsync();
            var style = await hm.HeatmapGrid.GetAttributeAsync("style") ?? "";
            Assert.Contains($"repeat({colCount}, 1fr)", style);
            Assert.Contains("160px", style);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_GridTemplate_MatchesMonthCount));
            throw;
        }
    }

    #endregion

    #region Heatmap Category Colors

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_ShippedRow_HasGreenTheme()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            // ship-hdr background: #E8F5E9 = rgb(232,245,233)
            var bg = await hm.ShippedHeader.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            Assert.Contains("232", bg);
            Assert.Contains("245", bg);

            // ship-hdr text color: #1B7A28 = rgb(27,122,40)
            var color = await hm.ShippedHeader.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");
            Assert.Contains("27", color);
            Assert.Contains("122", color);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_ShippedRow_HasGreenTheme));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_InProgressRow_HasBlueTheme()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            // prog-hdr background: #E3F2FD = rgb(227,242,253)
            var bg = await hm.InProgressHeader.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            Assert.Contains("227", bg);
            Assert.Contains("242", bg);

            // prog-hdr text color: #1565C0 = rgb(21,101,192)
            var color = await hm.InProgressHeader.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");
            Assert.Contains("21", color);
            Assert.Contains("101", color);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_InProgressRow_HasBlueTheme));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_CarryoverRow_HasAmberTheme()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            // carry-hdr background: #FFF8E1 = rgb(255,248,225)
            var bg = await hm.CarryoverHeader.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            Assert.Contains("255", bg);
            Assert.Contains("248", bg);

            // carry-hdr text color: #B45309 = rgb(180,83,9)
            var color = await hm.CarryoverHeader.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");
            Assert.Contains("180", color);
            Assert.Contains("83", color);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_CarryoverRow_HasAmberTheme));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_BlockersRow_HasRedTheme()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            // block-hdr background: #FEF2F2 = rgb(254,242,242)
            var bg = await hm.BlockersHeader.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            Assert.Contains("254", bg);
            Assert.Contains("242", bg);

            // block-hdr text color: #991B1B = rgb(153,27,27)
            var color = await hm.BlockersHeader.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");
            Assert.Contains("153", color);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_BlockersRow_HasRedTheme));
            throw;
        }
    }

    #endregion

    #region Heatmap Item Rendering

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_Items_Have12pxFontSize()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            var itemCount = await hm.ItemDivs.CountAsync();
            if (itemCount > 0)
            {
                var fontSize = await hm.ItemDivs.First.EvaluateAsync<string>(
                    "el => getComputedStyle(el).fontSize");
                Assert.Equal("12px", fontSize);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_Items_Have12pxFontSize));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_Items_HaveColoredDotPseudoElement()
    {
        var page = await _fixture.NewPageAsync();
        var hm = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await hm.NavigateAsync();

            // Check shipped items have the green dot via ::before pseudo-element
            var shippedCellCount = await hm.ShippedCells.CountAsync();
            if (shippedCellCount > 0)
            {
                var hasDot = await page.EvaluateAsync<bool>(
                    "() => { const el = document.querySelector('.ship-cell .it'); " +
                    "if (!el) return false; " +
                    "const before = getComputedStyle(el, '::before'); " +
                    "return before.content !== 'none' && before.width === '6px'; }");
                Assert.True(hasDot, "Shipped items should have 6px colored dot via ::before");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_Items_HaveColoredDotPseudoElement));
            throw;
        }
    }

    #endregion
}