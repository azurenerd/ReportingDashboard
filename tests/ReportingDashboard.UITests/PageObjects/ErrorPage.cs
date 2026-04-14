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

    // Selectors
    public ILocator ErrorPanel => _page.Locator(".error-panel");
    public ILocator ErrorMessage => _page.Locator(".error-message");
    public ILocator ErrorHelp => _page.Locator(".error-help");

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<bool> IsErrorVisibleAsync()
    {
        return await ErrorPanel.CountAsync() > 0;
    }

    public async Task<string> GetErrorTextAsync()
    {
        return await ErrorPanel.InnerTextAsync();
    }
}