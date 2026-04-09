using Xunit;
using Microsoft.Playwright;
using AgentSquad.Runner.UITests.PageObjects;

namespace AgentSquad.Runner.UITests.Tests
{
    [Collection("Playwright")]
    [Trait("Category", "UI")]
    public class ResponsiveDesignTests : IAsyncLifetime
    {
        private readonly PlaywrightFixture _fixture;
        private IPage _page;
        private DashboardPage _pageObject;

        public ResponsiveDesignTests(PlaywrightFixture fixture)
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
        public async Task Dashboard_Desktop1024Width_DisplaysAllSections()
        {
            await _page.SetViewportSizeAsync(1024, 768);
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForDashboardLoadAsync();

            var timelineVisible = await _pageObject.IsMilestoneTimelineVisibleAsync();
            var cardsVisible = await _pageObject.AreStatusCardsVisibleAsync();

            Assert.True(timelineVisible && cardsVisible);
        }

        [Fact]
        public async Task Dashboard_Desktop1920Width_DisplaysAllSections()
        {
            await _page.SetViewportSizeAsync(1920, 1080);
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForDashboardLoadAsync();

            var timelineVisible = await _pageObject.IsMilestoneTimelineVisibleAsync();
            var cardsVisible = await _pageObject.AreStatusCardsVisibleAsync();

            Assert.True(timelineVisible && cardsVisible);
        }

        [Fact]
        public async Task Dashboard_LargeScreen_ContentRemainsReadable()
        {
            await _page.SetViewportSizeAsync(2560, 1440);
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForDashboardLoadAsync();

            var timelineVisible = await _pageObject.IsMilestoneTimelineVisibleAsync();
            Assert.True(timelineVisible);
        }
    }
}