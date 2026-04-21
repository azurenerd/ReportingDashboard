using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.Web.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class ErrorBannerUITests
{
    private readonly PlaywrightFixture _fx;
    public ErrorBannerUITests(PlaywrightFixture fx) => _fx = fx;

    [Fact]
    public async Task Root_FirstByteHtml_ContainsFullyRenderedContent_OrErrorBanner()
    {
        var page = await _fx.NewPageAsync();
        var response = await page.GotoAsync(_fx.BaseUrl);
        response.Should().NotBeNull();
        response!.Status.Should().Be(200);

        var html = await response.TextAsync();
        // First-byte SSR: either a happy-path dashboard (contains <svg) or a degraded error banner.
        var hasHappyPath = html.Contains("<svg") && html.Contains("hm-grid");
        var hasBanner = html.Contains("error-banner");
        (hasHappyPath || hasBanner).Should().BeTrue("root must render fully on first byte, not via client JS");
    }

    [Fact]
    public async Task ErrorBanner_WhenPresent_HasAlertRoleAndExpectedText()
    {
        var page = await _fx.NewPageAsync();
        await page.GotoAsync(_fx.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var bannerCount = await page.Locator(".error-banner").CountAsync();
        if (bannerCount == 0)
        {
            return; // happy path; banner UI not exercised here
        }

        var role = await page.Locator(".error-banner").First.GetAttributeAsync("role");
        role.Should().Be("alert");

        var text = await page.Locator(".error-banner .error-text").First.InnerTextAsync();
        text.Should().ContainAny("data.json not found", "Failed to load data.json", "data.json validation failed");
    }

    [Fact]
    public async Task Root_DoesNotExposeYsodStackTrace()
    {
        var page = await _fx.NewPageAsync();
        var response = await page.GotoAsync(_fx.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        response!.Status.Should().Be(200);
        var html = await response.TextAsync();
        html.Should().NotContain("An unhandled exception occurred while processing the request");
        html.Should().NotContain("HEADERS");
    }
}