using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using AgentSquad.Services;
using AgentSquad.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AgentSquad.Tests.Services
{
    public class DataProviderTests
    {
        private readonly Mock<ILogger<DataProvider>> _mockLogger;
        private readonly Mock<IDataCache> _mockDataCache;
        private readonly Mock<IWebHostEnvironment> _mockHostEnvironment;
        private readonly DataProvider _dataProvider;

        public DataProviderTests()
        {
            _mockLogger = new Mock<ILogger<DataProvider>>();
            _mockDataCache = new Mock<IDataCache>();
            _mockHostEnvironment = new Mock<IWebHostEnvironment>();
            
            _mockHostEnvironment.Setup(h => h.ContentRootPath)
                .Returns(Path.GetTempPath());

            _dataProvider = new DataProvider(_mockLogger.Object, _mockDataCache.Object, _mockHostEnvironment.Object);
        }

        [Fact]
        public async Task LoadProjectsAsync_ValidJsonStructure_ReturnsProjects()
        {
            var json = @"
            {
                ""projects"": [
                    {
                        ""id"": ""p1"",
                        ""name"": ""Test Project"",
                        ""startDate"": ""2024-01-01T00:00:00Z"",
                        ""endDate"": ""2024-12-31T23:59:59Z""
                    }
                ]
            }";

            var tempFile = Path.Combine(Path.GetTempPath(), "data.json");
            await File.WriteAllTextAsync(tempFile, json);

            try
            {
                var projects = await _dataProvider.LoadProjectsAsync(tempFile);
                Assert.NotEmpty(projects);
                Assert.Equal("p1", projects[0].Id);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LoadProjectsAsync_MissingFile_ThrowsFileNotFoundException()
        {
            var nonexistentPath = Path.Combine(Path.GetTempPath(), "nonexistent_data.json");
            
            await Assert.ThrowsAsync<FileNotFoundException>(() => 
                _dataProvider.LoadProjectsAsync(nonexistentPath));
        }

        [Fact]
        public async Task LoadProjectsAsync_InvalidJsonFormat_ThrowsJsonException()
        {
            var invalidJson = "{ invalid json }";
            var tempFile = Path.Combine(Path.GetTempPath(), "invalid.json");
            await File.WriteAllTextAsync(tempFile, invalidJson);

            try
            {
                await Assert.ThrowsAsync<JsonException>(() => 
                    _dataProvider.LoadProjectsAsync(tempFile));
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Theory]
        [InlineData("2024-13-40")]
        [InlineData("2024-12-32")]
        [InlineData("invalid-date-format")]
        public async Task LoadProjectsAsync_InvalidDateFormat_ThrowsFormatException(string invalidDate)
        {
            var json = $@"
            {{
                ""projects"": [
                    {{
                        ""id"": ""p1"",
                        ""name"": ""Test"",
                        ""startDate"": ""{invalidDate}"",
                        ""endDate"": ""2024-12-31T23:59:59Z""
                    }}
                ]
            }}";

            var tempFile = Path.Combine(Path.GetTempPath(), "date_test.json");
            await File.WriteAllTextAsync(tempFile, json);

            try
            {
                await Assert.ThrowsAsync<FormatException>(() => 
                    _dataProvider.LoadProjectsAsync(tempFile));
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Theory]
        [InlineData("InvalidMilestoneStatus")]
        [InlineData("in_progress")]
        [InlineData("COMPLETED")]
        public async Task LoadProjectsAsync_InvalidMilestoneStatus_ThrowsInvalidOperationException(string invalidStatus)
        {
            var json = $@"
            {{
                ""projects"": [
                    {{
                        ""id"": ""p1"",
                        ""name"": ""Test"",
                        ""startDate"": ""2024-01-01T00:00:00Z"",
                        ""endDate"": ""2024-12-31T23:59:59Z"",
                        ""milestones"": [
                            {{
                                ""id"": ""m1"",
                                ""name"": ""v1.0"",
                                ""status"": ""{invalidStatus}"",
                                ""dueDate"": ""2024-06-30T23:59:59Z""
                            }}
                        ]
                    }}
                ]
            }}";

            var tempFile = Path.Combine(Path.GetTempPath(), "enum_test.json");
            await File.WriteAllTextAsync(tempFile, json);

            try
            {
                await Assert.ThrowsAsync<JsonException>(() => 
                    _dataProvider.LoadProjectsAsync(tempFile));
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Theory]
        [InlineData("InvalidWorkItemStatus")]
        [InlineData("todo")]
        public async Task LoadProjectsAsync_InvalidWorkItemStatus_ThrowsInvalidOperationException(string invalidStatus)
        {
            var json = $@"
            {{
                ""projects"": [
                    {{
                        ""id"": ""p1"",
                        ""name"": ""Test"",
                        ""startDate"": ""2024-01-01T00:00:00Z"",
                        ""endDate"": ""2024-12-31T23:59:59Z"",
                        ""workItems"": [
                            {{
                                ""id"": ""wi1"",
                                ""title"": ""Task"",
                                ""status"": ""{invalidStatus}"",
                                ""milestoneId"": ""m1"",
                                ""completionPercentage"": 0
                            }}
                        ]
                    }}
                ]
            }}";

            var tempFile = Path.Combine(Path.GetTempPath(), "wi_enum_test.json");
            await File.WriteAllTextAsync(tempFile, json);

            try
            {
                await Assert.ThrowsAsync<JsonException>(() => 
                    _dataProvider.LoadProjectsAsync(tempFile));
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-50)]
        [InlineData(101)]
        [InlineData(150)]
        public async Task LoadProjectsAsync_WithInvalidCompletionPercentage_ThrowsInvalidOperationException(int percentage)
        {
            var json = $@"
            {{
                ""projects"": [
                    {{
                        ""id"": ""p1"",
                        ""name"": ""Test"",
                        ""startDate"": ""2024-01-01T00:00:00Z"",
                        ""endDate"": ""2024-12-31T23:59:59Z"",
                        ""workItems"": [
                            {{
                                ""id"": ""wi1"",
                                ""title"": ""Task"",
                                ""status"": ""Todo"",
                                ""milestoneId"": ""m1"",
                                ""completionPercentage"": {percentage}
                            }}
                        ]
                    }}
                ]
            }}";

            var tempFile = Path.Combine(Path.GetTempPath(), "cp_test.json");
            await File.WriteAllTextAsync(tempFile, json);

            try
            {
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                    _dataProvider.LoadProjectsAsync(tempFile));
                Assert.Contains("CompletionPercentage", ex.Message);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task LoadProjectsAsync_WithNullableFields_SuccessfullyDeserializes()
        {
            var json = @"
            {
                ""projects"": [
                    {
                        ""id"": ""p1"",
                        ""name"": ""Test"",
                        ""startDate"": ""2024-01-01T00:00:00Z"",
                        ""endDate"": ""2024-12-31T23:59:59Z"",
                        ""milestones"": [
                            {
                                ""id"": ""m1"",
                                ""name"": ""v1.0"",
                                ""status"": ""InProgress"",
                                ""dueDate"": ""2024-06-30T23:59:59Z"",
                                ""description"": null
                            }
                        ],
                        ""workItems"": [
                            {
                                ""id"": ""wi1"",
                                ""title"": ""Task"",
                                ""status"": ""Todo"",
                                ""milestoneId"": ""m1"",
                                ""assignedTo"": null,
                                ""completionPercentage"": 0
                            }
                        ]
                    }
                ]
            }";

            var tempFile = Path.Combine(Path.GetTempPath(), "nullable_test.json");
            await File.WriteAllTextAsync(tempFile, json);

            try
            {
                var projects = await _dataProvider.LoadProjectsAsync(tempFile);
                Assert.NotEmpty(projects);
                Assert.Null(projects[0].Milestones[0].Description);
                Assert.Null(projects[0].WorkItems[0].AssignedTo);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }
}