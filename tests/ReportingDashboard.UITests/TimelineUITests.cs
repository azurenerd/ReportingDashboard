using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class TimelineUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;
    private bool _serverAvailable;

    public TimelineUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _page = await _fixture.Browser.NewPageAsync();
        _page.SetDefaultTimeout(60000);

        // Check if the server is available before running tests
        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await httpClient.GetAsync(_fixture.BaseUrl);
            _serverAvailable = response.IsSuccessStatusCode;
        }
        catch
        {
            _serverAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
    }

    private void SkipIfServerUnavailable()
    {
        Skip.If(!_serverAvailable, $"Server not available at {_fixture.BaseUrl}. Start the app with 'dotnet run' before running UI tests.");
    }

    [SkippableFact]
    [Trait("Category", "UI")]
    public async Task Timeline_TlArea_IsVisibleOnPage()
    {
        SkipIfServerUnavailable();

        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var tlArea = _page.Locator(".tl-area");
        await tlArea.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        (await tlArea.IsVisibleAsync()).Should().BeTrue();
    }

    [SkippableFact]
    [Trait("Category", "UI")]
    public async Task Timeline_SvgElement_HasCorrectDimensions()
    {
        SkipIfServerUnavailable();

        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var svg = _page.Locator(".tl-svg-box svg");
        await svg.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var width = await svg.GetAttributeAsync("width");
        var height = await svg.GetAttributeAsync("height");

        width.Should().Be("1560");
        height.Should().Be("185");
    }

    [SkippableFact]
    [Trait("Category", "UI")]
    public async Task Timeline_Sidebar_DisplaysMilestoneIds()
    {
        SkipIfServerUnavailable();

        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var sidebar = _page.Locator(".tl-sidebar");
        await sidebar.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var milestoneIds = _page.Locator(".tl-ms-id");
        var count = await milestoneIds.CountAsync();
        count.Should().BeGreaterOrEqualTo(1, "at least one milestone ID should render in sidebar");
    }

    [SkippableFact]
    [Trait("Category", "UI")]
    public async Task Timeline_NowLine_IsRenderedWithRedStroke()
    {
        SkipIfServerUnavailable();

        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Find the NOW dashed line by its distinctive stroke color and dasharray
        var nowLine = _page.Locator("svg line[stroke='#EA4335'][stroke-dasharray='5,3']");
        var count = await nowLine.CountAsync();
        count.Should().BeGreaterOrEqualTo(1, "NOW dashed line should be present in the SVG");

        // Verify NOW text label exists
        var nowText = _page.GetByText("NOW");
        (await nowText.CountAsync()).Should().BeGreaterOrEqualTo(1);
    }

    [SkippableFact]
    [Trait("Category", "UI")]
    public async Task Timeline_EventMarkers_CirclesAndDiamondsArePresent()
    {
        SkipIfServerUnavailable();

        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var svg = _page.Locator(".tl-svg-box svg");
        await svg.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        // Check for circle elements (checkpoint markers)
        var circles = _page.Locator("svg circle");
        var circleCount = await circles.CountAsync();
        circleCount.Should().BeGreaterOrEqualTo(1, "at least one checkpoint circle should render");

        // Check for polygon elements (diamond markers for poc/production)
        var polygons = _page.Locator("svg polygon");
        var polygonCount = await polygons.CountAsync();
        polygonCount.Should().BeGreaterOrEqualTo(1, "at least one diamond polygon should render");
    }
}