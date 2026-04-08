using Xunit;
using System.Text.Json;
using AgentSquad.Services;
using AgentSquad.Models;

namespace AgentSquad.Tests.Services
{
    public class ProjectDataServiceTests
    {
        private readonly ProjectDataService _service;
        private readonly string _testDataPath;

        public ProjectDataServiceTests()
        {
            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            _service = new ProjectDataService();
            _testDataPath = Path.Combine(webRootPath, "data.json");
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithValidFile_ReturnsProjectData()
        {
            var result = await _service.LoadProjectDataAsync(_testDataPath);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task LoadProjectDataAsync_WithMissingFile_ThrowsFileNotFoundException()
        {
            var invalidPath = Path.Combine(Directory.GetCurrentDirectory(), "nonexistent.json");
            await Assert.ThrowsAsync<FileNotFoundException>(
                () => _service.LoadProjectDataAsync(invalidPath));
        }

        [Fact]
        public void ParseProjectData_ValidJson_ReturnsProjectData()
        {
            var json = CreateProjectJson();
            var result = _service.ParseProjectData(json);
            Assert.NotNull(result);
            Assert.NotEmpty(result.ProjectName);
        }

        [Fact]
        public void ParseProjectData_MissingRequiredFields_ThrowsException()
        {
            var json = "{}";
            Assert.Throws<JsonException>(() => _service.ParseProjectData(json));
        }

        [Fact]
        public void ParseProjectData_WithModifiedMetadata_ParsesCorrectly()
        {
            var json = CreateProjectJsonWithModification("ProjectName", "ModifiedProject");
            var result = _service.ParseProjectData(json);
            Assert.Equal("ModifiedProject", result.ProjectName);
        }

        [Fact]
        public void ParseProjectData_WithModifiedStatus_ParsesCorrectly()
        {
            var json = CreateProjectJsonWithModification("Status", "Completed");
            var result = _service.ParseProjectData(json);
            Assert.Equal("Completed", result.Status);
        }

        [Fact]
        public void ParseProjectData_WithModifiedProgress_ParsesCorrectly()
        {
            var json = CreateProjectJsonWithModification("CompletionPercentage", "95");
            var result = _service.ParseProjectData(json);
            Assert.Equal(95, result.CompletionPercentage);
        }

        private string CreateProjectJson()
        {
            return @"{
                ""ProjectName"": ""Test Project"",
                ""Status"": ""InProgress"",
                ""CompletionPercentage"": 50,
                ""StartDate"": ""2024-01-01"",
                ""EndDate"": ""2024-12-31"",
                ""Milestones"": []
            }";
        }

        private string CreateProjectJsonWithModification(string fieldName, string newValue)
        {
            var json = CreateProjectJson();
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                var root = doc.RootElement;
                using (var stream = new MemoryStream())
                {
                    using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
                    {
                        writer.WriteStartObject();
                        foreach (var property in root.EnumerateObject())
                        {
                            if (property.Name == fieldName)
                            {
                                writer.WritePropertyName(fieldName);
                                if (fieldName == "CompletionPercentage" && int.TryParse(newValue, out var intVal))
                                {
                                    writer.WriteNumberValue(intVal);
                                }
                                else
                                {
                                    writer.WriteStringValue(newValue);
                                }
                            }
                            else
                            {
                                writer.WritePropertyName(property.Name);
                                property.Value.WriteTo(writer);
                            }
                        }
                        writer.WriteEndObject();
                    }
                    return System.Text.Encoding.UTF8.GetString(stream.ToArray());
                }
            }
        }
    }
}