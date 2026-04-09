using Microsoft.Playwright;
using System;
using System.Threading.Tasks;

namespace AgentSquad.Runner.UITests.PageObjects
{
    public class MainLayoutPage
    {
        private readonly IPage _page;

        public MainLayoutPage(IPage page)
        {
            _page = page;
        }

        public async Task NavigateAsync(string url)
        {
            await _page.GotoAsync(url);
        }

        public async Task<string> GetPageTitleAsync()
        {
            return await _page.TitleAsync();
        }

        public async Task<bool> IsBootstrapLoadedAsync()
        {
            return await _page.EvaluateAsync<bool>(
                "() => typeof bootstrap !== 'undefined'");
        }

        public async Task<bool> IsChartJsLoadedAsync()
        {
            return await _page.EvaluateAsync<bool>(
                "() => typeof Chart !== 'undefined'");
        }

        public async Task<bool> IsBlazorLoadedAsync()
        {
            return await _page.EvaluateAsync<bool>(
                "() => typeof Blazor !== 'undefined'");
        }

        public async Task<string> GetMainContainerClassAsync()
        {
            var element = await _page.QuerySelectorAsync(".container-fluid");
            if (element == null) return null;
            return await element.GetAttributeAsync("class");
        }

        public async Task<bool> HasCustomCssAsync()
        {
            var links = await _page.QuerySelectorAllAsync("link[href*='app.css']");
            return links.Count > 0;
        }

        public async Task<string> GetViewportSizeAsync()
        {
            var size = await _page.ViewportSizeAsync();
            return size != null ? $"{size.Value.Width}x{size.Value.Height}" : "null";
        }

        public async Task WaitForLoadAsync(int timeoutMs = 5000)
        {
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle, 
                new PageWaitForLoadStateOptions { Timeout = timeoutMs });
        }
    }
}