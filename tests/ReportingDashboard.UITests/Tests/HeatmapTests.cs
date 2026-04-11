using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeatmapTests
{
    private readonly PlaywrightFixture _fixture;

    public HeatmapTests(PlaywrightFixture fixture) => _fixture = fixture;

    private async Task<DashboardPage> LoadDashboardAsync()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        await dashboard.NavigateAsync();
        await dashboard.WaitForDashboardLoadedAsync();
        return dashboard;
    }

    [Fact]
    public async Task Heatmap_WrapIsVisible()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            await Assertions.Expect(dashboard.HeatmapWrap).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Heatmap_WrapIsVisible));
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_TitleContainsExpectedText()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            await Assertions.Expect(dashboard.HeatmapTitle).ToBeVisibleAsync();

            var titleText = await dashboard.HeatmapTitle.TextContentAsync();
            titleText.Should().NotBeNull();
            titleText = titleText!.ToUpperInvariant();
            titleText.Should().Contain("MONTHLY EXECUTION HEATMAP");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Heatmap_TitleContainsExpectedText));
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_TitleIsUppercase14pxBold()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var fontSize = await dashboard.HeatmapTitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            var fontWeight = await dashboard.HeatmapTitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontWeight");
            var textTransform = await dashboard.HeatmapTitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).textTransform");

            fontSize.Should().Be("14px");
            fontWeight.Should().BeOneOf("700", "bold");
            textTransform.Should().Be("uppercase");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Heatmap_TitleIsUppercase14pxBold));
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_GridIsVisible()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            await Assertions.Expect(dashboard.HeatmapGrid).ToBeVisibleAsync();

            var display = await dashboard.HeatmapGrid.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            display.Should().Be("grid");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Heatmap_GridIsVisible));
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_CornerCellShowsSTATUS()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            await Assertions.Expect(dashboard.HeatmapCorner).ToBeVisibleAsync();

            var text = await dashboard.HeatmapCorner.TextContentAsync();
            text.Should().NotBeNull();
            text!.Trim().Should().Be("STATUS");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Heatmap_CornerCellShowsSTATUS));
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_HasColumnHeaders()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var count = await dashboard.ColumnHeaders.CountAsync();
            count.Should().BeGreaterThan(0, "heatmap should have month column headers");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Heatmap_HasColumnHeaders));
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_CurrentMonthHeaderIsHighlighted()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var count = await dashboard.CurrentMonthHeader.CountAsync();
            count.Should().Be(1, "exactly one month should be highlighted as current");

            var text = await dashboard.CurrentMonthHeader.TextContentAsync();
            text.Should().Contain("Now");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Heatmap_CurrentMonthHeaderIsHighlighted));
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_HasFourCategoryRowHeaders()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var count = await dashboard.RowHeaders.CountAsync();
            count.Should().Be(4, "heatmap should have 4 category rows: shipped, in progress, carryover, blockers");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Heatmap_HasFourCategoryRowHeaders));
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_ShippedRowHeaderExists()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            await Assertions.Expect(dashboard.ShippedHeader).ToBeVisibleAsync();

            var text = await dashboard.ShippedHeader.TextContentAsync();
            text.Should().Contain("Shipped");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Heatmap_ShippedRowHeaderExists));
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_InProgressRowHeaderExists()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            await Assertions.Expect(dashboard.ProgHeader).ToBeVisibleAsync();

            var text = await dashboard.ProgHeader.TextContentAsync();
            text.Should().Contain("Progress");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Heatmap_InProgressRowHeaderExists));
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_CarryoverRowHeaderExists()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            await Assertions.Expect(dashboard.CarryHeader).ToBeVisibleAsync();

            var text = await dashboard.CarryHeader.TextContentAsync();
            text.Should().Contain("Carryover");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Heatmap_CarryoverRowHeaderExists));
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_BlockersRowHeaderExists()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            await Assertions.Expect(dashboard.BlockHeader).ToBeVisibleAsync();

            var text = await dashboard.BlockHeader.TextContentAsync();
            text.Should().Contain("Blocker");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Heatmap_BlockersRowHeaderExists));
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_CellsContainWorkItems()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var cellCount = await dashboard.HeatmapCells.CountAsync();
            cellCount.Should().BeGreaterThan(0, "heatmap should have data cells");

            // Check that at least some cells have items or show dash
            var itemCount = await dashboard.HeatmapItems.CountAsync();
            var emptyCount = await dashboard.EmptyCells.CountAsync();

            (itemCount + emptyCount).Should().BeGreaterThan(0,
                "cells should contain either items or empty indicators");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Heatmap_CellsContainWorkItems));
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_WorkItemsHave12pxFont()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var itemCount = await dashboard.HeatmapItems.CountAsync();
            if (itemCount > 0)
            {
                var fontSize = await dashboard.HeatmapItems.First.EvaluateAsync<string>(
                    "el => getComputedStyle(el).fontSize");
                fontSize.Should().Be("12px");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Heatmap_WorkItemsHave12pxFont));
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_GridUsesCorrectColumnLayout()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var style = await dashboard.HeatmapGrid.GetAttributeAsync("style");
            style.Should().NotBeNull();
            style.Should().Contain("160px");
            style.Should().Contain("repeat(");
            style.Should().Contain("1fr)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Heatmap_GridUsesCorrectColumnLayout));
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_CellCountMatchesRowsTimesMonths()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var monthCount = await dashboard.ColumnHeaders.CountAsync();
            var rowCount = await dashboard.RowHeaders.CountAsync();
            var cellCount = await dashboard.HeatmapCells.CountAsync();

            // Total cells should be rows * months
            cellCount.Should().Be(rowCount * monthCount);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Heatmap_CellCountMatchesRowsTimesMonths));
            throw;
        }
    }
}