using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgentSquad.Runner.UITests.PageObjects
{
    public class DashboardPage
    {
        private readonly IPage _page;

        public DashboardPage(IPage page)
        {
            _page = page;
        }

        public async Task NavigateAsync(string baseUrl)
        {
            await _page.GotoAsync($"{baseUrl}/");
        }

        public async Task<bool> IsMilestoneTimelineVisibleAsync()
        {
            var timeline = await _page.QuerySelectorAsync("[data-testid='milestone-timeline']");
            return timeline != null;
        }

        public async Task<int> GetMilestoneCountAsync()
        {
            var milestones = await _page.QuerySelectorAllAsync("[data-testid='milestone-item']");
            return milestones.Count;
        }

        public async Task<bool> AreStatusCardsVisibleAsync()
        {
            var cards = await _page.QuerySelectorAllAsync("[data-testid='status-card']");
            return cards.Count == 3;
        }

        public async Task<string> GetStatusCardColorAsync(string statusType)
        {
            var card = await _page.QuerySelectorAsync($"[data-testid='status-card-{statusType}']");
            if (card == null) return null;
            return await card.GetAttributeAsync("class");
        }

        public async Task<int> GetShippedTaskCountAsync()
        {
            var element = await _page.QuerySelectorAsync("[data-testid='shipped-count']");
            if (element == null) return 0;
            var text = await element.InnerTextAsync();
            return int.TryParse(text, out var count) ? count : 0;
        }

        public async Task<int> GetInProgressTaskCountAsync()
        {
            var element = await _page.QuerySelectorAsync("[data-testid='inprogress-count']");
            if (element == null) return 0;
            var text = await element.InnerTextAsync();
            return int.TryParse(text, out var count) ? count : 0;
        }

        public async Task<int> GetCarriedOverTaskCountAsync()
        {
            var element = await _page.QuerySelectorAsync("[data-testid='carriedover-count']");
            if (element == null) return 0;
            var text = await element.InnerTextAsync();
            return int.TryParse(text, out var count) ? count : 0;
        }

        public async Task<bool> IsProgressMetricsVisibleAsync()
        {
            var metrics = await _page.QuerySelectorAsync("[data-testid='progress-metrics']");
            return metrics != null;
        }

        public async Task<string> GetProgressPercentageAsync()
        {
            var element = await _page.QuerySelectorAsync("[data-testid='progress-percentage']");
            if (element == null) return null;
            return await element.InnerTextAsync();
        }

        public async Task<bool> IsErrorMessageDisplayedAsync()
        {
            var errorElement = await _page.QuerySelectorAsync("[data-testid='error-message']");
            return errorElement != null;
        }

        public async Task<string> GetErrorMessageTextAsync()
        {
            var element = await _page.QuerySelectorAsync("[data-testid='error-message']");
            if (element == null) return null;
            return await element.InnerTextAsync();
        }

        public async Task RefreshPageAsync()
        {
            await _page.ReloadAsync();
        }

        public async Task WaitForDashboardLoadAsync(int timeoutMs = 5000)
        {
            await _page.WaitForSelectorAsync("[data-testid='dashboard-container']", 
                new PageWaitForSelectorOptions { Timeout = timeoutMs });
        }
    }
}