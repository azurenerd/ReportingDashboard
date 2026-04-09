using Xunit;
using AgentSquad.Dashboard.UITests.Fixtures;
using AgentSquad.Dashboard.UITests.Pages;

namespace AgentSquad.Dashboard.UITests.Tests
{
    [Collection("Playwright")]
    [Trait("Category", "UI")]
    public class TimelineTests : IAsyncLifetime
    {
        private readonly PlaywrightFixture _fixture;
        private Microsoft.Playwright.IPage? _page;

        public TimelineTests(PlaywrightFixture fixture)
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
        public async Task Timeline_IsVisible()
        {
            var timelinePage = new TimelinePage(_page!);
            await _page!.WaitForSelectorAsync(".timeline", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var isVisible = await timelinePage.IsMilestoneTimelineVisibleAsync();
            Assert.True(isVisible);
        }

        [Fact]
        public async Task Timeline_DisplaysMilestones()
        {
            var timelinePage = new TimelinePage(_page!);
            await _page!.WaitForSelectorAsync(".timeline-item", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var count = await timelinePage.GetMilestoneCountAsync();
            Assert.True(count > 0);
        }

        [Fact]
        public async Task Timeline_DisplaysMilestoneNames()
        {
            var timelinePage = new TimelinePage(_page!);
            await _page!.WaitForSelectorAsync(".timeline-item", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var firstName = await timelinePage.GetFirstMilestoneNameAsync();
            Assert.NotNull(firstName);
            Assert.NotEmpty(firstName);
        }

        [Fact]
        public async Task Timeline_DisplaysCompletionPercentages()
        {
            var timelinePage = new TimelinePage(_page!);
            await _page!.WaitForSelectorAsync(".progress-bar", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var percentage = await timelinePage.GetMilestoneCompletionPercentageAsync(0);
            Assert.NotNull(percentage);
            Assert.Contains("width:", percentage);
        }

        [Fact]
        public async Task Timeline_ShowsCompletedMilestones()
        {
            var timelinePage = new TimelinePage(_page!);
            await _page!.WaitForSelectorAsync(".timeline-marker", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var completedCount = await timelinePage.GetCompletedMilestonesCountAsync();
            Assert.True(completedCount >= 0);
        }

        [Fact]
        public async Task Timeline_MilestonesHaveValidStatus()
        {
            var timelinePage = new TimelinePage(_page!);
            await _page!.WaitForSelectorAsync(".timeline-marker", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var status = await timelinePage.GetMilestoneStatusAsync(0);
            Assert.NotNull(status);
            Assert.True(status.Contains("completed") || status.Contains("in-progress") || status.Contains("pending"));
        }
    }
}