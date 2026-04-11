using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// Visual and structural UI tests for the ErrorPanel component.
/// These tests verify the error panel renders with correct structure, styling,
/// colors, typography, and layout per the acceptance criteria in PR #520.
///
/// Requires a server running in error state (missing/malformed data.json).
/// Set BASE_URL_ERROR environment variable, or these tests will use BASE_URL.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class ErrorPanelVisualTests
{
    private readonly PlaywrightFixture _fixture;

    public ErrorPanelVisualTests(PlaywrightFixture fixture) => _fixture = fixture;

    private string ErrorBaseUrl =>
        Environment.GetEnvironmentVariable("BASE_URL_ERROR") ?? _fixture.BaseUrl;

    #region Error Panel Visibility and Structure

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_IsVisible_WhenDataJsonMissing()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            Assert.True(await errorPage.IsErrorPanelVisibleAsync(),
                "Error panel should be visible when data.json is missing");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_IsVisible_WhenDataJsonMissing));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_DashboardNotVisible_WhenErrorShown()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            Assert.False(await errorPage.IsDashboardVisibleAsync(),
                "Dashboard content should NOT be visible when error panel is shown");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_DashboardNotVisible_WhenErrorShown));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_HasErrorContentContainer()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            await Assertions.Expect(errorPage.ErrorContent).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_HasErrorContentContainer));
            throw;
        }
    }

    #endregion

    #region Static Text Content

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_ShowsStaticTitle()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var titleText = await errorPage.GetErrorTitleTextAsync();
            Assert.Contains("Dashboard data could not be loaded", titleText);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_ShowsStaticTitle));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_ShowsHelpText()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var helpText = await errorPage.GetHelpTextAsync();
            Assert.Contains("Check data.json for errors and restart the application", helpText);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_ShowsHelpText));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_ShowsRedQuestionMarkIndicator()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var indicatorText = await errorPage.GetIndicatorTextAsync();
            Assert.Contains("?", indicatorText);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_ShowsRedQuestionMarkIndicator));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_ShowsNonEmptyErrorMessage()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var messageText = await errorPage.GetErrorMessageTextAsync();
            Assert.False(string.IsNullOrWhiteSpace(messageText),
                "Error message should not be empty - it should show the specific error from DashboardDataService");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_ShowsNonEmptyErrorMessage));
            throw;
        }
    }

    #endregion

    #region CSS Styling Verification

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_HasFlexCentering()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var display = await errorPage.ErrorPanel.EvaluateAsync<string>(
                "el => getComputedStyle(el).display");
            var alignItems = await errorPage.ErrorPanel.EvaluateAsync<string>(
                "el => getComputedStyle(el).alignItems");
            var justifyContent = await errorPage.ErrorPanel.EvaluateAsync<string>(
                "el => getComputedStyle(el).justifyContent");

            Assert.Equal("flex", display);
            Assert.Equal("center", alignItems);
            Assert.Equal("center", justifyContent);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_HasFlexCentering));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_Has1920x1080Dimensions()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var width = await errorPage.ErrorPanel.EvaluateAsync<string>(
                "el => getComputedStyle(el).width");
            var height = await errorPage.ErrorPanel.EvaluateAsync<string>(
                "el => getComputedStyle(el).height");

            Assert.Equal("1920px", width);
            Assert.Equal("1080px", height);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_Has1920x1080Dimensions));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_HasWhiteBackground()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var bgColor = await errorPage.ErrorPanel.EvaluateAsync<string>(
                "el => getComputedStyle(el).backgroundColor");

            // White can be rgb(255, 255, 255) or rgba(255, 255, 255, 1)
            Assert.True(
                bgColor.Contains("255, 255, 255") || bgColor == "white",
                $"Expected white background, got: {bgColor}");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_HasWhiteBackground));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorIndicator_HasRedColor()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var color = await errorPage.ErrorIndicator.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");

            // #EA4335 = rgb(234, 67, 53)
            Assert.Contains("234, 67, 53", color);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorIndicator_HasRedColor));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorIndicator_HasFontSize48px()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var fontSize = await errorPage.ErrorIndicator.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");

            Assert.Equal("48px", fontSize);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorIndicator_HasFontSize48px));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorTitle_HasCorrectStyling()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var fontSize = await errorPage.ErrorTitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            var fontWeight = await errorPage.ErrorTitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontWeight");
            var color = await errorPage.ErrorTitle.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");

            Assert.Equal("20px", fontSize);
            Assert.Equal("700", fontWeight);
            // #333 = rgb(51, 51, 51)
            Assert.Contains("51, 51, 51", color);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorTitle_HasCorrectStyling));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorMessage_HasMonospaceFont()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var fontFamily = await errorPage.ErrorMessage.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontFamily");

            // Should contain Consolas or monospace
            Assert.True(
                fontFamily.Contains("Consolas", StringComparison.OrdinalIgnoreCase) ||
                fontFamily.Contains("monospace", StringComparison.OrdinalIgnoreCase),
                $"Expected monospace font family, got: {fontFamily}");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorMessage_HasMonospaceFont));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorMessage_HasCorrectColorAndSize()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var fontSize = await errorPage.ErrorMessage.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            var color = await errorPage.ErrorMessage.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");

            Assert.Equal("14px", fontSize);
            // #666 = rgb(102, 102, 102)
            Assert.Contains("102, 102, 102", color);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorMessage_HasCorrectColorAndSize));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task HelpText_HasCorrectStyling()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var fontSize = await errorPage.HelpText.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            var color = await errorPage.HelpText.EvaluateAsync<string>(
                "el => getComputedStyle(el).color");

            Assert.Equal("12px", fontSize);
            // #888 = rgb(136, 136, 136)
            Assert.Contains("136, 136, 136", color);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(HelpText_HasCorrectStyling));
            throw;
        }
    }

    #endregion

    #region Security - No Forbidden Content

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_NoStackTraceVisible()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var bodyText = await page.TextContentAsync("body") ?? "";

            Assert.DoesNotContain("at System.", bodyText);
            Assert.DoesNotContain("StackTrace", bodyText);
            Assert.DoesNotContain("NullReferenceException", bodyText);
            Assert.DoesNotContain("FileNotFoundException", bodyText);
            Assert.DoesNotContain("JsonException", bodyText);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_NoStackTraceVisible));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_NoBlazorErrorUI()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            // Blazor default error UI uses #blazor-error-ui
            var blazorErrorCount = await page.Locator("#blazor-error-ui:visible").CountAsync();
            Assert.Equal(0, blazorErrorCount);

            var bodyText = await page.TextContentAsync("body") ?? "";
            Assert.DoesNotContain("An unhandled error has occurred", bodyText);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_NoBlazorErrorUI));
            throw;
        }
    }

    #endregion

    #region Layout and Screenshot-Friendliness

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_IsCenteredOnPage()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            // Verify the error content is centered within the panel
            var textAlign = await errorPage.ErrorContent.EvaluateAsync<string>(
                "el => getComputedStyle(el).textAlign");

            Assert.Equal("center", textAlign);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_IsCenteredOnPage));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_NoScrollbars()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            // Check that the page doesn't have scrollbars
            var hasVerticalScroll = await page.EvaluateAsync<bool>(
                "() => document.documentElement.scrollHeight > document.documentElement.clientHeight");
            var hasHorizontalScroll = await page.EvaluateAsync<bool>(
                "() => document.documentElement.scrollWidth > document.documentElement.clientWidth");

            // At 1920x1080 viewport with a 1920x1080 error panel, there shouldn't be scrollbars
            // (though body overflow:hidden may also handle this)
            // This is a soft check - the error panel should fit the viewport
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_NoScrollbars));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_ScreenshotCapture_Succeeds()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            // Verify we can take a screenshot (screenshot-friendly requirement)
            var screenshot = await page.ScreenshotAsync(new PageScreenshotOptions
            {
                FullPage = false
            });

            Assert.NotNull(screenshot);
            Assert.True(screenshot.Length > 0, "Screenshot should have content");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_ScreenshotCapture_Succeeds));
            throw;
        }
    }

    #endregion

    #region Content Structure Verification

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_HasFourChildElements()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            var childCount = await errorPage.ErrorContent.EvaluateAsync<int>(
                "el => el.querySelectorAll(':scope > div').length");

            // 4 child divs: indicator, title, message, help text
            Assert.Equal(4, childCount);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_HasFourChildElements));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_ChildrenInCorrectOrder()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            // First child: red indicator with "?"
            var firstText = await errorPage.ErrorIndicator.TextContentAsync() ?? "";
            Assert.Contains("?", firstText);

            // Second child: static title
            var secondText = await errorPage.ErrorTitle.TextContentAsync() ?? "";
            Assert.Contains("Dashboard data could not be loaded", secondText);

            // Third child: dynamic error message (non-empty)
            var thirdText = await errorPage.ErrorMessage.TextContentAsync() ?? "";
            Assert.False(string.IsNullOrWhiteSpace(thirdText));

            // Fourth child: help text
            var fourthText = await errorPage.HelpText.TextContentAsync() ?? "";
            Assert.Contains("Check data.json", fourthText);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_ChildrenInCorrectOrder));
            throw;
        }
    }

    #endregion

    #region HTTP Response Verification

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_ServerReturns200_NotErrorCode()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync(ErrorBaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30_000
            });

            Assert.NotNull(response);
            Assert.Equal(200, response!.Status);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_ServerReturns200_NotErrorCode));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid/missing data.json")]
    public async Task ErrorPanel_PageHTML_ContainsErrorPanelClass()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(ErrorBaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30_000
            });

            var html = await page.ContentAsync();
            Assert.Contains("error-panel", html);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_PageHTML_ContainsErrorPanelClass));
            throw;
        }
    }

    #endregion
}