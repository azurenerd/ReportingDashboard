using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardSectionsUITests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardSectionsUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dashboard_HeaderSection_IsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            await Assertions.Expect(dashboard.HeaderSection).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_HeaderVisible");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_TimelineSection_IsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            await Assertions.Expect(dashboard.TimelineSection).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_TimelineVisible");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_HeatmapSection_IsVisible()
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
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_HeatmapVisible");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_AllThreeSections_AreRenderedInOrder()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            await Assertions.Expect(dashboard.HeaderSection).ToHaveCountAsync(1);
            await Assertions.Expect(dashboard.TimelineSection).ToHaveCountAsync(1);
            await Assertions.Expect(dashboard.HeatmapSection).ToHaveCountAsync(1);

            // Verify order: header before timeline before heatmap
            var headerBox = await dashboard.HeaderSection.BoundingBoxAsync();
            var timelineBox = await dashboard.TimelineSection.BoundingBoxAsync();
            var heatmapBox = await dashboard.HeatmapSection.BoundingBoxAsync();

            headerBox.Should().NotBeNull();
            timelineBox.Should().NotBeNull();
            heatmapBox.Should().NotBeNull();

            headerBox!.Y.Should().BeLessThan(timelineBox!.Y, "header should be above timeline");
            timelineBox.Y.Should().BeLessThan(heatmapBox!.Y, "timeline should be above heatmap");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_SectionsOrder");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_ErrorPanel_IsNotVisible_WhenDataValid()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var errorCount = await dashboard.ErrorPanel.CountAsync();
            errorCount.Should().Be(0, "error panel should not be visible when data loads successfully");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_NoErrorPanel");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_HeaderSection_HasBottomBorder()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var borderBottom = await dashboard.HeaderSection.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).borderBottomStyle");
            borderBottom.Should().Be("solid");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_HeaderBorder");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_TimelineSection_HasCorrectBackground()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var bg = await dashboard.TimelineSection.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).backgroundColor");
            // #FAFAFA = rgb(250, 250, 250)
            bg.Should().Be("rgb(250, 250, 250)");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_TimelineBg");
            throw;
        }
    }
}