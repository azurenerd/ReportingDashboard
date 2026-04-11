using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Fixtures;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeaderLegendVisualUITests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderLegendVisualUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Legend_Diamonds_AreRotated45Degrees()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            // PoC diamond
            var pocDiamond = page.Locator("div.hdr > div:last-child > span:nth-child(1) > span:first-child");
            var pocTransform = await pocDiamond.EvaluateAsync<string>(
                "el => getComputedStyle(el).transform");
            // rotate(45deg) computes to matrix(0.707107, 0.707107, -0.707107, 0.707107, 0, 0)
            pocTransform.Should().Contain("matrix", "PoC diamond should have a rotation transform");

            // Production diamond
            var prodDiamond = page.Locator("div.hdr > div:last-child > span:nth-child(2) > span:first-child");
            var prodTransform = await prodDiamond.EvaluateAsync<string>(
                "el => getComputedStyle(el).transform");
            prodTransform.Should().Contain("matrix", "Production diamond should have a rotation transform");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_Diamonds_AreRotated45Degrees));
            throw;
        }
    }

    [Fact]
    public async Task Legend_Diamonds_Are12x12Pixels()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var pocDiamond = page.Locator("div.hdr > div:last-child > span:nth-child(1) > span:first-child");
            var width = await pocDiamond.EvaluateAsync<string>("el => getComputedStyle(el).width");
            var height = await pocDiamond.EvaluateAsync<string>("el => getComputedStyle(el).height");

            width.Should().Be("12px");
            height.Should().Be("12px");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_Diamonds_Are12x12Pixels));
            throw;
        }
    }

    [Fact]
    public async Task Legend_Checkpoint_Is8x8Circle()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var checkpoint = page.Locator("div.hdr > div:last-child > span:nth-child(3) > span:first-child");
            var width = await checkpoint.EvaluateAsync<string>("el => getComputedStyle(el).width");
            var height = await checkpoint.EvaluateAsync<string>("el => getComputedStyle(el).height");
            var borderRadius = await checkpoint.EvaluateAsync<string>("el => getComputedStyle(el).borderRadius");

            width.Should().Be("8px");
            height.Should().Be("8px");
            borderRadius.Should().Be("50%");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_Checkpoint_Is8x8Circle));
            throw;
        }
    }

    [Fact]
    public async Task Legend_NowBar_Is2x14Pixels()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var nowBar = page.Locator("div.hdr > div:last-child > span:nth-child(4) > span:first-child");
            var width = await nowBar.EvaluateAsync<string>("el => getComputedStyle(el).width");
            var height = await nowBar.EvaluateAsync<string>("el => getComputedStyle(el).height");

            width.Should().Be("2px");
            height.Should().Be("14px");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_NowBar_Is2x14Pixels));
            throw;
        }
    }

    [Fact]
    public async Task Legend_Labels_Have12pxFontSize()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            // Check all four legend label spans (second child of each legend item)
            for (int i = 1; i <= 4; i++)
            {
                var label = page.Locator($"div.hdr > div:last-child > span:nth-child({i}) > span:last-child");
                var fontSize = await label.EvaluateAsync<string>("el => getComputedStyle(el).fontSize");
                fontSize.Should().Be("12px", $"legend label {i} should have 12px font-size");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_Labels_Have12pxFontSize));
            throw;
        }
    }

    [Fact]
    public async Task Legend_Labels_HaveWhiteSpaceNowrap()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            for (int i = 1; i <= 4; i++)
            {
                var label = page.Locator($"div.hdr > div:last-child > span:nth-child({i}) > span:last-child");
                var whiteSpace = await label.EvaluateAsync<string>("el => getComputedStyle(el).whiteSpace");
                whiteSpace.Should().Be("nowrap", $"legend label {i} should not wrap");
            }
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_Labels_HaveWhiteSpaceNowrap));
            throw;
        }
    }

    [Fact]
    public async Task Legend_NowLabel_ContainsMonthText()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var nowLabel = page.Locator("div.hdr > div:last-child > span:nth-child(4) > span:last-child");
            var text = await nowLabel.TextContentAsync();

            text.Should().StartWith("Now", "Now label should start with 'Now'");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Legend_NowLabel_ContainsMonthText));
            throw;
        }
    }

    [Fact]
    public async Task Header_IsAlignedWithin1920pxWidth()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var headerBox = await headerPage.Header.BoundingBoxAsync();
            headerBox.Should().NotBeNull();
            headerBox!.Width.Should().BeLessOrEqualTo(1920, "header should not exceed viewport width");
            headerBox.X.Should().BeGreaterOrEqualTo(0, "header should not overflow left");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_IsAlignedWithin1920pxWidth));
            throw;
        }
    }

    [Fact]
    public async Task Header_TitleAndLegend_AreOnSameRow()
    {
        var page = await _fixture.NewPageAsync();
        var headerPage = new HeaderPage(page, _fixture.BaseUrl);

        try
        {
            await headerPage.NavigateAsync();

            var titleDiv = page.Locator("div.hdr > div:first-child");
            var legendDiv = page.Locator("div.hdr > div:last-child");

            var titleBox = await titleDiv.BoundingBoxAsync();
            var legendBox = await legendDiv.BoundingBoxAsync();

            titleBox.Should().NotBeNull();
            legendBox.Should().NotBeNull();

            // They should overlap vertically (same row)
            var titleVerticalCenter = titleBox!.Y + (titleBox.Height / 2);
            var legendVerticalCenter = legendBox!.Y + (legendBox.Height / 2);

            Math.Abs(titleVerticalCenter - legendVerticalCenter).Should().BeLessThan(30,
                "title and legend should be roughly on the same horizontal line");

            // Legend should be to the right of title
            legendBox.X.Should().BeGreaterThan(titleBox.X + titleBox.Width - 50,
                "legend should be positioned to the right of the title area");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, nameof(Header_TitleAndLegend_AreOnSameRow));
            throw;
        }
    }
}