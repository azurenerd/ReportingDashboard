using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page object for detailed ErrorPanel CSS styling verification.
/// Covers the root Components/ErrorPanel.razor inline styles from PR #555.
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

    public async Task NavigateAsync(string? url = null)
    {
        await _page.GotoAsync(url ?? _baseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });
    }

    // Error panel elements
    public ILocator ErrorPanel => _page.Locator(".error-panel");
    public ILocator ErrorIcon => _page.Locator(".error-icon");
    public ILocator ErrorTitle => _page.Locator(".error-title");
    public ILocator ErrorDetails => _page.Locator(".error-details");
    public ILocator ErrorHelp => _page.Locator(".error-help");

    // Helpers
    public async Task<bool> IsErrorVisibleAsync() =>
        await ErrorPanel.CountAsync() > 0 && await ErrorPanel.IsVisibleAsync();

    public async Task<string> GetIconFontSizeAsync() =>
        await ErrorIcon.EvaluateAsync<string>("el => getComputedStyle(el).fontSize");

    public async Task<string> GetIconColorAsync() =>
        await ErrorIcon.EvaluateAsync<string>("el => getComputedStyle(el).color");

    public async Task<string> GetDetailsFontFamilyAsync() =>
        await ErrorDetails.EvaluateAsync<string>("el => getComputedStyle(el).fontFamily");

    public async Task<string> GetDetailsMaxWidthAsync() =>
        await ErrorDetails.EvaluateAsync<string>("el => getComputedStyle(el).maxWidth");

    public async Task<string> GetDetailsWordBreakAsync() =>
        await ErrorDetails.EvaluateAsync<string>("el => getComputedStyle(el).wordBreak");

    public async Task<string> GetTitleFontSizeAsync() =>
        await ErrorTitle.EvaluateAsync<string>("el => getComputedStyle(el).fontSize");

    public async Task<string> GetTitleFontWeightAsync() =>
        await ErrorTitle.EvaluateAsync<string>("el => getComputedStyle(el).fontWeight");

    public async Task<string> GetHelpFontSizeAsync() =>
        await ErrorHelp.EvaluateAsync<string>("el => getComputedStyle(el).fontSize");

    public async Task<string?> GetErrorMessageAsync() =>
        await ErrorDetails.TextContentAsync();

    public async Task<string?> GetTitleTextAsync() =>
        await ErrorTitle.TextContentAsync();

    public async Task<string?> GetHelpTextAsync() =>
        await ErrorHelp.TextContentAsync();
}