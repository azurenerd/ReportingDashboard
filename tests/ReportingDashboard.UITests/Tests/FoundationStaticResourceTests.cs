using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Tests for static file serving and CSS stylesheet validation from PR #539.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class FoundationStaticResourceTests
{
    private readonly PlaywrightFixture _fixture;

    public FoundationStaticResourceTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task CssFile_IsAccessible()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            Assert.NotNull(response);
            Assert.Equal(200, response!.Status);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(CssFile_IsAccessible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task CssFile_HasCorrectContentType()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var contentType = response!.Headers["content-type"];
            Assert.Contains("text/css", contentType);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(CssFile_HasCorrectContentType));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task CssFile_ContainsRequiredClasses()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var css = await response!.TextAsync();

            // Validate all required CSS classes from OriginalDesignConcept.html
            Assert.Contains(".hdr", css);
            Assert.Contains(".sub", css);
            Assert.Contains(".tl-area", css);
            Assert.Contains(".tl-svg-box", css);
            Assert.Contains(".hm-wrap", css);
            Assert.Contains(".hm-title", css);
            Assert.Contains(".hm-grid", css);
            Assert.Contains(".hm-corner", css);
            Assert.Contains(".hm-col-hdr", css);
            Assert.Contains(".hm-row-hdr", css);
            Assert.Contains(".hm-cell", css);
            Assert.Contains(".it", css);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(CssFile_ContainsRequiredClasses));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task CssFile_ContainsCategoryVariants()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var css = await response!.TextAsync();

            Assert.Contains(".ship-hdr", css);
            Assert.Contains(".ship-cell", css);
            Assert.Contains(".prog-hdr", css);
            Assert.Contains(".prog-cell", css);
            Assert.Contains(".carry-hdr", css);
            Assert.Contains(".carry-cell", css);
            Assert.Contains(".block-hdr", css);
            Assert.Contains(".block-cell", css);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(CssFile_ContainsCategoryVariants));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task CssFile_ContainsCurrentMonthHighlightClasses()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var css = await response!.TextAsync();

            Assert.Contains(".apr-hdr", css);
            Assert.Contains(".apr", css);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(CssFile_ContainsCurrentMonthHighlightClasses));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task CssFile_ContainsBodyDimensions()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var css = await response!.TextAsync();

            Assert.Contains("1920px", css);
            Assert.Contains("1080px", css);
            Assert.Contains("overflow", css);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(CssFile_ContainsBodyDimensions));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_IsAccessible()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/data.json");
            Assert.NotNull(response);
            Assert.Equal(200, response!.Status);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_IsAccessible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_ContainsRequiredFields()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/data.json");
            var json = await response!.TextAsync();

            Assert.Contains("\"title\"", json);
            Assert.Contains("\"subtitle\"", json);
            Assert.Contains("\"backlogLink\"", json);
            Assert.Contains("\"currentMonth\"", json);
            Assert.Contains("\"months\"", json);
            Assert.Contains("\"timeline\"", json);
            Assert.Contains("\"heatmap\"", json);
            Assert.Contains("\"tracks\"", json);
            Assert.Contains("\"shipped\"", json);
            Assert.Contains("\"inProgress\"", json);
            Assert.Contains("\"carryover\"", json);
            Assert.Contains("\"blockers\"", json);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_ContainsRequiredFields));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_Has3PlusTimelineTracks()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/data.json");
            var json = await response!.TextAsync();
            // Count occurrences of "name" within tracks to estimate track count
            var trackCount = System.Text.RegularExpressions.Regex.Matches(json, "\"name\"\\s*:").Count;
            // Need at least 3 tracks
            Assert.True(trackCount >= 3, $"Expected 3+ timeline tracks, found ~{trackCount}");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_Has3PlusTimelineTracks));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task NonExistentFile_Returns404()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/nonexistent.css");
            Assert.NotNull(response);
            Assert.Equal(404, response!.Status);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(NonExistentFile_Returns404));
            throw;
        }
    }
}