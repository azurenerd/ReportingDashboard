using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

public class HeaderPage
{
    private readonly IPage _page;

    public HeaderPage(IPage page)
    {
        _page = page;
    }

    public ILocator Header => _page.Locator(".hdr");
    public ILocator Title => _page.Locator(".hdr h1");
    public ILocator Subtitle => _page.Locator(".sub");
    public ILocator BacklogLink => _page.Locator(".hdr a[href]");
    public ILocator LegendItems => _page.Locator(".hdr .legend-item, .hdr [style*='gap']");

    public async Task<string?> GetTitleTextAsync()
    {
        return await Title.TextContentAsync();
    }

    public async Task<string?> GetSubtitleTextAsync()
    {
        return await Subtitle.TextContentAsync();
    }

    public async Task<string?> GetBacklogLinkHrefAsync()
    {
        return await BacklogLink.First.GetAttributeAsync("href");
    }
}