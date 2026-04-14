using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

[Collection("Playwright")]
[Trait("Category", "UI")]
public class DashboardHeaderUITests : IAsyncLifetime
{
    private readonly PlaywrightFixture _fixture;
    private IPage _page = null!;

    public DashboardHeaderUITests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _page = await _fixture.Browser.NewPageAsync();
        _page.SetDefaultTimeout(60000);
        await _page.SetViewportSizeAsync(1920, 1080);
        await _page.GotoAsync(_fixture.BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DashboardHeader_IsVisibleOnPage()
    {
        var header = _page.Locator(".hdr");
        await header.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        (await header.IsVisibleAsync()).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DashboardHeader_DisplaysProjectTitle()
    {
        var title = _page.Locator(".hdr h1");
        var text = await title.TextContentAsync();
        text.Should().NotBeNullOrWhiteSpace("header should display a project title");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DashboardHeader_DisplaysSubtitle()
    {
        var subtitle = _page.Locator(".sub");
        var text = await subtitle.TextContentAsync();
        text.Should().NotBeNullOrWhiteSpace("header should display a subtitle");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DashboardHeader_ContainsBacklogLink()
    {
        var link = _page.Locator(".hdr a");
        var count = await link.CountAsync();
        count.Should().BeGreaterThan(0, "header should contain at least one link");

        var text = await link.First.TextContentAsync();
        text.Should().Contain("ADO Backlog");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task DashboardHeader_HasLegendItems()
    {
        var header = _page.Locator(".hdr");
        var headerText = await header.TextContentAsync();
        headerText.Should().NotBeNull();
        // Legend should mention milestone types
        headerText!.Should().Contain("Milestone");
    }
}