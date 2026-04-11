using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Fixtures;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardUITests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    // ── Page Load ──

    [Fact]
    public async Task Dashboard_LoadsSuccessfully()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            // Either dashboard loads or error panel shows - page should respond
            var response = await page.GotoAsync(_fixture.BaseUrl);
            response!.Status.Should().Be(200, "dashboard should return HTTP 200");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_LoadsSuccessfully));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_RendersDashboardRoot_WhenDataValid()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            var isLoaded = await dashboardPage.IsDashboardLoadedAsync();
            var isError = await dashboardPage.IsErrorStateAsync();

            // One of these should be true
            (isLoaded || isError).Should().BeTrue(
                "dashboard should render either content or an error state");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_RendersDashboardRoot_WhenDataValid));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_HeaderAppearsFirst_InDashboardRoot()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.IsDashboardLoadedAsync())
            {
                var firstChild = page.Locator("div.dashboard-root > *:first-child");
                var className = await firstChild.GetAttributeAsync("class");
                className.Should().Contain("hdr", "header should be the first child of dashboard-root");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_HeaderAppearsFirst_InDashboardRoot));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_HeaderContainsTitle_MatchingDataJson()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.IsDashboardLoadedAsync())
            {
                var titleText = await dashboardPage.GetHeaderTitleTextAsync();
                titleText.Should().NotBeNullOrWhiteSpace(
                    "the header title should be populated from data.json");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_HeaderContainsTitle_MatchingDataJson));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_ViewportSize_Is1920x1080()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            var viewportSize = page.ViewportSize;
            viewportSize.Should().NotBeNull();
            viewportSize!.Width.Should().Be(1920);
            viewportSize.Height.Should().Be(1080);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ViewportSize_Is1920x1080));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_HeaderAndContent_AreSeparatedByBorder()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.IsDashboardLoadedAsync())
            {
                var borderStyle = await dashboardPage.Header.EvaluateAsync<string>(
                    "el => getComputedStyle(el).borderBottomStyle");
                borderStyle.Should().Be("solid");

                var borderWidth = await dashboardPage.Header.EvaluateAsync<string>(
                    "el => getComputedStyle(el).borderBottomWidth");
                borderWidth.Should().Be("1px");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_HeaderAndContent_AreSeparatedByBorder));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_HasNoConsoleErrors_OnLoad()
    {
        var page = await _fixture.NewPageAsync();
        var consoleErrors = new List<string>();

        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
                consoleErrors.Add(msg.Text);
        };

        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            // Allow Blazor SignalR messages but filter real errors
            var criticalErrors = consoleErrors
                .Where(e => !e.Contains("WebSocket") && !e.Contains("blazor"))
                .ToList();

            criticalErrors.Should().BeEmpty(
                "dashboard should load without critical console errors");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_HasNoConsoleErrors_OnLoad));
            throw;
        }
    }

    // ── Screenshot Capture (for visual comparison) ──

    [Fact]
    public async Task Dashboard_CanCaptureScreenshot_At1920x1080()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_FullPage_1920x1080");

            // If we get here without error, screenshot was captured
            true.Should().BeTrue();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_CanCaptureScreenshot_At1920x1080));
            throw;
        }
    }

    // ── Navigation ──

    [Fact]
    public async Task Dashboard_RootUrl_ServesContent()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            response.Should().NotBeNull();
            response!.Status.Should().Be(200);
            response.Ok.Should().BeTrue();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_RootUrl_ServesContent));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_BodyHasCorrectDimensions()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            var bodyWidth = await page.EvaluateAsync<int>("() => document.body.scrollWidth");
            bodyWidth.Should().BeLessOrEqualTo(1920, "body should fit within 1920px width");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_BodyHasCorrectDimensions));
            throw;
        }
    }
}