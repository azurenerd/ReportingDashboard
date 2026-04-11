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

    [Fact]
    public async Task Heatmap_Section_IsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            await Assertions.Expect(heatmap.HeatmapWrap).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_Visible");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_Title_IsUppercase()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var textTransform = await heatmap.HeatmapTitle.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).textTransform");
            textTransform.Should().Be("uppercase");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_Title_Uppercase");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_Title_FontSize_Is14px()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var fontSize = await heatmap.HeatmapTitle.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).fontSize");
            fontSize.Should().Be("14px");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_Title_FontSize");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_Grid_IsDisplayGrid()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var display = await heatmap.HeatmapGrid.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).display");
            display.Should().Be("grid");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_Grid_Display");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_Grid_HasBorder()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var borderStyle = await heatmap.HeatmapGrid.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).borderStyle");
            borderStyle.Should().Contain("solid");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_Grid_Border");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_Corner_IsPresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            await Assertions.Expect(heatmap.Corner).ToHaveCountAsync(1);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_Corner");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_ColumnHeaders_ArePresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var count = await heatmap.ColumnHeaders.CountAsync();
            count.Should().BeGreaterThanOrEqualTo(1, "at least one column header should exist");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_ColumnHeaders");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_CurrentMonthHeader_HasGoldBackground()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var aprHdrCount = await heatmap.CurrentMonthHeader.CountAsync();
            if (aprHdrCount > 0)
            {
                var bg = await heatmap.CurrentMonthHeader.First.EvaluateAsync<string>(
                    "el => window.getComputedStyle(el).backgroundColor");
                // #FFF0D0 = rgb(255, 240, 208)
                bg.Should().Be("rgb(255, 240, 208)");

                var color = await heatmap.CurrentMonthHeader.First.EvaluateAsync<string>(
                    "el => window.getComputedStyle(el).color");
                // #C07700 = rgb(192, 119, 0)
                color.Should().Be("rgb(192, 119, 0)");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_CurrentMonth_Gold");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_FourRowHeaders_ArePresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var count = await heatmap.RowHeaders.CountAsync();
            count.Should().Be(4, "shipped, in progress, carryover, blockers");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_RowHeaders");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_ShippedHeader_HasGreenTheme()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var bg = await heatmap.ShippedHeader.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).backgroundColor");
            // #E8F5E9 = rgb(232, 245, 233)
            bg.Should().Be("rgb(232, 245, 233)");

            var color = await heatmap.ShippedHeader.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).color");
            // #1B7A28 = rgb(27, 122, 40)
            color.Should().Be("rgb(27, 122, 40)");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_ShippedHeader_Green");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_InProgressHeader_HasBlueTheme()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var bg = await heatmap.InProgressHeader.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).backgroundColor");
            // #E3F2FD = rgb(227, 242, 253)
            bg.Should().Be("rgb(227, 242, 253)");

            var color = await heatmap.InProgressHeader.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).color");
            // #1565C0 = rgb(21, 101, 192)
            color.Should().Be("rgb(21, 101, 192)");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_InProgressHeader_Blue");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_CarryoverHeader_HasAmberTheme()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var bg = await heatmap.CarryoverHeader.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).backgroundColor");
            // #FFF8E1 = rgb(255, 248, 225)
            bg.Should().Be("rgb(255, 248, 225)");

            var color = await heatmap.CarryoverHeader.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).color");
            // #B45309 = rgb(180, 83, 9)
            color.Should().Be("rgb(180, 83, 9)");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_CarryoverHeader_Amber");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_BlockersHeader_HasRedTheme()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var bg = await heatmap.BlockersHeader.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).backgroundColor");
            // #FEF2F2 = rgb(254, 242, 242)
            bg.Should().Be("rgb(254, 242, 242)");

            var color = await heatmap.BlockersHeader.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).color");
            // #991B1B = rgb(153, 27, 27)
            color.Should().Be("rgb(153, 27, 27)");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_BlockersHeader_Red");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_ShippedCells_HaveGreenBackground()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var cellCount = await heatmap.ShippedCells.CountAsync();
            cellCount.Should().BeGreaterThanOrEqualTo(1);

            var bg = await heatmap.ShippedCells.First.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).backgroundColor");
            // Either #F0FBF0 (rgb(240,251,240)) or #D8F2DA (rgb(216,242,218)) for .apr
            bg.Should().Match(v =>
                v == "rgb(240, 251, 240)" || v == "rgb(216, 242, 218)");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_ShippedCells_Green");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_WorkItems_AreRendered()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var itemCount = await heatmap.AllWorkItems.CountAsync();
            itemCount.Should().BeGreaterThanOrEqualTo(1, "at least one work item should be displayed");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_WorkItems");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_WorkItems_FontSize_Is12px()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var itemCount = await heatmap.AllWorkItems.CountAsync();
            if (itemCount > 0)
            {
                var fontSize = await heatmap.AllWorkItems.First.EvaluateAsync<string>(
                    "el => window.getComputedStyle(el).fontSize");
                fontSize.Should().Be("12px");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_WorkItems_FontSize");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_Wrap_FlexDirection_IsColumn()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var flexDir = await heatmap.HeatmapWrap.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).flexDirection");
            flexDir.Should().Be("column");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_FlexColumn");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_IsPositionedBelowTimeline()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var heatmap = new HeatmapPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var tlBox = await dashboard.TimelineSection.BoundingBoxAsync();
            var hmBox = await heatmap.HeatmapWrap.BoundingBoxAsync();

            tlBox.Should().NotBeNull();
            hmBox.Should().NotBeNull();
            hmBox!.Y.Should().BeGreaterThan(tlBox!.Y, "heatmap should be below timeline");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_BelowTimeline");
            throw;
        }
    }
}