using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// E2E tests verifying static file serving via browser requests.
/// Validates CSS and data.json are served with correct content types and content.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class StaticAssetServingTests
{
    private readonly PlaywrightFixture _fixture;

    public StaticAssetServingTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DashboardCss_IsAccessible()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css",
                new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            Assert.NotNull(response);
            Assert.True(response!.Ok, $"Expected 200 for dashboard.css, got {response.Status}");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DashboardCss_IsAccessible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DashboardCss_HasCssContentType()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            Assert.NotNull(response);

            var headers = response!.Headers;
            Assert.True(headers.ContainsKey("content-type"));
            Assert.Contains("text/css", headers["content-type"]);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DashboardCss_HasCssContentType));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DashboardCss_ContainsExpectedSelectors()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var content = await response!.TextAsync();

            Assert.Contains(".hdr", content);
            Assert.Contains(".tl-area", content);
            Assert.Contains(".hm-wrap", content);
            Assert.Contains(".hm-grid", content);
            Assert.Contains(".error-panel", content);
            Assert.Contains("1920px", content);
            Assert.Contains("1080px", content);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DashboardCss_ContainsExpectedSelectors));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DashboardCss_ContainsRowColorVariants()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var content = await response!.TextAsync();

            Assert.Contains(".ship-hdr", content);
            Assert.Contains(".ship-cell", content);
            Assert.Contains(".prog-hdr", content);
            Assert.Contains(".prog-cell", content);
            Assert.Contains(".carry-hdr", content);
            Assert.Contains(".carry-cell", content);
            Assert.Contains(".block-hdr", content);
            Assert.Contains(".block-cell", content);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DashboardCss_ContainsRowColorVariants));
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
            Assert.True(response!.Ok, $"Expected 200 for data.json, got {response.Status}");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_IsAccessible));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_HasJsonContentType()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/data.json");
            var headers = response!.Headers;
            Assert.True(headers.ContainsKey("content-type"));
            Assert.Contains("json", headers["content-type"]);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_HasJsonContentType));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task DataJson_ContainsExpectedStructure()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/data.json");
            var content = await response!.TextAsync();

            Assert.Contains("\"title\"", content);
            Assert.Contains("\"subtitle\"", content);
            Assert.Contains("\"timeline\"", content);
            Assert.Contains("\"heatmap\"", content);
            Assert.Contains("\"months\"", content);
            Assert.Contains("\"tracks\"", content);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(DataJson_ContainsExpectedStructure));
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