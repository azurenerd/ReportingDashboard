using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// UI tests for the error state rendered by the root Components/ErrorPanel.razor.
/// Uses the `Message` parameter (not `ErrorMessage` from the Sections version).
/// Requires a server started with invalid or missing data.json.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class FinalIntegrationErrorStateTests
{
    private readonly PlaywrightFixture _fixture;

    public FinalIntegrationErrorStateTests(PlaywrightFixture fixture) => _fixture = fixture;

    private string ErrorBaseUrl =>
        Environment.GetEnvironmentVariable("BASE_URL_ERROR") ?? _fixture.BaseUrl;

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorState_ShowsErrorPanel_NotDashboard()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(ErrorBaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });

        try
        {
            var errorPanel = page.Locator(".error-panel");
            Assert.True(await errorPanel.CountAsync() > 0,
                "Error panel should be visible when data.json is invalid");

            var dashboardDiv = page.Locator(".dashboard");
            Assert.Equal(0, await dashboardDiv.CountAsync());
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorState_ShowsErrorPanel_NotDashboard));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorState_ShowsWarningEmoji()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(ErrorBaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });

        try
        {
            var errorContent = page.Locator(".error-content");
            await Assertions.Expect(errorContent).ToBeVisibleAsync();

            var text = await errorContent.TextContentAsync() ?? "";
            // The error panel shows a "?" warning emoji at 48px
            Assert.Contains("?", text);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorState_ShowsWarningEmoji));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorState_ShowsMainErrorTitle()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(ErrorBaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });

        try
        {
            var errorContent = page.Locator(".error-content");
            var text = await errorContent.TextContentAsync() ?? "";
            Assert.Contains("Dashboard data could not be loaded", text);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorState_ShowsMainErrorTitle));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorState_ShowsDetailedErrorMessage()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(ErrorBaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });

        try
        {
            var errorContent = page.Locator(".error-content");
            var text = await errorContent.TextContentAsync() ?? "";
            // Should contain specifics about why loading failed
            Assert.False(string.IsNullOrWhiteSpace(text), "Error panel should have content");
            Assert.True(text.Length > 50, "Error panel should have detailed message");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorState_ShowsDetailedErrorMessage));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorState_ShowsHelpText()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(ErrorBaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });

        try
        {
            var errorContent = page.Locator(".error-content");
            var text = await errorContent.TextContentAsync() ?? "";
            Assert.Contains("Check data.json for errors and restart the application", text);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorState_ShowsHelpText));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorState_ErrorMessage_UsesMonospaceFont()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(ErrorBaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });

        try
        {
            // The error message div uses inline style font-family: Consolas
            var hasConsolas = await page.EvaluateAsync<bool>(
                "() => { const content = document.querySelector('.error-content'); " +
                "if (!content) return false; " +
                "const divs = content.querySelectorAll('div'); " +
                "return Array.from(divs).some(d => d.style.fontFamily && d.style.fontFamily.includes('Consolas')); }");
            Assert.True(hasConsolas, "Error message should use Consolas monospace font");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorState_ErrorMessage_UsesMonospaceFont));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorState_IsCentered_OnPage()
    {
        var page = await _fixture.NewPageAsync();
        await page.GotoAsync(ErrorBaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });

        try
        {
            var panelBox = await page.Locator(".error-panel").BoundingBoxAsync();
            Assert.NotNull(panelBox);

            // Should be centered-ish horizontally (within reasonable margin)
            var centerX = panelBox!.X + panelBox.Width / 2;
            Assert.True(Math.Abs(centerX - 960) < 100,
                $"Error panel should be roughly centered (center at {centerX}px, expected ~960px)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorState_IsCentered_OnPage));
            throw;
        }
    }
}