using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

/// <summary>
/// CSS design fidelity tests verifying the Header component matches
/// the OriginalDesignConcept.html pixel spec at 1920x1080.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeaderCssDesignTests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderCssDesignTests(PlaywrightFixture fixture) => _fixture = fixture;

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_H1Link_HasFontSize13px()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var link = dashboard.BacklogLink;
            var fontSize = await link.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontSize");
            Assert.Equal("13px", fontSize);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_H1Link_HasFontSize13px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_H1Link_HasFontWeight400()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var link = dashboard.BacklogLink;
            var fontWeight = await link.EvaluateAsync<string>(
                "el => getComputedStyle(el).fontWeight");
            Assert.Equal("400", fontWeight);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_H1Link_HasFontWeight400));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_H1Link_HasMarginLeft16px()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var link = dashboard.BacklogLink;
            var marginLeft = await link.EvaluateAsync<string>(
                "el => getComputedStyle(el).marginLeft");
            Assert.Equal("16px", marginLeft);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_H1Link_HasMarginLeft16px));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_H1_HasMarginZero()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var margin = await dashboard.Title.EvaluateAsync<string>(
                "el => getComputedStyle(el).margin");
            Assert.Equal("0px", margin);
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_H1_HasMarginZero));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_IsFirstChildInDashboard()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            // Header should appear before timeline and heatmap
            var headerBox = await dashboard.Header.BoundingBoxAsync();
            var tlBox = await dashboard.TimelineArea.BoundingBoxAsync();

            Assert.NotNull(headerBox);
            Assert.NotNull(tlBox);
            Assert.True(headerBox!.Y < tlBox!.Y,
                "Header should be above the timeline area");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_IsFirstChildInDashboard));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_LeftSideContainsTitleAndSubtitle()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var titleBox = await dashboard.Title.BoundingBoxAsync();
            var subtitleBox = await dashboard.Subtitle.BoundingBoxAsync();

            Assert.NotNull(titleBox);
            Assert.NotNull(subtitleBox);
            // Subtitle should be below the title
            Assert.True(subtitleBox!.Y > titleBox!.Y,
                "Subtitle should render below the title");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_LeftSideContainsTitleAndSubtitle));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_NoOverflowOrScrollbar()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPageObject(page, _fixture.BaseUrl);

        try
        {
            await dashboard.NavigateAsync();

            var hasScrollbar = await page.EvaluateAsync<bool>(
                "() => document.documentElement.scrollHeight > document.documentElement.clientHeight || document.documentElement.scrollWidth > document.documentElement.clientWidth");
            Assert.False(hasScrollbar,
                "Dashboard should not have scrollbars at 1920x1080");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_NoOverflowOrScrollbar));
            throw;
        }
    }

    [Fact(Skip = "Requires running server at BASE_URL")]
    public async Task Header_SpansBetween_WithSpaceBetween()
    {
        var page = await _fixture.NewPageAsync();
        var header = new HeaderPageObject(page, _fixture.BaseUrl);

        try
        {
            await header.NavigateAsync();

            // Get bounding boxes of left (title) and right (legend) sides
            var leftDiv = header.HeaderContainer.Locator("> div:first-child");
            var rightDiv = header.LegendContainer;

            var leftBox = await leftDiv.BoundingBoxAsync();
            var rightBox = await rightDiv.BoundingBoxAsync();

            Assert.NotNull(leftBox);
            Assert.NotNull(rightBox);

            // Right side should be to the right of left side (space-between)
            Assert.True(rightBox!.X > leftBox!.X + leftBox.Width * 0.5,
                "Legend should be on the right side of the header (justify-content: space-between)");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_SpansBetween_WithSpaceBetween));
            throw;
        }
    }
}