using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// E2E tests verifying CSS styles match OriginalDesignConcept.html exactly.
/// Covers header, timeline area, heatmap grid, link colors, and section borders.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class CssDesignFidelityTests
{
    private readonly PlaywrightFixture _fixture;

    public CssDesignFidelityTests(PlaywrightFixture fixture) => _fixture = fixture;

    private async Task<DashboardPageObject> NavigateToDashboard()
    {
        var page = await _fixture.NewPageAsync();
        var po = new DashboardPageObject(page, _fixture.BaseUrl);
        await po.NavigateAsync();
        return po;
    }

    #region Header CSS

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_HasCorrectPadding()
    {
        var po = await NavigateToDashboard();
        try
        {
            var padding = await po.Header.EvaluateAsync<string>("el => getComputedStyle(el).padding");
            // Should be 12px 44px 10px (or equivalent computed form)
            Assert.Contains("44px", padding);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(po.Page, nameof(Header_HasCorrectPadding));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_HasBottomBorder()
    {
        var po = await NavigateToDashboard();
        try
        {
            var borderBottom = await po.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomStyle");
            Assert.Equal("solid", borderBottom);

            var borderColor = await po.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomColor");
            // #E0E0E0 = rgb(224, 224, 224)
            Assert.Contains("224", borderColor);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(po.Page, nameof(Header_HasBottomBorder));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_HasFlexLayout()
    {
        var po = await NavigateToDashboard();
        try
        {
            var display = await po.Header.EvaluateAsync<string>("el => getComputedStyle(el).display");
            Assert.Equal("flex", display);

            var justifyContent = await po.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).justifyContent");
            Assert.Equal("space-between", justifyContent);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(po.Page, nameof(Header_HasFlexLayout));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_TitleIs24pxBold()
    {
        var po = await NavigateToDashboard();
        try
        {
            var fontSize = await po.Title.EvaluateAsync<string>("el => getComputedStyle(el).fontSize");
            Assert.Equal("24px", fontSize);

            var fontWeight = await po.Title.EvaluateAsync<string>("el => getComputedStyle(el).fontWeight");
            Assert.Equal("700", fontWeight);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(po.Page, nameof(Header_TitleIs24pxBold));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_SubtitleIs12pxGray()
    {
        var po = await NavigateToDashboard();
        try
        {
            var fontSize = await po.Subtitle.EvaluateAsync<string>("el => getComputedStyle(el).fontSize");
            Assert.Equal("12px", fontSize);

            var color = await po.Subtitle.EvaluateAsync<string>("el => getComputedStyle(el).color");
            // #888 = rgb(136, 136, 136)
            Assert.Contains("136", color);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(po.Page, nameof(Header_SubtitleIs12pxGray));
            throw;
        }
    }

    #endregion

    #region Link Color

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Links_AreMicrosoftBlue()
    {
        var po = await NavigateToDashboard();
        try
        {
            var link = po.BacklogLink;
            if (await link.CountAsync() > 0)
            {
                var color = await link.EvaluateAsync<string>("el => getComputedStyle(el).color");
                // #0078D4 = rgb(0, 120, 212)
                Assert.Contains("0, 120, 212", color);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(po.Page, nameof(Links_AreMicrosoftBlue));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Links_HaveNoTextDecoration()
    {
        var po = await NavigateToDashboard();
        try
        {
            var link = po.BacklogLink;
            if (await link.CountAsync() > 0)
            {
                var textDecoration = await link.EvaluateAsync<string>(
                    "el => getComputedStyle(el).textDecorationLine");
                Assert.Equal("none", textDecoration);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(po.Page, nameof(Links_HaveNoTextDecoration));
            throw;
        }
    }

    #endregion

    #region Timeline Area CSS

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TimelineArea_HasHeight196px()
    {
        var po = await NavigateToDashboard();
        try
        {
            var height = await po.TimelineArea.EvaluateAsync<string>("el => getComputedStyle(el).height");
            Assert.Equal("196px", height);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(po.Page, nameof(TimelineArea_HasHeight196px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TimelineArea_HasFAFAFABackground()
    {
        var po = await NavigateToDashboard();
        try
        {
            var bg = await po.TimelineArea.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // #FAFAFA = rgb(250, 250, 250)
            Assert.Contains("250, 250, 250", bg);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(po.Page, nameof(TimelineArea_HasFAFAFABackground));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TimelineArea_HasFlexDisplay()
    {
        var po = await NavigateToDashboard();
        try
        {
            var display = await po.TimelineArea.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            Assert.Equal("flex", display);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(po.Page, nameof(TimelineArea_HasFlexDisplay));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task TimelineArea_HasBottomBorder()
    {
        var po = await NavigateToDashboard();
        try
        {
            var borderWidth = await po.TimelineArea.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomWidth");
            Assert.Equal("2px", borderWidth);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(po.Page, nameof(TimelineArea_HasBottomBorder));
            throw;
        }
    }

    #endregion

    #region Heatmap CSS

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task HeatmapWrap_HasFlexColumnLayout()
    {
        var po = await NavigateToDashboard();
        try
        {
            var display = await po.HeatmapWrap.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            var direction = await po.HeatmapWrap.EvaluateAsync<string>(
                "el => getComputedStyle(el).flexDirection");
            Assert.Equal("flex", display);
            Assert.Equal("column", direction);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(po.Page, nameof(HeatmapWrap_HasFlexColumnLayout));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task HeatmapTitle_IsUppercase()
    {
        var po = await NavigateToDashboard();
        try
        {
            if (await po.HeatmapTitle.CountAsync() > 0)
            {
                var textTransform = await po.HeatmapTitle.EvaluateAsync<string>(
                    "el => getComputedStyle(el).textTransform");
                Assert.Equal("uppercase", textTransform);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(po.Page, nameof(HeatmapTitle_IsUppercase));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task HeatmapGrid_UsesCssGrid()
    {
        var po = await NavigateToDashboard();
        try
        {
            if (await po.HeatmapGrid.CountAsync() > 0)
            {
                var display = await po.HeatmapGrid.EvaluateAsync<string>(
                    "el => getComputedStyle(el).display");
                Assert.Equal("grid", display);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(po.Page, nameof(HeatmapGrid_UsesCssGrid));
            throw;
        }
    }

    #endregion
}