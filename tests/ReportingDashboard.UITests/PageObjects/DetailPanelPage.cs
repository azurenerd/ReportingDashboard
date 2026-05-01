using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

/// <summary>
/// Page Object for the Detail Panel slide-in component.
/// </summary>
public class DetailPanelPage
{
    private readonly IPage _page;

    public DetailPanelPage(IPage page)
    {
        _page = page;
    }

    public ILocator Panel => _page.Locator("[role='dialog'][aria-label='Report detail panel']");

    public ILocator CloseButton => _page.GetByLabel("Close panel");

    public ILocator Backdrop => _page.Locator("div.fixed.inset-0");

    public ILocator HeaderTitle => _page.Locator("h2:has-text('Detail')");

    public ILocator LoadingSpinner => _page.Locator(".animate-spin");

    public async Task WaitForPanelVisibleAsync()
    {
        await Panel.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task WaitForPanelHiddenAsync()
    {
        await Panel.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = 5000
        });
    }

    public async Task ClickCloseButtonAsync()
    {
        await CloseButton.ClickAsync();
    }

    public async Task PressEscapeAsync()
    {
        await _page.Keyboard.PressAsync("Escape");
    }

    public async Task ClickBackdropAsync()
    {
        await Backdrop.ClickAsync(new LocatorClickOptions { Position = new Position { X = 50, Y = 50 } });
    }

    public async Task<bool> IsPanelVisibleAsync()
    {
        return await Panel.IsVisibleAsync();
    }
}