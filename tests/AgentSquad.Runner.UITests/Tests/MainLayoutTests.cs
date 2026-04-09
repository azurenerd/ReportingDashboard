using Xunit;
using Microsoft.Playwright;
using AgentSquad.Runner.UITests.PageObjects;

namespace AgentSquad.Runner.UITests.Tests
{
    [Collection("Playwright")]
    [Trait("Category", "UI")]
    public class MainLayoutTests : IAsyncLifetime
    {
        private readonly PlaywrightFixture _fixture;
        private IPage _page;
        private MainLayoutPage _pageObject;

        public MainLayoutTests(PlaywrightFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            _page = await _fixture.NewPageAsync();
            _pageObject = new MainLayoutPage(_page);
        }

        public async Task DisposeAsync()
        {
            if (_page != null)
                await _page.CloseAsync();
        }

        [Fact]
        public async Task MainLayout_PageLoads_SuccessfullyNavigates()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            var title = await _pageObject.GetPageTitleAsync();

            Assert.NotNull(title);
            Assert.Contains("AgentSquad", title);
        }

        [Fact]
        public async Task MainLayout_HasCorrectTitle_AgentSquadDashboard()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            var title = await _pageObject.GetPageTitleAsync();

            Assert.Equal("AgentSquad - Executive Dashboard", title);
        }

        [Fact]
        public async Task MainLayout_BootstrapLoaded_BeforePageInteraction()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForLoadAsync();
            var isBootstrapLoaded = await _pageObject.IsBootstrapLoadedAsync();

            Assert.True(isBootstrapLoaded);
        }

        [Fact]
        public async Task MainLayout_ChartJsLoaded_BeforePageInteraction()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForLoadAsync();
            var isChartJsLoaded = await _pageObject.IsChartJsLoadedAsync();

            Assert.True(isChartJsLoaded);
        }

        [Fact]
        public async Task MainLayout_BlazorServerLoaded_ForInteractivity()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForLoadAsync();
            var isBlazorLoaded = await _pageObject.IsBlazorLoadedAsync();

            Assert.True(isBlazorLoaded);
        }

        [Fact]
        public async Task MainLayout_ContainerFluid_AppliedToMainContainer()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            var containerClass = await _pageObject.GetMainContainerClassAsync();

            Assert.NotNull(containerClass);
            Assert.Contains("container-fluid", containerClass);
        }

        [Fact]
        public async Task MainLayout_CustomCss_AppCssLoaded()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            var hasCss = await _pageObject.HasCustomCssAsync();

            Assert.True(hasCss);
        }

        [Fact]
        public async Task MainLayout_PageContent_DisplayedWithinContainer()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForLoadAsync();
            var containerClass = await _pageObject.GetMainContainerClassAsync();

            Assert.NotNull(containerClass);
        }

        [Fact]
        public async Task MainLayout_OnRefresh_ReloadsSuccessfully()
        {
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            await _pageObject.WaitForLoadAsync();
            
            var titleBefore = await _pageObject.GetPageTitleAsync();
            await _pageObject.NavigateAsync(_fixture.BaseUrl);
            var titleAfter = await _pageObject.GetPageTitleAsync();

            Assert.Equal(titleBefore, titleAfter);
        }
    }
}