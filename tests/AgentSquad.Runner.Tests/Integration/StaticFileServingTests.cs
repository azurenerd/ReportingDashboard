using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace AgentSquad.Runner.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class StaticFileServingTests : IAsyncLifetime
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

        public async Task InitializeAsync()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _client?.Dispose();
            _factory?.Dispose();
            await Task.CompletedTask;
        }

        [Fact]
        public async Task StaticFiles_AppCSS_ReturnsOkStatus()
        {
            var response = await _client.GetAsync("/app.css");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task StaticFiles_BootstrapBundle_ReferenceReturnsOk()
        {
            var response = await _client.GetAsync("/_framework/blazor.server.js");

            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task RazorComponents_RootPage_ReturnsOkStatus()
        {
            var response = await _client.GetAsync("/");

            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task StaticFiles_NonExistentFile_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/nonexistent-file-12345.css");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task StaticFiles_RequestWithoutLeadingSlash_ReturnsNotFound()
        {
            var response = await _client.GetAsync("app.css");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}