using Microsoft.Playwright;

namespace AgentSquad.Dashboard.UITests.Pages
{
    public class DashboardPage
    {
        private readonly IPage _page;

        public DashboardPage(IPage page)
        {
            _page = page;
        }

        public async Task<string> GetPageTitleAsync()
        {
            return await _page.GetTitleAsync();
        }

        public async Task<string?> GetProjectTitleAsync()
        {
            var element = await _page.QuerySelectorAsync("h1.display-5");
            return element != null ? await element.TextContentAsync() : null;
        }

        public async Task<string?> GetProjectDescriptionAsync()
        {
            var element = await _page.QuerySelectorAsync("p.lead");
            return element != null ? await element.TextContentAsync() : null;
        }

        public async Task<bool> IsLoadingAsync()
        {
            var loadingElement = await _page.QuerySelectorAsync(".alert-info");
            return loadingElement != null;
        }

        public async Task<bool> HasErrorAsync()
        {
            var errorElement = await _page.QuerySelectorAsync(".alert-danger");
            return errorElement != null;
        }

        public async Task<string?> GetErrorMessageAsync()
        {
            var errorElement = await _page.QuerySelectorAsync(".alert-danger");
            return errorElement != null ? await errorElement.TextContentAsync() : null;
        }

        public async Task ClickRetryAsync()
        {
            var retryButton = await _page.QuerySelectorAsync("button.btn-outline-danger");
            if (retryButton != null)
            {
                await retryButton.ClickAsync();
            }
        }

        public async Task<bool> IsMilestoneTimelineVisibleAsync()
        {
            var timeline = await _page.QuerySelectorAsync(".card-header.bg-primary");
            return timeline != null;
        }

        public async Task<int> GetStatusCardCountAsync()
        {
            var cards = await _page.QuerySelectorAllAsync(".card.shadow-sm");
            return cards.Count;
        }

        public async Task<string?> GetMetricsCompletionPercentageAsync()
        {
            var percentageElement = await _page.QuerySelectorAsync("div.display-4");
            return percentageElement != null ? await percentageElement.TextContentAsync() : null;
        }

        public async Task<bool> IsProgressMetricsVisibleAsync()
        {
            var progressMetrics = await _page.QuerySelectorAsync(".card-header.bg-dark");
            return progressMetrics != null;
        }

        public async Task WaitForDataLoadAsync()
        {
            await _page.WaitForSelectorAsync(".card.shadow-sm", new PageWaitForSelectorOptions { Timeout = 10000 });
        }
    }
}