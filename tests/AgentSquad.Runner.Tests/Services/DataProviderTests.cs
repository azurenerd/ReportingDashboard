using Xunit;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgentSquad.Runner.Tests.Services
{
    public class DataProviderTests
    {
        private string _tempDir;
        private string _testDataPath;

        public DataProviderTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
            _testDataPath = Path.Combine(_tempDir, "data.json");
        }

        private void CreateTestDataFile(string content)
        {
            File.WriteAllText(_testDataPath, content);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidJson_ReturnsParsedProject()
        {
            var validJson = """
                {
                    "milestones": [
                        {
                            "name": "Q1",
                            "status": "InProgress",
                            "items": [
                                {"title": "Feature A", "status": "InProgress"}
                            ]
                        }
                    ]
                }
                """;
            CreateTestDataFile(validJson);

            var cache = new MemoryCacheAdapter();
            var provider = new DataProvider(_testDataPath, cache);
            var project = await provider.LoadProjectDataAsync();

            Assert.NotNull(project);
            Assert.Single(project.Milestones);
            Assert.Equal("Q1", project.Milestones[0].Name);
            Assert.Equal(MilestoneStatus.InProgress, project.Milestones[0].Status);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingFile_ThrowsFileNotFoundException()
        {
            var provider = new DataProvider(Path.Combine(_tempDir, "nonexistent.json"), new MemoryCacheAdapter());
            await Assert.ThrowsAsync<FileNotFoundException>(() => provider.LoadProjectDataAsync());
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithInvalidJson_ThrowsJsonException()
        {
            CreateTestDataFile("{ invalid json }");
            var provider = new DataProvider(_testDataPath, new MemoryCacheAdapter());
            await Assert.ThrowsAsync<JsonException>(() => provider.LoadProjectDataAsync());
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingRequiredField_ThrowsJsonException()
        {
            var jsonMissingMilestones = """
                {
                    "metadata": "test"
                }
                """;
            CreateTestDataFile(jsonMissingMilestones);
            var provider = new DataProvider(_testDataPath, new MemoryCacheAdapter());
            await Assert.ThrowsAsync<JsonException>(() => provider.LoadProjectDataAsync());
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithInvalidEnumValue_ThrowsJsonException()
        {
            var jsonInvalidEnum = """
                {
                    "milestones": [
                        {"name": "Q1", "status": "InvalidStatus", "items": []}
                    ]
                }
                """;
            CreateTestDataFile(jsonInvalidEnum);
            var provider = new DataProvider(_testDataPath, new MemoryCacheAdapter());
            await Assert.ThrowsAsync<JsonException>(() => provider.LoadProjectDataAsync());
        }

        [Fact]
        public async Task LoadProjectDataAsync_UsesCache_ReturnsCachedDataOnSecondCall()
        {
            var validJson = """
                {
                    "milestones": [
                        {"name": "Q1", "status": "Completed", "items": []}
                    ]
                }
                """;
            CreateTestDataFile(validJson);

            var cache = new MemoryCacheAdapter();
            var provider = new DataProvider(_testDataPath, cache);

            var first = await provider.LoadProjectDataAsync();
            File.Delete(_testDataPath);
            var second = await provider.LoadProjectDataAsync();

            Assert.Equal(first.Milestones[0].Name, second.Milestones[0].Name);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
    }
}