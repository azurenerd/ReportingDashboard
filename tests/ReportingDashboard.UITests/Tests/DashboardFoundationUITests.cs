using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardFoundationUITests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardFoundationUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dashboard_LoadsSuccessfully_ReturnsHttp200()
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
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_LoadsSuccessfully");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_PageTitle_IsExecutiveProjectDashboard()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();

            var title = await dashboard.GetTitleAsync();
            title.Should().Contain("Executive Project Dashboard");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_PageTitle");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_IncludesDashboardCssStylesheet()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();

            await Assertions.Expect(dashboard.CssLink).ToHaveCountAsync(1);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_CssStylesheet");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_IncludesBlazorServerScript()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();

            await Assertions.Expect(dashboard.BlazorScript).ToHaveCountAsync(1);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_BlazorScript");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_CssLoads_NoCss404()
    {
        var page = await _fixture.NewPageAsync();
        var cssLoaded = false;
        var cssFailed = false;

        page.Response += (_, response) =>
        {
            if (response.Url.Contains("dashboard.css"))
            {
                cssLoaded = true;
                if (response.Status == 404) cssFailed = true;
            }
        };

        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            cssLoaded.Should().BeTrue("dashboard.css should be requested");
            cssFailed.Should().BeFalse("dashboard.css should not return 404");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_CssLoads");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_ViewportMeta_IsSetTo1920()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();

            var viewport = page.Locator("meta[name='viewport']");
            var content = await viewport.GetAttributeAsync("content");
            content.Should().Contain("1920");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_ViewportMeta");
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_HtmlLangAttribute_IsEnglish()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            var lang = await page.Locator("html").GetAttributeAsync("lang");
            lang.Should().Be("en");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Dashboard_HtmlLang");
            throw;
        }
    }
}