using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeatmapUITests
{
    private readonly PlaywrightFixture _fixture;

    public HeatmapUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<(IPage page, HeatmapPage heatmap)> SetupAsync()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);
        await heatmap.NavigateAsync();
        return (page, heatmap);
    }

    #region Section Title Tests

    [Fact]
    public async Task HeatmapTitle_IsVisible_OnDashboard()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            await Assertions.Expect(heatmap.HeatmapTitle).ToBeVisibleAsync();
        }
        catch
        {
            await heatmap.ScreenshotAsync("HeatmapTitle_IsVisible_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapTitle_ContainsExpectedText()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var titleText = await heatmap.GetTitleTextAsync();
            titleText.Should().ContainAll("MONTHLY EXECUTION HEATMAP", "Shipped", "In Progress", "Carryover", "Blockers");
        }
        catch
        {
            await heatmap.ScreenshotAsync("HeatmapTitle_ContainsText_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapTitle_HasCorrectCssClass()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            await Assertions.Expect(heatmap.HeatmapTitle).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("hm-title"));
        }
        catch
        {
            await heatmap.ScreenshotAsync("HeatmapTitle_CssClass_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapTitle_HasUppercaseTextTransform()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var textTransform = await heatmap.HeatmapTitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).textTransform");
            textTransform.Should().Be("uppercase");
        }
        catch
        {
            await heatmap.ScreenshotAsync("HeatmapTitle_Uppercase_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapTitle_HasCorrectFontWeight()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var fontWeight = await heatmap.HeatmapTitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontWeight");
            // 700 = bold
            fontWeight.Should().BeOneOf("700", "bold");
        }
        catch
        {
            await heatmap.ScreenshotAsync("HeatmapTitle_FontWeight_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapTitle_HasCorrectColor()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var color = await heatmap.HeatmapTitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");
            // #888 = rgb(136, 136, 136)
            color.Should().Contain("136");
        }
        catch
        {
            await heatmap.ScreenshotAsync("HeatmapTitle_Color_Failure");
            throw;
        }
    }

    #endregion

    #region Grid Container Tests

    [Fact]
    public async Task HeatmapGrid_IsVisible()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            await Assertions.Expect(heatmap.HeatmapGrid).ToBeVisibleAsync();
        }
        catch
        {
            await heatmap.ScreenshotAsync("HeatmapGrid_IsVisible_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapGrid_HasCorrectInlineStyleColumns()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var style = await heatmap.GetGridStyleAsync();
            style.Should().NotBeNull();
            style.Should().Contain("grid-template-columns");
            style.Should().Contain("160px");
            style.Should().MatchRegex(@"repeat\(\d+,\s*1fr\)");
        }
        catch
        {
            await heatmap.ScreenshotAsync("HeatmapGrid_InlineStyleColumns_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapGrid_HasCorrectInlineStyleRows()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var style = await heatmap.GetGridStyleAsync();
            style.Should().NotBeNull();
            style.Should().Contain("grid-template-rows");
            style.Should().Contain("36px");
            style.Should().Contain("repeat(4, 1fr)");
        }
        catch
        {
            await heatmap.ScreenshotAsync("HeatmapGrid_InlineStyleRows_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapGrid_HasBorder()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var border = await heatmap.HeatmapGrid.EvaluateAsync<string>(
                "el => getComputedStyle(el).border");
            // Should have a 1px solid border
            border.Should().Contain("1px");
        }
        catch
        {
            await heatmap.ScreenshotAsync("HeatmapGrid_Border_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapGrid_UsesGridDisplay()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var display = await heatmap.HeatmapGrid.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            display.Should().Be("grid");
        }
        catch
        {
            await heatmap.ScreenshotAsync("HeatmapGrid_GridDisplay_Failure");
            throw;
        }
    }

    #endregion

    #region Corner Cell Tests

    [Fact]
    public async Task CornerCell_IsVisible()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            await Assertions.Expect(heatmap.CornerCell).ToBeVisibleAsync();
        }
        catch
        {
            await heatmap.ScreenshotAsync("CornerCell_IsVisible_Failure");
            throw;
        }
    }

    [Fact]
    public async Task CornerCell_DisplaysStatusText()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            await Assertions.Expect(heatmap.CornerCell).ToContainTextAsync("STATUS");
        }
        catch
        {
            await heatmap.ScreenshotAsync("CornerCell_StatusText_Failure");
            throw;
        }
    }

    [Fact]
    public async Task CornerCell_HasCorrectBackground()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var bg = await heatmap.CornerCell.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // #F5F5F5 = rgb(245, 245, 245)
            bg.Should().Contain("245");
        }
        catch
        {
            await heatmap.ScreenshotAsync("CornerCell_Background_Failure");
            throw;
        }
    }

    [Fact]
    public async Task CornerCell_HasBoldFontWeight()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var fontWeight = await heatmap.CornerCell.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontWeight");
            fontWeight.Should().BeOneOf("700", "bold");
        }
        catch
        {
            await heatmap.ScreenshotAsync("CornerCell_FontWeight_Failure");
            throw;
        }
    }

    [Fact]
    public async Task CornerCell_IsFirstChildOfGrid()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var firstChildClass = await heatmap.HeatmapGrid.EvaluateAsync<string>(
                "el => el.firstElementChild?.className ?? ''");
            firstChildClass.Should().Contain("hm-corner");
        }
        catch
        {
            await heatmap.ScreenshotAsync("CornerCell_FirstChild_Failure");
            throw;
        }
    }

    #endregion

    #region Column Header Tests

    [Fact]
    public async Task ColumnHeaders_AreVisible()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var headerCount = await heatmap.ColumnHeaders.CountAsync();
            headerCount.Should().BeGreaterThan(0, "At least one column header should render");

            for (int i = 0; i < headerCount; i++)
            {
                await Assertions.Expect(heatmap.GetColumnHeader(i)).ToBeVisibleAsync();
            }
        }
        catch
        {
            await heatmap.ScreenshotAsync("ColumnHeaders_AreVisible_Failure");
            throw;
        }
    }

    [Fact]
    public async Task ColumnHeaders_DisplayMonthNames()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var headerCount = await heatmap.ColumnHeaders.CountAsync();
            headerCount.Should().BeGreaterThanOrEqualTo(1);

            for (int i = 0; i < headerCount; i++)
            {
                var text = await heatmap.GetColumnHeader(i).InnerTextAsync();
                text.Should().NotBeNullOrWhiteSpace($"Column header {i} should have month text");
            }
        }
        catch
        {
            await heatmap.ScreenshotAsync("ColumnHeaders_MonthNames_Failure");
            throw;
        }
    }

    [Fact]
    public async Task ColumnHeaders_HaveCorrectBaseBackground()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var headerCount = await heatmap.ColumnHeaders.CountAsync();
            headerCount.Should().BeGreaterThan(0);

            // Check first non-current header for base background
            for (int i = 0; i < headerCount; i++)
            {
                var header = heatmap.GetColumnHeader(i);
                var className = await header.GetAttributeAsync("class") ?? "";
                if (!className.Contains("cur-month-hdr"))
                {
                    var bg = await header.EvaluateAsync<string>(
                        "el => getComputedStyle(el).backgroundColor");
                    // #F5F5F5 = rgb(245, 245, 245)
                    bg.Should().Contain("245",
                        $"Non-current header at index {i} should have #F5F5F5 background");
                    break;
                }
            }
        }
        catch
        {
            await heatmap.ScreenshotAsync("ColumnHeaders_BaseBackground_Failure");
            throw;
        }
    }

    [Fact]
    public async Task ColumnHeaders_AreBoldText()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var headerCount = await heatmap.ColumnHeaders.CountAsync();
            headerCount.Should().BeGreaterThan(0);

            var fontWeight = await heatmap.GetColumnHeader(0).EvaluateAsync<string>(
                "el => getComputedStyle(el).fontWeight");
            fontWeight.Should().BeOneOf("700", "bold");
        }
        catch
        {
            await heatmap.ScreenshotAsync("ColumnHeaders_BoldText_Failure");
            throw;
        }
    }

    #endregion

    #region Current Month Highlighting Tests

    [Fact]
    public async Task CurrentMonthHeader_HasHighlightClass()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var currentCount = await heatmap.CurrentMonthHeader.CountAsync();
            currentCount.Should().BeGreaterThanOrEqualTo(1,
                "At least one column header should have the current-month highlight class");
        }
        catch
        {
            await heatmap.ScreenshotAsync("CurrentMonthHeader_HighlightClass_Failure");
            throw;
        }
    }

    [Fact]
    public async Task CurrentMonthHeader_ContainsNowIndicator()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var currentCount = await heatmap.CurrentMonthHeader.CountAsync();
            if (currentCount > 0)
            {
                var text = await heatmap.CurrentMonthHeader.First.InnerTextAsync();
                text.Should().Contain("Now",
                    "Current month header should display 'Now' indicator");
            }
        }
        catch
        {
            await heatmap.ScreenshotAsync("CurrentMonthHeader_NowIndicator_Failure");
            throw;
        }
    }

    [Fact]
    public async Task CurrentMonthHeader_OnlyOneIsHighlighted()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var currentCount = await heatmap.CurrentMonthHeader.CountAsync();
            currentCount.Should().BeLessThanOrEqualTo(1,
                "At most one column header should be highlighted as current month");
        }
        catch
        {
            await heatmap.ScreenshotAsync("CurrentMonthHeader_OnlyOne_Failure");
            throw;
        }
    }

    #endregion

    #region Heatmap Row Rendering Tests

    [Fact]
    public async Task HeatmapGrid_ContainsShippedRow()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            // Look for shipped-related content in the grid
            var gridHtml = await heatmap.HeatmapGrid.InnerHTMLAsync();
            gridHtml.Should().Contain("shipped",
                "Grid should contain shipped row category");
        }
        catch
        {
            await heatmap.ScreenshotAsync("Grid_ShippedRow_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapGrid_ContainsInProgressRow()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var gridHtml = await heatmap.HeatmapGrid.InnerHTMLAsync();
            gridHtml.Should().Contain("prog",
                "Grid should contain in-progress row category");
        }
        catch
        {
            await heatmap.ScreenshotAsync("Grid_InProgressRow_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapGrid_ContainsCarryoverRow()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var gridHtml = await heatmap.HeatmapGrid.InnerHTMLAsync();
            gridHtml.Should().Contain("carry",
                "Grid should contain carryover row category");
        }
        catch
        {
            await heatmap.ScreenshotAsync("Grid_CarryoverRow_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapGrid_ContainsBlockersRow()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var gridHtml = await heatmap.HeatmapGrid.InnerHTMLAsync();
            gridHtml.Should().Contain("block",
                "Grid should contain blockers row category");
        }
        catch
        {
            await heatmap.ScreenshotAsync("Grid_BlockersRow_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapRows_AppearInCorrectOrder()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var gridHtml = await heatmap.HeatmapGrid.InnerHTMLAsync();

            var shippedIdx = gridHtml.IndexOf("shipped", StringComparison.OrdinalIgnoreCase);
            var progIdx = gridHtml.IndexOf("prog", StringComparison.OrdinalIgnoreCase);
            var carryIdx = gridHtml.IndexOf("carry", StringComparison.OrdinalIgnoreCase);
            var blockIdx = gridHtml.IndexOf("block", StringComparison.OrdinalIgnoreCase);

            shippedIdx.Should().BeGreaterThan(-1, "Shipped should appear in grid");
            progIdx.Should().BeGreaterThan(-1, "In Progress should appear in grid");
            carryIdx.Should().BeGreaterThan(-1, "Carryover should appear in grid");
            blockIdx.Should().BeGreaterThan(-1, "Blockers should appear in grid");

            shippedIdx.Should().BeLessThan(progIdx, "Shipped before In Progress");
            progIdx.Should().BeLessThan(carryIdx, "In Progress before Carryover");
            carryIdx.Should().BeLessThan(blockIdx, "Carryover before Blockers");
        }
        catch
        {
            await heatmap.ScreenshotAsync("HeatmapRows_Order_Failure");
            throw;
        }
    }

    #endregion

    #region Layout and Visual Fidelity Tests

    [Fact]
    public async Task HeatmapSection_FillsVerticalSpace()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var hmWrapCount = await heatmap.HeatmapWrap.CountAsync();
            if (hmWrapCount > 0)
            {
                var flex = await heatmap.HeatmapWrap.EvaluateAsync<string>(
                    "el => getComputedStyle(el).flex");
                // Should have flex: 1 or similar
                flex.Should().Contain("1", "hm-wrap should flex to fill available space");
            }
        }
        catch
        {
            await heatmap.ScreenshotAsync("HeatmapSection_FillsSpace_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapGrid_FillsRemainingSpace()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var flex = await heatmap.HeatmapGrid.EvaluateAsync<string>(
                "el => getComputedStyle(el).flex");
            flex.Should().Contain("1", "hm-grid should use flex: 1");
        }
        catch
        {
            await heatmap.ScreenshotAsync("HeatmapGrid_FillsSpace_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapTitle_AppearsAboveGrid()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var titleBox = await heatmap.HeatmapTitle.BoundingBoxAsync();
            var gridBox = await heatmap.HeatmapGrid.BoundingBoxAsync();

            titleBox.Should().NotBeNull("Title should be visible");
            gridBox.Should().NotBeNull("Grid should be visible");

            titleBox!.Y.Should().BeLessThan(gridBox!.Y,
                "Title should be positioned above the grid");
        }
        catch
        {
            await heatmap.ScreenshotAsync("HeatmapTitle_AboveGrid_Failure");
            throw;
        }
    }

    [Fact]
    public async Task CornerCell_IsTopLeftOfGrid()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var cornerBox = await heatmap.CornerCell.BoundingBoxAsync();
            var gridBox = await heatmap.HeatmapGrid.BoundingBoxAsync();

            cornerBox.Should().NotBeNull();
            gridBox.Should().NotBeNull();

            // Corner cell should be at or near top-left of grid
            cornerBox!.X.Should().BeApproximately(gridBox!.X, 5,
                "Corner cell should be at grid's left edge");
            cornerBox.Y.Should().BeApproximately(gridBox.Y, 5,
                "Corner cell should be at grid's top edge");
        }
        catch
        {
            await heatmap.ScreenshotAsync("CornerCell_TopLeft_Failure");
            throw;
        }
    }

    [Fact]
    public async Task ColumnHeaders_AreHorizontallyAligned()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var headerCount = await heatmap.ColumnHeaders.CountAsync();
            if (headerCount < 2) return;

            var firstBox = await heatmap.GetColumnHeader(0).BoundingBoxAsync();
            var secondBox = await heatmap.GetColumnHeader(1).BoundingBoxAsync();

            firstBox.Should().NotBeNull();
            secondBox.Should().NotBeNull();

            // Headers should share the same Y position (same row)
            firstBox!.Y.Should().BeApproximately(secondBox!.Y, 2,
                "Column headers should be on the same row");

            // Second header should be to the right of the first
            secondBox.X.Should().BeGreaterThan(firstBox.X,
                "Headers should be ordered left to right");
        }
        catch
        {
            await heatmap.ScreenshotAsync("ColumnHeaders_Aligned_Failure");
            throw;
        }
    }

    #endregion

    #region Full Page Context Tests

    [Fact]
    public async Task Dashboard_LoadsWithoutError_WhenDataValid()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var isError = await heatmap.IsErrorStateAsync();
            // If error state, the heatmap won't render; this test checks happy path
            if (!isError)
            {
                await Assertions.Expect(heatmap.HeatmapTitle).ToBeVisibleAsync();
                await Assertions.Expect(heatmap.HeatmapGrid).ToBeVisibleAsync();
            }
        }
        catch
        {
            await heatmap.ScreenshotAsync("Dashboard_LoadsWithoutError_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapSection_RendersAfterHeaderAndTimeline()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            // The heatmap should be below the header
            var headerLocator = page.Locator(".hdr");
            var headerCount = await headerLocator.CountAsync();
            if (headerCount > 0)
            {
                var headerBox = await headerLocator.BoundingBoxAsync();
                var heatmapBox = await heatmap.HeatmapTitle.BoundingBoxAsync();

                if (headerBox != null && heatmapBox != null)
                {
                    heatmapBox.Y.Should().BeGreaterThan(headerBox.Y + headerBox.Height - 5,
                        "Heatmap should appear below the header section");
                }
            }
        }
        catch
        {
            await heatmap.ScreenshotAsync("HeatmapSection_BelowHeader_Failure");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_At1920x1080_HeatmapFitsWithoutScroll()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var gridBox = await heatmap.HeatmapGrid.BoundingBoxAsync();
            if (gridBox != null)
            {
                // Bottom of grid should be within 1080px viewport
                var gridBottom = gridBox.Y + gridBox.Height;
                gridBottom.Should().BeLessThanOrEqualTo(1085,
                    "Heatmap grid bottom should fit within 1080px viewport (5px tolerance)");
            }
        }
        catch
        {
            await heatmap.ScreenshotAsync("Dashboard_FitsViewport_Failure");
            throw;
        }
    }

    #endregion

    #region Grid Column Count Matches Data

    [Fact]
    public async Task GridColumns_MatchMonthHeaderCount()
    {
        var (page, heatmap) = await SetupAsync();
        try
        {
            var headerCount = await heatmap.ColumnHeaders.CountAsync();
            var style = await heatmap.GetGridStyleAsync() ?? "";

            // Extract repeat count from style
            var match = System.Text.RegularExpressions.Regex.Match(style, @"repeat\((\d+)");
            if (match.Success)
            {
                var repeatCount = int.Parse(match.Groups[1].Value);
                repeatCount.Should().Be(headerCount,
                    "Grid column repeat count should match number of month headers");
            }
        }
        catch
        {
            await heatmap.ScreenshotAsync("GridColumns_MatchHeaders_Failure");
            throw;
        }
    }

    #endregion

    #region Screenshot for Visual Comparison

    [Fact]
    public async Task Heatmap_CaptureBaselineScreenshot()
    {
        var (page, heatmap) = await SetupAsync();

        // Always capture a baseline screenshot for visual comparison
        await heatmap.ScreenshotAsync("heatmap_baseline");

        // Basic sanity: grid rendered
        var gridCount = await heatmap.HeatmapGrid.CountAsync();
        gridCount.Should().BeGreaterThan(0, "Heatmap grid should be rendered for screenshot");
    }

    #endregion
}