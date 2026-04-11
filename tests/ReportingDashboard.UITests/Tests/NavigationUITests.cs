using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Tests navigation behavior: root route, not-found handling, page reload.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class NavigationUITests
{
    private readonly PlaywrightFixture _fixture;

    public NavigationUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RootRoute_LoadsDashboard()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            // Should show dashboard sections, not error
            await Assertions.Expect(dashboard.HeaderSection).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Nav_RootRoute");
            throw;
        }
    }

    [Fact]
    public async Task RootRoute_HttpResponse_Is200()
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
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Nav_Http200");
            throw;
        }
    }

    [Fact]
    public async Task PageReload_StillRendersDashboard()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            // Reload
            await page.ReloadAsync(new PageReloadOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            await dashboard.WaitForDashboardLoadedAsync();
            await Assertions.Expect(dashboard.HeaderSection).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Nav_Reload");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_LoadsWithin5Seconds()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var start = DateTime.UtcNow;

            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 10000
            });

            var elapsed = DateTime.UtcNow - start;
            elapsed.TotalSeconds.Should().BeLessThan(5, "dashboard should load within 5 seconds");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Nav_LoadTime");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_HtmlContainsNoNavElement()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var navCount = await page.Locator("nav").CountAsync();
            navCount.Should().Be(0, "dashboard should have no <nav> element");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Nav_NoNavElement");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_HtmlCharset_IsUTF8()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            var charset = await page.Locator("meta[charset]").GetAttributeAsync("charset");
            charset.Should().NotBeNull();
            charset!.ToUpperInvariant().Should().Be("UTF-8");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Nav_Charset");
            throw;
        }
    }
}