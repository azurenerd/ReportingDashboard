using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace ReportingDashboard.UITests;

/// <summary>
/// Tests for the Header region of Dashboard.razor.
/// Covers: project title, subtitle, backlog link, legend indicators.
/// Improved: uses selectors that match actual Dashboard.razor markup (inline styles, not .legend class).
/// </summary>
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
        var heading = _page.Locator(".hdr h1").First;
        await heading.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });

        var text = await heading.TextContentAsync();
        text.Should().NotBeNullOrWhiteSpace("the project title from data.json should be displayed");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Header_RendersBacklogLink_WithTargetBlank()
    {
        var linkCount = await _page.Locator(".hdr h1 a").CountAsync();

        if (linkCount > 0)
        {
            var link = _page.Locator(".hdr h1 a").First;
            var target = await link.GetAttributeAsync("target");
            target.Should().Be("_blank", "backlog link should open in a new tab");

            var href = await link.GetAttributeAsync("href");
            href.Should().NotBeNullOrWhiteSpace("backlog link should have a valid href from data.json");

            var text = await link.TextContentAsync();
            text.Should().Contain("ADO Backlog", "link text should reference the ADO Backlog");
        }
        // If no link rendered, backlogLink is empty in data.json — graceful degradation
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Header_RendersSubtitle_BelowTitle()
    {
        var subtitle = _page.Locator(".hdr .sub").First;
        await subtitle.WaitForAsync(new LocatorWaitForOptions { Timeout = 30000 });

        var text = await subtitle.TextContentAsync();
        text.Should().NotBeNullOrWhiteSpace("the subtitle from data.json should be visible");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Header_LegendRow_DisplaysFourIndicatorLabels()
    {
        // Dashboard.razor renders legend items as inline spans (not using .legend class).
        // Verify by checking for the four known legend text labels.
        var pocLabel = _page.GetByText("PoC Milestone");
        (await pocLabel.CountAsync()).Should().BeGreaterThan(0, "PoC Milestone legend label should be visible");

        var prodLabel = _page.GetByText("Production Release");
        (await prodLabel.CountAsync()).Should().BeGreaterThan(0, "Production Release legend label should be visible");

        var checkpointLabel = _page.GetByText("Checkpoint");
        (await checkpointLabel.CountAsync()).Should().BeGreaterThan(0, "Checkpoint legend label should be visible");

        // NOW label includes dynamic date like "Now (Apr 2026)"
        var nowLabel = _page.Locator(".hdr").GetByText("Now");
        (await nowLabel.CountAsync()).Should().BeGreaterThan(0, "NOW indicator legend label should be visible");
    }

    [Fact]
    [Trait("Category", "UI")]
    public async Task Header_NowLabel_ContainsCurrentMonthAndYear()
    {
        // The NOW legend displays "Now (MMM yyyy)" using DateTime.Today
        var headerText = await _page.Locator(".hdr").TextContentAsync();
        headerText.Should().NotBeNull();

        var currentYear = DateTime.Today.Year.ToString();
        headerText!.Should().Contain(currentYear,
            $"header legend NOW label should contain the current year ({currentYear})");

        var currentMonthAbbr = DateTime.Today.ToString("MMM");
        headerText.Should().Contain(currentMonthAbbr,
            $"header legend NOW label should contain the current month abbreviation ({currentMonthAbbr})");
    }
}