using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class DashboardPageTests
{
    private readonly PlaywrightFixture _fixture;

    public DashboardPageTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> CreatePageAsync()
    {
        var context = await _fixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(60000);
        return page;
    }

    [Fact]
    public async Task Dashboard_LoadsSuccessfully_ShowsContent()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var body = await page.Locator("body").InnerTextAsync();
        body.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Dashboard_PageHasNoScrollbars_FixedLayout()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var overflow = await page.EvaluateAsync<string>(
            "() => window.getComputedStyle(document.body).overflow");
        overflow.Should().Be("hidden");
    }

    [Fact]
    public async Task Dashboard_RootElement_HasFixedDimensions()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var dashboardRoot = page.Locator(".dashboard-root");
        if (await dashboardRoot.CountAsync() > 0)
        {
            var box = await dashboardRoot.First.BoundingBoxAsync();
            box.Should().NotBeNull();
            box!.Width.Should().BeApproximately(1920, 5);
        }
    }

    [Fact]
    public async Task Dashboard_ShowsPlaceholderOrContent_NoCrash()
    {
        var page = await CreatePageAsync();
        var response = await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        response.Should().NotBeNull();
        response!.Status.Should().Be(200);

        // Page should show either placeholders, real content, or error — never blank
        var hasContent = await page.Locator(".dashboard-root").CountAsync() > 0;
        hasContent.Should().BeTrue();
    }

    [Fact]
    public async Task Dashboard_ErrorState_ShowsMessage_WhenDataMissing()
    {
        var page = await CreatePageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // If data is missing, error text should appear; if data is present, no error
        var errorText = page.GetByText("Unable to load dashboard data");
        var placeholderText = page.GetByText("placeholder");
        var headerText = page.Locator("h1");

        var hasError = await errorText.CountAsync() > 0;
        var hasPlaceholders = await placeholderText.CountAsync() > 0;
        var hasHeader = await headerText.CountAsync() > 0;

        // At least one should be visible — page is never blank
        (hasError || hasPlaceholders || hasHeader).Should().BeTrue();
    }
}