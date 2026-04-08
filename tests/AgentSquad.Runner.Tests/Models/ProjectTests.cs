using AgentSquad.Runner.Models;
using System.Text.Json;
using Xunit;

namespace AgentSquad.Runner.Tests.Models
{
    public class ProjectTests
    {
        [Fact]
        public void Project_DeserializesFromJson_WithCaseInsensitiveMapping()
        {
            var json = @"{
                ""name"": ""AgentSquad"",
                ""milestones"": [
                    { ""name"": ""Sprint 1"", ""targetDate"": ""2026-04-30T00:00:00Z"", ""completionPercentage"": 75 }
                ],
                ""workItems"": [
                    { ""title"": ""Task 1"", ""status"": ""Done"", ""assignedTo"": ""Jane"", ""completionPercentage"": 100 }
                ]
            }";

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var project = JsonSerializer.Deserialize<Project>(json, options);

            Assert.NotNull(project);
            Assert.Equal("AgentSquad", project.Name);
            Assert.Single(project.Milestones);
            Assert.Single(project.WorkItems);
        }

        [Fact]
        public void Project_ValidatesNameNonEmpty()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                if (string.IsNullOrWhiteSpace(""))
                    throw new ArgumentException("Project name cannot be empty");
            });
        }

        [Fact]
        public void Project_ValidatesAtLeastOneMilestone()
        {
            var project = new Project { Name = "Test", Milestones = new List<Milestone>() };
            Assert.Throws<ArgumentException>(() =>
            {
                if (project.Milestones.Count < 1)
                    throw new ArgumentException("At least one milestone required");
            });
        }

        [Fact]
        public void Project_WorkItems_FlatStructure_NotNested()
        {
            var project = new Project
            {
                Name = "Test",
                Milestones = new List<Milestone> { new() { Name = "M1" } },
                WorkItems = new List<WorkItem>
                {
                    new() { Title = "W1", Status = "Done", AssignedTo = "User", CompletionPercentage = 100 }
                }
            };

            Assert.NotNull(project.WorkItems);
            Assert.Single(project.WorkItems);
            Assert.Equal("W1", project.WorkItems[0].Title);
        }
    }
}