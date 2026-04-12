using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

/// <summary>
/// Tests for the App.razor router and overall application shell rendering.
/// Covers: App.razor routing, Blazor circuit initialization, viewport layout,
/// and the full page structure that proves all three dashboard regions render together.
/// </summary>
[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardAppUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;

    public DashboardAppUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _page = await _fixture.CreatePageAsync();
    }

    public async Task DisposeAsync()
    {
        await _page.Context.DisposeAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task App_RootRoute_RendersDashboardPage()
    {
        // The App.razor Router should resolve "/" to Dashboard.razor
        var response = await _page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });

        response.Should().NotBeNull();
        response!.Ok.Should().BeTrue("the root URL should return HTTP 200");

        // Wait for Blazor SignalR circuit to establish and render content
        await _page.WaitForSelectorAsync(".hdr", new PageWaitForSelectorOptions { Timeout = 30000 });

        // Confirm the three major dashboard regions are all present
        var headerCount = await _page.Locator(".hdr").CountAsync();
        var timelineCount = await _page.Locator(".tl-area").CountAsync();
        var heatmapCount = await _page.Locator(".hm-wrap").CountAsync();

        headerCount.Should().Be(1, "exactly one header region should render");
        timelineCount.Should().Be(1, "exactly one timeline region should render");
        heatmapCount.Should().Be(1, "exactly one heatmap region should render");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task App_NotFoundRoute_DisplaysPageNotFoundMessage()
    {
        // App.razor has a <NotFound> block that renders "Page not found"
        await _page.GotoAsync($"{_fixture.BaseUrl}/nonexistent-route", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });

        // Allow Blazor circuit time to initialize and render the NotFound content
        await _page.WaitForTimeoutAsync(5000);

        var bodyText = await _page.Locator("body").TextContentAsync();
        bodyText.Should().Contain("Page not found",
            "navigating to an unknown route should display the NotFound template from App.razor");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task App_ViewportSize_FitsDesignSpecAt1920x1080()
    {
        // The dashboard is designed for 1920x1080 screenshot capture
        await _page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });

        await _page.WaitForSelectorAsync(".hdr", new PageWaitForSelectorOptions { Timeout = 30000 });

        // The body should not have horizontal scrollbar at 1920px wide viewport
        var bodyScrollWidth = await _page.EvaluateAsync<int>("document.body.scrollWidth");
        bodyScrollWidth.Should().BeLessOrEqualTo(1920,
            "dashboard content should fit within 1920px width without horizontal scroll");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task App_BlazorCircuit_EstablishesSignalRConnection()
    {
        await _page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });

        // Blazor Server injects a _framework/blazor.server.js script that manages the SignalR circuit.
        // If the circuit fails, the page would show a reconnection UI or remain blank.
        // Verify that rendered Blazor content is present (not just the static _Host.cshtml shell).
        var h1 = _page.Locator("h1").First;
        await h1.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });

        var titleText = await h1.TextContentAsync();
        titleText.Should().NotBeNullOrWhiteSpace(
            "Blazor circuit must initialize and render dashboard content from DashboardDataService");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task App_EmptyLayout_SuppressesDefaultBlazorChrome()
    {
        // Dashboard uses @layout EmptyLayout to avoid NavMenu, sidebar, etc.
        await _page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });

        await _page.WaitForSelectorAsync(".hdr", new PageWaitForSelectorOptions { Timeout = 30000 });

        // Default Blazor template includes .sidebar, .top-row, .main — none should be present
        var sidebarCount = await _page.Locator(".sidebar").CountAsync();
        var topRowCount = await _page.Locator(".top-row").CountAsync();

        sidebarCount.Should().Be(0, "EmptyLayout should suppress the default Blazor sidebar");
        topRowCount.Should().Be(0, "EmptyLayout should suppress the default Blazor top-row nav");
    }
}