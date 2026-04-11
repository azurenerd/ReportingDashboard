using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// UI tests verifying Heatmap cell color theming from PR #555.
/// Covers category-specific background colors, current month highlighting (gold),
/// row header colors, item dot colors, and CSS Grid structure.
/// References OriginalDesignConcept.html for expected color values.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeatmapCellThemeTests
{
    private readonly PlaywrightFixture _fixture;

    public HeatmapCellThemeTests(PlaywrightFixture fixture) => _fixture = fixture;

    #region Shipped Row Theme (Green)

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task ShippedHeader_HasGreenTheme()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var bgColor = await heatmap.GetCellBackgroundColorAsync(heatmap.ShippedHeader);
            // #E8F5E9 = rgb(232, 245, 233)
            Assert.Contains("232", bgColor);
            Assert.Contains("245", bgColor);

            var textColor = await heatmap.GetCellTextColorAsync(heatmap.ShippedHeader);
            // #1B7A28 = rgb(27, 122, 40)
            Assert.Contains("27", textColor);
            Assert.Contains("122", textColor);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ShippedHeader_HasGreenTheme));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task ShippedCells_HaveLightGreenBackground()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var cellCount = await heatmap.ShippedCells.CountAsync();
            if (cellCount > 0)
            {
                var bgColor = await heatmap.GetCellBackgroundColorAsync(heatmap.ShippedCells.First);
                // #F0FBF0 = rgb(240, 251, 240) or #D8F2DA for current month
                Assert.Contains("240", bgColor);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ShippedCells_HaveLightGreenBackground));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task ShippedCurrentCell_HasDarkerGreenBackground()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var curCellCount = await heatmap.ShippedCurrentCell.CountAsync();
            if (curCellCount > 0)
            {
                var bgColor = await heatmap.GetCellBackgroundColorAsync(heatmap.ShippedCurrentCell);
                // #D8F2DA = rgb(216, 242, 218)
                Assert.Contains("216", bgColor);
                Assert.Contains("242", bgColor);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ShippedCurrentCell_HasDarkerGreenBackground));
            throw;
        }
    }

    #endregion

    #region In Progress Row Theme (Blue)

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task InProgressHeader_HasBlueTheme()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var bgColor = await heatmap.GetCellBackgroundColorAsync(heatmap.InProgressHeader);
            // #E3F2FD = rgb(227, 242, 253)
            Assert.Contains("227", bgColor);
            Assert.Contains("253", bgColor);

            var textColor = await heatmap.GetCellTextColorAsync(heatmap.InProgressHeader);
            // #1565C0 = rgb(21, 101, 192)
            Assert.Contains("21", textColor);
            Assert.Contains("192", textColor);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(InProgressHeader_HasBlueTheme));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task InProgressCells_HaveLightBlueBackground()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var cellCount = await heatmap.InProgressCells.CountAsync();
            if (cellCount > 0)
            {
                var bgColor = await heatmap.GetCellBackgroundColorAsync(heatmap.InProgressCells.First);
                // #EEF4FE = rgb(238, 244, 254)
                Assert.Contains("238", bgColor);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(InProgressCells_HaveLightBlueBackground));
            throw;
        }
    }

    #endregion

    #region Carryover Row Theme (Amber)

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task CarryoverHeader_HasAmberTheme()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var bgColor = await heatmap.GetCellBackgroundColorAsync(heatmap.CarryoverHeader);
            // #FFF8E1 = rgb(255, 248, 225)
            Assert.Contains("248", bgColor);
            Assert.Contains("225", bgColor);

            var textColor = await heatmap.GetCellTextColorAsync(heatmap.CarryoverHeader);
            // #B45309 = rgb(180, 83, 9)
            Assert.Contains("180", textColor);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(CarryoverHeader_HasAmberTheme));
            throw;
        }
    }

    #endregion

    #region Blockers Row Theme (Red)

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task BlockersHeader_HasRedTheme()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var bgColor = await heatmap.GetCellBackgroundColorAsync(heatmap.BlockersHeader);
            // #FEF2F2 = rgb(254, 242, 242)
            Assert.Contains("254", bgColor);
            Assert.Contains("242", bgColor);

            var textColor = await heatmap.GetCellTextColorAsync(heatmap.BlockersHeader);
            // #991B1B = rgb(153, 27, 27)
            Assert.Contains("153", textColor);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(BlockersHeader_HasRedTheme));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task BlockerCells_HaveLightRedBackground()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var cellCount = await heatmap.BlockerCells.CountAsync();
            if (cellCount > 0)
            {
                var bgColor = await heatmap.GetCellBackgroundColorAsync(heatmap.BlockerCells.First);
                // #FFF5F5 = rgb(255, 245, 245)
                Assert.Contains("245", bgColor);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(BlockerCells_HaveLightRedBackground));
            throw;
        }
    }

    #endregion

    #region Current Month Column Highlighting

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task CurrentMonthHeader_HasGoldBackground()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var curHdrCount = await heatmap.CurrentMonthHeader.CountAsync();
            Assert.Equal(1, curHdrCount);

            var bgColor = await heatmap.GetCellBackgroundColorAsync(heatmap.CurrentMonthHeader);
            // #FFF0D0 = rgb(255, 240, 208)
            Assert.Contains("255", bgColor);
            Assert.Contains("240", bgColor);
            Assert.Contains("208", bgColor);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(CurrentMonthHeader_HasGoldBackground));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task CurrentMonthHeader_HasGoldTextColor()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var curHdrCount = await heatmap.CurrentMonthHeader.CountAsync();
            if (curHdrCount > 0)
            {
                var textColor = await heatmap.GetCellTextColorAsync(heatmap.CurrentMonthHeader);
                // #C07700 = rgb(192, 119, 0)
                Assert.Contains("192", textColor);
                Assert.Contains("119", textColor);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(CurrentMonthHeader_HasGoldTextColor));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task CurrentMonthHeader_ContainsNowLabel()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var curHdrCount = await heatmap.CurrentMonthHeader.CountAsync();
            if (curHdrCount > 0)
            {
                var text = await heatmap.CurrentMonthHeader.TextContentAsync();
                Assert.Contains("Now", text ?? "");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(CurrentMonthHeader_ContainsNowLabel));
            throw;
        }
    }

    #endregion

    #region Grid Layout

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task HeatmapGrid_HasCorrectColumnTemplate()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var style = await heatmap.HeatmapGrid.GetAttributeAsync("style") ?? "";
            Assert.Contains("160px", style);
            Assert.Contains("repeat(", style);
            Assert.Contains("1fr)", style);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(HeatmapGrid_HasCorrectColumnTemplate));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task HeatmapGrid_HasBorder()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var borderStyle = await heatmap.HeatmapGrid.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderStyle");
            Assert.Equal("solid", borderStyle);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(HeatmapGrid_HasBorder));
            throw;
        }
    }

    #endregion

    #region Item Dot Colors

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task ShippedItems_HaveGreenDots()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var itemCount = await heatmap.ShippedItems.CountAsync();
            if (itemCount > 0)
            {
                var dotColor = await heatmap.ShippedItems.First.EvaluateAsync<string>(
                    "el => getComputedStyle(el, '::before').backgroundColor");
                // #34A853 = rgb(52, 168, 83)
                if (!string.IsNullOrEmpty(dotColor) && dotColor != "rgba(0, 0, 0, 0)")
                {
                    Assert.Contains("52", dotColor);
                    Assert.Contains("168", dotColor);
                }
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ShippedItems_HaveGreenDots));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task BlockerItems_HaveRedDots()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var itemCount = await heatmap.BlockerItems.CountAsync();
            if (itemCount > 0)
            {
                var dotColor = await heatmap.BlockerItems.First.EvaluateAsync<string>(
                    "el => getComputedStyle(el, '::before').backgroundColor");
                // #EA4335 = rgb(234, 67, 53)
                if (!string.IsNullOrEmpty(dotColor) && dotColor != "rgba(0, 0, 0, 0)")
                {
                    Assert.Contains("234", dotColor);
                }
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(BlockerItems_HaveRedDots));
            throw;
        }
    }

    #endregion

    #region Item Content Rendering

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task HeatmapItems_HaveFontSize12px()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var itemCount = await heatmap.AllItems.CountAsync();
            if (itemCount > 0)
            {
                var fontSize = await heatmap.AllItems.First.EvaluateAsync<string>(
                    "el => getComputedStyle(el).fontSize");
                Assert.Equal("12px", fontSize);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(HeatmapItems_HaveFontSize12px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task HeatmapItems_HaveTextColor333()
    {
        var page = await _fixture.NewPageAsync();
        var heatmap = new HeatmapPageObject(page, _fixture.BaseUrl);

        try
        {
            await heatmap.NavigateAsync();

            var itemCount = await heatmap.AllItems.CountAsync();
            if (itemCount > 0)
            {
                var color = await heatmap.AllItems.First.EvaluateAsync<string>(
                    "el => getComputedStyle(el).color");
                // #333 = rgb(51, 51, 51)
                Assert.Contains("51", color);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(HeatmapItems_HaveTextColor333));
            throw;
        }
    }

    #endregion
}