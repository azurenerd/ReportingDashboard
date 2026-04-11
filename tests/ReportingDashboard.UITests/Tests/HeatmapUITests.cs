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
    public async Task Heatmap_WrapSection_IsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            await Assertions.Expect(dashboard.HeatmapSection).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_WrapVisible");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_Grid_IsPresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            await Assertions.Expect(dashboard.HeatmapGrid).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_GridPresent");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_Grid_UsesCssGrid()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var display = await dashboard.HeatmapGrid.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).display");
            display.Should().Be("grid");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_CssGrid");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_ShippedRowHeader_IsPresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var shipHdr = page.Locator(".ship-hdr");
            await Assertions.Expect(shipHdr).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_ShippedHeader");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_InProgressRowHeader_IsPresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var progHdr = page.Locator(".prog-hdr");
            await Assertions.Expect(progHdr).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_InProgressHeader");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_CarryoverRowHeader_IsPresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var carryHdr = page.Locator(".carry-hdr");
            await Assertions.Expect(carryHdr).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_CarryoverHeader");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_BlockersRowHeader_IsPresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var blockHdr = page.Locator(".block-hdr");
            await Assertions.Expect(blockHdr).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_BlockersHeader");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_ColumnHeaders_ArePresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var colHeaders = page.Locator(".hm-col-hdr");
            var count = await colHeaders.CountAsync();
            count.Should().BeGreaterThanOrEqualTo(1, "at least one month column header should exist");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_ColumnHeaders");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_DataCells_ContainItems()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var items = page.Locator(".hm-cell .it");
            var count = await items.CountAsync();
            count.Should().BeGreaterThanOrEqualTo(1, "heatmap should contain at least one work item");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_DataItems");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_ShippedCells_HaveGreenBackground()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var shipCell = page.Locator(".ship-cell").First;
            var bg = await shipCell.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).backgroundColor");
            // #F0FBF0 = rgb(240, 251, 240)
            bg.Should().NotBeNullOrEmpty();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_ShippedCellBg");
            throw;
        }
    }

    [Fact]
    public async Task Heatmap_Corner_IsPresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var corner = page.Locator(".hm-corner");
            await Assertions.Expect(corner).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Heatmap_Corner");
            throw;
        }
    }
}