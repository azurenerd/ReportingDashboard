using System.Threading.Tasks;
using Microsoft.Playwright;

namespace AgentSquad.Runner.UITests.Pages
{
    public class DashboardPage
    {
        private readonly IPage _page;
        private readonly string _baseUrl;

        public DashboardPage(IPage page, string baseUrl)
        {
            _page = page;
            _baseUrl = baseUrl;
        }

        public async Task NavigateToDashboardAsync()
        {
            await _page.GotoAsync(_baseUrl);
        }

        public async Task<string?> GetPageTitleAsync()
        {
            return await _page.TitleAsync();
        }

        public async Task<bool> IsLoadingIndicatorVisibleAsync()
        {
            var indicator = _page.Locator(".loading-indicator");
            return await indicator.IsVisibleAsync();
        }

        public async Task<bool> IsErrorBannerVisibleAsync()
        {
            var banner = _page.Locator(".error-banner");
            return await banner.IsVisibleAsync();
        }

        public async Task<string?> GetErrorMessageAsync()
        {
            var banner = _page.Locator(".error-banner p");
            return await banner.TextContentAsync();
        }

        public async Task<bool> IsDashboardContainerVisibleAsync()
        {
            var container = _page.Locator(".dashboard-container");
            return await container.IsVisibleAsync();
        }

        public async Task WaitForDataLoadAsync(int timeoutMs = 5000)
        {
            var loading = _page.Locator(".loading-indicator");
            await loading.WaitForAsync(new LocatorWaitForOptions 
            { 
                State = WaitForSelectorState.Hidden, 
                Timeout = timeoutMs 
            });
        }
    }
}