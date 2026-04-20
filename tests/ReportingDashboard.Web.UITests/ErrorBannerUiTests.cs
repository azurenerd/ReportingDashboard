using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.Web.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class ErrorBannerUiTests
{
    private readonly PlaywrightFixture _fx;

    public ErrorBannerUiTests(PlaywrightFixture fx) => _fx = fx;

    private static string BaseUrl =>
        Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5080";

    [Fact]
    public async Task Dashboard_AlwaysRendersHeaderOrErrorBanner_NeverBlank()
    {
        var page = await _fx.Browser.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var headerCount = await page.Locator("div.hdr, header.hdr").CountAsync();
        var bannerCount = await page.Locator("div.error-banner[role='alert']").CountAsync();

        (headerCount + bannerCount).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Dashboard_IfErrorBannerPresent_HasStrongHeadingAndPathSpan()
    {
        var page = await _fx.Browser.NewPageAsync();
        await page.GotoAsync(BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var banner = page.Locator("div.error-banner[role='alert']");
        if (await banner.CountAsync() == 0)
        {
            return; // happy path: no banner expected
        }

        (await banner.Locator("strong").CountAsync()).Should().Be(1);
        (await banner.Locator(".error-path").CountAsync()).Should().Be(1);
        (await banner.Locator(".error-message").CountAsync()).Should().Be(1);
        var headingText = await banner.Locator("strong").First.TextContentAsync();
        headingText.Should().NotBeNullOrWhiteSpace();
    }
}