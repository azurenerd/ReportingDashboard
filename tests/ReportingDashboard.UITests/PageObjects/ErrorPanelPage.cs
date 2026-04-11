using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

public class ErrorPanelPage
{
    private readonly IPage _page;

    public ErrorPanelPage(IPage page)
    {
        _page = page;
    }

    public ILocator Panel => _page.Locator(".error-panel");
    public ILocator Icon => _page.Locator(".error-icon");
    public ILocator Title => _page.Locator(".error-title");
    public ILocator Details => _page.Locator(".error-details");
    public ILocator Help => _page.Locator(".error-help");

    public async Task<bool> IsVisibleAsync()
    {
        return await Panel.CountAsync() > 0 && await Panel.IsVisibleAsync();
    }

    public async Task<string> GetTitleTextAsync()
    {
        return await Title.InnerTextAsync();
    }

    public async Task<string> GetDetailsTextAsync()
    {
        return await Details.InnerTextAsync();
    }

    public async Task<string> GetHelpTextAsync()
    {
        return await Help.InnerTextAsync();
    }
}