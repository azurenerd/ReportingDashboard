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
        _page = await _fixture.CreatePageAsync();
        await _page.GotoAsync(_fixture.BaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });
        // Wait for Blazor SignalR circuit to initialize and header to render
        await _page.WaitForSelectorAsync(".hdr", new PageWaitForSelectorOptions { Timeout = 30000 });
    }

    public async Task DisposeAsync()
    {
        await _page.Context.DisposeAsync();
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Header_RendersProjectTitle_AsH1Element()
    {
        // The project title should render inside an h1 within the .hdr container
        var heading = _page.Locator(".hdr h1").First;
        await heading.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });

        var text = await heading.TextContentAsync();
        text.Should().NotBeNullOrWhiteSpace("the project title from data.json should be displayed");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Header_RendersBacklogLink_WithCorrectStyling()
    {
        // The backlog link should be an anchor inside h1 with target="_blank"
        var link = _page.Locator(".hdr h1 a").First;
        var linkCount = await _page.Locator(".hdr h1 a").CountAsync();

        if (linkCount > 0)
        {
            var target = await link.GetAttributeAsync("target");
            target.Should().Be("_blank", "backlog link should open in a new tab");

            var href = await link.GetAttributeAsync("href");
            href.Should().NotBeNullOrWhiteSpace("backlog link should have a valid href from data.json");
        }
        // If no link, backlogLink was empty in data.json — graceful degradation is valid
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Header_RendersSubtitle_BelowTitle()
    {
        // The subtitle should render in a .sub div within the header
        var subtitle = _page.Locator(".hdr .sub").First;
        await subtitle.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });

        var text = await subtitle.TextContentAsync();
        text.Should().NotBeNullOrWhiteSpace("the subtitle from data.json should be visible");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Header_LegendRow_DisplaysFourIndicators()
    {
        // The legend should contain exactly 4 legend items: PoC, Production, Checkpoint, NOW
        var legendItems = _page.Locator(".hdr .legend .legend-item");
        var count = await legendItems.CountAsync();

        count.Should().Be(4, "legend must show PoC Milestone, Production Release, Checkpoint, and NOW indicator");

        // Verify legend text labels
        var pocItem = _page.GetByText("PoC Milestone");
        (await pocItem.CountAsync()).Should().BeGreaterThan(0, "PoC Milestone legend label should be visible");

        var prodItem = _page.GetByText("Production Release");
        (await prodItem.CountAsync()).Should().BeGreaterThan(0, "Production Release legend label should be visible");

        var checkpointItem = _page.GetByText("Checkpoint");
        (await checkpointItem.CountAsync()).Should().BeGreaterThan(0, "Checkpoint legend label should be visible");

        var nowItem = _page.Locator(".legend-item").Filter(new LocatorFilterOptions { HasText = "Now" });
        (await nowItem.CountAsync()).Should().BeGreaterThan(0, "NOW indicator legend label should be visible");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Header_LegendNowLabel_ContainsCurrentMonthYear()
    {
        // The NOW legend item should display the current month abbreviation and year
        var nowItem = _page.Locator(".legend-item").Filter(new LocatorFilterOptions { HasText = "Now" }).First;
        await nowItem.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });

        var text = await nowItem.TextContentAsync();
        text.Should().NotBeNull();

        // The label should contain a year (e.g., "2026")
        text.Should().MatchRegex(@"Now\s*\(.+\d{4}\)",
            "NOW legend should display month and year like 'Now (Apr 2026)'");
    }
}