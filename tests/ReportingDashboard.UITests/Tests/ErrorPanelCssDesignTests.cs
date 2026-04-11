using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// CSS design fidelity tests for the ErrorPanel component from PR #555.
/// Verifies inline styles match the spec: 48px icon, Consolas font,
/// #EA4335 color, max-width:600px, word-break:break-word.
/// Requires a server started with invalid/missing data.json.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class ErrorPanelCssDesignTests
{
    private readonly PlaywrightFixture _fixture;

    public ErrorPanelCssDesignTests(PlaywrightFixture fixture) => _fixture = fixture;

    private string ErrorBaseUrl =>
        Environment.GetEnvironmentVariable("BASE_URL_ERROR") ?? _fixture.BaseUrl;

    #region Icon Styling

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorIcon_HasFontSize48px()
    {
        var page = await _fixture.NewPageAsync();
        var errorPanel = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPanel.NavigateAsync();

            if (!await errorPanel.IsErrorVisibleAsync()) return;

            var fontSize = await errorPanel.GetIconFontSizeAsync();
            Assert.Equal("48px", fontSize);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorIcon_HasFontSize48px));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorIcon_HasRedColor_EA4335()
    {
        var page = await _fixture.NewPageAsync();
        var errorPanel = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPanel.NavigateAsync();

            if (!await errorPanel.IsErrorVisibleAsync()) return;

            var color = await errorPanel.GetIconColorAsync();
            // #EA4335 = rgb(234, 67, 53)
            Assert.Contains("234", color);
            Assert.Contains("67", color);
            Assert.Contains("53", color);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorIcon_HasRedColor_EA4335));
            throw;
        }
    }

    #endregion

    #region Title Styling

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorTitle_HasFontSize20px()
    {
        var page = await _fixture.NewPageAsync();
        var errorPanel = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPanel.NavigateAsync();

            if (!await errorPanel.IsErrorVisibleAsync()) return;

            var fontSize = await errorPanel.GetTitleFontSizeAsync();
            Assert.Equal("20px", fontSize);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorTitle_HasFontSize20px));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorTitle_HasFontWeight700()
    {
        var page = await _fixture.NewPageAsync();
        var errorPanel = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPanel.NavigateAsync();

            if (!await errorPanel.IsErrorVisibleAsync()) return;

            var fontWeight = await errorPanel.GetTitleFontWeightAsync();
            Assert.Equal("700", fontWeight);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorTitle_HasFontWeight700));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorTitle_ContainsExpectedText()
    {
        var page = await _fixture.NewPageAsync();
        var errorPanel = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPanel.NavigateAsync();

            if (!await errorPanel.IsErrorVisibleAsync()) return;

            var text = await errorPanel.GetTitleTextAsync();
            Assert.Contains("Dashboard data could not be loaded", text ?? "");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorTitle_ContainsExpectedText));
            throw;
        }
    }

    #endregion

    #region Error Details Styling

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorDetails_HasConsolasFontFamily()
    {
        var page = await _fixture.NewPageAsync();
        var errorPanel = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPanel.NavigateAsync();

            if (!await errorPanel.IsErrorVisibleAsync()) return;

            var fontFamily = await errorPanel.GetDetailsFontFamilyAsync();
            Assert.Contains("Consolas", fontFamily, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorDetails_HasConsolasFontFamily));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorDetails_HasMaxWidth600px()
    {
        var page = await _fixture.NewPageAsync();
        var errorPanel = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPanel.NavigateAsync();

            if (!await errorPanel.IsErrorVisibleAsync()) return;

            var maxWidth = await errorPanel.GetDetailsMaxWidthAsync();
            Assert.Equal("600px", maxWidth);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorDetails_HasMaxWidth600px));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorDetails_HasWordBreakBreakWord()
    {
        var page = await _fixture.NewPageAsync();
        var errorPanel = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPanel.NavigateAsync();

            if (!await errorPanel.IsErrorVisibleAsync()) return;

            var wordBreak = await errorPanel.GetDetailsWordBreakAsync();
            Assert.Equal("break-word", wordBreak);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorDetails_HasWordBreakBreakWord));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorDetails_HasFontSize14px()
    {
        var page = await _fixture.NewPageAsync();
        var errorPanel = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPanel.NavigateAsync();

            if (!await errorPanel.IsErrorVisibleAsync()) return;

            var fontSize = await errorPanel.ErrorDetails.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            Assert.Equal("14px", fontSize);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorDetails_HasFontSize14px));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorDetails_ContainsNonEmptyMessage()
    {
        var page = await _fixture.NewPageAsync();
        var errorPanel = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPanel.NavigateAsync();

            if (!await errorPanel.IsErrorVisibleAsync()) return;

            var message = await errorPanel.GetErrorMessageAsync();
            Assert.False(string.IsNullOrWhiteSpace(message),
                "Error details should contain a non-empty error message");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorDetails_ContainsNonEmptyMessage));
            throw;
        }
    }

    #endregion

    #region Help Text Styling

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorHelp_HasFontSize12px()
    {
        var page = await _fixture.NewPageAsync();
        var errorPanel = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPanel.NavigateAsync();

            if (!await errorPanel.IsErrorVisibleAsync()) return;

            var fontSize = await errorPanel.GetHelpFontSizeAsync();
            Assert.Equal("12px", fontSize);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorHelp_HasFontSize12px));
            throw;
        }
    }

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorHelp_ContainsGuidanceText()
    {
        var page = await _fixture.NewPageAsync();
        var errorPanel = new ErrorPanelPageObject(page, ErrorBaseUrl);

        try
        {
            await errorPanel.NavigateAsync();

            if (!await errorPanel.IsErrorVisibleAsync()) return;

            var text = await errorPanel.GetHelpTextAsync();
            Assert.Contains("Check data.json for errors and restart the application", text ?? "");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorHelp_ContainsGuidanceText));
            throw;
        }
    }

    #endregion

    #region Error Panel Layout

    [Fact(Skip = "Requires server started with invalid data.json")]
    public async Task ErrorPanel_NoDashboardContentVisible()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, ErrorBaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            if (!await dashboard.IsErrorVisibleAsync()) return;

            Assert.False(await dashboard.IsDashboardVisibleAsync(),
                "Dashboard root should not be visible when error panel is shown");
            Assert.Equal(0, await dashboard.TimelineArea.CountAsync());
            Assert.Equal(0, await dashboard.HeatmapWrap.CountAsync());
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_NoDashboardContentVisible));
            throw;
        }
    }

    #endregion
}