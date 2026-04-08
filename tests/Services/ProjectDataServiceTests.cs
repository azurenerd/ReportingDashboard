using Xunit;
using Moq;
using AgentSquad.Services;
using AgentSquad.Data;
using System.Text.Json;

namespace AgentSquad.Tests.Services
{
    public class ProjectDataServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly ProjectDataService _service;

        public ProjectDataServiceTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _service = new ProjectDataService(_httpClientFactoryMock.Object);
        }

        [Fact]
        public async Task LoadProjectDataAsync_SucceedsWithValidJson()
        {
            var mockData = new ProjectInfo { Name = "Test", Status = "Active" };
            var json = JsonSerializer.Serialize(mockData);
            var handler = new MockHttpMessageHandler(json);
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            var result = await _service.LoadProjectDataAsync();
            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
        }

        [Fact]
        public async Task LoadProjectDataAsync_ThrowsWithMissingFile()
        {
            var handler = new MockHttpMessageHandler(null);
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            var ex = await Assert.ThrowsAsync<DataLoadException>(() => _service.LoadProjectDataAsync());
            Assert.Contains("data.json not found in wwwroot directory", ex.Message);
        }

        [Fact]
        public async Task LoadProjectDataAsync_ThrowsWithInvalidJson()
        {
            var handler = new MockHttpMessageHandler("{invalid json}");
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            var ex = await Assert.ThrowsAsync<DataLoadException>(() => _service.LoadProjectDataAsync());
            Assert.Contains("Invalid JSON format", ex.Message);
        }

        [Fact]
        public async Task GetCachedData_ReturnsPreviouslyLoadedData()
        {
            var mockData = new ProjectInfo { Name = "Cached", Status = "Active" };
            var json = JsonSerializer.Serialize(mockData);
            var handler = new MockHttpMessageHandler(json);
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            await _service.LoadProjectDataAsync();
            var cached = _service.GetCachedData();
            Assert.NotNull(cached);
            Assert.Equal("Cached", cached.Name);
        }

        [Fact]
        public async Task RefreshData_ClearsCacheAndResetsTimestamp()
        {
            var mockData = new ProjectInfo { Name = "Test", Status = "Active" };
            var json = JsonSerializer.Serialize(mockData);
            var handler = new MockHttpMessageHandler(json);
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
            _httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            await _service.LoadProjectDataAsync();
            _service.RefreshData();
            var cached = _service.GetCachedData();
            Assert.Null(cached);
        }

        [Fact]
        public void GetCachedData_ReturnsNullBeforeLoad()
        {
            var result = _service.GetCachedData();
            Assert.Null(result);
        }
    }

    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _content;

        public MockHttpMessageHandler(string content)
        {
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_content == null)
                return Task.FromResult(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.NotFound });

            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(_content)
            });
        }
    }
}