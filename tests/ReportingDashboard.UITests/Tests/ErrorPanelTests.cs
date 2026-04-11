using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class ErrorPanelTests
{
    private readonly PlaywrightFixture _fixture;

    public ErrorPanelTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    // ── Error Panel Visibility Tests ───────────────────────────────────

    [Fact]
    public async Task ErrorPanel_WhenVisible_DisplaysErrorPanelContainer()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            // The page either shows error panel or dashboard - test whichever is present
            var hasErrorPanel = await errorPage.IsErrorPanelVisibleAsync();
            var hasDashboard = await errorPage.IsDashboardVisibleAsync();

            // At least one should be visible
            (hasErrorPanel || hasDashboard).Should().BeTrue(
                "page should render either error panel or dashboard content");

            // They should be mutually exclusive
            if (hasErrorPanel)
            {
                hasDashboard.Should().BeFalse(
                    "dashboard content should not be visible when error panel is shown");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_DisplaysErrorPanelContainer));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_DisplaysWarningIcon()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var iconText = await errorPage.GetIconTextAsync();
                iconText.Should().Contain("\u26A0", "error icon should display warning symbol ⚠");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_DisplaysWarningIcon));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_DisplaysCorrectTitle()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var titleText = await errorPage.GetTitleTextAsync();
                titleText.Should().Be("Dashboard data could not be loaded");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_DisplaysCorrectTitle));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_DisplaysHintText()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var hintText = await errorPage.GetHintTextAsync();
                hintText.Should().Be("Check data.json for errors and restart the application.");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_DisplaysHintText));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_HidesDashboardSections()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                (await errorPage.DashboardHeader.CountAsync()).Should().Be(0,
                    "header section should not exist when error panel is shown");
                (await errorPage.TimelineArea.CountAsync()).Should().Be(0,
                    "timeline section should not exist when error panel is shown");
                (await errorPage.HeatmapWrap.CountAsync()).Should().Be(0,
                    "heatmap section should not exist when error panel is shown");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_HidesDashboardSections));
            throw;
        }
    }

    // ── Error Panel CSS/Styling Tests ──────────────────────────────────

    [Fact]
    public async Task ErrorPanel_WhenVisible_HasErrorPanelCssClass()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var panelClass = await errorPage.ErrorPanel.GetAttributeAsync("class");
                panelClass.Should().Contain("error-panel");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_HasErrorPanelCssClass));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_IconHasErrorIconCssClass()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var iconClass = await errorPage.ErrorIcon.GetAttributeAsync("class");
                iconClass.Should().Contain("error-icon");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_IconHasErrorIconCssClass));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_HintHasErrorHintCssClass()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var hintClass = await errorPage.HintText.GetAttributeAsync("class");
                hintClass.Should().Contain("error-hint");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_HintHasErrorHintCssClass));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_IsCenteredInViewport()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                // Check that error-panel uses flexbox centering
                var display = await errorPage.ErrorPanel.EvaluateAsync<string>(
                    "el => getComputedStyle(el).display");
                var justifyContent = await errorPage.ErrorPanel.EvaluateAsync<string>(
                    "el => getComputedStyle(el).justifyContent");
                var alignItems = await errorPage.ErrorPanel.EvaluateAsync<string>(
                    "el => getComputedStyle(el).alignItems");

                display.Should().Be("flex", "error panel should use flex layout");
                justifyContent.Should().Be("center", "error panel should center content horizontally");
                alignItems.Should().Be("center", "error panel should center content vertically");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_IsCenteredInViewport));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_Has1920x1080Dimensions()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var width = await errorPage.ErrorPanel.EvaluateAsync<double>(
                    "el => el.getBoundingClientRect().width");
                var height = await errorPage.ErrorPanel.EvaluateAsync<double>(
                    "el => el.getBoundingClientRect().height");

                width.Should().Be(1920, "error panel should be exactly 1920px wide");
                height.Should().Be(1080, "error panel should be exactly 1080px tall");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_Has1920x1080Dimensions));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_HasNoScrollbars()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var overflow = await errorPage.ErrorPanel.EvaluateAsync<string>(
                    "el => getComputedStyle(el).overflow");

                overflow.Should().Be("hidden", "error panel should hide overflow to prevent scrollbars");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_HasNoScrollbars));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_IconIsRedColor()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var color = await errorPage.ErrorIcon.EvaluateAsync<string>(
                    "el => getComputedStyle(el).color");

                // #EA4335 = rgb(234, 67, 53)
                color.Should().Be("rgb(234, 67, 53)",
                    "error icon should be colored #EA4335 (red)");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_IconIsRedColor));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_IconHas48pxFontSize()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var fontSize = await errorPage.ErrorIcon.EvaluateAsync<string>(
                    "el => getComputedStyle(el).fontSize");

                fontSize.Should().Be("48px", "error icon should be 48px font size");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_IconHas48pxFontSize));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_TitleHasBoldWeight()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var fontWeight = await errorPage.Title.EvaluateAsync<string>(
                    "el => getComputedStyle(el).fontWeight");

                // 700 or "bold"
                fontWeight.Should().BeOneOf("700", "bold",
                    "title should have font-weight 700 (bold)");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_TitleHasBoldWeight));
            throw;
        }
    }

    // ── DOM Structure Tests ────────────────────────────────────────────

    [Fact]
    public async Task ErrorPanel_WhenVisible_ContainsExpectedChildElements()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                // Icon div
                (await errorPage.ErrorIcon.CountAsync()).Should().Be(1,
                    "should have exactly one error icon");

                // H2 title
                (await errorPage.Title.CountAsync()).Should().Be(1,
                    "should have exactly one h2 title");

                // Hint text
                (await errorPage.HintText.CountAsync()).Should().Be(1,
                    "should have exactly one hint paragraph");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_ContainsExpectedChildElements));
            throw;
        }
    }

    [Fact]
    public async Task ErrorPanel_WhenVisible_ChildElementsAreInCorrectOrder()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                // Get all direct children of error-panel
                var childTags = await page.EvalOnSelectorAllAsync<string[]>(
                    ".error-panel > *",
                    "elements => elements.map(el => el.tagName.toLowerCase())");

                childTags.Should().NotBeEmpty();

                // First child should be the icon div
                childTags[0].Should().Be("div", "first child should be the icon div");

                // Second child should be h2
                childTags[1].Should().Be("h2", "second child should be the h2 title");

                // Last child should be a p (the hint)
                childTags[^1].Should().Be("p", "last child should be the hint paragraph");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_ChildElementsAreInCorrectOrder));
            throw;
        }
    }

    // ── No Raw Exception Tests ─────────────────────────────────────────

    [Fact]
    public async Task Page_NeverShowsRawStackTrace()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            await page.GotoAsync(_fixture.BaseUrl + "/", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 15000
            });

            var bodyText = await page.InnerTextAsync("body");

            bodyText.Should().NotContain("System.Exception",
                "raw exceptions should not be shown to user");
            bodyText.Should().NotContain("Unhandled exception",
                "unhandled exceptions should not be shown to user");
            bodyText.Should().NotContain("stack trace",
                "stack traces should not be shown to user");
            bodyText.Should().NotContainEquivalentOf("NullReferenceException",
                "null reference exceptions should not surface");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_NeverShowsRawStackTrace));
            throw;
        }
    }

    [Fact]
    public async Task Page_ReturnsSuccessStatusCode()
    {
        var page = await _fixture.NewPageAsync();

        try
        {
            var response = await page.GotoAsync(_fixture.BaseUrl + "/", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 15000
            });

            response.Should().NotBeNull();
            response!.Status.Should().Be(200,
                "page should return HTTP 200 regardless of data.json state");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Page_ReturnsSuccessStatusCode));
            throw;
        }
    }

    // ── Screenshot Capture Tests ───────────────────────────────────────

    [Fact]
    public async Task ErrorPanel_WhenVisible_CanBeCapturedAsScreenshot()
    {
        var page = await _fixture.NewPageAsync();
        var errorPage = new ErrorPanelPage(page, _fixture.BaseUrl);

        try
        {
            await errorPage.NavigateAsync();

            if (await errorPage.IsErrorPanelVisibleAsync())
            {
                var screenshotBytes = await page.ScreenshotAsync(new PageScreenshotOptions
                {
                    FullPage = false // viewport only, matching 1920x1080
                });

                screenshotBytes.Should().NotBeEmpty(
                    "screenshot should capture error panel content");
                screenshotBytes.Length.Should().BeGreaterThan(1000,
                    "screenshot should have meaningful content, not blank");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(ErrorPanel_WhenVisible_CanBeCapturedAsScreenshot));
            throw;
        }
    }
}