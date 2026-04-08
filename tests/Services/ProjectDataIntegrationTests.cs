using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using AgentSquad.Services;
using AgentSquad.Models;

namespace AgentSquad.Tests.Integration
{
    public class ProjectDataIntegrationTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly ProjectDataService _projectDataService;

        public ProjectDataIntegrationTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), $"ProjectDataTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
            _projectDataService = new ProjectDataService();
        }

        [Fact]
        public void LoadProjectData_WithValidJson_DeserializesCorrectly()
        {
            var testFile = Path.Combine(_testDirectory, "sample.json");
            var sampleData = GetValidSampleData();
            System.Text.Json.JsonSerializer.SerializeToFile(sampleData, testFile);

            var result = _projectDataService.LoadProjectDataAsync(testFile).Result;

            Assert.NotNull(result);
            Assert.Equal(3, result.Milestones.Count);
            Assert.Equal(3, result.Tasks.Count);
        }

        [Fact]
        public void LoadProjectData_WithInvalidJson_ThrowsException()
        {
            var testFile = Path.Combine(_testDirectory, "invalid.json");
            File.WriteAllText(testFile, "{ invalid json }");

            Assert.ThrowsAsync<System.Text.Json.JsonException>(
                async () => await _projectDataService.LoadProjectDataAsync(testFile)
            );
        }

        [Fact]
        public void LoadProjectData_WithMissingFile_ThrowsFileNotFoundException()
        {
            var testFile = Path.Combine(_testDirectory, "nonexistent.json");

            Assert.ThrowsAsync<FileNotFoundException>(
                async () => await _projectDataService.LoadProjectDataAsync(testFile)
            );
        }

        private ProjectData GetValidSampleData()
        {
            return new ProjectData
            {
                Milestones = new List<Milestone>
                {
                    new Milestone { Id = "m1", Name = "Phase 1", Status = "InProgress", TargetDate = DateTime.Now.AddMonths(1) },
                    new Milestone { Id = "m2", Name = "Phase 2", Status = "Planning", TargetDate = DateTime.Now.AddMonths(2) },
                    new Milestone { Id = "m3", Name = "Phase 3", Status = "Planning", TargetDate = DateTime.Now.AddMonths(3) }
                },
                Tasks = new List<ProjectTask>
                {
                    new ProjectTask { Id = "t1", Name = "Task 1", Status = "Shipped", Owner = "Dev1", EstimatedDays = 5, RelatedMilestone = "m1" },
                    new ProjectTask { Id = "t2", Name = "Task 2", Status = "InProgress", Owner = "Dev2", EstimatedDays = 3, RelatedMilestone = "m1" },
                    new ProjectTask { Id = "t3", Name = "Task 3", Status = "CarriedOver", Owner = "Dev3", EstimatedDays = 8, RelatedMilestone = "m2" }
                }
            };
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
        }
    }
}