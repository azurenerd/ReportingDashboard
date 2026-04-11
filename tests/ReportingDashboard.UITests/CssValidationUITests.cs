using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Fixtures;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class CssValidationUITests
{
    private readonly PlaywrightFixture _fixture;

    public CssValidationUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DashboardCss_ContainsHdrClass()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var css = await response!.TextAsync();

            css.Should().Contain(".hdr", "CSS should define .hdr class");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Css_HdrClass_Failed");
            throw;
        }
    }

    [Fact]
    public async Task DashboardCss_ContainsSubClass()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var css = await response!.TextAsync();

            css.Should().Contain(".sub", "CSS should define .sub class");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Css_SubClass_Failed");
            throw;
        }
    }

    [Fact]
    public async Task DashboardCss_ContainsTimelineClasses()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var css = await response!.TextAsync();

            css.Should().Contain(".tl-area", "CSS should define .tl-area class");
            css.Should().Contain(".tl-svg-box", "CSS should define .tl-svg-box class");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Css_TimelineClasses_Failed");
            throw;
        }
    }

    [Fact]
    public async Task DashboardCss_ContainsHeatmapClasses()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var css = await response!.TextAsync();

            css.Should().Contain(".hm-wrap", "CSS should define .hm-wrap class");
            css.Should().Contain(".hm-grid", "CSS should define .hm-grid class");
            css.Should().Contain(".hm-cell", "CSS should define .hm-cell class");
            css.Should().Contain(".hm-title", "CSS should define .hm-title class");
            css.Should().Contain(".hm-corner", "CSS should define .hm-corner class");
            css.Should().Contain(".hm-col-hdr", "CSS should define .hm-col-hdr class");
            css.Should().Contain(".hm-row-hdr", "CSS should define .hm-row-hdr class");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Css_HeatmapClasses_Failed");
            throw;
        }
    }

    [Fact]
    public async Task DashboardCss_ContainsCategoryRowHeaders()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var css = await response!.TextAsync();

            css.Should().Contain(".ship-hdr", "CSS should define .ship-hdr for shipped row");
            css.Should().Contain(".prog-hdr", "CSS should define .prog-hdr for in-progress row");
            css.Should().Contain(".carry-hdr", "CSS should define .carry-hdr for carryover row");
            css.Should().Contain(".block-hdr", "CSS should define .block-hdr for blockers row");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Css_CategoryHeaders_Failed");
            throw;
        }
    }

    [Fact]
    public async Task DashboardCss_ContainsCategoryCellClasses()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var css = await response!.TextAsync();

            css.Should().Contain(".ship-cell", "CSS should define .ship-cell");
            css.Should().Contain(".prog-cell", "CSS should define .prog-cell");
            css.Should().Contain(".carry-cell", "CSS should define .carry-cell");
            css.Should().Contain(".block-cell", "CSS should define .block-cell");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Css_CategoryCells_Failed");
            throw;
        }
    }

    [Fact]
    public async Task DashboardCss_ContainsItemClass()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var css = await response!.TextAsync();

            css.Should().Contain(".it", "CSS should define .it class for work items");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Css_ItemClass_Failed");
            throw;
        }
    }

    [Fact]
    public async Task DashboardCss_DefinesBodyDimensions()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var css = await response!.TextAsync();

            css.Should().Contain("1920", "CSS should reference 1920px width");
            css.Should().Contain("1080", "CSS should reference 1080px height");
            css.Should().Contain("overflow", "CSS should control overflow");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Css_BodyDimensions_Failed");
            throw;
        }
    }

    [Fact]
    public async Task DashboardCss_DefinesFontFamily()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var css = await response!.TextAsync();

            css.Should().Contain("Segoe UI", "CSS should use Segoe UI font family");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Css_FontFamily_Failed");
            throw;
        }
    }

    [Fact]
    public async Task DashboardCss_ContainsCurrentMonthHighlight()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var css = await response!.TextAsync();

            css.Should().Contain("apr", "CSS should define current month (.apr) styles");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Css_CurrentMonthHighlight_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Body_HasOverflowHidden()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            var overflow = await page.Locator("body").EvaluateAsync<string>(
                "el => getComputedStyle(el).overflow");

            overflow.Should().Contain("hidden", "body should have overflow: hidden");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Body_OverflowHidden_Failed");
            throw;
        }
    }

    [Fact]
    public async Task Body_HasCorrectFontFamily()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            var fontFamily = await page.Locator("body").EvaluateAsync<string>(
                "el => getComputedStyle(el).fontFamily");

            // Should contain Segoe UI or fallback
            (fontFamily.Contains("Segoe UI") || fontFamily.Contains("Arial") || fontFamily.Contains("sans-serif"))
                .Should().BeTrue($"body font-family should include Segoe UI stack, got: {fontFamily}");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Body_FontFamily_Failed");
            throw;
        }
    }
}