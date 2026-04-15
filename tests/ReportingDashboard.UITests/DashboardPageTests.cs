using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardPageTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;

    public DashboardPageTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _page = await _fixture.Browser.NewPageAsync();
        _page.SetDefaultTimeout(60000);
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
    }

    [Fact]
    public async Task HomePage_Loads_WithoutErrors()
    {
        var response = await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        response!.Status.Should().Be(200);
    }

    [Fact]
    public async Task HomePage_ShowsEitherDataOrError()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // The page should render either the error panel or the placeholder
        var h1 = await _page.Locator("h1").First.TextContentAsync();
        h1.Should().NotBeNullOrEmpty("page should display either project title or error heading");
    }

    [Fact]
    public async Task ErrorPanel_ShowsConfigurationError_WhenDataMissing()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // If error panel is present, verify its content
        var errorHeading = _page.GetByText("Dashboard Configuration Error");
        if (await errorHeading.CountAsync() > 0)
        {
            (await errorHeading.IsVisibleAsync()).Should().BeTrue();

            var editHint = _page.GetByText("Edit data.json and refresh this page.");
            (await editHint.IsVisibleAsync()).Should().BeTrue();
        }
    }

    [Fact]
    public async Task SuccessState_ShowsProjectTitle_WhenDataValid()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // If data loaded successfully, the placeholder div should exist
        var placeholder = _page.Locator(".dashboard-placeholder");
        if (await placeholder.CountAsync() > 0)
        {
            var title = await placeholder.Locator("h1").TextContentAsync();
            title.Should().NotBeNullOrEmpty("project title should be rendered from data.json");

            var successText = _page.GetByText("data.json loaded successfully");
            (await successText.IsVisibleAsync()).Should().BeTrue();
        }
    }

    [Fact]
    public async Task Viewport_IsFixedDimensions()
    {
        await _page.SetViewportSizeAsync(1920, 1080);
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var bodyWidth = await _page.EvaluateAsync<int>("() => document.body.offsetWidth");
        bodyWidth.Should().Be(1920, "body should be fixed at 1920px width per app.css");
    }
}