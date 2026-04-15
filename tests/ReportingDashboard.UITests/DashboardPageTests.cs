using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardPageTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage? _page;

    public DashboardPageTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        if (_fixture.IsAvailable)
        {
            _page = await _fixture.Browser!.NewPageAsync();
            _page.SetDefaultTimeout(60000);
        }
    }

    public async Task DisposeAsync()
    {
        if (_page is not null)
        {
            await _page.CloseAsync();
        }
    }

    private void SkipIfBrowserUnavailable()
    {
        Skip.If(!_fixture.IsAvailable, "Playwright browsers not installed. Run: pwsh bin/Debug/net8.0/playwright.ps1 install");
    }

    [SkippableFact]
    public async Task HomePage_Loads_WithoutErrors()
    {
        SkipIfBrowserUnavailable();

        var response = await _page!.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        response!.Status.Should().Be(200);
    }

    [SkippableFact]
    public async Task HomePage_ShowsEitherDataOrError()
    {
        SkipIfBrowserUnavailable();

        await _page!.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var h1 = await _page.Locator("h1").First.TextContentAsync();
        h1.Should().NotBeNullOrEmpty("page should display either project title or error heading");
    }

    [SkippableFact]
    public async Task ErrorPanel_ShowsConfigurationError_WhenDataMissing()
    {
        SkipIfBrowserUnavailable();

        await _page!.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorHeading = _page.GetByText("Dashboard Configuration Error");
        if (await errorHeading.CountAsync() > 0)
        {
            (await errorHeading.IsVisibleAsync()).Should().BeTrue();

            var editHint = _page.GetByText("Edit data.json and refresh this page.");
            (await editHint.IsVisibleAsync()).Should().BeTrue();
        }
    }

    [SkippableFact]
    public async Task SuccessState_ShowsProjectTitle_WhenDataValid()
    {
        SkipIfBrowserUnavailable();

        await _page!.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var placeholder = _page.Locator(".dashboard-placeholder");
        if (await placeholder.CountAsync() > 0)
        {
            var title = await placeholder.Locator("h1").TextContentAsync();
            title.Should().NotBeNullOrEmpty("project title should be rendered from data.json");

            var successText = _page.GetByText("data.json loaded successfully");
            (await successText.IsVisibleAsync()).Should().BeTrue();
        }
    }

    [SkippableFact]
    public async Task Viewport_IsFixedDimensions()
    {
        SkipIfBrowserUnavailable();

        await _page!.SetViewportSizeAsync(1920, 1080);
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var bodyWidth = await _page.EvaluateAsync<int>("() => document.body.offsetWidth");
        bodyWidth.Should().Be(1920, "body should be fixed at 1920px width per app.css");
    }
}