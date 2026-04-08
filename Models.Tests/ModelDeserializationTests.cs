using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Models.Tests
{
    public class ModelDeserializationTests
    {
        [Fact]
        public void ProjectJsonDeserialization_ValidData_DeserializesCorrectly()
        {
            // Arrange
            var json = @"{
                ""name"": ""Test Project"",
                ""description"": ""A test project"",
                ""startDate"": ""2024-01-01"",
                ""targetEndDate"": ""2024-12-31"",
                ""completionPercentage"": 45,
                ""healthStatus"": ""OnTrack"",
                ""velocityThisMonth"": 12,
                ""milestones"": [],
                ""workItems"": []
            }";

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var project = JsonSerializer.Deserialize<Project>(json, options);

            // Assert
            Assert.NotNull(project);
            Assert.Equal("Test Project", project.Name);
            Assert.Equal("A test project", project.Description);
            Assert.Equal(new DateTime(2024, 1, 1), project.StartDate);
            Assert.Equal(new DateTime(2024, 12, 31), project.TargetEndDate);
            Assert.Equal(45, project.CompletionPercentage);
            Assert.Equal(HealthStatus.OnTrack, project.HealthStatus);
            Assert.Equal(12, project.VelocityThisMonth);
            Assert.NotNull(project.Milestones);
            Assert.NotNull(project.WorkItems);
        }

        [Fact]
        public void MilestoneJsonDeserialization_ValidData_DeserializesCorrectly()
        {
            // Arrange
            var json = @"{
                ""name"": ""Phase 1 Launch"",
                ""targetDate"": ""2024-03-31"",
                ""status"": ""InProgress"",
                ""description"": ""Core feature rollout""
            }";

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var milestone = JsonSerializer.Deserialize<Milestone>(json, options);

            // Assert
            Assert.NotNull(milestone);
            Assert.Equal("Phase 1 Launch", milestone.Name);
            Assert.Equal(new DateTime(2024, 3, 31), milestone.TargetDate);
            Assert.Equal(MilestoneStatus.InProgress, milestone.Status);
            Assert.Equal("Core feature rollout", milestone.Description);
        }

        [Fact]
        public void WorkItemJsonDeserialization_ValidData_DeserializesCorrectly()
        {
            // Arrange
            var json = @"{
                ""title"": ""API Integration"",
                ""description"": ""Connect to external data source"",
                ""status"": ""InProgress"",
                ""assignedTo"": ""Team A""
            }";

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var workItem = JsonSerializer.Deserialize<WorkItem>(json, options);

            // Assert
            Assert.NotNull(workItem);
            Assert.Equal("API Integration", workItem.Title);
            Assert.Equal("Connect to external data source", workItem.Description);
            Assert.Equal(WorkItemStatus.InProgress, workItem.Status);
            Assert.Equal("Team A", workItem.AssignedTo);
        }

        [Fact]
        public void ProjectMetricsJsonDeserialization_ValidData_DeserializesCorrectly()
        {
            // Arrange
            var json = @"{
                ""completionPercentage"": 50,
                ""healthStatus"": ""AtRisk"",
                ""velocityThisMonth"": 8,
                ""totalMilestones"": 5,
                ""completedMilestones"": 2
            }";

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var metrics = JsonSerializer.Deserialize<ProjectMetrics>(json, options);

            // Assert
            Assert.NotNull(metrics);
            Assert.Equal(50, metrics.CompletionPercentage);
            Assert.Equal(HealthStatus.AtRisk, metrics.HealthStatus);
            Assert.Equal(8, metrics.VelocityThisMonth);
            Assert.Equal(5, metrics.TotalMilestones);
            Assert.Equal(2, metrics.CompletedMilestones);
        }

        [Fact]
        public void WorkItemJsonRoundtrip_SerializeDeserialize_Matches()
        {
            // Arrange
            var original = new WorkItem
            {
                Title = "Backend API Development",
                Description = "Implement REST endpoints",
                Status = WorkItemStatus.Shipped,
                AssignedTo = "Developer A"
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var json = JsonSerializer.Serialize(original, options);
            var deserialized = JsonSerializer.Deserialize<WorkItem>(json, options);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(original.Title, deserialized.Title);
            Assert.Equal(original.Description, deserialized.Description);
            Assert.Equal(original.Status, deserialized.Status);
            Assert.Equal(original.AssignedTo, deserialized.AssignedTo);
        }

        [Fact]
        public void MilestoneJsonRoundtrip_SerializeDeserialize_Matches()
        {
            // Arrange
            var original = new Milestone
            {
                Name = "Beta Release",
                TargetDate = new DateTime(2024, 6, 30),
                Status = MilestoneStatus.Completed,
                Description = "Public beta launch"
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var json = JsonSerializer.Serialize(original, options);
            var deserialized = JsonSerializer.Deserialize<Milestone>(json, options);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(original.Name, deserialized.Name);
            Assert.Equal(original.TargetDate, deserialized.TargetDate);
            Assert.Equal(original.Status, deserialized.Status);
            Assert.Equal(original.Description, deserialized.Description);
        }

        [Fact]
        public void ProjectJsonRoundtrip_WithCollections_Matches()
        {
            // Arrange
            var original = new Project
            {
                Name = "Q1 Initiative",
                Description = "First quarter goals",
                StartDate = new DateTime(2024, 1, 1),
                TargetEndDate = new DateTime(2024, 3, 31),
                CompletionPercentage = 33,
                HealthStatus = HealthStatus.OnTrack,
                VelocityThisMonth = 5,
                Milestones = new List<Milestone>
                {
                    new Milestone { Name = "Sprint 1", TargetDate = new DateTime(2024, 1, 31), Status = MilestoneStatus.Completed, Description = "Initial release" }
                },
                WorkItems = new List<WorkItem>
                {
                    new WorkItem { Title = "Feature A", Description = "Implement A", Status = WorkItemStatus.Shipped, AssignedTo = "Dev1" }
                }
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var json = JsonSerializer.Serialize(original, options);
            var deserialized = JsonSerializer.Deserialize<Project>(json, options);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(original.Name, deserialized.Name);
            Assert.Equal(original.CompletionPercentage, deserialized.CompletionPercentage);
            Assert.Single(deserialized.Milestones);
            Assert.Single(deserialized.WorkItems);
        }
    }
}