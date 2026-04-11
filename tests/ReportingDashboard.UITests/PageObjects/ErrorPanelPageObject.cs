using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page object for the ErrorPanel component, providing locators and helpers
/// specific to the error state UI rendered by Components/ErrorPanel.razor.
/// </summary>
public class ErrorPanelPageObject
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public ErrorPanelPageObject(IPage page, string baseUrl)
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

    // Error panel container
    public ILocator ErrorPanel => _page.Locator(".error-panel");
    public ILocator ErrorContent => _page.Locator(".error-content");

    // Error indicator (red ? symbol)
    public ILocator ErrorIndicator => _page.Locator(".error-content > div:nth-child(1)");

    // Static title: "Dashboard data could not be loaded"
    public ILocator ErrorTitle => _page.Locator(".error-content > div:nth-child(2)");

    // Dynamic error message in monospace
    public ILocator ErrorMessage => _page.Locator(".error-content > div:nth-child(3)");

    // Help text: "Check data.json for errors..."
    public ILocator HelpText => _page.Locator(".error-content > div:nth-child(4)");

    // Dashboard root (should NOT be present when error panel is shown)
    public ILocator DashboardRoot => _page.Locator(".dashboard-root");
    public ILocator DashboardDiv => _page.Locator(".dashboard");

    // Helpers
    public async Task<bool> IsErrorPanelVisibleAsync() =>
        await ErrorPanel.CountAsync() > 0;

    public async Task<bool> IsDashboardVisibleAsync() =>
        (await DashboardRoot.CountAsync() > 0) || (await DashboardDiv.CountAsync() > 0);

    public async Task<string> GetErrorTitleTextAsync() =>
        await ErrorTitle.TextContentAsync() ?? string.Empty;

    public async Task<string> GetErrorMessageTextAsync() =>
        await ErrorMessage.TextContentAsync() ?? string.Empty;

    public async Task<string> GetHelpTextAsync() =>
        await HelpText.TextContentAsync() ?? string.Empty;

    public async Task<string> GetIndicatorTextAsync() =>
        await ErrorIndicator.TextContentAsync() ?? string.Empty;
}