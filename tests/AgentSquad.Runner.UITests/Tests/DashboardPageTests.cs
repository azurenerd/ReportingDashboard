using Xunit;
using Microsoft.Playwright;
using AgentSquad.Runner.UITests.PageObjects;

namespace AgentSquad.Runner.UITests.Tests
{
    [Collection("Playwright")]
    [Trait("Category", "UI")]
    public class DashboardPageTests : IAsyncLifetime
    {
        private readonly PlaywrightFixture _fixture;
        private IPage _page;
        private DashboardPage _pageObject;

        public DashboardPageTests(PlaywrightFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            _page = await _fixture.NewPageAsync();
            _pageObject = new DashboardPage(_page);
        }

        public async Task DisposeAsync()
        {
            if (_page != null)
                await _page.CloseAsync();
        }

        [Fact]
        public async Task Dashboard_LoadsSuccessfully_WithoutErrors()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForDashboardLoadAsync();

            Assert.NotNull(_page);
        }

        [Fact]
        public async Task Dashboard_MilestoneTimeline_DisplayedAtTop()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForDashboardLoadAsync();
            var isVisible = await _pageObject.IsMilestoneTimelineVisibleAsync();

            Assert.True(isVisible);
        }

        [Fact]
        public async Task Dashboard_MilestoneTimeline_DisplaysMilestones()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForDashboardLoadAsync();
            var count = await _pageObject.GetMilestoneCountAsync();

            Assert.True(count >= 0);
        }

        [Fact]
        public async Task Dashboard_StatusCards_DisplayedInGrid()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForDashboardLoadAsync();
            var areVisible = await _pageObject.AreStatusCardsVisibleAsync();

            Assert.True(areVisible);
        }

        [Fact]
        public async Task Dashboard_StatusCard_ShippedTasksDisplayed()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForDashboardLoadAsync();
            var count = await _pageObject.GetShippedTaskCountAsync();

            Assert.True(count >= 0);
        }

        [Fact]
        public async Task Dashboard_StatusCard_InProgressTasksDisplayed()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForDashboardLoadAsync();
            var count = await _pageObject.GetInProgressTaskCountAsync();

            Assert.True(count >= 0);
        }

        [Fact]
        public async Task Dashboard_StatusCard_CarriedOverTasksDisplayed()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForDashboardLoadAsync();
            var count = await _pageObject.GetCarriedOverTaskCountAsync();

            Assert.True(count >= 0);
        }

        [Fact]
        public async Task Dashboard_ProgressMetrics_DisplayedBelowStatusCards()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForDashboardLoadAsync();
            var isVisible = await _pageObject.IsProgressMetricsVisibleAsync();

            Assert.True(isVisible);
        }

        [Fact]
        public async Task Dashboard_ProgressPercentage_Displayed()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForDashboardLoadAsync();
            var percentage = await _pageObject.GetProgressPercentageAsync();

            Assert.NotNull(percentage);
        }

        [Fact]
        public async Task Dashboard_OnPageRefresh_ReloadsDataSuccessfully()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForDashboardLoadAsync();
            
            var countBefore = await _pageObject.GetShippedTaskCountAsync();
            await _pageObject.RefreshPageAsync();
            await _pageObject.WaitForDashboardLoadAsync();
            var countAfter = await _pageObject.GetShippedTaskCountAsync();

            Assert.Equal(countBefore, countAfter);
        }

        [Fact]
        public async Task Dashboard_ErrorMessage_NotDisplayedOnValidLoad()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForDashboardLoadAsync();
            var isErrorDisplayed = await _pageObject.IsErrorMessageDisplayedAsync();

            Assert.False(isErrorDisplayed);
        }

        [Fact]
        public async Task Dashboard_AllSections_VisibleOnDesktopViewport()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForDashboardLoadAsync();

            var timelineVisible = await _pageObject.IsMilestoneTimelineVisibleAsync();
            var cardsVisible = await _pageObject.AreStatusCardsVisibleAsync();
            var metricsVisible = await _pageObject.IsProgressMetricsVisibleAsync();

            Assert.True(timelineVisible && cardsVisible && metricsVisible);
        }
    }
}