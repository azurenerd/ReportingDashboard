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
    public async Task Dashboard_LoadsSuccessfully_ShowsPlaceholderWithProjectData()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var placeholder = _page.Locator(".placeholder");
        await placeholder.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var heading = await _page.Locator(".placeholder h2").TextContentAsync();
        heading.Should().Contain("Dashboard data loaded successfully");

        var titleText = await _page.Locator(".placeholder").TextContentAsync();
        titleText.Should().Contain("Title:");
        titleText.Should().Contain("Timeline Tracks:");
        titleText.Should().Contain("Heatmap Columns:");
        titleText.Should().Contain("Schema Version:");
    }

    [Fact]
    public async Task Dashboard_MissingDataFile_ShowsErrorMessage()
    {
        await _page.GotoAsync($"{_fixture.BaseUrl}/?data=nonexistent.json");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var errorContainer = _page.Locator(".error-container");
        await errorContainer.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var errorText = await _page.Locator(".error-message").TextContentAsync();
        errorText.Should().Contain("nonexistent.json not found");
        errorText.Should().Contain("Place your data file at wwwroot/data/nonexistent.json");
    }

    [Fact]
    public async Task Dashboard_ViewportIs1920x1080_BodyFitsWithoutScroll()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var overflow = await _page.EvaluateAsync<string>("() => getComputedStyle(document.body).overflow");
        overflow.Should().Be("hidden");

        var bodyWidth = await _page.EvaluateAsync<int>("() => document.body.offsetWidth");
        bodyWidth.Should().Be(1920);
    }

    [Fact]
    public async Task Dashboard_BlazorReconnectModal_IsHidden()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var display = await _page.EvaluateAsync<string>(
            "() => { const el = document.getElementById('components-reconnect-modal'); " +
            "return el ? getComputedStyle(el).display : 'none'; }");
        display.Should().Be("none");
    }

    [Fact]
    public async Task Dashboard_CssCustomProperties_AreDefined()
    {
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var fontFamily = await _page.EvaluateAsync<string>(
            "() => getComputedStyle(document.body).fontFamily");
        fontFamily.Should().Contain("Segoe UI");
    }
}