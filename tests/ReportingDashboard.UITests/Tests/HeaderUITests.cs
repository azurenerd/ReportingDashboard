using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeaderUITests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Header_IsVisible()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var header = new HeaderPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            await Assertions.Expect(header.Header).ToBeVisibleAsync();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Header_IsVisible");
            throw;
        }
    }

    [Fact]
    public async Task Header_Title_IsDisplayed()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var header = new HeaderPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            await Assertions.Expect(header.Title).ToBeVisibleAsync();
            var text = await header.GetTitleTextAsync();
            text.Should().NotBeNullOrWhiteSpace();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Header_Title");
            throw;
        }
    }

    [Fact]
    public async Task Header_Title_FontSize_Is24px()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var header = new HeaderPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var fontSize = await header.Title.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).fontSize");
            fontSize.Should().Be("24px");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Header_Title_FontSize");
            throw;
        }
    }

    [Fact]
    public async Task Header_Title_FontWeight_IsBold()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var header = new HeaderPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var fontWeight = await header.Title.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).fontWeight");
            fontWeight.Should().BeOneOf("700", "bold");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Header_Title_FontWeight");
            throw;
        }
    }

    [Fact]
    public async Task Header_Subtitle_IsDisplayed()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var header = new HeaderPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            await Assertions.Expect(header.Subtitle).ToBeVisibleAsync();
            var text = await header.GetSubtitleTextAsync();
            text.Should().NotBeNullOrWhiteSpace();
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Header_Subtitle");
            throw;
        }
    }

    [Fact]
    public async Task Header_Subtitle_FontSize_Is12px()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var header = new HeaderPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var fontSize = await header.Subtitle.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).fontSize");
            fontSize.Should().Be("12px");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Header_Subtitle_FontSize");
            throw;
        }
    }

    [Fact]
    public async Task Header_Subtitle_Color_IsGray()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var header = new HeaderPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var color = await header.Subtitle.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).color");
            // #888 = rgb(136, 136, 136)
            color.Should().Be("rgb(136, 136, 136)");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Header_Subtitle_Color");
            throw;
        }
    }

    [Fact]
    public async Task Header_BacklogLink_IsPresent()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var header = new HeaderPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var linkCount = await header.BacklogLink.CountAsync();
            linkCount.Should().BeGreaterThanOrEqualTo(1, "backlog link should be present");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Header_BacklogLink");
            throw;
        }
    }

    [Fact]
    public async Task Header_BacklogLink_Color_IsMicrosoftBlue()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var header = new HeaderPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var color = await header.BacklogLink.First.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).color");
            // #0078D4 = rgb(0, 120, 212)
            color.Should().Be("rgb(0, 120, 212)");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Header_BacklogLink_Color");
            throw;
        }
    }

    [Fact]
    public async Task Header_BorderBottom_IsSolid()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var header = new HeaderPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var borderStyle = await header.Header.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).borderBottomStyle");
            borderStyle.Should().Be("solid");

            var borderColor = await header.Header.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).borderBottomColor");
            // #E0E0E0 = rgb(224, 224, 224)
            borderColor.Should().Be("rgb(224, 224, 224)");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Header_BorderBottom");
            throw;
        }
    }

    [Fact]
    public async Task Header_Layout_IsFlexRow()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        var header = new HeaderPage(page);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var display = await header.Header.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).display");
            display.Should().Be("flex");

            var justifyContent = await header.Header.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).justifyContent");
            justifyContent.Should().Be("space-between");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "Header_Layout");
            throw;
        }
    }
}