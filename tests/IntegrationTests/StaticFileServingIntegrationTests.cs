using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AgentSquad.Tests.IntegrationTests
{
    public class StaticFileServingIntegrationTests : IAsyncLifetime
    {
        private TestServer _server;
        private HttpClient _client;
        private readonly string _projectRoot;

        public StaticFileServingIntegrationTests()
        {
            _projectRoot = Directory.GetCurrentDirectory();
        }

        public async Task InitializeAsync()
        {
            var builder = new WebHostBuilder()
                .UseContentRoot(_projectRoot)
                .ConfigureServices(services =>
                {
                    services.AddRazorComponents()
                        .AddInteractiveServerComponents();
                })
                .Configure(app =>
                {
                    app.UseStaticFiles();
                });

            _server = new TestServer(builder);
            _client = _server.CreateClient();
            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _client?.Dispose();
            _server?.Dispose();
            await Task.CompletedTask;
        }

        [Fact]
        public async Task IndexHtmlReturns200()
        {
            var response = await _client.GetAsync("/index.html");
            Assert.True(response.IsSuccessStatusCode, $"Expected 200, got {response.StatusCode}");
        }

        [Fact]
        public async Task FaviconReturns200()
        {
            var response = await _client.GetAsync("/favicon.ico");
            Assert.True(
                response.IsSuccessStatusCode,
                $"favicon.ico should return 200, got {response.StatusCode}"
            );
        }

        [Fact]
        public async Task SiteCssReturns200()
        {
            var response = await _client.GetAsync("/css/site.css");
            Assert.True(response.IsSuccessStatusCode, $"Expected 200, got {response.StatusCode}");
        }

        [Fact]
        public async Task SiteCssHasCorrectContentType()
        {
            var response = await _client.GetAsync("/css/site.css");
            Assert.True(
                response.Content.Headers.ContentType?.ToString().Contains("text/css") ?? false,
                "CSS should have text/css content type"
            );
        }

        [Fact]
        public async Task IndexHtmlHasCorrectContentType()
        {
            var response = await _client.GetAsync("/index.html");
            Assert.True(
                response.Content.Headers.ContentType?.ToString().Contains("text/html") ?? false,
                "HTML should have text/html content type"
            );
        }

        [Fact]
        public async Task StaticFilesHaveCacheHeaders()
        {
            var response = await _client.GetAsync("/css/site.css");
            Assert.NotNull(response.Headers.CacheControl);
        }

        [Fact]
        public async Task StaticFilesNotFoundReturn404()
        {
            var response = await _client.GetAsync("/nonexistent.css");
            Assert.False(response.IsSuccessStatusCode, "Non-existent files should return 404");
        }
    }
}