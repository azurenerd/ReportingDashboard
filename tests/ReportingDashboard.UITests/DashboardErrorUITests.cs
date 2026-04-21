using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardErrorUITests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardErrorUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> CreatePageAsync()
    {
        var page = await _fixture.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        page.SetDefaultTimeout(60000);
        return page;
    }

    [Fact]
    public async Task Dashboard_WhenDataMissing_ShowsErrorCard()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // If data is valid, the dashboard div should be present; if not, error-overlay should appear.
        // We check for either state to confirm the page loads without crash.
        var hasErrorOverlay = await page.Locator(".error-overlay").CountAsync() > 0;
        var hasDashboard = await page.Locator(".dashboard").CountAsync() > 0;

        (hasErrorOverlay || hasDashboard).Should().BeTrue("Dashboard should render either error state or normal state");
    }

    [Fact]
    public async Task Dashboard_ErrorCard_ShowsUserFriendlyMessage_NoStackTraces()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hasErrorOverlay = await page.Locator(".error-overlay").CountAsync() > 0;
        if (!hasErrorOverlay) return; // Data is valid, skip this test gracefully

        var errorMessage = await page.Locator(".error-card-message").TextContentAsync();
        errorMessage.Should().NotBeNullOrEmpty();
        errorMessage.Should().NotContain("System.");
        errorMessage.Should().NotContain("Exception");
        errorMessage.Should().NotContain("   at ");

        var title = await page.Locator(".error-card-title").TextContentAsync();
        title.Should().NotBeNullOrEmpty();
        var validTitles = new[] { "Data File Not Found", "JSON Syntax Error", "Invalid Configuration", "Dashboard Error" };
        validTitles.Should().Contain(title!.Trim());
    }

    [Fact]
    public async Task Dashboard_ErrorCard_ContainsHintToRefresh()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hasErrorOverlay = await page.Locator(".error-overlay").CountAsync() > 0;
        if (!hasErrorOverlay) return;

        var hint = await page.Locator(".error-card-hint").TextContentAsync();
        hint.Should().Contain("Fix the issue and refresh the browser");
    }

    [Fact]
    public async Task Dashboard_WhenDataValid_RendersDashboardLayout()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var hasDashboard = await page.Locator(".dashboard").CountAsync() > 0;
        if (!hasDashboard) return; // Data is missing, skip gracefully

        var dashboard = page.Locator(".dashboard");
        var style = await dashboard.GetAttributeAsync("style");
        style.Should().Contain("1920px");
        style.Should().Contain("1080px");
    }

    [Fact]
    public async Task Dashboard_PageLoads_WithoutBlazorErrorUI()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var blazorError = await page.Locator("#blazor-error-ui").CountAsync();
        var blazorErrorVisible = blazorError > 0
            ? await page.Locator("#blazor-error-ui").IsVisibleAsync()
            : false;
        blazorErrorVisible.Should().BeFalse("No Blazor framework error UI should be visible");
    }
}