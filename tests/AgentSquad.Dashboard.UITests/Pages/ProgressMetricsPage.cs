using Microsoft.Playwright;

namespace AgentSquad.Dashboard.UITests.Pages
{
    public class ProgressMetricsPage
    {
        private readonly IPage _page;

        public ProgressMetricsPage(IPage page)
        {
            _page = page;
        }

        public async Task<bool> IsProgressMetricsVisibleAsync()
        {
            var metricsCard = await _page.QuerySelectorAsync(".card-header.bg-dark");
            return metricsCard != null;
        }

        public async Task<string?> GetCompletionPercentageAsync()
        {
            var element = await _page.QuerySelectorAsync("div.display-4");
            return element != null ? await element.TextContentAsync() : null;
        }

        public async Task<string?> GetProgressBarWidthAsync()
        {
            var progressBar = await _page.QuerySelectorAsync(".progress-bar");
            return progressBar != null ? await progressBar.GetAttributeAsync("style") : null;
        }

        public async Task<string?> GetBurnRateAsync()
        {
            var elements = await _page.QuerySelectorAllAsync("div.display-6");
            if (elements.Count > 0)
            {
                return await elements[0].TextContentAsync();
            }
            return null;
        }

        public async Task<string?> GetCompletedTasksAsync()
        {
            var elements = await _page.QuerySelectorAllAsync("div.display-6");
            if (elements.Count > 1)
            {
                return await elements[1].TextContentAsync();
            }
            return null;
        }

        public async Task<string?> GetRemainingTasksAsync()
        {
            var elements = await _page.QuerySelectorAllAsync("div.display-6");
            if (elements.Count > 2)
            {
                return await elements[2].TextContentAsync();
            }
            return null;
        }

        public async Task<string?> GetCompletionTextAsync()
        {
            var elements = await _page.QuerySelectorAllAsync("small.text-muted");
            if (elements.Count > 0)
            {
                return await elements[0].TextContentAsync();
            }
            return null;
        }

        public async Task<bool> HasValidProgressBarAsync()
        {
            var progressBar = await _page.QuerySelectorAsync(".progress-bar");
            if (progressBar == null) return false;

            var style = await progressBar.GetAttributeAsync("style");
            return style?.Contains("width:") == true;
        }
    }
}