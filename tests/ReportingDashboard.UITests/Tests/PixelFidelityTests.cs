using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Pixel fidelity tests verifying the dashboard renders at exactly
/// 1920x1080 without scrollbars, overflow, or layout issues.
/// References the OriginalDesignConcept.html spec.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class PixelFidelityTests
{
    private readonly PlaywrightFixture _fixture;

    public PixelFidelityTests(PlaywrightFixture fixture) => _fixture = fixture;

    #region Viewport and Overflow

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_FitsWithin1920x1080_NoScrollbars()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var hasScrollbar = await page.EvaluateAsync<bool>(
                "() => document.documentElement.scrollHeight > document.documentElement.clientHeight || " +
                "document.documentElement.scrollWidth > document.documentElement.clientWidth");
            Assert.False(hasScrollbar,
                "Dashboard should not produce scrollbars at 1920x1080");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_FitsWithin1920x1080_NoScrollbars));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_BodyHasNoMarginOrPadding()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30_000
            });

            var margin = await page.EvaluateAsync<string>(
                "() => getComputedStyle(document.body).margin");
            Assert.Equal("0px", margin);

            var padding = await page.EvaluateAsync<string>(
                "() => getComputedStyle(document.body).padding");
            Assert.Equal("0px", padding);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_BodyHasNoMarginOrPadding));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_BodyHasHiddenOverflow()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30_000
            });

            var overflow = await page.EvaluateAsync<string>(
                "() => getComputedStyle(document.body).overflow");
            Assert.Equal("hidden", overflow);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_BodyHasHiddenOverflow));
            throw;
        }
    }

    #endregion

    #region Section Height Allocation

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_HasCompactHeight()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var headerBox = await dashboard.Header.BoundingBoxAsync();
            Assert.NotNull(headerBox);
            // Header should be compact, around 60-80px based on design
            Assert.True(headerBox!.Height < 120,
                $"Header height {headerBox.Height} should be under 120px");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_HasCompactHeight));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Timeline_Has196pxHeight()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var tlBox = await dashboard.TimelineArea.BoundingBoxAsync();
            Assert.NotNull(tlBox);
            // Timeline area should be close to 196px height
            Assert.True(tlBox!.Height >= 180 && tlBox.Height <= 220,
                $"Timeline height {tlBox.Height} should be approximately 196px");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Timeline_Has196pxHeight));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Heatmap_FillsRemainingSpace()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var hmBox = await dashboard.HeatmapWrap.BoundingBoxAsync();
            Assert.NotNull(hmBox);
            // Heatmap should take the remaining space (1080 - header - timeline)
            Assert.True(hmBox!.Height > 500,
                $"Heatmap height {hmBox.Height} should fill remaining space (>500px)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Heatmap_FillsRemainingSpace));
            throw;
        }
    }

    #endregion

    #region Full Width Usage

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_AllSections_SpanFullWidth()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var headerBox = await dashboard.Header.BoundingBoxAsync();
            var hmBox = await dashboard.HeatmapWrap.BoundingBoxAsync();

            Assert.NotNull(headerBox);
            Assert.NotNull(hmBox);

            // All sections should span close to 1920px
            Assert.True(headerBox!.Width >= 1800,
                $"Header width {headerBox.Width} should span close to 1920px");
            Assert.True(hmBox!.Width >= 1800,
                $"Heatmap width {hmBox.Width} should span close to 1920px");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_AllSections_SpanFullWidth));
            throw;
        }
    }

    #endregion

    #region Screenshot Capture

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_Screenshot_CapturesAt1920x1080()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30_000
            });

            // Wait briefly for any Blazor SignalR rendering
            await page.WaitForTimeoutAsync(1000);

            var viewport = page.ViewportSize;
            Assert.NotNull(viewport);
            Assert.Equal(1920, viewport!.Width);
            Assert.Equal(1080, viewport.Height);

            // Capture screenshot for visual verification
            await _fixture.CaptureScreenshotAsync(page, "dashboard_full_render");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_Screenshot_CapturesAt1920x1080));
            throw;
        }
    }

    #endregion
}