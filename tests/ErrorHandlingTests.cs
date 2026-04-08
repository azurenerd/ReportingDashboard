using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AgentSquad.Runner.Services;
using Xunit;

namespace AgentSquad.Runner.Tests
{
    public class ErrorHandlingTests
    {
        private readonly ProjectDataService _service;
        private readonly string _testPath;

        public ErrorHandlingTests()
        {
            _service = new ProjectDataService();
            _testPath = Path.Combine(Path.GetTempPath(), "error_test.json");
        }

        private void Cleanup()
        {
            if (File.Exists(_testPath))
                File.Delete(_testPath);
        }

        [Fact]
        public async Task MissingFile_ThrowsFileNotFound()
        {
            var missingPath = Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid()}.json");

            var exception = await Assert.ThrowsAsync<FileNotFoundException>(
                () => _service.LoadProjectDataAsync(missingPath));

            Assert.NotNull(exception);
            Cleanup();
        }

        [Fact]
        public async Task MalformedJson_ThrowsJsonException()
        {
            File.WriteAllText(_testPath, "{ invalid: json }");

            await Assert.ThrowsAsync<JsonException>(() => _service.LoadProjectDataAsync(_testPath));

            Cleanup();
        }

        [Fact]
        public async Task EmptyFile_ThrowsJsonException()
        {
            File.WriteAllText(_testPath, "");

            await Assert.ThrowsAsync<JsonException>(() => _service.LoadProjectDataAsync(_testPath));

            Cleanup();
        }

        [Fact]
        public async Task JsonWithNullValues_HandlesGracefully()
        {
            var data = new
            {
                projectName = (string)null,
                projectStartDate = "2024-01-01",
                projectEndDate = "2024-12-31",
                milestones = new object[] { },
                tasks = new object[] { }
            };

            File.WriteAllText(_testPath, JsonSerializer.Serialize(data));

            var result = await _service.LoadProjectDataAsync(_testPath);

            Assert.Null(result.ProjectName);
            Cleanup();
        }

        [Fact]
        public async Task JsonWithMissingRequiredFields_ThrowsException()
        {
            File.WriteAllText(_testPath, "{ projectName: 'Test' }");

            await Assert.ThrowsAsync<JsonException>(() => _service.LoadProjectDataAsync(_testPath));

            Cleanup();
        }

        [Fact]
        public async Task LargeJsonFile_LoadsSuccessfully()
        {
            var tasks = new System.Collections.Generic.List<object>();
            for (int i = 0; i < 1000; i++)
            {
                tasks.Add(new { name = $"Task {i}", status = "Shipped", owner = $"Owner {i}" });
            }

            var data = new
            {
                projectName = "Large Project",
                projectStartDate = "2024-01-01",
                projectEndDate = "2024-12-31",
                milestones = new object[] { },
                tasks = tasks.ToArray()
            };

            File.WriteAllText(_testPath, JsonSerializer.Serialize(data));

            var result = await _service.LoadProjectDataAsync(_testPath);

            Assert.Equal(1000, result.Tasks.Count);
            Cleanup();
        }

        [Fact]
        public void CalculateCompletionPercentage_WithNullList_ThrowsArgumentNull()
        {
            Assert.Throws<ArgumentNullException>(() => _service.CalculateCompletionPercentage(null));
        }
    }
}