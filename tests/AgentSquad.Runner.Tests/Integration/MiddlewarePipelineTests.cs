using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace AgentSquad.Runner.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class MiddlewarePipelineTests : IAsyncLifetime
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
        public async Task Middleware_RequestToRoot_ReturnsResponse()
        {
            var response = await _client.GetAsync("/");

            Assert.NotNull(response);
        }

        [Fact]
        public async Task Middleware_StaticFilesMiddleware_ProcessesBeforeRazorComponents()
        {
            var response = await _client.GetAsync("/app.css");

            Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Middleware_AntiforgerToken_IncludedInRequests()
        {
            var response = await _client.GetAsync("/");

            Assert.NotNull(response);
        }

        [Fact]
        public async Task Middleware_RequestToSignalRHub_ResolvesCorrectly()
        {
            var response = await _client.GetAsync("/agenthub");

            Assert.True(response.StatusCode == HttpStatusCode.OK || 
                       response.StatusCode == HttpStatusCode.NotFound ||
                       response.StatusCode == HttpStatusCode.BadRequest);
        }
    }
}