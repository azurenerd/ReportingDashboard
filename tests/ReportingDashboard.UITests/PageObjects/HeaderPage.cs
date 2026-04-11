using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page object for interacting with the Header component (.hdr) of the dashboard.
/// Encapsulates all header-related selectors and actions.
/// </summary>
public class HeaderPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    // Selectors
    private const string HdrSelector = ".hdr";
    private const string TitleSelector = ".hdr h1";
    private const string SubtitleSelector = ".hdr .sub";
    private const string BacklogLinkSelector = ".hdr h1 a";
    private const string LegendContainerSelector = ".hdr div[style*='gap:22px']";
    private const string LegendItemSelector = ".hdr div[style*='gap:22px'] > span";
    private const string PocSymbolSelector = "span[style*='#F4B400']";
    private const string ProductionSymbolSelector = "span[style*='#34A853'][style*='rotate']";
    private const string CheckpointSymbolSelector = "span[style*='border-radius'][style*='#999']";
    private const string NowBarSymbolSelector = "span[style*='#EA4335'][style*='height:14px']";

    public HeaderPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });
    }

    public ILocator Header => _page.Locator(HdrSelector);
    public ILocator Title => _page.Locator(TitleSelector);
    public ILocator Subtitle => _page.Locator(SubtitleSelector);
    public ILocator BacklogLink => _page.Locator(BacklogLinkSelector);
    public ILocator LegendContainer => _page.Locator(LegendContainerSelector);
    public ILocator LegendItems => _page.Locator(LegendItemSelector);
    public ILocator PocSymbol => _page.Locator(PocSymbolSelector).First;
    public ILocator ProductionSymbol => _page.Locator(ProductionSymbolSelector).First;
    public ILocator CheckpointSymbol => _page.Locator(CheckpointSymbolSelector).First;
    public ILocator NowBarSymbol => _page.Locator(NowBarSymbolSelector).First;

    public async Task<string> GetTitleTextAsync()
    {
        return await Title.TextContentAsync() ?? "";
    }

    public async Task<string> GetSubtitleTextAsync()
    {
        return await Subtitle.TextContentAsync() ?? "";
    }

    public async Task<int> GetLegendItemCountAsync()
    {
        return await LegendItems.CountAsync();
    }

    public async Task<string> GetLegendItemTextAsync(int index)
    {
        return await LegendItems.Nth(index).TextContentAsync() ?? "";
    }

    public async Task<string?> GetBacklogLinkHrefAsync()
    {
        return await BacklogLink.GetAttributeAsync("href");
    }

    public async Task<string?> GetElementStyleAsync(ILocator locator)
    {
        return await locator.GetAttributeAsync("style");
    }

    public async Task<bool> IsHeaderVisibleAsync()
    {
        return await Header.IsVisibleAsync();
    }

    public async Task<bool> IsLegendVisibleAsync()
    {
        return await LegendContainer.IsVisibleAsync();
    }
}