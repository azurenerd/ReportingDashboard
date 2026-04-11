using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// UI tests for the Header and Metadata component (PR #533).
/// Tests the inline-styled Header.razor rendered via Components/Header.razor
/// including title, subtitle, backlog link, and legend symbols.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeaderMetadataTests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderMetadataTests(PlaywrightFixture fixture) => _fixture = fixture;

    #region Header Container

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_IsVisible_OnDashboardLoad()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            Assert.True(await header.IsHeaderVisibleAsync(),
                "Header container (.hdr) should be visible");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_IsVisible_OnDashboardLoad));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_HasFlexboxLayout()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var display = await header.HeaderContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            Assert.Equal("flex", display);

            var justifyContent = await header.HeaderContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).justifyContent");
            Assert.Equal("space-between", justifyContent);

            var alignItems = await header.HeaderContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).alignItems");
            Assert.Equal("center", alignItems);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_HasFlexboxLayout));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_HasBottomBorder()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var borderBottom = await header.HeaderContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomStyle");
            Assert.Equal("solid", borderBottom);

            var borderColor = await header.HeaderContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomColor");
            // #E0E0E0 = rgb(224, 224, 224)
            Assert.Contains("224", borderColor);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_HasBottomBorder));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_HasCorrectPadding()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var paddingLeft = await header.HeaderContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).paddingLeft");
            Assert.Equal("44px", paddingLeft);

            var paddingRight = await header.HeaderContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).paddingRight");
            Assert.Equal("44px", paddingRight);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_HasCorrectPadding));
            throw;
        }
    }

    #endregion

    #region Title

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Title_IsDisplayed()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            await Assertions.Expect(header.TitleH1).ToBeVisibleAsync();
            var titleText = await header.GetTitleTextAsync();
            Assert.False(string.IsNullOrWhiteSpace(titleText),
                "Title text should not be empty");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Title_IsDisplayed));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Title_Has24pxFontSize()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var fontSize = await header.TitleH1.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            Assert.Equal("24px", fontSize);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Title_Has24pxFontSize));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Title_HasBoldFontWeight()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var fontWeight = await header.TitleH1.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontWeight");
            Assert.Equal("700", fontWeight);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Title_HasBoldFontWeight));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Title_HasCorrectColor()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var color = await header.TitleH1.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");
            // #111 = rgb(17, 17, 17)
            Assert.Contains("17", color);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Title_HasCorrectColor));
            throw;
        }
    }

    #endregion

    #region Subtitle

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Subtitle_IsDisplayed()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            await Assertions.Expect(header.SubtitleDiv).ToBeVisibleAsync();
            var text = await header.GetSubtitleTextAsync();
            Assert.False(string.IsNullOrWhiteSpace(text),
                "Subtitle should not be empty");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Subtitle_IsDisplayed));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Subtitle_Has12pxFontSize()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var fontSize = await header.SubtitleDiv.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            Assert.Equal("12px", fontSize);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Subtitle_Has12pxFontSize));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Subtitle_HasGrayColor()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var color = await header.SubtitleDiv.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");
            // #888 = rgb(136, 136, 136)
            Assert.Contains("136", color);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Subtitle_HasGrayColor));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Subtitle_HasMarginTop2px()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var marginTop = await header.SubtitleDiv.EvaluateAsync<string>(
                "el => getComputedStyle(el).marginTop");
            Assert.Equal("2px", marginTop);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Subtitle_HasMarginTop2px));
            throw;
        }
    }

    #endregion

    #region Backlog Link

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_IsPresent()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            Assert.True(await header.HasBacklogLinkAsync(),
                "Backlog link should be present in header");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_IsPresent));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_HasNonEmptyHref()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var href = await header.GetBacklogHrefAsync();
            Assert.False(string.IsNullOrWhiteSpace(href),
                "Backlog link href should not be empty");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_HasNonEmptyHref));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_OpensInNewTab()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var target = await header.GetBacklogTargetAsync();
            Assert.Equal("_blank", target);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_OpensInNewTab));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_HasNoopenerRel()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var rel = await header.GetBacklogRelAsync();
            Assert.Contains("noopener", rel ?? "");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_HasNoopenerRel));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_ContainsADOBacklogText()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var text = await header.BacklogLink.TextContentAsync() ?? "";
            Assert.Contains("ADO Backlog", text);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_ContainsADOBacklogText));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_HasMicrosoftBlueColor()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var color = await header.BacklogLink.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");
            // #0078D4 = rgb(0, 120, 212)
            Assert.Contains("0, 120, 212", color);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_HasMicrosoftBlueColor));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_HasNoUnderline()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var textDecoration = await header.BacklogLink.EvaluateAsync<string>(
                "el => getComputedStyle(el).textDecorationLine");
            Assert.Equal("none", textDecoration);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_HasNoUnderline));
            throw;
        }
    }

    #endregion

    #region Legend Labels

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_HasFourItems()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var count = await header.GetLegendItemCountAsync();
            Assert.Equal(4, count);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_HasFourItems));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_ContainsPocMilestoneLabel()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            await Assertions.Expect(header.PocMilestoneLegend).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_ContainsPocMilestoneLabel));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_ContainsProductionReleaseLabel()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            await Assertions.Expect(header.ProductionReleaseLegend).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_ContainsProductionReleaseLabel));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_ContainsCheckpointLabel()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            await Assertions.Expect(header.CheckpointLegend).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_ContainsCheckpointLabel));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_ContainsNowWithCurrentMonth()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            await Assertions.Expect(header.NowLegend).ToBeVisibleAsync();
            var nowText = await header.NowLegend.TextContentAsync() ?? "";
            Assert.Contains("Now (", nowText);
            Assert.Contains(")", nowText);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_ContainsNowWithCurrentMonth));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_Has12pxFontSize()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var fontSize = await header.LegendContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            Assert.Equal("12px", fontSize);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_Has12pxFontSize));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_Has22pxGap()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var gap = await header.LegendContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).gap");
            Assert.Equal("22px", gap);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_Has22pxGap));
            throw;
        }
    }

    #endregion

    #region Legend Symbol Shapes and Colors

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_PocDiamond_HasGoldBackground()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            // Find the first span child inside the PoC Milestone legend item
            var symbol = header.PocMilestoneLegend.Locator("span").First;
            var bgColor = await symbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // #F4B400 = rgb(244, 180, 0)
            Assert.Contains("244, 180, 0", bgColor);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_PocDiamond_HasGoldBackground));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_PocDiamond_HasRotation()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var symbol = header.PocMilestoneLegend.Locator("span").First;
            var transform = await symbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).transform");
            // rotate(45deg) produces a matrix transform
            Assert.NotEqual("none", transform);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_PocDiamond_HasRotation));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_ProductionDiamond_HasGreenBackground()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var symbol = header.ProductionReleaseLegend.Locator("span").First;
            var bgColor = await symbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // #34A853 = rgb(52, 168, 83)
            Assert.Contains("52, 168, 83", bgColor);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_ProductionDiamond_HasGreenBackground));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_Checkpoint_HasGrayCircle()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var symbol = header.CheckpointLegend.Locator("span").First;

            var bgColor = await symbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // #999 = rgb(153, 153, 153)
            Assert.Contains("153, 153, 153", bgColor);

            var borderRadius = await symbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderRadius");
            Assert.Equal("50%", borderRadius);

            var width = await symbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).width");
            Assert.Equal("8px", width);

            var height = await symbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).height");
            Assert.Equal("8px", height);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_Checkpoint_HasGrayCircle));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_NowBar_HasRedBar()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var symbol = header.NowLegend.Locator("span").First;

            var bgColor = await symbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            // #EA4335 = rgb(234, 67, 53)
            Assert.Contains("234, 67, 53", bgColor);

            var width = await symbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).width");
            Assert.Equal("2px", width);

            var height = await symbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).height");
            Assert.Equal("14px", height);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_NowBar_HasRedBar));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_Diamonds_Are12x12()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var pocSymbol = header.PocMilestoneLegend.Locator("span").First;
            var pocWidth = await pocSymbol.EvaluateAsync<string>("el => getComputedStyle(el).width");
            var pocHeight = await pocSymbol.EvaluateAsync<string>("el => getComputedStyle(el).height");
            Assert.Equal("12px", pocWidth);
            Assert.Equal("12px", pocHeight);

            var prodSymbol = header.ProductionReleaseLegend.Locator("span").First;
            var prodWidth = await prodSymbol.EvaluateAsync<string>("el => getComputedStyle(el).width");
            var prodHeight = await prodSymbol.EvaluateAsync<string>("el => getComputedStyle(el).height");
            Assert.Equal("12px", prodWidth);
            Assert.Equal("12px", prodHeight);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_Diamonds_Are12x12));
            throw;
        }
    }

    #endregion

    #region Legend No SVG/Image

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_ContainsNoSvgOrImages()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var svgCount = await header.LegendContainer.Locator("svg").CountAsync();
            Assert.Equal(0, svgCount);

            var imgCount = await header.LegendContainer.Locator("img").CountAsync();
            Assert.Equal(0, imgCount);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_ContainsNoSvgOrImages));
            throw;
        }
    }

    #endregion

    #region Screenshot at 1920x1080

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Screenshot_CapturesCleanly()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            // Verify the header fits within the viewport without overflow
            var headerBox = await header.HeaderContainer.BoundingBoxAsync();
            Assert.NotNull(headerBox);
            Assert.True(headerBox!.X >= 0, "Header should not overflow left");
            Assert.True(headerBox.Width <= 1920, "Header should fit within 1920px viewport");
            Assert.True(headerBox.Y >= 0, "Header should be at top of page");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Screenshot_CapturesCleanly));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_LegendNotPushedOffScreen()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var legendBox = await header.LegendContainer.BoundingBoxAsync();
            Assert.NotNull(legendBox);
            // Legend right edge should be within viewport (with some tolerance for padding)
            Assert.True(legendBox!.X + legendBox.Width <= 1920,
                "Legend should not overflow the 1920px viewport");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_LegendNotPushedOffScreen));
            throw;
        }
    }

    #endregion

    #region Data-Driven Content

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Title_MatchesDataJson()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var titleText = await header.GetTitleTextAsync();
            // Title should contain meaningful text from data.json
            Assert.True(titleText.Length > 5,
                "Title should contain meaningful content from data.json");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Title_MatchesDataJson));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Subtitle_MatchesDataJson()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var subtitleText = await header.GetSubtitleTextAsync();
            Assert.True(subtitleText.Length > 5,
                "Subtitle should contain meaningful content from data.json");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Subtitle_MatchesDataJson));
            throw;
        }
    }

    #endregion
}