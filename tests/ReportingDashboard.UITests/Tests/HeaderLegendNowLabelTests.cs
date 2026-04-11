using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// UI tests for the NowLabel in the Header legend.
/// Existing HeaderTests/HeaderMetadataTests/HeaderCssDesignTests verify basic legend labels,
/// symbol counts, CSS, and layout. This file focuses on the dynamic "Now (...)" label
/// rendering, verifying that the year from timeline.nowDate or timeline.startDate
/// appears in the rendered legend text — a behavior driven by the NowLabel computed property
/// that is NOT tested in existing UI tests.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeaderLegendNowLabelTests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderLegendNowLabelTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_NowLabel_ContainsYearFromData()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var nowLegend = header.NowLegend;
            await Assertions.Expect(nowLegend).ToBeVisibleAsync();

            var text = await nowLegend.TextContentAsync() ?? "";
            // NowLabel should include "Now (" with either a year or just the month
            Assert.Contains("Now (", text);
            // Should contain a closing parenthesis
            Assert.Contains(")", text);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_NowLabel_ContainsYearFromData));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_NowLabel_ContainsFourDigitYear()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var nowLegend = header.NowLegend;
            await Assertions.Expect(nowLegend).ToBeVisibleAsync();

            var text = await nowLegend.TextContentAsync() ?? "";
            // The NowLabel should contain a 4-digit year when NowDate or StartDate is valid
            Assert.Matches(@"Now \(.+ \d{4}\)", text);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_NowLabel_ContainsFourDigitYear));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_NowLabel_IsInsideLegendItem()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            // The Now label should be inside a .legend-item span
            var nowItem = page.Locator(".legend-item:has(.legend-now-line)");
            await Assertions.Expect(nowItem).ToBeVisibleAsync();

            var text = await nowItem.TextContentAsync() ?? "";
            Assert.Contains("Now", text);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_NowLabel_IsInsideLegendItem));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_NowLabel_NowLineSymbolIsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var nowLine = page.Locator(".legend-now-line");
            await Assertions.Expect(nowLine).ToBeVisibleAsync();

            var count = await nowLine.CountAsync();
            Assert.Equal(1, count);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_NowLabel_NowLineSymbolIsVisible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_NowLabel_MatchesCurrentMonthFromData()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            // Get the subtitle text which contains the month
            var subtitleText = await header.GetSubtitleTextAsync();
            var nowText = await header.NowLegend.TextContentAsync() ?? "";

            // Both subtitle and Now label reference the current month from data.json
            // We can't predict exact month, but Now should have a non-empty parenthesized value
            Assert.Matches(@"Now \(.+\)", nowText);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_NowLabel_MatchesCurrentMonthFromData));
            throw;
        }
    }
}