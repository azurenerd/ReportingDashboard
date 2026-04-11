using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

public class ErrorPanelPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    // Selectors matching ErrorPanel.razor CSS classes
    private const string ErrorPanelSelector = ".error-panel";
    private const string ErrorIconSelector = ".error-icon";
    private const string ErrorTitleSelector = ".error-title";
    private const string ErrorDetailsSelector = ".error-details";
    private const string ErrorHelpSelector = ".error-help";

    public ErrorPanelPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public async Task NavigateAsync(string path = "/")
    {
        await _page.GotoAsync($"{_baseUrl}{path}", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });
    }

    public ILocator ErrorPanel => _page.Locator(ErrorPanelSelector);
    public ILocator ErrorIcon => _page.Locator(ErrorIconSelector);
    public ILocator ErrorTitle => _page.Locator(ErrorTitleSelector);
    public ILocator ErrorDetails => _page.Locator(ErrorDetailsSelector);
    public ILocator ErrorHelp => _page.Locator(ErrorHelpSelector);

    public async Task<bool> IsErrorPanelVisibleAsync()
    {
        return await ErrorPanel.CountAsync() > 0 && await ErrorPanel.IsVisibleAsync();
    }

    public async Task<string> GetTitleTextAsync()
    {
        return await ErrorTitle.TextContentAsync() ?? string.Empty;
    }

    public async Task<string> GetDetailsTextAsync()
    {
        if (await ErrorDetails.CountAsync() == 0)
            return string.Empty;

        return await ErrorDetails.TextContentAsync() ?? string.Empty;
    }

    public async Task<string> GetHelpTextAsync()
    {
        return await ErrorHelp.TextContentAsync() ?? string.Empty;
    }

    public async Task<string> GetIconTextAsync()
    {
        return await ErrorIcon.TextContentAsync() ?? string.Empty;
    }

    public async Task<bool> IsDetailsVisibleAsync()
    {
        return await ErrorDetails.CountAsync() > 0;
    }
}