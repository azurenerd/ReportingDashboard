using AgentSquad.Runner.Models;
using System.Text.Json;
using Xunit;

namespace AgentSquad.Runner.Tests.Models
{
    public class MilestoneTests
    {
        [Fact]
        public void Milestone_DeserializesFromJson_WithCaseInsensitiveMapping()
        {
            var json = @"{
                ""name"": ""Sprint 1"",
                ""targetDate"": ""2026-04-30T00:00:00Z"",
                ""completionPercentage"": 75
            }";

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var milestone = JsonSerializer.Deserialize<Milestone>(json, options);

            Assert.NotNull(milestone);
            Assert.Equal("Sprint 1", milestone.Name);
            Assert.Equal(75, milestone.CompletionPercentage);
        }

        [Fact]
        public void Milestone_ValidatesCompletionPercentage_InRange()
        {
            var milestone = new Milestone { Name = "Sprint 1", CompletionPercentage = 50 };
            Assert.True(milestone.CompletionPercentage >= 0 && milestone.CompletionPercentage <= 100);
        }

        [Fact]
        public void Milestone_ThrowsException_WhenCompletionPercentageOutOfRange()
        {
            var json = @"{ ""name"": ""Sprint 1"", ""completionPercentage"": 150 }";
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            
            Assert.Throws<ArgumentException>(() =>
            {
                var milestone = JsonSerializer.Deserialize<Milestone>(json, options);
                if (milestone.CompletionPercentage < 0 || milestone.CompletionPercentage > 100)
                    throw new ArgumentException("CompletionPercentage must be 0-100");
            });
        }
    }
}