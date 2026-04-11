using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class TimelineUITests
{
    private readonly PlaywrightFixture _fixture;

    public TimelineUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Timeline_SvgElement_IsPresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var timeline = new TimelinePage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            await Assertions.Expect(timeline.SvgElement).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_SvgPresent");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_SvgWidth_Is1560()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var timeline = new TimelinePage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var width = await timeline.GetSvgWidthAsync();
            width.Should().Be("1560");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_SvgWidth");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_TrackLabels_AreRendered()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var timeline = new TimelinePage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var count = await timeline.GetTrackCountAsync();
            count.Should().BeGreaterThanOrEqualTo(1, "at least one track should be rendered");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_TrackLabels");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_TrackIds_HaveColorStyling()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var timeline = new TimelinePage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var firstId = timeline.TrackIds.First;
            var style = await firstId.GetAttributeAsync("style");
            style.Should().NotBeNullOrEmpty();
            style.Should().Contain("color:");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_TrackIdColors");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_NowLine_IsPresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var timeline = new TimelinePage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var nowLineCount = await timeline.NowLine.CountAsync();
            nowLineCount.Should().BeGreaterThanOrEqualTo(1, "NOW line should be rendered");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_NowLine");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_NowLabel_IsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var timeline = new TimelinePage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var nowLabelCount = await timeline.NowLabel.CountAsync();
            nowLabelCount.Should().BeGreaterThanOrEqualTo(1, "NOW label text should be present");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_NowLabel");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_DropShadowFilter_IsDefined()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var timeline = new TimelinePage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            await Assertions.Expect(timeline.DropShadowFilter).ToHaveCountAsync(1);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_DropShadow");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_PocDiamonds_AreGold()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var timeline = new TimelinePage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var pocCount = await timeline.PocDiamonds.CountAsync();
            pocCount.Should().BeGreaterThanOrEqualTo(1, "at least one PoC diamond should be rendered");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_PocDiamonds");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_ProductionDiamonds_AreGreen()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var timeline = new TimelinePage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var prodCount = await timeline.ProductionDiamonds.CountAsync();
            prodCount.Should().BeGreaterThanOrEqualTo(1, "at least one production diamond should be rendered");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_ProductionDiamonds");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_Checkpoints_AreCircles()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var timeline = new TimelinePage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var circleCount = await timeline.CheckpointCircles.CountAsync();
            circleCount.Should().BeGreaterThanOrEqualTo(1, "at least one checkpoint circle should be rendered");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_Checkpoints");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_MilestoneTooltips_ArePresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var timeline = new TimelinePage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var tooltipCount = await timeline.MilestoneTooltips.CountAsync();
            tooltipCount.Should().BeGreaterThanOrEqualTo(1, "milestones should have SVG title tooltips");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_Tooltips");
            throw;
        }
    }

    [Fact]
    public async Task Timeline_HorizontalTrackLines_AreRendered()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var timeline = new TimelinePage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var lineCount = await timeline.SvgLines.CountAsync();
            lineCount.Should().BeGreaterThanOrEqualTo(3,
                "there should be track lines, month grid lines, and NOW line");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Timeline_HorizontalLines");
            throw;
        }
    }
}