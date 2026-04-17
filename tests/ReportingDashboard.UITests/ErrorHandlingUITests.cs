using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class ErrorHandlingUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;

    public ErrorHandlingUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _page = await _fixture.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        _page.SetDefaultTimeout(60000);
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
    }

    [Fact]
    public async Task Dashboard_InvalidFilenameWithTraversal_ShowsPathSanitizationError()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/?data=../appsettings.json");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorContainer = _page.Locator(".error-container");
        await errorContainer.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var errorText = await _page.Locator(".error-message").TextContentAsync();
        errorText.Should().Contain("Path separators and '..' are not allowed");
    }

    [Fact]
    public async Task Dashboard_NonJsonExtension_ShowsExtensionError()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/?data=readme.txt");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorContainer = _page.Locator(".error-container");
        await errorContainer.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var errorText = await _page.Locator(".error-message").TextContentAsync();
        errorText.Should().Contain("Only .json files are supported");
    }

    [Fact]
    public async Task Dashboard_ErrorMessages_NeverContainStackTraces()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/?data=nonexistent.json");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorContainer = _page.Locator(".error-container");
        await errorContainer.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var fullHtml = await _page.ContentAsync();
        fullHtml.Should().NotContain("at ReportingDashboard");
        fullHtml.Should().NotContain("StackTrace");
        fullHtml.Should().NotContain("   at ");
    }

    [Fact]
    public async Task Dashboard_ExplicitDataParam_LoadsSameAsDefault()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/?data=data.json");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var placeholder = _page.Locator(".placeholder");
        await placeholder.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var heading = await _page.Locator(".placeholder h2").TextContentAsync();
        heading.Should().Contain("Dashboard data loaded successfully");
    }

    [Fact]
    public async Task Dashboard_ErrorContainer_IsCenteredAndVisible()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/?data=nonexistent.json");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorContainer = _page.Locator(".error-container");
        await errorContainer.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var isVisible = await errorContainer.IsVisibleAsync();
        isVisible.Should().BeTrue();

        var errorMessage = _page.Locator(".error-message");
        var msgVisible = await errorMessage.IsVisibleAsync();
        msgVisible.Should().BeTrue();
    }
}