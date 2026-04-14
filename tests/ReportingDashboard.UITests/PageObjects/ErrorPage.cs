using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page Object for the error state when data.json cannot be loaded.
/// </summary>
public class ErrorPage
{
    private readonly IPage _page;

    public ErrorPage(IPage page)
    {
        _page = page;
    }

    // Error panel selectors
    public ILocator ErrorPanel => _page.Locator(".error-panel");
    public ILocator ErrorIcon => _page.Locator(".error-icon");
    public ILocator ErrorTitle => _page.Locator(".error-title");
    public ILocator ErrorDetails => _page.Locator(".error-details");
    public ILocator ErrorHelp => _page.Locator(".error-help");

    public async Task<bool> IsDisplayedAsync()
    {
        return await ErrorPanel.IsVisibleAsync();
    }

    public async Task<string> GetErrorTitleAsync()
    {
        return await ErrorTitle.TextContentAsync() ?? string.Empty;
    }

    public async Task<string> GetErrorMessageAsync()
    {
        return await ErrorDetails.TextContentAsync() ?? string.Empty;
    }

    public async Task<string> GetHelpTextAsync()
    {
        return await ErrorHelp.TextContentAsync() ?? string.Empty;
    }

    public async Task AssertErrorStateAsync()
    {
        var isVisible = await IsDisplayedAsync();
        if (!isVisible)
            throw new InvalidOperationException("Error panel is not visible");

        var title = await GetErrorTitleAsync();
        if (!title.Contains("could not be loaded"))
            throw new InvalidOperationException($"Unexpected error title: {title}");
    }
}