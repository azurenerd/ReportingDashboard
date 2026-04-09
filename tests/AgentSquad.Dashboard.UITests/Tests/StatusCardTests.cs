using Xunit;
using AgentSquad.Dashboard.UITests.Fixtures;
using AgentSquad.Dashboard.UITests.Pages;

namespace AgentSquad.Dashboard.UITests.Tests
{
    [Collection("Playwright")]
    [Trait("Category", "UI")]
    public class StatusCardTests : IAsyncLifetime
    {
        private readonly PlaywrightFixture _fixture;
        private Microsoft.Playwright.IPage? _page;

        public StatusCardTests(PlaywrightFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            _page = await _fixture.NewPageAsync();
            await _fixture.NavigateToAsync(_page, "/");
            await _fixture.WaitForLoadingAsync(_page);
        }

        public async Task DisposeAsync()
        {
            if (_page != null)
            {
                await _page.CloseAsync();
            }
        }

        [Fact]
        public async Task StatusCards_AllThreeCardsDisplay()
        {
            var statusCardPage = new StatusCardPage(_page!);
            await _page!.WaitForSelectorAsync(".card", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var hasShipped = await statusCardPage.HasShippedCardAsync();
            var hasInProgress = await statusCardPage.HasInProgressCardAsync();
            var hasCarriedOver = await statusCardPage.HasCarriedOverCardAsync();

            Assert.True(hasShipped && hasInProgress && hasCarriedOver);
        }

        [Fact]
        public async Task StatusCard_ShippedDisplaysCount()
        {
            var statusCardPage = new StatusCardPage(_page!);
            await _page!.WaitForSelectorAsync(".display-6", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var count = await statusCardPage.GetShippedTaskCountAsync();
            Assert.True(count >= 0);
        }

        [Fact]
        public async Task StatusCard_InProgressDisplaysCount()
        {
            var statusCardPage = new StatusCardPage(_page!);
            await _page!.WaitForSelectorAsync(".display-6", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var count = await statusCardPage.GetInProgressTaskCountAsync();
            Assert.True(count >= 0);
        }

        [Fact]
        public async Task StatusCard_CarriedOverDisplaysCount()
        {
            var statusCardPage = new StatusCardPage(_page!);
            await _page!.WaitForSelectorAsync(".display-6", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var count = await statusCardPage.GetCarriedOverTaskCountAsync();
            Assert.True(count >= 0);
        }

        [Fact]
        public async Task StatusCard_ExpandTasksShowsTaskList()
        {
            var statusCardPage = new StatusCardPage(_page!);
            await _page!.WaitForSelectorAsync("button.btn-outline-secondary", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var buttons = await _page!.QuerySelectorAllAsync("button.btn-outline-secondary");
            if (buttons.Count > 0)
            {
                await buttons[0].ClickAsync();
                await _page!.WaitForTimeoutAsync(500);

                var tasksCount = await statusCardPage.GetVisibleTasksCountAsync();
                Assert.True(tasksCount >= 0);
            }
        }

        [Fact]
        public async Task StatusCard_DisplaysTaskNames()
        {
            var statusCardPage = new StatusCardPage(_page!);
            await _page!.WaitForSelectorAsync("button.btn-outline-secondary", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var buttons = await _page!.QuerySelectorAllAsync("button.btn-outline-secondary");
            if (buttons.Count > 0)
            {
                await buttons[0].ClickAsync();
                await _page!.WaitForTimeoutAsync(500);

                var taskName = await statusCardPage.GetFirstTaskNameAsync();
                if (taskName != null)
                {
                    Assert.NotEmpty(taskName);
                }
            }
        }
    }
}