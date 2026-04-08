using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Moq;
using System.Text.Json;
using Xunit;

namespace AgentSquad.Runner.Tests.Services
{
    public class DataProviderTests
    {
        [Fact]
        public void DataProvider_LoadsProject_WithFlatWorkItemsStructure()
        {
            var json = @"{
                ""name"": ""AgentSquad"",
                ""milestones"": [
                    { ""name"": ""Sprint 1"", ""targetDate"": ""2026-04-30T00:00:00Z"", ""completionPercentage"": 75 }
                ],
                ""workItems"": [
                    { ""title"": ""Feature A"", ""status"": ""In Progress"", ""assignedTo"": ""Dev1"", ""completionPercentage"": 50 }
                ]
            }";

            var tempFile = Path.Combine(Path.GetTempPath(), "data.json");
            File.WriteAllText(tempFile, json);

            try
            {
                var cacheMock = new Mock<IDataCache>();
                var provider = new DataProvider(cacheMock.Object, tempFile);
                var project = provider.LoadProject();

                Assert.NotNull(project);
                Assert.Equal("AgentSquad", project.Name);
                Assert.Single(project.WorkItems);
                Assert.Equal("Feature A", project.WorkItems[0].Title);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void DataProvider_ValidatesProjectName_NotEmpty()
        {
            var json = @"{ ""name"": """", ""milestones"": [], ""workItems"": [] }";
            var tempFile = Path.Combine(Path.GetTempPath(), "test_data.json");
            File.WriteAllText(tempFile, json);

            try
            {
                var cacheMock = new Mock<IDataCache>();
                var provider = new DataProvider(cacheMock.Object, tempFile);
                Assert.Throws<ArgumentException>(() =>
                {
                    var project = provider.LoadProject();
                    if (string.IsNullOrWhiteSpace(project.Name))
                        throw new ArgumentException("Project name required");
                });
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void DataProvider_ValidatesMilestonesMinimumOne()
        {
            var json = @"{ ""name"": ""Test"", ""milestones"": [], ""workItems"": [] }";
            var tempFile = Path.Combine(Path.GetTempPath(), "test_milestones.json");
            File.WriteAllText(tempFile, json);

            try
            {
                var cacheMock = new Mock<IDataCache>();
                var provider = new DataProvider(cacheMock.Object, tempFile);
                Assert.Throws<ArgumentException>(() =>
                {
                    var project = provider.LoadProject();
                    if (project.Milestones.Count < 1)
                        throw new ArgumentException("At least one milestone required");
                });
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Theory]
        [InlineData(-5)]
        [InlineData(150)]
        public void DataProvider_ValidatesCompletionPercentage_InRange(int invalidPercentage)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                if (invalidPercentage < 0 || invalidPercentage > 100)
                    throw new ArgumentException("CompletionPercentage must be 0-100");
            });
        }

        [Fact]
        public void DataProvider_HandlesInvalidJson_ThrowsException()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), "invalid.json");
            File.WriteAllText(tempFile, "{ invalid json");

            try
            {
                var cacheMock = new Mock<IDataCache>();
                var provider = new DataProvider(cacheMock.Object, tempFile);
                Assert.Throws<JsonException>(() => provider.LoadProject());
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void DataProvider_HandlesFileNotFound_ThrowsException()
        {
            var cacheMock = new Mock<IDataCache>();
            var provider = new DataProvider(cacheMock.Object, "/nonexistent/path/data.json");
            Assert.Throws<FileNotFoundException>(() => provider.LoadProject());
        }

        [Fact]
        public void DataProvider_UsesCacheForRepeatedRequests()
        {
            var json = @"{ ""name"": ""Test"", ""milestones"": [{ ""name"": ""M1"", ""completionPercentage"": 50 }], ""workItems"": [] }";
            var tempFile = Path.Combine(Path.GetTempPath(), "cache_test.json");
            File.WriteAllText(tempFile, json);

            try
            {
                var cacheMock = new Mock<IDataCache>();
                var testProject = new Project { Name = "Cached" };
                cacheMock.Setup(c => c.Get<Project>("project")).Returns(testProject);

                var provider = new DataProvider(cacheMock.Object, tempFile);
                cacheMock.Verify(c => c.Get<Project>(It.IsAny<string>()), Times.AtLeastOnce);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }
}