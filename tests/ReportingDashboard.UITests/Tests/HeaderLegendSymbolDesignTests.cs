using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// UI tests for the legend symbol CSS classes and visual fidelity in Header.razor.
/// Existing HeaderCssDesignTests cover .hdr layout, font sizes, padding, margins.
/// Existing HeaderTests cover symbol count via DashboardPageObject locators.
/// This file covers the CSS class-based legend symbols (.legend-diamond, .legend-circle,
/// .legend-now-line) that the PR #519 Header.razor uses, verifying their presence,
/// visibility, and CSS-driven rendering properties.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeaderLegendSymbolDesignTests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderLegendSymbolDesignTests(PlaywrightFixture fixture) => _fixture = fixture;

    #region Legend Container

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_LegendContainer_IsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var legend = page.Locator(".legend");
            await Assertions.Expect(legend).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_LegendContainer_IsVisible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_LegendContainer_HasFlexDisplay()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var legend = page.Locator(".legend");
            var display = await legend.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            Assert.Equal("flex", display);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_LegendContainer_HasFlexDisplay));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_LegendContainer_HasGap22px()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var legend = page.Locator(".legend");
            var gap = await legend.EvaluateAsync<string>(
                "el => getComputedStyle(el).gap");
            Assert.Equal("22px", gap);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_LegendContainer_HasGap22px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_LegendContainer_HasAlignItemsCenter()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var legend = page.Locator(".legend");
            var alignItems = await legend.EvaluateAsync<string>(
                "el => getComputedStyle(el).alignItems");
            Assert.Equal("center", alignItems);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_LegendContainer_HasAlignItemsCenter));
            throw;
        }
    }

    #endregion

    #region Legend Items CSS Classes

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_LegendItems_HaveCorrectCount()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var items = page.Locator(".legend-item");
            var count = await items.CountAsync();
            Assert.Equal(4, count);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_LegendItems_HaveCorrectCount));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_LegendItems_AreAllVisible()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var items = page.Locator(".legend-item");
            var count = await items.CountAsync();
            for (var i = 0; i < count; i++)
            {
                await Assertions.Expect(items.Nth(i)).ToBeVisibleAsync();
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_LegendItems_AreAllVisible));
            throw;
        }
    }

    #endregion

    #region PoC Diamond Symbol

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_PocDiamond_IsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var poc = page.Locator(".legend-diamond.legend-poc");
            await Assertions.Expect(poc).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_PocDiamond_IsVisible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_PocDiamond_HasGoldBackground()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var poc = page.Locator(".legend-diamond.legend-poc");
            var bg = await poc.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // #F4B400 = rgb(244, 180, 0)
            Assert.Contains("244", bg);
            Assert.Contains("180", bg);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_PocDiamond_HasGoldBackground));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_PocDiamond_HasRotation()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var poc = page.Locator(".legend-diamond.legend-poc");
            var transform = await poc.EvaluateAsync<string>(
                "el => getComputedStyle(el).transform");
            // rotate(45deg) produces a matrix transform
            Assert.NotEqual("none", transform);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_PocDiamond_HasRotation));
            throw;
        }
    }

    #endregion

    #region Production Diamond Symbol

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_ProdDiamond_IsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var prod = page.Locator(".legend-diamond.legend-prod");
            await Assertions.Expect(prod).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_ProdDiamond_IsVisible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_ProdDiamond_HasGreenBackground()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var prod = page.Locator(".legend-diamond.legend-prod");
            var bg = await prod.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // #34A853 = rgb(52, 168, 83)
            Assert.Contains("52", bg);
            Assert.Contains("168", bg);
            Assert.Contains("83", bg);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_ProdDiamond_HasGreenBackground));
            throw;
        }
    }

    #endregion

    #region Checkpoint Circle Symbol

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_CheckpointCircle_IsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var circle = page.Locator(".legend-circle");
            await Assertions.Expect(circle).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_CheckpointCircle_IsVisible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_CheckpointCircle_HasGrayBackground()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var circle = page.Locator(".legend-circle");
            var bg = await circle.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // #999 = rgb(153, 153, 153)
            Assert.Contains("153", bg);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_CheckpointCircle_HasGrayBackground));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_CheckpointCircle_HasBorderRadius50Percent()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var circle = page.Locator(".legend-circle");
            var borderRadius = await circle.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderRadius");
            Assert.Equal("50%", borderRadius);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_CheckpointCircle_HasBorderRadius50Percent));
            throw;
        }
    }

    #endregion

    #region Now Line Symbol

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_NowLine_IsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var nowLine = page.Locator(".legend-now-line");
            await Assertions.Expect(nowLine).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_NowLine_IsVisible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_NowLine_HasRedBackground()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var nowLine = page.Locator(".legend-now-line");
            var bg = await nowLine.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // #EA4335 = rgb(234, 67, 53)
            Assert.Contains("234", bg);
            Assert.Contains("67", bg);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_NowLine_HasRedBackground));
            throw;
        }
    }

    #endregion

    #region Legend Position Relative to Header

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_IsRightAligned()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var hdrBox = await header.HeaderContainer.BoundingBoxAsync();
            var legendBox = await page.Locator(".legend").BoundingBoxAsync();

            Assert.NotNull(hdrBox);
            Assert.NotNull(legendBox);

            // Legend should be on the right side of the header
            // Its right edge should be near the header's right edge
            var legendRight = legendBox!.X + legendBox.Width;
            var hdrRight = hdrBox!.X + hdrBox.Width;
            Assert.True(legendRight <= hdrRight,
                "Legend should not overflow the header container");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_IsRightAligned));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_IsOnSameRowAsTitle()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var titleBox = await header.TitleH1.BoundingBoxAsync();
            var legendBox = await page.Locator(".legend").BoundingBoxAsync();

            Assert.NotNull(titleBox);
            Assert.NotNull(legendBox);

            // Legend and title should overlap vertically (same row in flex container)
            var titleBottom = titleBox!.Y + titleBox.Height;
            var legendBottom = legendBox!.Y + legendBox.Height;

            // They should share vertical space (both within the .hdr flex container)
            Assert.True(legendBox.Y < titleBottom,
                "Legend should share vertical space with the title row");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_IsOnSameRowAsTitle));
            throw;
        }
    }

    #endregion
}