using Xunit;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AgentSquad.Tests.Integration
{
    public class StaticFileServingTests : IAsyncLifetime
    {
        private HttpClient _client;
        private string _baseUrl = "http://localhost:5000";

        public async Task InitializeAsync()
        {
            _client = new HttpClient();
            _client.BaseAddress = new System.Uri(_baseUrl);
        }

        public async Task DisposeAsync()
        {
            _client?.Dispose();
        }

        [Fact]
        public async Task DashboardJSServersWithout404()
        {
            var response = await _client.GetAsync("/js/dashboard.js");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PrintHandlerJSServersWithout404()
        {
            var response = await _client.GetAsync("/js/print-handler.js");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task BaseCSSServersWithout404()
        {
            var response = await _client.GetAsync("/css/base.css");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DashboardCSSServersWithout404()
        {
            var response = await _client.GetAsync("/css/dashboard.css");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PrintCSSServersWithout404()
        {
            var response = await _client.GetAsync("/css/print.css");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task StaticFilesReturnCorrectContentType()
        {
            var jsResponse = await _client.GetAsync("/js/dashboard.js");
            Assert.Contains("application/javascript", jsResponse.Content.Headers.ContentType?.MediaType ?? "");
            
            var cssResponse = await _client.GetAsync("/css/base.css");
            Assert.Contains("text/css", cssResponse.Content.Headers.ContentType?.MediaType ?? "");
        }
    }
}