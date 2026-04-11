using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeaderLegendTests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderLegendTests(PlaywrightFixture fixture) => _fixture = fixture;

    private async Task<(IPage page, HeaderLegendPageObject po)> SetupAsync()
    {
        var page = await _fixture.NewPageAsync();
        var po = new HeaderLegendPageObject(page, _fixture.BaseUrl);
        await po.NavigateAsync();
        return (page, po);
    }

    #region Legend Container Visibility & Structure

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Legend_IsVisibleInHeader()
    {
        var (page, po) = await SetupAsync();
        try
        {
            await Assertions.Expect(po.Header).ToBeVisibleAsync();
            await Assertions.Expect(po.LegendContainer).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_IsVisibleInHeader));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Legend_ContainsExactlyFourItems()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var count = await po.LegendItems.CountAsync();
            count.Should().Be(4, "legend must have PoC, Production, Checkpoint, and Now items");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_ContainsExactlyFourItems));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Legend_HasFlexDisplayWith22pxGap()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var display = await po.LegendContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            display.Should().Be("flex");

            var gap = await po.LegendContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).gap");
            gap.Should().Contain("22px");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_HasFlexDisplayWith22pxGap));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Legend_FontSizeIs12px()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var fontSize = await po.LegendContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            fontSize.Should().Be("12px");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_FontSizeIs12px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Legend_AlignItemsCenter()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var alignItems = await po.LegendContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).alignItems");
            alignItems.Should().Be("center");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_AlignItemsCenter));
            throw;
        }
    }

    #endregion

    #region Legend Labels

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Legend_PocMilestone_HasCorrectLabel()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var text = await po.PocMilestoneItem.TextContentAsync();
            text.Should().Contain("PoC Milestone");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_PocMilestone_HasCorrectLabel));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Legend_ProductionRelease_HasCorrectLabel()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var text = await po.ProductionReleaseItem.TextContentAsync();
            text.Should().Contain("Production Release");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_ProductionRelease_HasCorrectLabel));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Legend_Checkpoint_HasCorrectLabel()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var text = await po.CheckpointItem.TextContentAsync();
            text.Should().Contain("Checkpoint");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_Checkpoint_HasCorrectLabel));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Legend_NowLine_ContainsNowAndCurrentMonth()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var text = await po.NowLineItem.TextContentAsync();
            text.Should().Contain("Now (");
            text.Should().Contain(")");
            text.Should().MatchRegex(@"Now \(.+\)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_NowLine_ContainsNowAndCurrentMonth));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Legend_AllFourLabelsPresent()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var legendText = await po.LegendContainer.TextContentAsync() ?? "";
            legendText.Should().Contain("PoC Milestone");
            legendText.Should().Contain("Production Release");
            legendText.Should().Contain("Checkpoint");
            legendText.Should().Contain("Now");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_AllFourLabelsPresent));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Legend_LabelsAreInCorrectOrder()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var items = po.LegendItems;
            var count = await items.CountAsync();
            count.Should().Be(4);

            var text0 = await items.Nth(0).TextContentAsync();
            var text1 = await items.Nth(1).TextContentAsync();
            var text2 = await items.Nth(2).TextContentAsync();
            var text3 = await items.Nth(3).TextContentAsync();

            text0.Should().Contain("PoC Milestone");
            text1.Should().Contain("Production Release");
            text2.Should().Contain("Checkpoint");
            text3.Should().Contain("Now");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_LabelsAreInCorrectOrder));
            throw;
        }
    }

    #endregion

    #region PoC Milestone Symbol

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task PocDiamond_HasGoldBackground()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var bg = await po.PocDiamondSymbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            bg.Should().Contain("244").And.Contain("180").And.Contain("0");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(PocDiamond_HasGoldBackground));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task PocDiamond_Is12x12px()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var width = await po.PocDiamondSymbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).width");
            var height = await po.PocDiamondSymbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).height");
            width.Should().Be("12px");
            height.Should().Be("12px");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(PocDiamond_Is12x12px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task PocDiamond_IsRotated45Degrees()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var transform = await po.PocDiamondSymbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).transform");
            transform.Should().NotBe("none", "diamond should be rotated 45 degrees");
            transform.Should().Contain("matrix");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(PocDiamond_IsRotated45Degrees));
            throw;
        }
    }

    #endregion

    #region Production Release Symbol

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task ProductionDiamond_HasGreenBackground()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var bg = await po.ProductionDiamondSymbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            bg.Should().Contain("52").And.Contain("168").And.Contain("83");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ProductionDiamond_HasGreenBackground));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task ProductionDiamond_Is12x12pxAndRotated()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var width = await po.ProductionDiamondSymbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).width");
            var height = await po.ProductionDiamondSymbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).height");
            var transform = await po.ProductionDiamondSymbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).transform");

            width.Should().Be("12px");
            height.Should().Be("12px");
            transform.Should().NotBe("none");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ProductionDiamond_Is12x12pxAndRotated));
            throw;
        }
    }

    #endregion

    #region Checkpoint Symbol

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task CheckpointCircle_HasGrayBackground()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var bg = await po.CheckpointCircleSymbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            bg.Should().Contain("153");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(CheckpointCircle_HasGrayBackground));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task CheckpointCircle_Is8x8pxWithBorderRadius()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var width = await po.CheckpointCircleSymbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).width");
            var height = await po.CheckpointCircleSymbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).height");
            var borderRadius = await po.CheckpointCircleSymbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderRadius");

            width.Should().Be("8px");
            height.Should().Be("8px");
            borderRadius.Should().Be("50%");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(CheckpointCircle_Is8x8pxWithBorderRadius));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task CheckpointCircle_IsNotRotated()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var transform = await po.CheckpointCircleSymbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).transform");
            transform.Should().Be("none");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(CheckpointCircle_IsNotRotated));
            throw;
        }
    }

    #endregion

    #region Now Line Symbol

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task NowBar_HasRedBackground()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var bg = await po.NowBarSymbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");
            bg.Should().Contain("234").And.Contain("67").And.Contain("53");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(NowBar_HasRedBackground));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task NowBar_Is2x14px()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var width = await po.NowBarSymbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).width");
            var height = await po.NowBarSymbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).height");

            width.Should().Be("2px");
            height.Should().Be("14px");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(NowBar_Is2x14px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task NowBar_IsNotRotated()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var transform = await po.NowBarSymbol.EvaluateAsync<string>(
                "el => getComputedStyle(el).transform");
            transform.Should().Be("none");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(NowBar_IsNotRotated));
            throw;
        }
    }

    #endregion

    #region Header Layout: Title + Legend Coexistence

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_TitleAndLegend_BothVisible()
    {
        var (page, po) = await SetupAsync();
        try
        {
            await Assertions.Expect(po.Title).ToBeVisibleAsync();
            await Assertions.Expect(po.LegendContainer).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_TitleAndLegend_BothVisible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_UsesSpaceBetweenLayout()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var justifyContent = await po.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).justifyContent");
            justifyContent.Should().Be("space-between");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_UsesSpaceBetweenLayout));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_LegendIsToRightOfTitle()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var titleBox = await po.Title.BoundingBoxAsync();
            var legendBox = await po.LegendContainer.BoundingBoxAsync();

            titleBox.Should().NotBeNull();
            legendBox.Should().NotBeNull();

            ((double)legendBox!.X).Should().BeGreaterThan((double)(titleBox!.X + titleBox.Width * 0.5f),
                "legend should be positioned to the right of the title area");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_LegendIsToRightOfTitle));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_NoHorizontalScrollbarAt1920px()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var hasHScrollbar = await page.EvaluateAsync<bool>(
                "() => document.documentElement.scrollWidth > document.documentElement.clientWidth");
            hasHScrollbar.Should().BeFalse("no horizontal scrollbar should appear at 1920px viewport");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_NoHorizontalScrollbarAt1920px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_LegendDoesNotOverlapTitle()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var titleBox = await po.Title.BoundingBoxAsync();
            var legendBox = await po.LegendContainer.BoundingBoxAsync();

            titleBox.Should().NotBeNull();
            legendBox.Should().NotBeNull();

            var titleRight = (double)(titleBox!.X + titleBox.Width);
            ((double)legendBox!.X).Should().BeGreaterOrEqualTo(titleRight - 5,
                "legend should not overlap with title (allowing 5px tolerance)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_LegendDoesNotOverlapTitle));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_HasBottomBorder()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var borderBottom = await po.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottom");
            borderBottom.Should().Contain("1px").And.Contain("solid");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_HasBottomBorder));
            throw;
        }
    }

    #endregion

    #region Legend Item Internal Layout

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task LegendItems_EachHasFlexDisplay()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var count = await po.LegendItems.CountAsync();
            for (var i = 0; i < count; i++)
            {
                var display = await po.LegendItems.Nth(i).EvaluateAsync<string>(
                    "el => getComputedStyle(el).display");
                display.Should().Contain("flex",
                    $"legend item {i} should use flex layout");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(LegendItems_EachHasFlexDisplay));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task LegendItems_EachHasAlignItemsCenter()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var count = await po.LegendItems.CountAsync();
            for (var i = 0; i < count; i++)
            {
                var alignItems = await po.LegendItems.Nth(i).EvaluateAsync<string>(
                    "el => getComputedStyle(el).alignItems");
                alignItems.Should().Be("center",
                    $"legend item {i} should center-align its symbol and label");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(LegendItems_EachHasAlignItemsCenter));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task LegendItems_EachHasGapBetweenSymbolAndLabel()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var count = await po.LegendItems.CountAsync();
            for (var i = 0; i < count; i++)
            {
                var gap = await po.LegendItems.Nth(i).EvaluateAsync<string>(
                    "el => getComputedStyle(el).gap");
                gap.Should().MatchRegex(@"\d+px",
                    $"legend item {i} should have a gap between symbol and label");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(LegendItems_EachHasGapBetweenSymbolAndLabel));
            throw;
        }
    }

    #endregion

    #region Diamond Visual Verification (not clipped)

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task PocDiamond_IsVisuallyRenderedAsDiamond()
    {
        var (page, po) = await SetupAsync();
        try
        {
            await Assertions.Expect(po.PocDiamondSymbol).ToBeVisibleAsync();

            var box = await po.PocDiamondSymbol.BoundingBoxAsync();
            box.Should().NotBeNull();
            ((double)box!.Width).Should().BeGreaterThan(0);
            ((double)box.Height).Should().BeGreaterThan(0);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(PocDiamond_IsVisuallyRenderedAsDiamond));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task ProductionDiamond_IsVisuallyRenderedAsDiamond()
    {
        var (page, po) = await SetupAsync();
        try
        {
            await Assertions.Expect(po.ProductionDiamondSymbol).ToBeVisibleAsync();

            var box = await po.ProductionDiamondSymbol.BoundingBoxAsync();
            box.Should().NotBeNull();
            ((double)box!.Width).Should().BeGreaterThan(0);
            ((double)box.Height).Should().BeGreaterThan(0);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ProductionDiamond_IsVisuallyRenderedAsDiamond));
            throw;
        }
    }

    #endregion

    #region Screenshot Capture for Visual QA

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Legend_CaptureScreenshotForVisualQA()
    {
        var (page, po) = await SetupAsync();
        try
        {
            await Assertions.Expect(po.Header).ToBeVisibleAsync();

            var dir = Path.Combine(AppContext.BaseDirectory, "screenshots");
            Directory.CreateDirectory(dir);
            var headerElement = po.Header;
            await headerElement.ScreenshotAsync(new LocatorScreenshotOptions
            {
                Path = Path.Combine(dir, $"header_legend_visual_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png")
            });
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_CaptureScreenshotForVisualQA));
            throw;
        }
    }

    #endregion

    #region Subtitle Visibility

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_SubtitleIsVisible()
    {
        var (page, po) = await SetupAsync();
        try
        {
            await Assertions.Expect(po.Subtitle).ToBeVisibleAsync();
            var text = await po.Subtitle.TextContentAsync();
            text.Should().NotBeNullOrWhiteSpace();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_SubtitleIsVisible));
            throw;
        }
    }

    #endregion

    #region Backlog Link with Legend

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLinkAndLegend_BothPresent()
    {
        var (page, po) = await SetupAsync();
        try
        {
            var linkCount = await po.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                await Assertions.Expect(po.BacklogLink).ToBeVisibleAsync();
                var href = await po.BacklogLink.GetAttributeAsync("href");
                href.Should().NotBeNullOrWhiteSpace();
            }

            await Assertions.Expect(po.LegendContainer).ToBeVisibleAsync();
            var legendText = await po.LegendContainer.TextContentAsync();
            legendText.Should().Contain("PoC Milestone");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLinkAndLegend_BothPresent));
            throw;
        }
    }

    #endregion
}