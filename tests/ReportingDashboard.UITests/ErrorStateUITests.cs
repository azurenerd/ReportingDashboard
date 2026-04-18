using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Trait("Category", "UI")]
[Collection("Playwright")]
public class ErrorStateUITests
{
    private readonly PlaywrightFixture _fixture;

    public ErrorStateUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> NewPageAsync()
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
    public async Task Dashboard_ErrorContainer_H2TextMatchesSourceCode()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorContainer = page.Locator(".error-container");
        if (await errorContainer.IsVisibleAsync())
        {
            var h2Text = await page.Locator(".error-container h2").TextContentAsync();
            h2Text.Should().Be("Unable to load dashboard data.");
        }
    }

    [Fact]
    public async Task Dashboard_ErrorContainer_SubtextMatchesSourceCode()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorContainer = page.Locator(".error-container");
        if (await errorContainer.IsVisibleAsync())
        {
            var pText = await page.Locator(".error-container p").First.TextContentAsync();
            pText.Should().Contain("Please check that data.json exists and contains valid JSON.");
        }
    }

    [Fact]
    public async Task Dashboard_ErrorDetail_IsVisibleWhenErrorContainerPresent()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorContainer = page.Locator(".error-container");
        if (await errorContainer.IsVisibleAsync())
        {
            var detailCount = await page.Locator(".error-detail").CountAsync();
            detailCount.Should().Be(1);

            var detailText = await page.Locator(".error-detail").TextContentAsync();
            detailText.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public async Task Dashboard_NoUnhandledExceptionPage_OnLoad()
    {
        var page = await NewPageAsync();
        var response = await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        response!.Status.Should().Be(200);
        var content = await page.ContentAsync();
        content.Should().NotContain("An unhandled exception occurred");
        content.Should().NotContain("blazor-error-ui");
    }

    [Fact]
    public async Task Dashboard_PageBody_ContainsEitherErrorContainerOrDataContent()
    {
        var page = await NewPageAsync();
        await page.GotoAsync(_fixture.BaseUrl);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorCount = await page.Locator(".error-container").CountAsync();
        var bodyHtml = await page.Locator("body").InnerHTMLAsync();

        // Either error state is shown, or some data content is present - never a blank page
        bodyHtml.Should().NotBeNullOrWhiteSpace();
        (errorCount > 0 || bodyHtml.Length > 50).Should().BeTrue();
    }
}