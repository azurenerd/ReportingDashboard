using Xunit;
using AgentSquad.Dashboard.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AgentSquad.Dashboard.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class ErrorHandlingIntegrationTests : IAsyncLifetime
    {
        private readonly string _testDirectory;
        private readonly string _dataFilePath;
        private ProjectDataService _service;

        public ErrorHandlingIntegrationTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), $"dashboard-errors-{Guid.NewGuid()}");
            _dataFilePath = Path.Combine(_testDirectory, "data.json");
        }

        public async Task InitializeAsync()
        {
            Directory.CreateDirectory(_testDirectory);
            _service = new ProjectDataService();
            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            await Task.CompletedTask;
        }

        [Fact]
        public async Task LoadData_FileNotFound_ThrowsException()
        {
            var nonexistentPath = Path.Combine(_testDirectory, "nonexistent.json");
            
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.LoadProjectDataAsync());
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task LoadData_InvalidJson_ThrowsException()
        {
            await File.WriteAllTextAsync(_dataFilePath, "{ invalid json syntax");

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.LoadProjectDataAsync());
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task LoadData_EmptyJson_ThrowsException()
        {
            await File.WriteAllTextAsync(_dataFilePath, "");

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.LoadProjectDataAsync());
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task LoadData_JsonWithoutRequiredFields_ThrowsException()
        {
            await File.WriteAllTextAsync(_dataFilePath, "{}");

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.LoadProjectDataAsync());
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task LoadData_CorruptedFileContent_ThrowsException()
        {
            var bytes = new byte[] { 0xFF, 0xFE, 0xFD };
            await File.WriteAllBytesAsync(_dataFilePath, bytes);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.LoadProjectDataAsync());
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task LoadData_FileWithOnlyWhitespace_ThrowsException()
        {
            await File.WriteAllTextAsync(_dataFilePath, "   \n\t\r  ");

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.LoadProjectDataAsync());
            Assert.NotNull(ex);
        }

        [Fact]
        public async Task LoadData_VeryLargeFile_HandlesSuccessfully()
        {
            var largeJson = new string('a', 1000000);
            await File.WriteAllTextAsync(_dataFilePath, largeJson);

            var ex = await Assert.ThrowsAsync<Exception>(() => _service.LoadProjectDataAsync());
            Assert.NotNull(ex);
        }
    }
}