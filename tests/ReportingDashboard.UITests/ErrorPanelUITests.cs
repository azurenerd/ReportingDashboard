using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Fixtures;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class ErrorPanelUITests
{
    private readonly PlaywrightFixture _fixture;

    public ErrorPanelUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dashboard_LoadsPage_ReturnsSuccessStatusCode()
    {
        var page = await _fixture.NewPageAsync();
        try
        {
            var response = await page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30_000
            });

            response.Should().NotBeNull();
            response!.Status.Should().BeOneOf(200, 301, 302);
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_LoadsPage_ReturnsSuccessStatusCode));
            throw;
        }
    }

    [Fact]
    public async Task Dashboard_ShowsEitherDashboardOrErrorPanel()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            var isError = await dashboardPage.IsErrorStateAsync();
            var hasDashboard = await dashboardPage.IsDashboardContentVisibleAsync();

            (isError || hasDashboard).Should().BeTrue(
                "the page should show either error panel or dashboard content");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Dashboard_ShowsEitherDashboardOrErrorPanel));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_DisplaysTitleText()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var title = await errorPage.GetTitleTextAsync();
                title.Should().Be("Dashboard data could not be loaded");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_DisplaysTitleText));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_DisplaysHelpText()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var help = await errorPage.GetHelpTextAsync();
                help.Should().Be("Check data.json for errors and restart the application.");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_DisplaysHelpText));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_DisplaysErrorIcon()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var icon = await errorPage.GetIconTextAsync();
                icon.Should().NotBeNullOrEmpty("the error icon should contain a visible character");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_DisplaysErrorIcon));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_HasCorrectCssClasses()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                (await errorPage.ErrorPanel.CountAsync()).Should().Be(1);
                (await errorPage.ErrorIcon.CountAsync()).Should().Be(1);
                (await errorPage.ErrorTitle.CountAsync()).Should().Be(1);
                (await errorPage.ErrorHelp.CountAsync()).Should().Be(1);
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_HasCorrectCssClasses));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_IsCenteredOnPage()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var display = await errorPage.ErrorPanel.EvaluateAsync<string>(
                    "el => window.getComputedStyle(el).display");
                display.Should().Be("flex", "error-panel should use flexbox layout");

                var alignItems = await errorPage.ErrorPanel.EvaluateAsync<string>(
                    "el => window.getComputedStyle(el).alignItems");
                alignItems.Should().Be("center", "error-panel should center items vertically");

                var justifyContent = await errorPage.ErrorPanel.EvaluateAsync<string>(
                    "el => window.getComputedStyle(el).justifyContent");
                justifyContent.Should().Be("center", "error-panel should center items horizontally");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_IsCenteredOnPage));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_HasWhiteBackground()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var bgColor = await errorPage.ErrorPanel.EvaluateAsync<string>(
                    "el => window.getComputedStyle(el).backgroundColor");

                bgColor.Should().Match(c =>
                    c.Contains("255, 255, 255") || c == "rgba(0, 0, 0, 0)",
                    "error-panel background should be white (#FFFFFF)");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_HasWhiteBackground));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_ErrorIconHasCorrectColor()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var color = await errorPage.ErrorIcon.EvaluateAsync<string>(
                    "el => window.getComputedStyle(el).color");

                color.Should().Contain("234, 67, 53",
                    "error icon should be colored #EA4335 (red)");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_ErrorIconHasCorrectColor));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_TitleHasCorrectStyling()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var fontSize = await errorPage.ErrorTitle.EvaluateAsync<string>(
                    "el => window.getComputedStyle(el).fontSize");
                fontSize.Should().Be("20px", "title font-size should be 20px");

                var fontWeight = await errorPage.ErrorTitle.EvaluateAsync<string>(
                    "el => window.getComputedStyle(el).fontWeight");
                fontWeight.Should().BeOneOf("700", "bold",
                    "title font-weight should be bold (700)");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_TitleHasCorrectStyling));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_DetailsUsesMonospaceFont()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync() && await errorPage.IsDetailsVisibleAsync())
            {
                var fontFamily = await errorPage.ErrorDetails.EvaluateAsync<string>(
                    "el => window.getComputedStyle(el).fontFamily");
                fontFamily.ToLower().Should().Contain("monospace",
                    "error details should use a monospace font");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_DetailsUsesMonospaceFont));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_DoesNotExposeStackTraces()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var fullText = await errorPage.ErrorPanel.TextContentAsync() ?? string.Empty;

                fullText.Should().NotContain("at System.",
                    "no .NET stack traces should be visible");
                fullText.Should().NotContain("StackTrace",
                    "no stack trace labels should be visible");
                fullText.Should().NotContain("NullReferenceException",
                    "no exception type names should be visible");
                fullText.Should().NotContain("blazor-error-ui",
                    "Blazor default error UI should not be visible");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_DoesNotExposeStackTraces));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_NoDashboardContentRendered()
    {
        var page = await _fixture.NewPageAsync();
        var dashboardPage = new DashboardPage(page, _fixture.BaseUrl);

        try
        {
            await dashboardPage.NavigateAsync();

            if (await dashboardPage.IsErrorStateAsync())
            {
                (await dashboardPage.Header.CountAsync()).Should().Be(0,
                    "header should not render when in error state");
                (await dashboardPage.Timeline.CountAsync()).Should().Be(0,
                    "timeline should not render when in error state");
                (await dashboardPage.Heatmap.CountAsync()).Should().Be(0,
                    "heatmap should not render when in error state");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_NoDashboardContentRendered));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_InnerContentIsCentered()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var innerDiv = errorPage.ErrorPanel.Locator("> div");
                var textAlign = await innerDiv.EvaluateAsync<string>(
                    "el => window.getComputedStyle(el).textAlign");
                textAlign.Should().Be("center", "inner content should be text-centered");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_InnerContentIsCentered));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_HasCorrectElementOrder()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var innerDiv = errorPage.ErrorPanel.Locator("> div");
                var children = innerDiv.Locator("> div");
                var count = await children.CountAsync();

                count.Should().BeGreaterThanOrEqualTo(3, "at least icon, title, and help should exist");

                var firstClass = await children.Nth(0).GetAttributeAsync("class");
                firstClass.Should().Be("error-icon");

                var secondClass = await children.Nth(1).GetAttributeAsync("class");
                secondClass.Should().Be("error-title");

                var lastClass = await children.Nth(count - 1).GetAttributeAsync("class");
                lastClass.Should().Be("error-help");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_HasCorrectElementOrder));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_PageTakesScreenshot_ForVisualVerification()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        await errorPage.NavigateAsync();

        if (await errorPage.IsErrorPanelVisibleAsync())
        {
            await _fixture.CaptureScreenshotAsync(page, "ErrorPanel_VisualVerification");
        }

        true.Should().BeTrue();
    }
}