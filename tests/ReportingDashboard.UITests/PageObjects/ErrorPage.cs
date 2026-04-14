using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page Object for the error state view.
/// Displayed when data.json is missing, malformed, or fails validation.
/// </summary>
public class ErrorPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public ErrorPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task<IResponse?> NavigateAsync()
    {
        return await _page.GotoAsync(_baseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
    }

    public ILocator ErrorPanel => _page.Locator(".error-panel");
    public ILocator ErrorIcon => _page.Locator(".error-icon");
    public ILocator ErrorTitle => _page.Locator(".error-title");
    public ILocator ErrorDetails => _page.Locator(".error-details");
    public ILocator ErrorHelp => _page.Locator(".error-help");

    public async Task<bool> IsErrorPanelVisibleAsync()
    {
        return await ErrorPanel.IsVisibleAsync();
    }

    public async Task<string> GetErrorMessageAsync()
    {
        return await ErrorDetails.TextContentAsync() ?? string.Empty;
    }

    public async Task<string> GetErrorHelpAsync()
    {
        return await ErrorHelp.TextContentAsync() ?? string.Empty;
    }

    public async Task<string> GetErrorTitleAsync()
    {
        return await ErrorTitle.TextContentAsync() ?? string.Empty;
    }
}