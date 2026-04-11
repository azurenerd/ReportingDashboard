using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

public class ErrorPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public ErrorPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task NavigateToAsync(string path = "/")
    {
        await _page.GotoAsync($"{_baseUrl}{path}", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });
    }

    public ILocator ErrorContainer => _page.Locator(".dashboard-error");
    public ILocator ErrorContent => _page.Locator(".dashboard-error-content");
    public ILocator ErrorHeading => _page.Locator(".dashboard-error-content h2");
    public ILocator ErrorText => _page.Locator(".dashboard-error-content p");

    public IPage Page => _page;

    public async Task<string> GetErrorTextAsync()
    {
        return await ErrorText.TextContentAsync() ?? "";
    }

    public async Task<bool> IsErrorVisibleAsync()
    {
        return await ErrorContainer.CountAsync() > 0 && await ErrorContainer.IsVisibleAsync();
    }
}