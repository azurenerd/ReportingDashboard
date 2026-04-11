using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Tests for the Header component from PR #539 aligned with actual source.
/// Header uses inline styles for legend (no .legend CSS class).
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class FoundationHeaderTests
{
    private readonly PlaywrightFixture _fixture;

    public FoundationHeaderTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_IsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            await Assertions.Expect(dashboard.Header).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_IsVisible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_DisplaysProjectTitle_24pxBold()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            await Assertions.Expect(dashboard.HeaderTitle).ToBeVisibleAsync();

            var titleText = await dashboard.HeaderTitle.TextContentAsync();
            Assert.False(string.IsNullOrWhiteSpace(titleText), "Title should not be empty");

            var fontSize = await dashboard.HeaderTitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            Assert.Equal("24px", fontSize);

            var fontWeight = await dashboard.HeaderTitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontWeight");
            Assert.True(fontWeight == "700" || fontWeight == "bold",
                $"Expected bold (700), got {fontWeight}");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_DisplaysProjectTitle_24pxBold));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_DisplaysSubtitle_12pxGray()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            await Assertions.Expect(dashboard.HeaderSubtitle).ToBeVisibleAsync();

            var subText = await dashboard.HeaderSubtitle.TextContentAsync();
            Assert.False(string.IsNullOrWhiteSpace(subText), "Subtitle should not be empty");

            var fontSize = await dashboard.HeaderSubtitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            Assert.Equal("12px", fontSize);

            var color = await dashboard.HeaderSubtitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");
            // #888 = rgb(136, 136, 136)
            Assert.Contains("136, 136, 136", color);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_DisplaysSubtitle_12pxGray));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_IsMicrosoftBlueAndOpensNewTab()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var linkCount = await dashboard.BacklogLink.CountAsync();
            Assert.True(linkCount > 0, "Expected backlog link to be present");

            var href = await dashboard.BacklogLink.GetAttributeAsync("href");
            Assert.False(string.IsNullOrWhiteSpace(href), "Backlog link should have an href");

            var target = await dashboard.BacklogLink.GetAttributeAsync("target");
            Assert.Equal("_blank", target);

            var text = await dashboard.BacklogLink.TextContentAsync();
            Assert.Contains("ADO Backlog", text ?? "");

            var color = await dashboard.BacklogLink.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");
            // #0078D4 = rgb(0, 120, 212)
            Assert.Contains("0, 120, 212", color);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_IsMicrosoftBlueAndOpensNewTab));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_BacklogLink_HasNoopenerAttribute()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var rel = await dashboard.BacklogLink.GetAttributeAsync("rel");
            Assert.Contains("noopener", rel ?? "");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_BacklogLink_HasNoopenerAttribute));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_HasBottomBorder()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var borderBottom = await dashboard.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomStyle");
            Assert.Equal("solid", borderBottom);

            var borderColor = await dashboard.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomColor");
            // #E0E0E0 = rgb(224, 224, 224)
            Assert.Contains("224, 224, 224", borderColor);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_HasBottomBorder));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_HasFlexJustifySpaceBetween()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var display = await dashboard.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            Assert.Equal("flex", display);

            var justify = await dashboard.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).justifyContent");
            Assert.Equal("space-between", justify);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_HasFlexJustifySpaceBetween));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_ContainsFourSymbolItems()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var legendText = await dashboard.LegendContainer.TextContentAsync() ?? "";

            Assert.Contains("PoC Milestone", legendText);
            Assert.Contains("Production Release", legendText);
            Assert.Contains("Checkpoint", legendText);
            Assert.Contains("Now", legendText);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_ContainsFourSymbolItems));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_HasGap22px()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var gap = await dashboard.LegendContainer.EvaluateAsync<string>(
                "el => getComputedStyle(el).gap");
            Assert.Equal("22px", gap);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_HasGap22px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_Has12pxFontSize()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var fontSize = await dashboard.LegendContainer.EvaluateAsync<string>(
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
    public async Task Header_Legend_PocDiamondIsGold()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            // PoC diamond: 12x12, rotated 45deg, #F4B400
            var pocDiamond = dashboard.LegendContainer
                .Locator("span span[style*='F4B400']");
            Assert.True(await pocDiamond.CountAsync() > 0,
                "Expected gold PoC diamond symbol (#F4B400)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_PocDiamondIsGold));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_ProductionDiamondIsGreen()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var prodDiamond = dashboard.LegendContainer
                .Locator("span span[style*='34A853']");
            Assert.True(await prodDiamond.CountAsync() > 0,
                "Expected green Production diamond symbol (#34A853)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_ProductionDiamondIsGreen));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_CheckpointIsGrayCircle()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            // Checkpoint: 8x8, border-radius 50%, #999
            var circle = dashboard.LegendContainer
                .Locator("span span[style*='border-radius']");
            Assert.True(await circle.CountAsync() > 0,
                "Expected gray circle checkpoint symbol");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_CheckpointIsGrayCircle));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_NowBarIsRed()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var nowBar = dashboard.LegendContainer
                .Locator("span span[style*='EA4335']");
            Assert.True(await nowBar.CountAsync() > 0,
                "Expected red NOW bar symbol (#EA4335)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_NowBarIsRed));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_NowShowsCurrentMonth()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new FoundationDashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();
            var legendText = await dashboard.LegendContainer.TextContentAsync() ?? "";
            // Should contain "Now (SomeMonth)" pattern
            Assert.Matches(@"Now\s*\(.+\)", legendText);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_NowShowsCurrentMonth));
            throw;
        }
    }
}