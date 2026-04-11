using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Fixtures;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeaderLegendUITests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderLegendUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<(IPage page, HeaderPage header)> SetupAsync()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPage(page, _fixture.BaseUrl);
        await header.NavigateAsync();
        return (page, header);
    }

    // --- Header Visibility & Structure ---

    [Fact]
    public async Task Header_IsVisibleOnPageLoad()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var isVisible = await header.IsHeaderVisibleAsync();
            isVisible.Should().BeTrue();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_IsVisibleOnPageLoad));
            throw;
        }
    }

    [Fact]
    public async Task Header_ContainsHdrCssClass()
    {
        var (page, header) = await SetupAsync();
        try
        {
            await Assertions.Expect(header.Header).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_ContainsHdrCssClass));
            throw;
        }
    }

    // --- Title Tests ---

    [Fact]
    public async Task Header_DisplaysProjectTitle()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var titleText = await header.GetTitleTextAsync();
            titleText.Should().NotBeNullOrWhiteSpace("header should display a project title");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_DisplaysProjectTitle));
            throw;
        }
    }

    [Fact]
    public async Task Header_TitleHas24pxBoldFont()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var style = await header.GetElementStyleAsync(header.Title);
            style.Should().NotBeNull();
            style.Should().Contain("font-size:24px");
            style.Should().Contain("font-weight:700");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_TitleHas24pxBoldFont));
            throw;
        }
    }

    // --- Subtitle Tests ---

    [Fact]
    public async Task Header_DisplaysSubtitle()
    {
        var (page, header) = await SetupAsync();
        try
        {
            await Assertions.Expect(header.Subtitle).ToBeVisibleAsync();
            var subtitleText = await header.GetSubtitleTextAsync();
            subtitleText.Should().NotBeNullOrWhiteSpace("header should display a subtitle");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_DisplaysSubtitle));
            throw;
        }
    }

    // --- Backlog Link Tests ---

    [Fact]
    public async Task Header_DisplaysBacklogLink()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var linkCount = await header.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                await Assertions.Expect(header.BacklogLink).ToBeVisibleAsync();
                var href = await header.GetBacklogLinkHrefAsync();
                href.Should().NotBeNullOrWhiteSpace();

                var text = await header.BacklogLink.TextContentAsync();
                text.Should().Contain("ADO Backlog");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_DisplaysBacklogLink));
            throw;
        }
    }

    [Fact]
    public async Task Header_BacklogLinkOpensInNewTab()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var linkCount = await header.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                var target = await header.BacklogLink.GetAttributeAsync("target");
                target.Should().Be("_blank");

                var rel = await header.BacklogLink.GetAttributeAsync("rel");
                rel.Should().Contain("noopener");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLinkOpensInNewTab));
            throw;
        }
    }

    [Fact]
    public async Task Header_BacklogLinkHasMicrosoftBlueColor()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var linkCount = await header.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                var style = await header.GetElementStyleAsync(header.BacklogLink);
                style.Should().Contain("#0078D4");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLinkHasMicrosoftBlueColor));
            throw;
        }
    }

    // --- Legend Container Tests ---

    [Fact]
    public async Task Legend_IsVisibleOnPageLoad()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var isVisible = await header.IsLegendVisibleAsync();
            isVisible.Should().BeTrue("legend container should be visible in the header");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_IsVisibleOnPageLoad));
            throw;
        }
    }

    [Fact]
    public async Task Legend_ContainsExactlyFourItems()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var count = await header.GetLegendItemCountAsync();
            count.Should().Be(4, "legend should display exactly 4 symbol-label pairs");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_ContainsExactlyFourItems));
            throw;
        }
    }

    [Fact]
    public async Task Legend_HasFlexLayoutWith22pxGap()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var style = await header.GetElementStyleAsync(header.LegendContainer);
            style.Should().NotBeNull();
            style.Should().Contain("display:flex");
            style.Should().Contain("gap:22px");
            style.Should().Contain("align-items:center");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_HasFlexLayoutWith22pxGap));
            throw;
        }
    }

    // --- Legend Item Order & Labels ---

    [Fact]
    public async Task Legend_FirstItem_IsPoCMilestone()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var text = await header.GetLegendItemTextAsync(0);
            text.Should().Contain("PoC Milestone");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_FirstItem_IsPoCMilestone));
            throw;
        }
    }

    [Fact]
    public async Task Legend_SecondItem_IsProductionRelease()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var text = await header.GetLegendItemTextAsync(1);
            text.Should().Contain("Production Release");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_SecondItem_IsProductionRelease));
            throw;
        }
    }

    [Fact]
    public async Task Legend_ThirdItem_IsCheckpoint()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var text = await header.GetLegendItemTextAsync(2);
            text.Should().Contain("Checkpoint");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_ThirdItem_IsCheckpoint));
            throw;
        }
    }

    [Fact]
    public async Task Legend_FourthItem_ContainsNowLabel()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var text = await header.GetLegendItemTextAsync(3);
            text.Should().StartWith("Now");
            // Should be either "Now (Month Year)" or "Now (Month)" or "Now"
            text.Should().MatchRegex(@"Now(\s*\(.+\))?");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_FourthItem_ContainsNowLabel));
            throw;
        }
    }

    // --- PoC Milestone Symbol ---

    [Fact]
    public async Task PocSymbol_IsVisibleAndHasGoldBackground()
    {
        var (page, header) = await SetupAsync();
        try
        {
            await Assertions.Expect(header.PocSymbol).ToBeVisibleAsync();
            var style = await header.GetElementStyleAsync(header.PocSymbol);
            style.Should().Contain("#F4B400", "PoC symbol should have gold background");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(PocSymbol_IsVisibleAndHasGoldBackground));
            throw;
        }
    }

    [Fact]
    public async Task PocSymbol_HasDiamondShape()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var style = await header.GetElementStyleAsync(header.PocSymbol);
            style.Should().Contain("rotate(45deg)", "PoC symbol should be rotated to form a diamond");
            style.Should().Contain("width:12px");
            style.Should().Contain("height:12px");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(PocSymbol_HasDiamondShape));
            throw;
        }
    }

    // --- Production Release Symbol ---

    [Fact]
    public async Task ProductionSymbol_IsVisibleAndHasGreenBackground()
    {
        var (page, header) = await SetupAsync();
        try
        {
            await Assertions.Expect(header.ProductionSymbol).ToBeVisibleAsync();
            var style = await header.GetElementStyleAsync(header.ProductionSymbol);
            style.Should().Contain("#34A853", "Production symbol should have green background");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ProductionSymbol_IsVisibleAndHasGreenBackground));
            throw;
        }
    }

    [Fact]
    public async Task ProductionSymbol_HasDiamondShape()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var style = await header.GetElementStyleAsync(header.ProductionSymbol);
            style.Should().Contain("rotate(45deg)");
            style.Should().Contain("width:12px");
            style.Should().Contain("height:12px");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ProductionSymbol_HasDiamondShape));
            throw;
        }
    }

    // --- Checkpoint Symbol ---

    [Fact]
    public async Task CheckpointSymbol_IsVisibleAndHasGrayBackground()
    {
        var (page, header) = await SetupAsync();
        try
        {
            await Assertions.Expect(header.CheckpointSymbol).ToBeVisibleAsync();
            var style = await header.GetElementStyleAsync(header.CheckpointSymbol);
            style.Should().Contain("#999", "Checkpoint symbol should have gray background");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(CheckpointSymbol_IsVisibleAndHasGrayBackground));
            throw;
        }
    }

    [Fact]
    public async Task CheckpointSymbol_HasCircleShape()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var style = await header.GetElementStyleAsync(header.CheckpointSymbol);
            style.Should().Contain("border-radius:50%", "Checkpoint should be circular");
            style.Should().Contain("width:8px");
            style.Should().Contain("height:8px");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(CheckpointSymbol_HasCircleShape));
            throw;
        }
    }

    // --- Now Line Symbol ---

    [Fact]
    public async Task NowBarSymbol_IsVisibleAndHasRedBackground()
    {
        var (page, header) = await SetupAsync();
        try
        {
            await Assertions.Expect(header.NowBarSymbol).ToBeVisibleAsync();
            var style = await header.GetElementStyleAsync(header.NowBarSymbol);
            style.Should().Contain("#EA4335", "Now bar should have red background");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(NowBarSymbol_IsVisibleAndHasRedBackground));
            throw;
        }
    }

    [Fact]
    public async Task NowBarSymbol_HasVerticalBarShape()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var style = await header.GetElementStyleAsync(header.NowBarSymbol);
            style.Should().Contain("width:2px");
            style.Should().Contain("height:14px");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(NowBarSymbol_HasVerticalBarShape));
            throw;
        }
    }

    // --- Layout & Visual Fidelity ---

    [Fact]
    public async Task Header_TitleAndLegend_AreHorizontallyAligned()
    {
        var (page, header) = await SetupAsync();
        try
        {
            // The .hdr uses flexbox space-between, so title and legend
            // should be at opposite ends on the same horizontal axis
            var hdrStyle = await page.Locator(".hdr").EvaluateAsync<string>(
                "el => window.getComputedStyle(el).display");
            hdrStyle.Should().Be("flex", ".hdr should use flexbox layout");

            var justifyContent = await page.Locator(".hdr").EvaluateAsync<string>(
                "el => window.getComputedStyle(el).justifyContent");
            justifyContent.Should().Be("space-between", ".hdr should use space-between");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_TitleAndLegend_AreHorizontallyAligned));
            throw;
        }
    }

    [Fact]
    public async Task Header_HasBottomBorder()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var borderBottom = await page.Locator(".hdr").EvaluateAsync<string>(
                "el => window.getComputedStyle(el).borderBottomStyle");
            borderBottom.Should().Be("solid", ".hdr should have a solid bottom border");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_HasBottomBorder));
            throw;
        }
    }

    [Fact]
    public async Task Legend_AllItemsAreInlineHorizontal()
    {
        var (page, header) = await SetupAsync();
        try
        {
            // All 4 legend items should have similar Y positions (same row)
            var count = await header.LegendItems.CountAsync();
            count.Should().Be(4);

            var boundingBoxes = new List<BoundingBoxResult>();
            for (var i = 0; i < count; i++)
            {
                var box = await header.LegendItems.Nth(i).BoundingBoxAsync();
                box.Should().NotBeNull($"legend item {i} should have a bounding box");
                boundingBoxes.Add(new BoundingBoxResult(box!.Y, box.Height));
            }

            // All items should be at roughly the same Y coordinate (within 5px tolerance)
            var referenceY = boundingBoxes[0].Y;
            foreach (var box in boundingBoxes)
            {
                Math.Abs(box.Y - referenceY).Should().BeLessThan(5,
                    "all legend items should be vertically aligned in a single row");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_AllItemsAreInlineHorizontal));
            throw;
        }
    }

    [Fact]
    public async Task Legend_LabelFontSize_Is12px()
    {
        var (page, header) = await SetupAsync();
        try
        {
            // Check computed font-size of the label spans inside legend items
            for (var i = 0; i < 4; i++)
            {
                var labelSelector = $".hdr div[style*='gap:22px'] > span:nth-child({i + 1}) span[style*='font-size:12px']";
                var fontSize = await page.Locator(labelSelector).First.EvaluateAsync<string>(
                    "el => window.getComputedStyle(el).fontSize");
                fontSize.Should().Be("12px", $"legend label {i} should have 12px font size");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_LabelFontSize_Is12px));
            throw;
        }
    }

    [Fact]
    public async Task Legend_SymbolsAreNotClipped()
    {
        var (page, header) = await SetupAsync();
        try
        {
            // Verify each symbol has a non-zero bounding box (not clipped away)
            var pocBox = await header.PocSymbol.BoundingBoxAsync();
            pocBox.Should().NotBeNull();
            pocBox!.Width.Should().BeGreaterThan(0);
            pocBox.Height.Should().BeGreaterThan(0);

            var prodBox = await header.ProductionSymbol.BoundingBoxAsync();
            prodBox.Should().NotBeNull();
            prodBox!.Width.Should().BeGreaterThan(0);

            var checkBox = await header.CheckpointSymbol.BoundingBoxAsync();
            checkBox.Should().NotBeNull();
            checkBox!.Width.Should().BeGreaterThan(0);

            var nowBox = await header.NowBarSymbol.BoundingBoxAsync();
            nowBox.Should().NotBeNull();
            nowBox!.Width.Should().BeGreaterThan(0);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_SymbolsAreNotClipped));
            throw;
        }
    }

    // --- Now Label Dynamic Content ---

    [Fact]
    public async Task NowLabel_DisplaysMonthAndYearOrFallback()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var nowText = await header.GetLegendItemTextAsync(3);

            // The Now label should be one of:
            // "Now (April 2026)" - from parsed NowDate
            // "Now (Apr)" - from CurrentMonth fallback
            // "Now" - bare fallback
            nowText.Should().StartWith("Now");

            // If it has parentheses, validate the format
            if (nowText.Contains('('))
            {
                nowText.Should().MatchRegex(@"Now \(.+\)");
                nowText.Should().Contain(")");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(NowLabel_DisplaysMonthAndYearOrFallback));
            throw;
        }
    }

    // --- Full Page Screenshot for Visual Comparison ---

    [Fact]
    public async Task Header_VisualSnapshot_ForManualComparison()
    {
        var (page, header) = await SetupAsync();

        // Always capture a screenshot for visual comparison against OriginalDesignConcept.html
        await _fixture.CaptureScreenshotAsync(page, "Header_VisualSnapshot");

        // Basic assertion that the page loaded
        await Assertions.Expect(header.Header).ToBeVisibleAsync();
        await Assertions.Expect(header.LegendContainer).ToBeVisibleAsync();
    }

    // --- Header Responsiveness at 1920x1080 ---

    [Fact]
    public async Task Header_At1920x1080_LegendDoesNotWrap()
    {
        var (page, header) = await SetupAsync();
        try
        {
            // At full 1920px width, the legend container height should be small
            // (single row, not wrapped). A wrapped layout would be significantly taller.
            var legendBox = await header.LegendContainer.BoundingBoxAsync();
            legendBox.Should().NotBeNull();

            // A single-row flexbox of small items should be under 40px tall
            legendBox!.Height.Should().BeLessThan(40,
                "legend should render as a single horizontal row at 1920px width");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_At1920x1080_LegendDoesNotWrap));
            throw;
        }
    }

    [Fact]
    public async Task Header_LegendIsRightAligned()
    {
        var (page, header) = await SetupAsync();
        try
        {
            var hdrBox = await header.Header.BoundingBoxAsync();
            var legendBox = await header.LegendContainer.BoundingBoxAsync();

            hdrBox.Should().NotBeNull();
            legendBox.Should().NotBeNull();

            // Legend right edge should be near the header right edge (within padding)
            var legendRight = legendBox!.X + legendBox.Width;
            var hdrRight = hdrBox!.X + hdrBox.Width;

            (hdrRight - legendRight).Should().BeLessThan(60,
                "legend should be positioned on the right side of the header (within padding)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_LegendIsRightAligned));
            throw;
        }
    }

    private record BoundingBoxResult(float Y, float Height);
}