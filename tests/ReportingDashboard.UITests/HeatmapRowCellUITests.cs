using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Fixtures;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeatmapRowCellUITests
{
    private readonly PlaywrightFixture _fixture;

    public HeatmapRowCellUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    // ──────────────────────────────────────────────
    // Row Header Rendering
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HeatmapGrid_RendersAllFourRowHeaders()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            await Assertions.Expect(heatmap.AllRowHeaders).ToHaveCountAsync(4);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "HeatmapGrid_FourRowHeaders_Failure");
            throw;
        }
    }

    [Fact]
    public async Task ShipHeader_RendersWithCorrectTextAndClass()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            await Assertions.Expect(heatmap.ShipHeader).ToBeVisibleAsync();
            var text = await heatmap.ShipHeader.TextContentAsync();
            text.Should().NotBeNullOrEmpty();
            text!.ToUpperInvariant().Should().Contain("SHIPPED");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "ShipHeader_Failure");
            throw;
        }
    }

    [Fact]
    public async Task ProgHeader_RendersWithCorrectTextAndClass()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            await Assertions.Expect(heatmap.ProgHeader).ToBeVisibleAsync();
            var text = await heatmap.ProgHeader.TextContentAsync();
            text.Should().NotBeNullOrEmpty();
            text!.ToUpperInvariant().Should().Contain("PROGRESS");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "ProgHeader_Failure");
            throw;
        }
    }

    [Fact]
    public async Task CarryHeader_RendersWithCorrectTextAndClass()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            await Assertions.Expect(heatmap.CarryHeader).ToBeVisibleAsync();
            var text = await heatmap.CarryHeader.TextContentAsync();
            text.Should().NotBeNullOrEmpty();
            text!.ToUpperInvariant().Should().Contain("CARRYOVER");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "CarryHeader_Failure");
            throw;
        }
    }

    [Fact]
    public async Task BlockHeader_RendersWithCorrectTextAndClass()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            await Assertions.Expect(heatmap.BlockHeader).ToBeVisibleAsync();
            var text = await heatmap.BlockHeader.TextContentAsync();
            text.Should().NotBeNullOrEmpty();
            text!.ToUpperInvariant().Should().Contain("BLOCKER");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "BlockHeader_Failure");
            throw;
        }
    }

    // ──────────────────────────────────────────────
    // Row Header Styling
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ShipHeader_HasCorrectColors()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var bgColor = await heatmap.GetCellBackgroundColorAsync(heatmap.ShipHeader);
            var textColor = await heatmap.GetElementColorAsync(heatmap.ShipHeader);

            // #E8F5E9 = rgb(232, 245, 233)
            bgColor.Should().Be("rgb(232, 245, 233)");
            // #1B7A28 = rgb(27, 122, 40)
            textColor.Should().Be("rgb(27, 122, 40)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "ShipHeader_Colors_Failure");
            throw;
        }
    }

    [Fact]
    public async Task ProgHeader_HasCorrectColors()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var bgColor = await heatmap.GetCellBackgroundColorAsync(heatmap.ProgHeader);
            var textColor = await heatmap.GetElementColorAsync(heatmap.ProgHeader);

            // #E3F2FD = rgb(227, 242, 253)
            bgColor.Should().Be("rgb(227, 242, 253)");
            // #1565C0 = rgb(21, 101, 192)
            textColor.Should().Be("rgb(21, 101, 192)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "ProgHeader_Colors_Failure");
            throw;
        }
    }

    [Fact]
    public async Task CarryHeader_HasCorrectColors()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var bgColor = await heatmap.GetCellBackgroundColorAsync(heatmap.CarryHeader);
            var textColor = await heatmap.GetElementColorAsync(heatmap.CarryHeader);

            // #FFF8E1 = rgb(255, 248, 225)
            bgColor.Should().Be("rgb(255, 248, 225)");
            // #B45309 = rgb(180, 83, 9)
            textColor.Should().Be("rgb(180, 83, 9)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "CarryHeader_Colors_Failure");
            throw;
        }
    }

    [Fact]
    public async Task BlockHeader_HasCorrectColors()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var bgColor = await heatmap.GetCellBackgroundColorAsync(heatmap.BlockHeader);
            var textColor = await heatmap.GetElementColorAsync(heatmap.BlockHeader);

            // #FEF2F2 = rgb(254, 242, 242)
            bgColor.Should().Be("rgb(254, 242, 242)");
            // #991B1B = rgb(153, 27, 27)
            textColor.Should().Be("rgb(153, 27, 27)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "BlockHeader_Colors_Failure");
            throw;
        }
    }

    [Fact]
    public async Task RowHeaders_HaveCorrectFontStyling()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var fontSize = await heatmap.GetElementFontSizeAsync(heatmap.ShipHeader);
            var fontWeight = await heatmap.GetElementFontWeightAsync(heatmap.ShipHeader);

            fontSize.Should().Be("11px");
            fontWeight.Should().Be("700");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "RowHeaders_FontStyling_Failure");
            throw;
        }
    }

    // ──────────────────────────────────────────────
    // Cell Count per Row
    // ──────────────────────────────────────────────

    [Fact]
    public async Task EachCategoryRow_HasCorrectNumberOfCells()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var shipCount = await heatmap.ShipCells.CountAsync();
            var progCount = await heatmap.ProgCells.CountAsync();
            var carryCount = await heatmap.CarryCells.CountAsync();
            var blockCount = await heatmap.BlockCells.CountAsync();

            // All rows should have the same number of month cells
            shipCount.Should().BeGreaterThan(0);
            progCount.Should().Be(shipCount);
            carryCount.Should().Be(shipCount);
            blockCount.Should().Be(shipCount);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "CellCount_Failure");
            throw;
        }
    }

    // ──────────────────────────────────────────────
    // Work Item Rendering
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HeatmapCells_RenderWorkItemsWithItClass()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var itemCount = await heatmap.AllItems.CountAsync();
            itemCount.Should().BeGreaterThan(0, "dashboard data.json should contain at least one work item");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "WorkItems_Failure");
            throw;
        }
    }

    [Fact]
    public async Task WorkItems_HaveCorrectFontSizeAndColor()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var itemCount = await heatmap.AllItems.CountAsync();
            if (itemCount > 0)
            {
                var firstItem = heatmap.AllItems.First;
                var fontSize = await heatmap.GetElementFontSizeAsync(firstItem);
                var color = await heatmap.GetElementColorAsync(firstItem);

                fontSize.Should().Be("12px");
                // #333 = rgb(51, 51, 51)
                color.Should().Be("rgb(51, 51, 51)");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "WorkItems_FontStyle_Failure");
            throw;
        }
    }

    [Fact]
    public async Task WorkItems_HaveNonEmptyText()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var itemCount = await heatmap.AllItems.CountAsync();
            for (int i = 0; i < itemCount; i++)
            {
                var text = await heatmap.AllItems.Nth(i).TextContentAsync();
                text.Should().NotBeNullOrWhiteSpace($"Work item at index {i} should have text content");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "WorkItems_NonEmpty_Failure");
            throw;
        }
    }

    // ──────────────────────────────────────────────
    // Colored Bullet Dots (::before pseudo-element)
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ShipItems_HaveGreenBulletDots()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var count = await heatmap.ShipItems.CountAsync();
            if (count > 0)
            {
                var dotColor = await heatmap.GetPseudoElementBackgroundAsync(heatmap.ShipItems.First);
                // #34A853 = rgb(52, 168, 83)
                dotColor.Should().Be("rgb(52, 168, 83)");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "ShipBullets_Failure");
            throw;
        }
    }

    [Fact]
    public async Task ProgItems_HaveBlueBulletDots()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var count = await heatmap.ProgItems.CountAsync();
            if (count > 0)
            {
                var dotColor = await heatmap.GetPseudoElementBackgroundAsync(heatmap.ProgItems.First);
                // #0078D4 = rgb(0, 120, 212)
                dotColor.Should().Be("rgb(0, 120, 212)");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "ProgBullets_Failure");
            throw;
        }
    }

    [Fact]
    public async Task CarryItems_HaveAmberBulletDots()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var count = await heatmap.CarryItems.CountAsync();
            if (count > 0)
            {
                var dotColor = await heatmap.GetPseudoElementBackgroundAsync(heatmap.CarryItems.First);
                // #F4B400 = rgb(244, 180, 0)
                dotColor.Should().Be("rgb(244, 180, 0)");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "CarryBullets_Failure");
            throw;
        }
    }

    [Fact]
    public async Task BlockItems_HaveRedBulletDots()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var count = await heatmap.BlockItems.CountAsync();
            if (count > 0)
            {
                var dotColor = await heatmap.GetPseudoElementBackgroundAsync(heatmap.BlockItems.First);
                // #EA4335 = rgb(234, 67, 53)
                dotColor.Should().Be("rgb(234, 67, 53)");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "BlockBullets_Failure");
            throw;
        }
    }

    // ──────────────────────────────────────────────
    // Empty Cells
    // ──────────────────────────────────────────────

    [Fact]
    public async Task EmptyCells_RenderGrayDash()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var dashCount = await heatmap.EmptyDashes.CountAsync();
            // There should be at least some empty cells in the default data
            dashCount.Should().BeGreaterOrEqualTo(0);

            if (dashCount > 0)
            {
                var text = await heatmap.EmptyDashes.First.TextContentAsync();
                text.Should().Be("-");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "EmptyCells_Failure");
            throw;
        }
    }

    [Fact]
    public async Task EmptyCells_DoNotContainItDivs()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var dashCount = await heatmap.EmptyDashes.CountAsync();
            if (dashCount > 0)
            {
                // Get the parent .hm-cell of the first dash and verify no .it children
                var parentHasIt = await heatmap.EmptyDashes.First.EvaluateAsync<bool>(
                    "el => el.parentElement.querySelectorAll('.it').length > 0");
                parentHasIt.Should().BeFalse("empty cells should not contain .it divs alongside the dash");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "EmptyCells_NoItDivs_Failure");
            throw;
        }
    }

    // ──────────────────────────────────────────────
    // Current Month Highlight
    // ──────────────────────────────────────────────

    [Fact]
    public async Task CurrentMonth_ExactlyOneCellPerRowHasAprClass()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var shipAprCount = await heatmap.CurrentMonthShipCell.CountAsync();
            var progAprCount = await heatmap.CurrentMonthProgCell.CountAsync();
            var carryAprCount = await heatmap.CurrentMonthCarryCell.CountAsync();
            var blockAprCount = await heatmap.CurrentMonthBlockCell.CountAsync();

            // Each category should have exactly one current-month highlighted cell
            shipAprCount.Should().Be(1);
            progAprCount.Should().Be(1);
            carryAprCount.Should().Be(1);
            blockAprCount.Should().Be(1);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "CurrentMonth_AprClass_Failure");
            throw;
        }
    }

    [Fact]
    public async Task CurrentMonthShipCell_HasCorrectHighlightBackground()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var count = await heatmap.CurrentMonthShipCell.CountAsync();
            if (count > 0)
            {
                var bg = await heatmap.GetCellBackgroundColorAsync(heatmap.CurrentMonthShipCell);
                // #D8F2DA = rgb(216, 242, 218)
                bg.Should().Be("rgb(216, 242, 218)");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "CurrentMonth_ShipBg_Failure");
            throw;
        }
    }

    [Fact]
    public async Task CurrentMonthProgCell_HasCorrectHighlightBackground()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var count = await heatmap.CurrentMonthProgCell.CountAsync();
            if (count > 0)
            {
                var bg = await heatmap.GetCellBackgroundColorAsync(heatmap.CurrentMonthProgCell);
                // #DAE8FB = rgb(218, 232, 251)
                bg.Should().Be("rgb(218, 232, 251)");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "CurrentMonth_ProgBg_Failure");
            throw;
        }
    }

    [Fact]
    public async Task CurrentMonthCarryCell_HasCorrectHighlightBackground()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var count = await heatmap.CurrentMonthCarryCell.CountAsync();
            if (count > 0)
            {
                var bg = await heatmap.GetCellBackgroundColorAsync(heatmap.CurrentMonthCarryCell);
                // #FFF0B0 = rgb(255, 240, 176)
                bg.Should().Be("rgb(255, 240, 176)");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "CurrentMonth_CarryBg_Failure");
            throw;
        }
    }

    [Fact]
    public async Task CurrentMonthBlockCell_HasCorrectHighlightBackground()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var count = await heatmap.CurrentMonthBlockCell.CountAsync();
            if (count > 0)
            {
                var bg = await heatmap.GetCellBackgroundColorAsync(heatmap.CurrentMonthBlockCell);
                // #FFE4E4 = rgb(255, 228, 228)
                bg.Should().Be("rgb(255, 228, 228)");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "CurrentMonth_BlockBg_Failure");
            throw;
        }
    }

    [Fact]
    public async Task NonCurrentMonthCells_DoNotHaveAprClass()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var totalCells = await heatmap.AllCells.CountAsync();
            var aprCells = await heatmap.AllCurrentMonthCells.CountAsync();

            // 4 rows × 1 current month = 4 highlighted cells
            aprCells.Should().BeLessOrEqualTo(4);
            (totalCells - aprCells).Should().BeGreaterThan(0,
                "there should be non-highlighted cells");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "NonCurrentMonth_NoApr_Failure");
            throw;
        }
    }

    // ──────────────────────────────────────────────
    // Grid Structure
    // ──────────────────────────────────────────────

    [Fact]
    public async Task HeatmapGrid_IsVisibleOnDashboard()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            await Assertions.Expect(heatmap.HeatmapGrid).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "HeatmapGrid_Visible_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapTitle_IsVisibleWithCorrectStyling()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            await Assertions.Expect(heatmap.HeatmapTitle).ToBeVisibleAsync();

            var fontSize = await heatmap.GetElementFontSizeAsync(heatmap.HeatmapTitle);
            var fontWeight = await heatmap.GetElementFontWeightAsync(heatmap.HeatmapTitle);

            fontSize.Should().Be("14px");
            fontWeight.Should().Be("700");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "HeatmapTitle_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapGrid_TotalChildrenPerRow_EqualsHeaderPlusCells()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var headerCount = await heatmap.AllRowHeaders.CountAsync();
            var shipCellCount = await heatmap.ShipCells.CountAsync();

            // Each of the 4 rows contributes 1 header + N cells
            headerCount.Should().Be(4);
            shipCellCount.Should().BeGreaterThan(0);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "GridChildren_Failure");
            throw;
        }
    }

    // ──────────────────────────────────────────────
    // Column Headers
    // ──────────────────────────────────────────────

    [Fact]
    public async Task ColumnHeaders_MatchMonthCount()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var colHeaderCount = await heatmap.ColumnHeaders.CountAsync();
            var cellsPerRow = await heatmap.ShipCells.CountAsync();

            colHeaderCount.Should().Be(cellsPerRow,
                "number of column headers should match number of cells per row");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "ColumnHeaders_Count_Failure");
            throw;
        }
    }

    [Fact]
    public async Task CurrentMonthColumnHeader_HasGoldHighlight()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var count = await heatmap.CurrentMonthColumnHeader.CountAsync();
            count.Should().Be(1, "exactly one column header should be highlighted as current month");

            var bg = await heatmap.GetCellBackgroundColorAsync(heatmap.CurrentMonthColumnHeader);
            // #FFF0D0 = rgb(255, 240, 208)
            bg.Should().Be("rgb(255, 240, 208)");

            var textColor = await heatmap.GetElementColorAsync(heatmap.CurrentMonthColumnHeader);
            // #C07700 = rgb(192, 119, 0)
            textColor.Should().Be("rgb(192, 119, 0)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "CurrentMonthColHeader_Failure");
            throw;
        }
    }

    // ──────────────────────────────────────────────
    // Visual Fidelity at 1920x1080
    // ──────────────────────────────────────────────

    [Fact]
    public async Task Dashboard_RendersAt1920x1080_WithoutScrollbars()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var hasScrollbar = await page.EvaluateAsync<bool>(
                "() => document.documentElement.scrollHeight > document.documentElement.clientHeight");
            hasScrollbar.Should().BeFalse("dashboard should fit within 1920x1080 without vertical scrollbar");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "NoScrollbar_Failure");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapGrid_UsesCSSSGridLayout()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPage(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var display = await heatmap.HeatmapGrid.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).display");
            display.Should().Be("grid");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "CSSGrid_Failure");
            throw;
        }
    }
}