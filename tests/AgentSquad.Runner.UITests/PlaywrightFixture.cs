using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace AgentSquad.Runner.UITests
{
    [SetUpFixture]
    public class PlaywrightFixture
    {
        private IPlaywright? _playwright;
        private IBrowser? _browser;
        protected IPage? Page { get; private set; }

        protected string BaseUrl => 
            Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5000";

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        }

        [OneTimeTearDown]
        public async Task OneTimeTeardown()
        {
            if (_browser != null)
                await _browser.CloseAsync();
            _playwright?.Dispose();
        }

        [SetUp]
        public async Task Setup()
        {
            if (_browser != null)
                Page = await _browser.NewPageAsync();
        }

        [TearDown]
        public async Task Teardown()
        {
            if (Page != null)
                await Page.CloseAsync();
        }
    }
}