using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// UI tests validating that the Dashboard page correctly renders either the error panel
/// or the normal dashboard based on DashboardDataService state. These tests verify
/// the conditional rendering logic through the browser.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardErrorStateTests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardErrorStateTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    // ── Mutual Exclusivity Tests ───────────────────────────────────────

    [Fact]
    public async Task Dashboard_ShowsEitherErrorPanelOrDashboard_NeverBoth()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl + "/", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 15000
            });

            var hasErrorPanel = await page.Locator(".error-panel").IsVisibleAsync();
            var hasDashboardHeader = await page.Locator(".hdr").IsVisibleAsync();

            // They must be mutually exclusive
            (hasErrorPanel && hasDashboardHeader).Should().BeFalse(
                "error panel and dashboard header should never both be visible");

            // At least one should be present (page is not blank)
            (hasErrorPanel || hasDashboardHeader).Should().BeTrue(
                "page should render either error panel or dashboard");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ShowsEitherErrorPanelOrDashboard_NeverBoth));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_WhenErrorPanel_TimelineNotRendered()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl + "/", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 15000
            });

            if (await page.Locator(".error-panel").IsVisibleAsync())
            {
                (await page.Locator(".tl-area").CountAsync()).Should().Be(0,
                    "timeline area should not exist in DOM when error panel is shown");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_WhenErrorPanel_TimelineNotRendered));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_WhenErrorPanel_HeatmapNotRendered()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl + "/", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 15000
            });

            if (await page.Locator(".error-panel").IsVisibleAsync())
            {
                (await page.Locator(".hm-wrap").CountAsync()).Should().Be(0,
                    "heatmap wrapper should not exist in DOM when error panel is shown");
                (await page.Locator(".hm-grid").CountAsync()).Should().Be(0,
                    "heatmap grid should not exist in DOM when error panel is shown");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_WhenErrorPanel_HeatmapNotRendered));
            throw;
        }
    }

    // ── Dashboard Normal State Tests ───────────────────────────────────

    [Fact]
    public async Task Dashboard_WhenNormal_ShowsHeaderSection()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl + "/", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 15000
            });

            if (await page.Locator(".hdr").IsVisibleAsync())
            {
                // Dashboard is showing normal content
                (await page.Locator(".error-panel").CountAsync()).Should().Be(0,
                    "error panel should not exist when dashboard is showing");

                // Header should contain expected elements
                var headerText = await page.Locator(".hdr").InnerTextAsync();
                headerText.Should().NotBeNullOrWhiteSpace(
                    "header should contain text content");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_WhenNormal_ShowsHeaderSection));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_WhenNormal_ShowsTimelineSection()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl + "/", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 15000
            });

            if (await page.Locator(".hdr").IsVisibleAsync())
            {
                (await page.Locator(".tl-area").CountAsync()).Should().BeGreaterThan(0,
                    "timeline area should exist when dashboard is showing");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_WhenNormal_ShowsTimelineSection));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_WhenNormal_ShowsHeatmapSection()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl + "/", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 15000
            });

            if (await page.Locator(".hdr").IsVisibleAsync())
            {
                (await page.Locator(".hm-wrap").CountAsync()).Should().BeGreaterThan(0,
                    "heatmap wrapper should exist when dashboard is showing");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_WhenNormal_ShowsHeatmapSection));
            throw;
        }
    }

    // ── Page Load Resilience Tests ─────────────────────────────────────

    [Fact]
    public async Task Dashboard_PageLoads_WithoutJsErrors()
    {
        var page = await _fixture.NewPageAsync();
        var jsErrors = new List<string>();

        page.PageError += (_, error) => jsErrors.Add(error);

        try
        {
            await page.GotoAsync(_fixture.BaseUrl + "/", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 15000
            });

            // Allow Blazor reconnection errors but no application errors
            var applicationErrors = jsErrors
                .Where(e => !e.Contains("circuit", StringComparison.OrdinalIgnoreCase))
                .Where(e => !e.Contains("reconnect", StringComparison.OrdinalIgnoreCase))
                .ToList();

            applicationErrors.Should().BeEmpty(
                "page should load without JavaScript errors");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_PageLoads_WithoutJsErrors));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_PageLoads_WithinReasonableTime()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            await page.GotoAsync(_fixture.BaseUrl + "/", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 15000
            });

            stopwatch.Stop();

            stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000,
                "page should load within 10 seconds");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_PageLoads_WithinReasonableTime));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_CssLoads_Successfully()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var failedRequests = new List<string>();
            page.RequestFailed += (_, request) =>
            {
                if (request.Url.Contains(".css"))
                    failedRequests.Add(request.Url);
            };

            await page.GotoAsync(_fixture.BaseUrl + "/", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 15000
            });

            failedRequests.Should().BeEmpty(
                "all CSS files should load successfully");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_CssLoads_Successfully));
            throw;
        }
    }

    // ── Viewport Tests ─────────────────────────────────────────────────

    [Fact]
    public async Task Dashboard_BodyHasCorrectDimensions()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl + "/", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 15000
            });

            var bodyWidth = await page.EvaluateAsync<double>(
                "() => document.body.getBoundingClientRect().width");

            bodyWidth.Should().Be(1920,
                "body should be 1920px wide for PowerPoint screenshot fidelity");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_BodyHasCorrectDimensions));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_NoHorizontalScrollbar()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl + "/", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 15000
            });

            var hasHorizontalScroll = await page.EvaluateAsync<bool>(
                "() => document.documentElement.scrollWidth > document.documentElement.clientWidth");

            hasHorizontalScroll.Should().BeFalse(
                "page should not have horizontal scrollbar at 1920px viewport");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_NoHorizontalScrollbar));
            throw;
        }
    }

    // ── White Background Test ──────────────────────────────────────────

    [Fact]
    public async Task Dashboard_HasWhiteBackground()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl + "/", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 15000
            });

            var bgColor = await page.EvaluateAsync<string>(
                "() => getComputedStyle(document.body).backgroundColor");

            // White can be rgb(255, 255, 255) or rgba(0, 0, 0, 0) (transparent defaults to white)
            bgColor.Should().BeOneOf(
                "rgb(255, 255, 255)",
                "rgba(0, 0, 0, 0)",
                "transparent",
                "page background should be white (#FFFFFF)");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_HasWhiteBackground));
            throw;
        }
    }
}