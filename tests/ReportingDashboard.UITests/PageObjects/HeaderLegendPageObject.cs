using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page object encapsulating selectors for the Header component's timeline legend.
/// Works with the inline-styled Header.razor (Components/Header.razor) from PR #534.
/// </summary>
public class HeaderLegendPageObject
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public HeaderLegendPageObject(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }

    // Header container
    public ILocator Header => _page.Locator(".hdr");

    // Title and subtitle
    public ILocator Title => _page.Locator(".hdr h1");
    public ILocator Subtitle => _page.Locator(".hdr .sub");
    public ILocator BacklogLink => _page.Locator(".hdr h1 a");

    // Legend container: the second direct child div of .hdr
    // The inline-styled variant uses style="display:flex;gap:22px;..." on the legend div
    public ILocator LegendContainer => _page.Locator(".hdr > div:last-child");

    // Legend items: the four top-level spans inside the legend div
    public ILocator LegendItems => LegendContainer.Locator("> span");

    // Individual legend items by position
    public ILocator PocMilestoneItem => LegendContainer.Locator("> span:nth-child(1)");
    public ILocator ProductionReleaseItem => LegendContainer.Locator("> span:nth-child(2)");
    public ILocator CheckpointItem => LegendContainer.Locator("> span:nth-child(3)");
    public ILocator NowLineItem => LegendContainer.Locator("> span:nth-child(4)");

    // Symbol spans inside each legend item (the inner span with background color)
    public ILocator PocDiamondSymbol => PocMilestoneItem.Locator("> span");
    public ILocator ProductionDiamondSymbol => ProductionReleaseItem.Locator("> span");
    public ILocator CheckpointCircleSymbol => CheckpointItem.Locator("> span");
    public ILocator NowBarSymbol => NowLineItem.Locator("> span");
}