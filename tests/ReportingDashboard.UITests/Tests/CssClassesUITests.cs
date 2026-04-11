using FluentAssertions;
using Microsoft.Playwright;
using ReportingDashboard.UITests.Infrastructure;
using ReportingDashboard.UITests.PageObjects;
using Xunit;

namespace ReportingDashboard.UITests.Tests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class CssClassesUITests
{
    private readonly PlaywrightFixture _fixture;

    public CssClassesUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData(".hdr")]
    [InlineData(".tl-area")]
    [InlineData(".tl-svg-box")]
    [InlineData(".hm-wrap")]
    [InlineData(".hm-grid")]
    [InlineData(".hm-corner")]
    [InlineData(".hm-col-hdr")]
    public async Task CssClass_IsPresent_InRenderedPage(string cssSelector)
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var elements = page.Locator(cssSelector);
            var count = await elements.CountAsync();
            count.Should().BeGreaterThanOrEqualTo(1, $"CSS class '{cssSelector}' should be present in the page");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, $"CssClass_{cssSelector.Replace(".", "")}");
            throw;
        }
    }

    [Theory]
    [InlineData(".ship-hdr")]
    [InlineData(".prog-hdr")]
    [InlineData(".carry-hdr")]
    [InlineData(".block-hdr")]
    public async Task CategoryRowHeader_IsPresent(string cssSelector)
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var elements = page.Locator(cssSelector);
            var count = await elements.CountAsync();
            count.Should().Be(1, $"exactly one '{cssSelector}' row header should exist");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, $"CategoryHeader_{cssSelector.Replace(".", "")}");
            throw;
        }
    }

    [Theory]
    [InlineData(".ship-cell")]
    [InlineData(".prog-cell")]
    [InlineData(".carry-cell")]
    [InlineData(".block-cell")]
    public async Task CategoryCells_ArePresent(string cssSelector)
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var elements = page.Locator(cssSelector);
            var count = await elements.CountAsync();
            count.Should().BeGreaterThanOrEqualTo(1, $"at least one '{cssSelector}' cell should exist");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, $"CategoryCells_{cssSelector.Replace(".", "")}");
            throw;
        }
    }

    [Fact]
    public async Task HeatmapTitle_IsPresent_WithCorrectStyling()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var title = page.Locator(".hm-title");
            await Assertions.Expect(title).ToBeVisibleAsync();

            var textTransform = await title.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).textTransform");
            textTransform.Should().Be("uppercase");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "HeatmapTitle_Styling");
            throw;
        }
    }

    [Fact]
    public async Task WorkItems_HaveBulletPseudoElement()
    {
        var page = await _fixture.NewPageAsync();
        var dashboard = new DashboardPage(page, _fixture.BaseUrl);
        try
        {
            await dashboard.NavigateAsync();
            await dashboard.WaitForDashboardLoadedAsync();

            var item = page.Locator(".it").First;
            var paddingLeft = await item.EvaluateAsync<string>(
                "el => window.getComputedStyle(el).paddingLeft");
            paddingLeft.Should().Be("12px", "items should have left padding for the bullet pseudo-element");
        }
        catch (Exception)
        {
            await _fixture.CaptureScreenshotAsync(page, "WorkItems_Bullet");
            throw;
        }
    }
}