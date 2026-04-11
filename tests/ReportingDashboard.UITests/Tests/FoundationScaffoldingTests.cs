using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// E2E tests for PR #532: Project Foundation & Scaffolding.
/// Verifies HTML document structure, viewport, CSS loading, bare layout (no Blazor chrome),
/// and the 1920x1080 constraint.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class FoundationScaffoldingTests
{
    private readonly PlaywrightFixture _fixture;

    public FoundationScaffoldingTests(PlaywrightFixture fixture) => _fixture = fixture;

    #region HTML Document Structure

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Page_HasHtmlLangAttribute()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            var lang = await po.GetLangAttributeAsync();
            Assert.Equal("en", lang);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_HasHtmlLangAttribute));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Page_HasCorrectTitle()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            var title = await po.GetPageTitleAsync();
            Assert.Contains("Executive", title, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_HasCorrectTitle));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Page_HasDashboardCssLinked()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            var cssCount = await po.CssLink.CountAsync();
            Assert.True(cssCount > 0, "Expected <link> to css/dashboard.css");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_HasDashboardCssLinked));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Page_HasViewportMetaWith1920()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            var content = await po.ViewportMeta.GetAttributeAsync("content");
            Assert.Contains("1920", content ?? "");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_HasViewportMetaWith1920));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Page_HasBaseHrefTag()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            var href = await po.BaseHref.GetAttributeAsync("href");
            Assert.Equal("/", href);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_HasBaseHrefTag));
            throw;
        }
    }

    #endregion

    #region Bare Layout - No Default Blazor Chrome

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Page_HasNoNavSidebar()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            Assert.False(await po.HasNavSidebarAsync(),
                "Expected no <nav> sidebar element - layout should be bare");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_HasNoNavSidebar));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Page_HasNoTopRow()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            Assert.False(await po.HasTopRowAsync(),
                "Expected no .top-row element - default Blazor chrome should be removed");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_HasNoTopRow));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Page_HasNoFooter()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            Assert.False(await po.HasFooterAsync(),
                "Expected no <footer> element - layout should be bare");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_HasNoFooter));
            throw;
        }
    }

    #endregion

    #region Body Dimensions and CSS

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Body_HasWidth1920px()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            var width = await po.Body.EvaluateAsync<string>("el => getComputedStyle(el).width");
            Assert.Equal("1920px", width);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Body_HasWidth1920px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Body_HasHeight1080px()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            var height = await po.Body.EvaluateAsync<string>("el => getComputedStyle(el).height");
            Assert.Equal("1080px", height);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Body_HasHeight1080px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Body_HasOverflowHidden()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            var overflow = await po.Body.EvaluateAsync<string>("el => getComputedStyle(el).overflow");
            Assert.Equal("hidden", overflow);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Body_HasOverflowHidden));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Body_HasFlexColumnLayout()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            var display = await po.Body.EvaluateAsync<string>("el => getComputedStyle(el).display");
            var direction = await po.Body.EvaluateAsync<string>("el => getComputedStyle(el).flexDirection");
            Assert.Equal("flex", display);
            Assert.Equal("column", direction);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Body_HasFlexColumnLayout));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Body_HasWhiteBackground()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            var bg = await po.Body.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
            // rgb(255, 255, 255) is #FFFFFF
            Assert.Contains("255", bg);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Body_HasWhiteBackground));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Body_HasSegoeUIFont()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            var fontFamily = await po.Body.EvaluateAsync<string>("el => getComputedStyle(el).fontFamily");
            Assert.Contains("Segoe UI", fontFamily, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Body_HasSegoeUIFont));
            throw;
        }
    }

    #endregion

    #region Dashboard Sections Visible

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_ShowsHeaderSection()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            await Assertions.Expect(po.Header).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ShowsHeaderSection));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_ShowsTimelineArea()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            await Assertions.Expect(po.TimelineArea).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ShowsTimelineArea));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_ShowsHeatmapWrap()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            await Assertions.Expect(po.HeatmapWrap).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ShowsHeatmapWrap));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Dashboard_NoErrorPanelWithValidData()
    {
        var page = await _fixture.NewPageAsync();
        var po = new FoundationPageObject(page, _fixture.BaseUrl);

        try
        {
            await po.NavigateAsync();
            Assert.False(await po.IsErrorVisibleAsync(),
                "Expected no error panel when data.json is valid");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_NoErrorPanelWithValidData));
            throw;
        }
    }

    #endregion
}