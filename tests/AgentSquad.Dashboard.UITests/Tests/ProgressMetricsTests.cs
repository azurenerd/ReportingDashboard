using Xunit;
using AgentSquad.Dashboard.UITests.Fixtures;
using AgentSquad.Dashboard.UITests.Pages;

namespace AgentSquad.Dashboard.UITests.Tests
{
    [Collection("Playwright")]
    [Trait("Category", "UI")]
    public class ProgressMetricsTests : IAsyncLifetime
    {
        private readonly PlaywrightFixture _fixture;
        private Microsoft.Playwright.IPage? _page;

        public ProgressMetricsTests(PlaywrightFixture fixture)
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
        public async Task ProgressMetrics_IsVisible()
        {
            var metricsPage = new ProgressMetricsPage(_page!);
            await _page!.WaitForSelectorAsync(".card-header.bg-dark", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var isVisible = await metricsPage.IsProgressMetricsVisibleAsync();
            Assert.True(isVisible);
        }

        [Fact]
        public async Task ProgressMetrics_DisplaysCompletionPercentage()
        {
            var metricsPage = new ProgressMetricsPage(_page!);
            await _page!.WaitForSelectorAsync("div.display-4", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var percentage = await metricsPage.GetCompletionPercentageAsync();
            Assert.NotNull(percentage);
            Assert.Matches(@"\d+", percentage);
        }

        [Fact]
        public async Task ProgressMetrics_DisplaysProgressBar()
        {
            var metricsPage = new ProgressMetricsPage(_page!);
            await _page!.WaitForSelectorAsync(".progress-bar", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var hasValidProgressBar = await metricsPage.HasValidProgressBarAsync();
            Assert.True(hasValidProgressBar);
        }

        [Fact]
        public async Task ProgressMetrics_DisplaysBurnRate()
        {
            var metricsPage = new ProgressMetricsPage(_page!);
            await _page!.WaitForSelectorAsync("div.display-6", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var burnRate = await metricsPage.GetBurnRateAsync();
            Assert.NotNull(burnRate);
        }

        [Fact]
        public async Task ProgressMetrics_DisplaysCompletedTasks()
        {
            var metricsPage = new ProgressMetricsPage(_page!);
            await _page!.WaitForSelectorAsync("div.display-6", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var completed = await metricsPage.GetCompletedTasksAsync();
            Assert.NotNull(completed);
            Assert.Matches(@"\d+", completed);
        }

        [Fact]
        public async Task ProgressMetrics_DisplaysRemainingTasks()
        {
            var metricsPage = new ProgressMetricsPage(_page!);
            await _page!.WaitForSelectorAsync("div.display-6", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var remaining = await metricsPage.GetRemainingTasksAsync();
            Assert.NotNull(remaining);
            Assert.Matches(@"\d+", remaining);
        }

        [Fact]
        public async Task ProgressMetrics_DisplaysCompletionText()
        {
            var metricsPage = new ProgressMetricsPage(_page!);
            await _page!.WaitForSelectorAsync("small.text-muted", new Microsoft.Playwright.PageWaitForSelectorOptions { Timeout = 10000 });

            var text = await metricsPage.GetCompletionTextAsync();
            Assert.NotNull(text);
        }
    }
}