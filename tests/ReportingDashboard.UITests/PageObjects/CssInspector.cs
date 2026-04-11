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

    public async Task<string> FetchAppCssAsync()
    {
        var response = await _page.APIRequest.GetAsync($"{_baseUrl}/css/app.css");
        return await response.TextAsync();
    }

    public async Task<bool> CssContainsClassAsync(string className)
    {
        var css = await FetchAppCssAsync();
        return css.Contains(className);
    }

    public async Task<bool> CssContainsRootCustomPropertiesAsync()
    {
        var css = await FetchAppCssAsync();
        return css.Contains(":root");
    }
}