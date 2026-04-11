using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

public class ErrorPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public ErrorPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });
    }

    public ILocator ErrorBanner => _page.Locator(".error-banner");

    public async Task<bool> IsErrorVisibleAsync()
    {
        return await ErrorBanner.IsVisibleAsync();
    }

    public async Task<string> GetErrorTextAsync()
    {
        return await ErrorBanner.InnerTextAsync();
    }
}