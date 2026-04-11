using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page object focused on the Header component (.hdr) from PR #533.
/// Covers title, subtitle, backlog link, and inline-styled legend symbols.
/// </summary>
public class HeaderPageObject
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public HeaderPageObject(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public IPage Page => _page;

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });
    }

    // Header container
    public ILocator HeaderContainer => _page.Locator(".hdr");

    // Title elements
    public ILocator TitleH1 => _page.Locator(".hdr h1");
    public ILocator SubtitleDiv => _page.Locator(".hdr .sub");

    // Backlog link (inside h1)
    public ILocator BacklogLink => _page.Locator(".hdr h1 a");

    // Legend container (right side div with inline flex style)
    // The inline Header.razor uses inline styles, not .legend class
    public ILocator LegendContainer => _page.Locator(".hdr > div:last-child");

    // Legend items (span elements inside legend container)
    public ILocator LegendSpans => _page.Locator(".hdr > div:last-child > span");

    // Individual legend items by text content
    public ILocator PocMilestoneLegend => _page.Locator("span:has-text('PoC Milestone')");
    public ILocator ProductionReleaseLegend => _page.Locator("span:has-text('Production Release')");
    public ILocator CheckpointLegend => _page.Locator("span:has-text('Checkpoint')");
    public ILocator NowLegend => _page.Locator("span:has-text('Now (')");

    // Helpers
    public async Task<bool> IsHeaderVisibleAsync() =>
        await HeaderContainer.CountAsync() > 0 && await HeaderContainer.IsVisibleAsync();

    public async Task<string> GetTitleTextAsync() =>
        await TitleH1.TextContentAsync() ?? "";

    public async Task<string> GetSubtitleTextAsync() =>
        await SubtitleDiv.TextContentAsync() ?? "";

    public async Task<bool> HasBacklogLinkAsync() =>
        await BacklogLink.CountAsync() > 0;

    public async Task<string?> GetBacklogHrefAsync() =>
        await BacklogLink.GetAttributeAsync("href");

    public async Task<string?> GetBacklogTargetAsync() =>
        await BacklogLink.GetAttributeAsync("target");

    public async Task<string?> GetBacklogRelAsync() =>
        await BacklogLink.GetAttributeAsync("rel");

    public async Task<int> GetLegendItemCountAsync() =>
        await LegendSpans.CountAsync();
}