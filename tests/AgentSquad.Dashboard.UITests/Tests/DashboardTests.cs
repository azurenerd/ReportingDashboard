using Xunit;
using AgentSquad.Dashboard.UITests.Fixtures;
using AgentSquad.Dashboard.UITests.Pages;

namespace AgentSquad.Dashboard.UITests.Tests
{
    [Collection("Playwright")]
    [Trait("Category", "UI")]
    public class DashboardTests : IAsyncLifetime
    {
        private readonly PlaywrightFixture _fixture;
        private Microsoft.Playwright.IPage? _page;

        public DashboardTests(PlaywrightFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            _page = await _fixture.NewPageAsync();
            await _fixture.NavigateToAsync(_page, "/");
        }

        public async Task DisposeAsync()
        {
            if (_page != null)
            {
                await _page.CloseAsync();
            }
        }

        [Fact]
        public async Task Dashboard_LoadsSuccessfully()
        {
            var dashboardPage = new DashboardPage(_page!);
            await _fixture.WaitForLoadingAsync(_page!);

            var title = await dashboardPage.GetPageTitleAsync();
            Assert.NotNull(title);
            Assert.Contains("Dashboard", title);
        }

        [Fact]
        public async Task Dashboard_DisplaysProjectTitle()
        {
            var dashboardPage = new DashboardPage(_page!);
            await _fixture.WaitForLoadingAsync(_page!);
            await dashboardPage.WaitForDataLoadAsync();

            var projectTitle = await dashboardPage.GetProjectTitleAsync();
            Assert.NotNull(projectTitle);
            Assert.NotEmpty(projectTitle);
        }

        [Fact]
        public async Task Dashboard_DisplaysProjectDescription()
        {
            var dashboardPage = new DashboardPage(_page!);
            await _fixture.WaitForLoadingAsync(_page!);
            await dashboardPage.WaitForDataLoadAsync();

            var description = await dashboardPage.GetProjectDescriptionAsync();
            Assert.NotNull(description);
        }

        [Fact]
        public async Task Dashboard_DisplaysMilestoneTimeline()
        {
            var dashboardPage = new DashboardPage(_page!);
            await _fixture.WaitForLoadingAsync(_page!);
            await dashboardPage.WaitForDataLoadAsync();

            var timelineVisible = await dashboardPage.IsMilestoneTimelineVisibleAsync();
            Assert.True(timelineVisible);
        }

        [Fact]
        public async Task Dashboard_DisplaysStatusCards()
        {
            var dashboardPage = new DashboardPage(_page!);
            await _fixture.WaitForLoadingAsync(_page!);
            await dashboardPage.WaitForDataLoadAsync();

            var cardCount = await dashboardPage.GetStatusCardCountAsync();
            Assert.True(cardCount >= 3);
        }

        [Fact]
        public async Task Dashboard_DisplaysProgressMetrics()
        {
            var dashboardPage = new DashboardPage(_page!);
            await _fixture.WaitForLoadingAsync(_page!);
            await dashboardPage.WaitForDataLoadAsync();

            var metricsVisible = await dashboardPage.IsProgressMetricsVisibleAsync();
            Assert.True(metricsVisible);
        }

        [Fact]
        public async Task Dashboard_DisplaysCompletionPercentage()
        {
            var dashboardPage = new DashboardPage(_page!);
            await _fixture.WaitForLoadingAsync(_page!);
            await dashboardPage.WaitForDataLoadAsync();

            var percentage = await dashboardPage.GetMetricsCompletionPercentageAsync();
            Assert.NotNull(percentage);
            Assert.Matches(@"\d+%", percentage);
        }

        [Fact]
        public async Task Dashboard_DoesNotDisplayErrorInitially()
        {
            var dashboardPage = new DashboardPage(_page!);
            await _fixture.WaitForLoadingAsync(_page!);
            await dashboardPage.WaitForDataLoadAsync();

            var hasError = await dashboardPage.HasErrorAsync();
            Assert.False(hasError);
        }
    }
}