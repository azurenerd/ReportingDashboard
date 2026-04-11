using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page object for the ErrorPanel component as rendered in the browser.
/// Matches the inline-styled structure from Components/ErrorPanel.razor.
/// </summary>
public class ErrorPageObject
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public ErrorPageObject(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });
    }

    public ILocator ErrorPanel => _page.Locator(".error-panel");
    public ILocator ErrorContent => _page.Locator(".error-content");

    // The ErrorPanel uses inline styles, so we locate by text content
    public ILocator ErrorTitle => _page.Locator("div:has-text('Dashboard data could not be loaded')").First;
    public ILocator ErrorHelpText => _page.Locator("div:has-text('Check data.json for errors')").First;

    public async Task<bool> IsErrorPanelVisibleAsync() =>
        await ErrorPanel.CountAsync() > 0;

    public async Task<string> GetErrorContentTextAsync()
    {
        if (await ErrorContent.CountAsync() > 0)
            return await ErrorContent.TextContentAsync() ?? "";
        return "";
    }
}