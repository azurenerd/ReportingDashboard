using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Tests static file serving for CSS and the Blazor SignalR script.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class StaticFilesUITests
{
    private readonly PlaywrightFixture _fixture;

    public StaticFilesUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task StaticFile_DashboardCss_Returns200()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");

            response.Should().NotBeNull();
            response!.Status.Should().Be(200);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "StaticFile_Css200");
            throw;
        }
    }

    [Fact]
    public async Task StaticFile_DashboardCss_ContentType_IsCss()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");

            var contentType = response!.Headers.GetValueOrDefault("content-type", "");
            contentType.Should().Contain("css");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "StaticFile_CssContentType");
            throw;
        }
    }

    [Fact]
    public async Task StaticFile_DashboardCss_IsNonEmpty()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/dashboard.css");
            var body = await response!.TextAsync();

            body.Should().NotBeNullOrWhiteSpace();
            body.Length.Should().BeGreaterThan(100, "CSS file should have substantial content");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "StaticFile_CssNonEmpty");
            throw;
        }
    }

    [Fact]
    public async Task StaticFile_NonExistentPath_Returns404()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var response = await page.GotoAsync($"{_fixture.BaseUrl}/css/nonexistent.css");

            response.Should().NotBeNull();
            response!.Status.Should().Be(404);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "StaticFile_404");
            throw;
        }
    }

    [Fact]
    public async Task BlazorScript_IsLoadedOnPage()
    {
        var page = await _fixture.NewPageAsync();
        var blazorScriptLoaded = false;

        page.Response += (_, response) =>
        {
            if (response.Url.Contains("blazor.server.js") && response.Status == 200)
                blazorScriptLoaded = true;
        };

        try
        {
            await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            blazorScriptLoaded.Should().BeTrue("blazor.server.js should be served successfully");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "BlazorScript_Loaded");
            throw;
        }
    }
}