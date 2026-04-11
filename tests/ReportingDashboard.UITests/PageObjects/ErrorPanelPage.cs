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
    public ILocator Heading => _page.Locator(".error-panel h2");
    public ILocator Message => _page.Locator(".error-panel p:not(.error-hint)");
    public ILocator Hint => _page.Locator(".error-hint");

    public async Task<bool> IsVisibleAsync()
    {
        return await Panel.IsVisibleAsync();
    }

    public async Task<string> GetHeadingTextAsync()
    {
        return await Heading.InnerTextAsync();
    }

    public async Task<string> GetHintTextAsync()
    {
        return await Hint.InnerTextAsync();
    }
}