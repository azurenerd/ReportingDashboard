using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class HeaderTests
{
    private readonly PlaywrightFixture _fixture;

    public HeaderTests(PlaywrightFixture fixture) => _fixture = fixture;

    private async Task<DashboardPage> LoadDashboardAsync()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        await dashboard.NavigateAsync();
        await dashboard.WaitForDashboardLoadedAsync();
        return dashboard;
    }

    [Fact]
    public async Task Header_IsVisible()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            await Assertions.Expect(dashboard.Header).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Header_IsVisible));
            throw;
        }
    }

    [Fact]
    public async Task Header_DisplaysProjectTitle()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            await Assertions.Expect(dashboard.Title).ToBeVisibleAsync();

            var titleText = await dashboard.Title.TextContentAsync();
            titleText.Should().NotBeNullOrWhiteSpace();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Header_DisplaysProjectTitle));
            throw;
        }
    }

    [Fact]
    public async Task Header_DisplaysSubtitle()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            await Assertions.Expect(dashboard.Subtitle).ToBeVisibleAsync();

            var subtitleText = await dashboard.Subtitle.TextContentAsync();
            subtitleText.Should().NotBeNullOrWhiteSpace();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Header_DisplaysSubtitle));
            throw;
        }
    }

    [Fact]
    public async Task Header_TitleHas24pxBoldFont()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var fontSize = await dashboard.Title.EvaluateAsync<string>("el => getComputedStyle(el).fontSize");
            var fontWeight = await dashboard.Title.EvaluateAsync<string>("el => getComputedStyle(el).fontWeight");

            fontSize.Should().Be("24px");
            // font-weight 700 or "bold"
            fontWeight.Should().BeOneOf("700", "bold");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Header_TitleHas24pxBoldFont));
            throw;
        }
    }

    [Fact]
    public async Task Header_SubtitleHas12pxGrayFont()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var fontSize = await dashboard.Subtitle.EvaluateAsync<string>("el => getComputedStyle(el).fontSize");
            fontSize.Should().Be("12px");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Header_SubtitleHas12pxGrayFont));
            throw;
        }
    }

    [Fact]
    public async Task Header_BacklogLinkIsClickable()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var linkCount = await dashboard.BacklogLink.CountAsync();
            if (linkCount > 0)
            {
                await Assertions.Expect(dashboard.BacklogLink).ToBeVisibleAsync();

                var href = await dashboard.BacklogLink.GetAttributeAsync("href");
                href.Should().NotBeNullOrWhiteSpace();

                var target = await dashboard.BacklogLink.GetAttributeAsync("target");
                target.Should().Be("_blank");

                var rel = await dashboard.BacklogLink.GetAttributeAsync("rel");
                rel.Should().Contain("noopener");

                var text = await dashboard.BacklogLink.TextContentAsync();
                text.Should().Contain("ADO Backlog");
            }
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Header_BacklogLinkIsClickable));
            throw;
        }
    }

    [Fact]
    public async Task Header_HasBottomBorder()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            var borderBottom = await dashboard.Header.EvaluateAsync<string>(
                "el => getComputedStyle(el).borderBottomStyle");
            borderBottom.Should().Be("solid");
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Header_HasBottomBorder));
            throw;
        }
    }

    [Fact]
    public async Task Header_HasLeftAndRightSections()
    {
        var dashboard = await LoadDashboardAsync();
        try
        {
            await Assertions.Expect(dashboard.HeaderLeft).ToBeVisibleAsync();
            await Assertions.Expect(dashboard.HeaderRight).ToBeVisibleAsync();
        }
        catch
        {
            await _fixture.CaptureScreenshotAsync(dashboard.Page, nameof(Header_HasLeftAndRightSections));
            throw;
        }
    }
}