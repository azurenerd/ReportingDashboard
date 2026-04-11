using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardLayoutUITests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardLayoutUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dashboard_LoadsSuccessfully()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var dashboard = new DashboardPage(page, _fixture.BaseUrl);
            await dashboard.NavigateAsync();

            var isVisible = await dashboard.IsFullDashboardVisibleAsync();
            isVisible.Should().BeTrue("all three dashboard sections should be visible");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_LoadsSuccessfully");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_ViewportIs1920x1080()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var dashboard = new DashboardPage(page, _fixture.BaseUrl);
            await dashboard.NavigateAsync();

            var viewport = dashboard.GetViewport();
            viewport.Should().NotBeNull();
            viewport!.Width.Should().Be(1920);
            viewport.Height.Should().Be(1080);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_Viewport");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_HeaderSectionIsVisible()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var dashboard = new DashboardPage(page, _fixture.BaseUrl);
            await dashboard.NavigateAsync();

            await Assertions.Expect(dashboard.HeaderSection).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_Header");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_TimelineSectionIsVisible()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var dashboard = new DashboardPage(page, _fixture.BaseUrl);
            await dashboard.NavigateAsync();

            await Assertions.Expect(dashboard.TimelineSection).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_Timeline");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_HeatmapSectionIsVisible()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var dashboard = new DashboardPage(page, _fixture.BaseUrl);
            await dashboard.NavigateAsync();

            await Assertions.Expect(dashboard.HeatmapSection).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_Heatmap");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_SectionsAppearInCorrectOrder()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var dashboard = new DashboardPage(page, _fixture.BaseUrl);
            await dashboard.NavigateAsync();

            var hdrBox = await dashboard.HeaderSection.BoundingBoxAsync();
            var tlBox = await dashboard.TimelineSection.BoundingBoxAsync();
            var hmBox = await dashboard.HeatmapSection.BoundingBoxAsync();

            hdrBox.Should().NotBeNull();
            tlBox.Should().NotBeNull();
            hmBox.Should().NotBeNull();

            hdrBox!.Y.Should().BeLessThan(tlBox!.Y, "header should be above timeline");
            tlBox.Y.Should().BeLessThan(hmBox!.Y, "timeline should be above heatmap");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_SectionOrder");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_TimelineAreaStyling_HasCorrectBackground()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var dashboard = new DashboardPage(page, _fixture.BaseUrl);
            await dashboard.NavigateAsync();

            var bgColor = await dashboard.TimelineSection.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");

            // #FAFAFA = rgb(250, 250, 250)
            bgColor.Should().Contain("250", "timeline background should be #FAFAFA");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_TimelineBg");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_TimelineAreaStyling_UsesFlexLayout()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var dashboard = new DashboardPage(page, _fixture.BaseUrl);
            await dashboard.NavigateAsync();

            var display = await dashboard.TimelineSection.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");

            display.Should().Be("flex", ".tl-area should use flex layout");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_TimelineFlex");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_FullPageScreenshot_RendersCorrectly()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var dashboard = new DashboardPage(page, _fixture.BaseUrl);
            await dashboard.NavigateAsync();

            var errors = new List<string>();
            page.Console += (_, msg) =>
            {
                if (msg.Type == "error")
                    errors.Add(msg.Text);
            };

            await _fixture.CaptureScreenshotAsync(page, "Dashboard_FullPage_Verification");

            var isVisible = await dashboard.IsFullDashboardVisibleAsync();
            isVisible.Should().BeTrue();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_FullPage_Failure");
            throw;
        }
    }
}