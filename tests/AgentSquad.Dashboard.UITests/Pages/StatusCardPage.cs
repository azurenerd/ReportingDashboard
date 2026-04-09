using Microsoft.Playwright;

namespace AgentSquad.Dashboard.UITests.Pages
{
    public class StatusCardPage
    {
        private readonly IPage _page;

        public StatusCardPage(IPage page)
        {
            _page = page;
        }

        public async Task<int> GetShippedTaskCountAsync()
        {
            var element = await GetCardDisplayValueAsync("Shipped");
            return int.TryParse(element, out var count) ? count : 0;
        }

        public async Task<int> GetInProgressTaskCountAsync()
        {
            var element = await GetCardDisplayValueAsync("In Progress");
            return int.TryParse(element, out var count) ? count : 0;
        }

        public async Task<int> GetCarriedOverTaskCountAsync()
        {
            var element = await GetCardDisplayValueAsync("Carried Over");
            return int.TryParse(element, out var count) ? count : 0;
        }

        public async Task<bool> HasShippedCardAsync()
        {
            return await HasCardAsync("Shipped");
        }

        public async Task<bool> HasInProgressCardAsync()
        {
            return await HasCardAsync("In Progress");
        }

        public async Task<bool> HasCarriedOverCardAsync()
        {
            return await HasCardAsync("Carried Over");
        }

        public async Task ExpandShippedTasksAsync()
        {
            await ExpandCardTasksAsync("Shipped");
        }

        public async Task ExpandInProgressTasksAsync()
        {
            await ExpandCardTasksAsync("In Progress");
        }

        public async Task ExpandCarriedOverTasksAsync()
        {
            await ExpandCardTasksAsync("Carried Over");
        }

        public async Task<int> GetVisibleTasksCountAsync()
        {
            var tasks = await _page.QuerySelectorAllAsync("li.mb-2");
            return tasks.Count;
        }

        public async Task<string?> GetFirstTaskNameAsync()
        {
            var taskElements = await _page.QuerySelectorAllAsync("li.mb-2 strong");
            if (taskElements.Count > 0)
            {
                return await taskElements[0].TextContentAsync();
            }
            return null;
        }

        public async Task<bool> HasEmptyCardMessageAsync()
        {
            var message = await _page.QuerySelectorAsync("text=No tasks in this category");
            return message != null;
        }

        private async Task<bool> HasCardAsync(string categoryName)
        {
            var cards = await _page.QuerySelectorAllAsync(".card-header");
            foreach (var card in cards)
            {
                var text = await card.TextContentAsync();
                if (text?.Contains(categoryName) == true)
                {
                    return true;
                }
            }
            return false;
        }

        private async Task<string?> GetCardDisplayValueAsync(string categoryName)
        {
            var cards = await _page.QuerySelectorAllAsync(".card-header");
            foreach (var card in cards)
            {
                var text = await card.TextContentAsync();
                if (text?.Contains(categoryName) == true)
                {
                    var cardBody = await card.EvaluateAsync<string?>("el => el.parentElement?.querySelector('.display-6')?.textContent");
                    return cardBody;
                }
            }
            return null;
        }

        private async Task ExpandCardTasksAsync(string categoryName)
        {
            var cards = await _page.QuerySelectorAllAsync(".card");
            foreach (var card in cards)
            {
                var header = await card.QuerySelectorAsync(".card-header");
                var text = await header?.TextContentAsync();
                if (text?.Contains(categoryName) == true)
                {
                    var button = await card.QuerySelectorAsync("button.btn-outline-secondary");
                    if (button != null)
                    {
                        await button.ClickAsync();
                    }
                }
            }
        }
    }
}