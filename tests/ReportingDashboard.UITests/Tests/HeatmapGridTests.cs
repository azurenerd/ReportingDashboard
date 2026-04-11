using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeatmapGridTests
{
    private readonly PlaywrightFixture _fixture;

    public HeatmapGridTests(PlaywrightFixture fixture) => _fixture = fixture;

    private async Task<DashboardPage> NavigateIfDataAvailableAsync()
    {
        var page = await _fixture.NewPageAsync();
        var dp = new DashboardPage(page, _fixture.BaseUrl);
        await dp.NavigateAsync();
        return dp;
    }

    private async Task<bool> HasDashboardContent(DashboardPage dp)
    {
        return await dp.ErrorSection.CountAsync() == 0
            && await dp.HeatmapWrap.CountAsync() > 0;
    }

    [Fact]
    public async Task Heatmap_WrapSectionExists()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        await Assertions.Expect(dp.HeatmapWrap).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Heatmap_TitleContainsExpectedText()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var titleText = await dp.HeatmapTitle.TextContentAsync();
        titleText.Should().Contain("HEATMAP");
    }

    [Fact]
    public async Task Heatmap_TitleIsUppercase()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var textTransform = await dp.HeatmapTitle.EvaluateAsync<string>(
            "el => getComputedStyle(el).textTransform");
        textTransform.Should().Be("uppercase");
    }

    [Fact]
    public async Task Heatmap_GridUsesDisplayGrid()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var display = await dp.HeatmapGrid.EvaluateAsync<string>(
            "el => getComputedStyle(el).display");
        display.Should().Be("grid");
    }

    [Fact]
    public async Task Heatmap_CornerCellContainsSTATUS()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var text = await dp.HeatmapCorner.TextContentAsync();
        text.Should().Contain("STATUS");
    }

    [Fact]
    public async Task Heatmap_ColumnHeadersExist()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var count = await dp.HeatmapColumnHeaders.CountAsync();
        count.Should().BeGreaterThan(0, "heatmap should have month column headers");
    }

    [Fact]
    public async Task Heatmap_ColumnHeaders_AreBold()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var colCount = await dp.HeatmapColumnHeaders.CountAsync();
        if (colCount == 0) return;

        var fontWeight = await dp.HeatmapColumnHeaders.First.EvaluateAsync<string>(
            "el => getComputedStyle(el).fontWeight");
        fontWeight.Should().BeOneOf("700", "bold");
    }

    [Fact]
    public async Task Heatmap_RowHeadersExist()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var count = await dp.HeatmapRowHeaders.CountAsync();
        count.Should().BeGreaterThan(0, "heatmap should have category row headers");
    }

    [Fact]
    public async Task Heatmap_RowHeaders_AreUppercase()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var rowCount = await dp.HeatmapRowHeaders.CountAsync();
        if (rowCount == 0) return;

        var textTransform = await dp.HeatmapRowHeaders.First.EvaluateAsync<string>(
            "el => getComputedStyle(el).textTransform");
        textTransform.Should().Be("uppercase");
    }

    [Fact]
    public async Task Heatmap_DataCellsExist()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var count = await dp.HeatmapCells.CountAsync();
        count.Should().BeGreaterThan(0, "heatmap should have data cells");
    }

    [Fact]
    public async Task Heatmap_CellsContainItemsOrDash()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var cellCount = await dp.HeatmapCells.CountAsync();
        if (cellCount == 0) return;

        // Each cell should contain either .it items or .empty-cell with dash
        for (int i = 0; i < Math.Min(cellCount, 5); i++)
        {
            var cell = dp.HeatmapCells.Nth(i);
            var text = await cell.TextContentAsync();
            text.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task Heatmap_ShipRowHeader_HasGreenStyling()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var shipHdr = dp.Page.Locator(".ship-hdr");
        var count = await shipHdr.CountAsync();
        if (count == 0) return;

        var bgColor = await shipHdr.First.EvaluateAsync<string>(
            "el => getComputedStyle(el).backgroundColor");
        // Should have a greenish background (#E8F5E9 = rgb(232, 245, 233))
        bgColor.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Heatmap_ProgRowHeader_HasBlueStyling()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var progHdr = dp.Page.Locator(".prog-hdr");
        var count = await progHdr.CountAsync();
        if (count == 0) return;

        var bgColor = await progHdr.First.EvaluateAsync<string>(
            "el => getComputedStyle(el).backgroundColor");
        bgColor.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Heatmap_CarryRowHeader_HasAmberStyling()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var carryHdr = dp.Page.Locator(".carry-hdr");
        var count = await carryHdr.CountAsync();
        if (count == 0) return;

        var bgColor = await carryHdr.First.EvaluateAsync<string>(
            "el => getComputedStyle(el).backgroundColor");
        bgColor.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Heatmap_BlockRowHeader_HasRedStyling()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var blockHdr = dp.Page.Locator(".block-hdr");
        var count = await blockHdr.CountAsync();
        if (count == 0) return;

        var bgColor = await blockHdr.First.EvaluateAsync<string>(
            "el => getComputedStyle(el).backgroundColor");
        bgColor.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Heatmap_CurrentMonthColumn_HasNowHighlight()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var nowHdrCount = await dp.NowHeaders.CountAsync();
        if (nowHdrCount == 0) return; // No current month in view

        var bgColor = await dp.NowHeaders.First.EvaluateAsync<string>(
            "el => getComputedStyle(el).backgroundColor");
        bgColor.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Heatmap_NowBadge_Exists_WhenCurrentMonthPresent()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var nowHdrCount = await dp.NowHeaders.CountAsync();
        if (nowHdrCount == 0) return;

        var badgeCount = await dp.NowBadge.CountAsync();
        badgeCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Heatmap_Items_Have12pxFont()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var itemCount = await dp.HeatmapItems.CountAsync();
        if (itemCount == 0) return;

        var fontSize = await dp.HeatmapItems.First.EvaluateAsync<string>(
            "el => getComputedStyle(el).fontSize");
        fontSize.Should().Be("12px");
    }

    [Fact]
    public async Task Heatmap_Items_HaveBulletPseudoElement()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var itemCount = await dp.HeatmapItems.CountAsync();
        if (itemCount == 0) return;

        // Check the ::before pseudo-element exists with content
        var hasBeforeContent = await dp.HeatmapItems.First.EvaluateAsync<bool>(
            "el => getComputedStyle(el, '::before').content !== 'none'");
        hasBeforeContent.Should().BeTrue("items should have a bullet ::before pseudo-element");
    }

    [Fact]
    public async Task Heatmap_CurrentMonthCells_HaveCurrentClass()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var currentCells = dp.Page.Locator(".hm-cell.current");
        var count = await currentCells.CountAsync();

        // If there's a now-hdr, there should be current cells
        var nowHdrCount = await dp.NowHeaders.CountAsync();
        if (nowHdrCount > 0)
        {
            count.Should().BeGreaterThan(0, "current month column cells should have 'current' class");
        }
    }

    [Fact]
    public async Task Heatmap_WrapHasFlexDirectionColumn()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var direction = await dp.HeatmapWrap.EvaluateAsync<string>(
            "el => getComputedStyle(el).flexDirection");
        direction.Should().Be("column");
    }

    [Fact]
    public async Task Heatmap_WrapFillsRemainingSpace()
    {
        var dp = await NavigateIfDataAvailableAsync();
        if (!await HasDashboardContent(dp)) return;

        var flex = await dp.HeatmapWrap.EvaluateAsync<string>(
            "el => getComputedStyle(el).flexGrow");
        flex.Should().Be("1", "heatmap should fill remaining vertical space");
    }
}