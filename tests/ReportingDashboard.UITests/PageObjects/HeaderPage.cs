using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

public class HeaderPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public HeaderPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    // Selectors
    private const string HeaderSelector = "div.hdr";
    private const string TitleSelector = "div.hdr h1";
    private const string BacklogLinkSelector = "div.hdr h1 a";
    private const string SubtitleSelector = "div.hdr div.sub";
    private const string LegendContainerSelector = "div.hdr > div:last-child";
    private const string DashboardRootSelector = "div.dashboard-root";
    private const string ErrorPanelSelector = "div.error-panel";

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });
    }

    public ILocator Header => _page.Locator(HeaderSelector);
    public ILocator Title => _page.Locator(TitleSelector);
    public ILocator BacklogLink => _page.Locator(BacklogLinkSelector);
    public ILocator Subtitle => _page.Locator(SubtitleSelector);
    public ILocator LegendContainer => _page.Locator(LegendContainerSelector);
    public ILocator DashboardRoot => _page.Locator(DashboardRootSelector);
    public ILocator ErrorPanel => _page.Locator(ErrorPanelSelector);

    public ILocator GetLegendItemByText(string text) =>
        _page.Locator($"div.hdr span:has-text('{text}')");

    public async Task<string> GetTitleTextAsync() =>
        await Title.TextContentAsync() ?? string.Empty;

    public async Task<string> GetSubtitleTextAsync() =>
        await Subtitle.TextContentAsync() ?? string.Empty;

    public async Task<string?> GetBacklogLinkHrefAsync() =>
        await BacklogLink.GetAttributeAsync("href");

    public async Task<string?> GetBacklogLinkTargetAsync() =>
        await BacklogLink.GetAttributeAsync("target");

    public async Task<bool> IsHeaderVisibleAsync() =>
        await Header.IsVisibleAsync();

    public async Task<bool> IsErrorPanelVisibleAsync()
    {
        var count = await ErrorPanel.CountAsync();
        return count > 0 && await ErrorPanel.IsVisibleAsync();
    }

    public async Task<int> GetLegendItemCountAsync()
    {
        var items = _page.Locator("div.hdr > div:last-child > span");
        return await items.CountAsync();
    }
}