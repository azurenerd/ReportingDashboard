using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using AgentSquad.Services;
using AgentSquad.Models;

namespace AgentSquad.Tests.Services
{
    public class ProjectDataServiceTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly string _originalDirectory;
        private readonly ProjectDataService _service;

        public ProjectDataServiceTests()
        {
            _originalDirectory = Directory.GetCurrentDirectory();
            _testDirectory = Path.Combine(Path.GetTempPath(), $"ProjectDataServiceTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
            _service = new ProjectDataService();
        }

        [Fact]
        public void LoadProjectDataAsync_WithValidPath_ReturnsProjectData()
        {
            var testFile = Path.Combine(_testDirectory, "valid.json");
            File.WriteAllText(testFile, "{}");

            var result = _service.LoadProjectDataAsync(testFile).Result;

            Assert.NotNull(result);
        }

        [Fact]
        public void LoadProjectDataAsync_WithInvalidPath_ThrowsException()
        {
            var invalidPath = Path.Combine(_testDirectory, "nonexistent.json");

            Assert.ThrowsAsync<FileNotFoundException>(
                async () => await _service.LoadProjectDataAsync(invalidPath)
            );
        }

        public void Dispose()
        {
            Directory.SetCurrentDirectory(_originalDirectory);
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
        }
    }
}