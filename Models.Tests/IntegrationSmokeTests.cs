using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Models.Tests
{
    public class IntegrationSmokeTests
    {
        private const string SampleDataJson = @"{
            ""name"": ""Executive Project Dashboard"",
            ""description"": ""Sample executive project for testing"",
            ""startDate"": ""2024-01-01"",
            ""targetEndDate"": ""2024-12-31"",
            ""completionPercentage"": 42,
            ""healthStatus"": ""OnTrack"",
            ""velocityThisMonth"": 12,
            ""milestones"": [
                {
                    ""name"": ""Phase 1 Launch"",
                    ""targetDate"": ""2024-03-31"",
                    ""status"": ""Completed"",
                    ""description"": ""Core features released""
                },
                {
                    ""name"": ""Phase 2 Enhancement"",
                    ""targetDate"": ""2024-06-30"",
                    ""status"": ""InProgress"",
                    ""description"": ""Advanced feature set""
                },
                {
                    ""name"": ""Phase 3 Optimization"",
                    ""targetDate"": ""2024-09-30"",
                    ""status"": ""Future"",
                    ""description"": ""Performance improvements""
                }
            ],
            ""workItems"": [
                {
                    ""title"": ""API Development"",
                    ""description"": ""Implement REST endpoints"",
                    ""status"": ""Shipped"",
                    ""assignedTo"": ""Team A""
                },
                {
                    ""title"": ""Frontend Dashboard"",
                    ""description"": ""Build executive dashboard UI"",
                    ""status"": ""InProgress"",
                    ""assignedTo"": ""Team B""
                },
                {
                    ""title"": ""Database Migration"",
                    ""description"": ""Migrate legacy data"",
                    ""status"": ""CarriedOver"",
                    ""assignedTo"": ""Team C""
                }
            ]
        }";

        [Fact]
        public void LoadSampleData_ValidJson_DeserializesSuccessfully()
        {
            // Arrange
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var project = JsonSerializer.Deserialize<Project>(SampleDataJson, options);

            // Assert
            Assert.NotNull(project);
            Assert.Equal("Executive Project Dashboard", project.Name);
            Assert.Equal("Sample executive project for testing", project.Description);
        }

        [Fact]
        public void LoadSampleData_MilestonesPopulated_CorrectCount()
        {
            // Arrange
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var project = JsonSerializer.Deserialize<Project>(SampleDataJson, options);

            // Assert
            Assert.NotNull(project.Milestones);
            Assert.Equal(3, project.Milestones.Count);
        }

        [Fact]
        public void LoadSampleData_MilestoneStatusesCorrect()
        {
            // Arrange
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var project = JsonSerializer.Deserialize<Project>(SampleDataJson, options);

            // Assert
            Assert.Equal(MilestoneStatus.Completed, project.Milestones[0].Status);
            Assert.Equal(MilestoneStatus.InProgress, project.Milestones[1].Status);
            Assert.Equal(MilestoneStatus.Future, project.Milestones[2].Status);
        }

        [Fact]
        public void LoadSampleData_WorkItemsPopulated_CorrectCount()
        {
            // Arrange
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var project = JsonSerializer.Deserialize<Project>(SampleDataJson, options);

            // Assert
            Assert.NotNull(project.WorkItems);
            Assert.Equal(3, project.WorkItems.Count);
        }

        [Fact]
        public void LoadSampleData_WorkItemStatusesCorrect()
        {
            // Arrange
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var project = JsonSerializer.Deserialize<Project>(SampleDataJson, options);

            // Assert
            Assert.Equal(WorkItemStatus.Shipped, project.WorkItems[0].Status);
            Assert.Equal(WorkItemStatus.InProgress, project.WorkItems[1].Status);
            Assert.Equal(WorkItemStatus.CarriedOver, project.WorkItems[2].Status);
        }

        [Fact]
        public void LoadSampleData_ProjectMetricsAccessible()
        {
            // Arrange
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var project = JsonSerializer.Deserialize<Project>(SampleDataJson, options);

            // Assert
            Assert.Equal(42, project.CompletionPercentage);
            Assert.Equal(HealthStatus.OnTrack, project.HealthStatus);
            Assert.Equal(12, project.VelocityThisMonth);
        }

        [Fact]
        public void LoadSampleData_DateTimesCorrect()
        {
            // Arrange
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var project = JsonSerializer.Deserialize<Project>(SampleDataJson, options);

            // Assert
            Assert.Equal(new DateTime(2024, 1, 1), project.StartDate);
            Assert.Equal(new DateTime(2024, 12, 31), project.TargetEndDate);
            Assert.Equal(new DateTime(2024, 3, 31), project.Milestones[0].TargetDate);
        }

        [Fact]
        public void LoadSampleData_NoNullReferencesOnPropertyAccess()
        {
            // Arrange
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var project = JsonSerializer.Deserialize<Project>(SampleDataJson, options);

            // Act & Assert - Should not throw NullReferenceException
            var name = project.Name;
            var milestoneCount = project.Milestones.Count;
            var firstMilestoneName = project.Milestones[0].Name;
            var workItemCount = project.WorkItems.Count;
            var firstItemTitle = project.WorkItems[0].Title;

            Assert.NotNull(name);
            Assert.NotEmpty(firstMilestoneName);
            Assert.NotEmpty(firstItemTitle);
        }

        [Fact]
        public void LoadSampleData_FullRoundtrip_SerializeDeserialize()
        {
            // Arrange
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var original = JsonSerializer.Deserialize<Project>(SampleDataJson, options);

            // Act
            var serialized = JsonSerializer.Serialize(original, options);
            var deserialized = JsonSerializer.Deserialize<Project>(serialized, options);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(original.Name, deserialized.Name);
            Assert.Equal(original.CompletionPercentage, deserialized.CompletionPercentage);
            Assert.Equal(original.Milestones.Count, deserialized.Milestones.Count);
            Assert.Equal(original.WorkItems.Count, deserialized.WorkItems.Count);
        }

        [Fact]
        public void LoadSampleData_AllMilestoneFieldsPopulated()
        {
            // Arrange
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var project = JsonSerializer.Deserialize<Project>(SampleDataJson, options);
            var milestone = project.Milestones[0];

            // Assert
            Assert.NotNull(milestone.Name);
            Assert.NotEqual(default(DateTime), milestone.TargetDate);
            Assert.NotEqual(default(MilestoneStatus), milestone.Status);
            Assert.NotNull(milestone.Description);
        }

        [Fact]
        public void LoadSampleData_AllWorkItemFieldsPopulated()
        {
            // Arrange
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var project = JsonSerializer.Deserialize<Project>(SampleDataJson, options);
            var workItem = project.WorkItems[0];

            // Assert
            Assert.NotNull(workItem.Title);
            Assert.NotNull(workItem.Description);
            Assert.NotEqual(default(WorkItemStatus), workItem.Status);
            Assert.NotNull(workItem.AssignedTo);
        }
    }
}