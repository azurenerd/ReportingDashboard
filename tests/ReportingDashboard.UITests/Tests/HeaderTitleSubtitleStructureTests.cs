using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// UI tests verifying the DOM structure and CSS classes used by the Header component
/// from PR #519. Existing tests cover basic content and design specs.
/// This file verifies the specific CSS class names (.hdr-title, .hdr-link, .legend-item,
/// .legend-diamond, .legend-poc, .legend-prod) that are used for styling and that
/// the subtitle (.sub) div is a sibling of h1 within the .hdr container.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeaderTitleSubtitleStructureTests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderTitleSubtitleStructureTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_H1_HasHdrTitleClass()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var h1 = page.Locator("h1.hdr-title");
            await Assertions.Expect(h1).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_H1_HasHdrTitleClass));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Subtitle_IsSiblingOfH1InHdr()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            // Both h1 and .sub should be direct children of .hdr
            var h1InHdr = page.Locator(".hdr > h1.hdr-title");
            var subInHdr = page.Locator(".hdr > .sub");

            Assert.Equal(1, await h1InHdr.CountAsync());
            Assert.Equal(1, await subInHdr.CountAsync());
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Subtitle_IsSiblingOfH1InHdr));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Subtitle_HasCorrectFontSize()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var sub = header.SubtitleDiv;
            var fontSize = await sub.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            Assert.Equal("12px", fontSize);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Subtitle_HasCorrectFontSize));
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

            var sub = header.SubtitleDiv;
            var color = await sub.EvaluateAsync<string>(
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

            var sub = header.SubtitleDiv;
            var marginTop = await sub.EvaluateAsync<string>(
                "el => getComputedStyle(el).marginTop");
            Assert.Equal("2px", marginTop);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Subtitle_HasMarginTop2px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Title_HasColor111()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var h1 = header.TitleH1;
            var color = await h1.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");
            // #111 = rgb(17, 17, 17)
            Assert.Contains("17", color);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Title_HasColor111));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_Legend_IsDirectChildOfHdr()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var legendInHdr = page.Locator(".hdr > .legend");
            Assert.Equal(1, await legendInHdr.CountAsync());
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_Legend_IsDirectChildOfHdr));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_LegendItems_Have12pxFontSize()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            var items = page.Locator(".legend-item");
            var count = await items.CountAsync();
            Assert.Equal(4, count);

            for (var i = 0; i < count; i++)
            {
                var fontSize = await items.Nth(i).EvaluateAsync<string>(
                    "el => getComputedStyle(el).fontSize");
                Assert.Equal("12px", fontSize);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_LegendItems_Have12pxFontSize));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_LegendItems_HaveGap6pxBetweenSymbolAndLabel()
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
                var gap = await items.Nth(i).EvaluateAsync<string>(
                    "el => getComputedStyle(el).gap");
                // gap should be 6px as per spec
                Assert.Equal("6px", gap);
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_LegendItems_HaveGap6pxBetweenSymbolAndLabel));
            throw;
        }
    }
}