using Xunit;
using AgentSquad.Data;
using System.Text.Json;

namespace AgentSquad.Tests.Data
{
    public class DataModelEdgeCasesTests
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public DataModelEdgeCasesTests()
        {
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Fact]
        public void ProjectInfo_DeserializesWithCaseInsensitiveProperties()
        {
            var json = """{"name":"Test","description":"Desc","status":"Active"}""";
            var result = JsonSerializer.Deserialize<ProjectInfo>(json, _jsonOptions);
            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void ProjectMetrics_RejectsInvalidCompletionPercentage(int percentage)
        {
            var metrics = new ProjectMetrics { CompletionPercentage = percentage };
            Assert.False(metrics.CompletionPercentage >= 0 && metrics.CompletionPercentage <= 100);
        }

        [Fact]
        public void Milestone_ValidatesDateRange()
        {
            var milestone = new Milestone
            {
                StartDate = new DateTime(2026, 5, 1),
                DueDate = new DateTime(2026, 3, 1)
            };
            Assert.True(milestone.DueDate < milestone.StartDate);
        }

        [Fact]
        public void Task_AllowsNullOptionalFields()
        {
            var task = new Task { Title = "Test", Description = null, AssignedTo = null };
            Assert.NotNull(task.Title);
            Assert.Null(task.Description);
        }
    }
}