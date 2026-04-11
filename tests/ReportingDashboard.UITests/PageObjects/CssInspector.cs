using Microsoft.Playwright;

namespace ReportingDashboard.UITests.PageObjects;

public class CssInspector
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public CssInspector(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task NavigateAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15000
        });
    }

    public async Task<string> GetComputedStyleAsync(string selector, string property)
    {
        var el = _page.Locator(selector).First;
        return await el.EvaluateAsync<string>($"el => getComputedStyle(el).{property}");
    }

    public async Task<string?> GetCssCustomPropertyAsync(string propertyName)
    {
        return await _page.EvaluateAsync<string>(
            $"() => getComputedStyle(document.documentElement).getPropertyValue('{propertyName}').trim()");
    }

    public async Task<bool> CssFileLoadsAsync()
    {
        var cssLink = _page.Locator("link[rel='stylesheet'][href='css/app.css']");
        var count = await cssLink.CountAsync();
        return count > 0;
    }

    public IPage Page => _page;
}