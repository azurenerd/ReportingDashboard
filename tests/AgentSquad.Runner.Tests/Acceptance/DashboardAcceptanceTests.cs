using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using Xunit;

namespace AgentSquad.Runner.Tests.Acceptance
{
    public class DashboardAcceptanceTests
    {
        private readonly string _wwwRootDataPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data.json");

        [Fact]
        public void Dashboard_LoadsProjectData_FromWwwRoot()
        {
            var testDataJson = @"{
                ""name"": ""AgentSquad Dashboard"",
                ""milestones"": [
                    { 
                        ""name"": ""Planning Phase"", 
                        ""targetDate"": ""2026-05-01T00:00:00Z"",
                        ""completionPercentage"": 100 
                    },
                    { 
                        ""name"": ""Development Sprint 1"", 
                        ""targetDate"": ""2026-06-01T00:00:00Z"",
                        ""completionPercentage"": 60 
                    }
                ],
                ""workItems"": [
                    { ""title"": ""Setup Project"", ""status"": ""Done"", ""assignedTo"": ""Alice"", ""completionPercentage"": 100 },
                    { ""title"": ""Create Data Models"", ""status"": ""In Progress"", ""assignedTo"": ""Bob"", ""completionPercentage"": 75 },
                    { ""title"": ""Build Dashboard"", ""status"": ""Pending"", ""assignedTo"": ""Charlie"", ""completionPercentage"": 25 }
                ]
            }";

            var tempPath = Path.Combine(Path.GetTempPath(), "dashboard_test.json");
            File.WriteAllText(tempPath, testDataJson);

            try
            {
                var cache = new MemoryCacheAdapter();
                var provider = new DataProvider(cache, tempPath);
                var project = provider.LoadProject();

                Assert.NotNull(project);
                Assert.Equal("AgentSquad Dashboard", project.Name);
                Assert.Equal(2, project.Milestones.Count);
                Assert.Equal(3, project.WorkItems.Count);
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        [Fact]
        public void Dashboard_DisplaysMetrics_Correctly()
        {
            var testDataJson = @"{
                ""name"": ""Dashboard Metrics"",
                ""milestones"": [
                    { ""name"": ""M1"", ""targetDate"": ""2026-05-01T00:00:00Z"", ""completionPercentage"": 80 }
                ],
                ""workItems"": [
                    { ""title"": ""Task 1"", ""status"": ""Done"", ""assignedTo"": ""User1"", ""completionPercentage"": 100 },
                    { ""title"": ""Task 2"", ""status"": ""Done"", ""assignedTo"": ""User2"", ""completionPercentage"": 100 },
                    { ""title"": ""Task 3"", ""status"": ""In Progress"", ""assignedTo"": ""User1"", ""completionPercentage"": 60 }
                ]
            }";

            var tempPath = Path.Combine(Path.GetTempPath(), "metrics_test.json");
            File.WriteAllText(tempPath, testDataJson);

            try
            {
                var cache = new MemoryCacheAdapter();
                var provider = new DataProvider(cache, tempPath);
                var project = provider.LoadProject();

                var totalItems = project.WorkItems.Count;
                var completedItems = project.WorkItems.Count(w => w.CompletionPercentage == 100);
                var overallProgress = project.Milestones.FirstOrDefault()?.CompletionPercentage ?? 0;

                Assert.Equal(3, totalItems);
                Assert.Equal(2, completedItems);
                Assert.Equal(80, overallProgress);
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        [Fact]
        public void Dashboard_GroupsWorkItemsByStatus()
        {
            var testDataJson = @"{
                ""name"": ""Status Grouping"",
                ""milestones"": [
                    { ""name"": ""M1"", ""targetDate"": ""2026-05-01T00:00:00Z"", ""completionPercentage"": 50 }
                ],
                ""workItems"": [
                    { ""title"": ""Done Item"", ""status"": ""Done"", ""assignedTo"": ""Dev1"", ""completionPercentage"": 100 },
                    { ""title"": ""In Progress Item"", ""status"": ""In Progress"", ""assignedTo"": ""Dev2"", ""completionPercentage"": 50 },
                    { ""title"": ""Pending Item"", ""status"": ""Pending"", ""assignedTo"": ""Dev3"", ""completionPercentage"": 0 }
                ]
            }";

            var tempPath = Path.Combine(Path.GetTempPath(), "grouping_test.json");
            File.WriteAllText(tempPath, testDataJson);

            try
            {
                var cache = new MemoryCacheAdapter();
                var provider = new DataProvider(cache, tempPath);
                var project = provider.LoadProject();

                var byStatus = project.WorkItems.GroupBy(w => w.Status).ToList();
                Assert.Equal(3, byStatus.Count);
                Assert.Single(byStatus.First(g => g.Key == "Done"));
                Assert.Single(byStatus.First(g => g.Key == "In Progress"));
                Assert.Single(byStatus.First(g => g.Key == "Pending"));
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        [Fact]
        public void Dashboard_ValidatesRequiredFields_InProductionPath()
        {
            var validJson = @"{
                ""name"": ""ValidProject"",
                ""milestones"": [
                    { ""name"": ""Milestone"", ""targetDate"": ""2026-05-01T00:00:00Z"", ""completionPercentage"": 50 }
                ],
                ""workItems"": [
                    { ""title"": ""Item"", ""status"": ""Pending"", ""assignedTo"": ""Dev"", ""completionPercentage"": 25 }
                ]
            }";

            var tempPath = Path.Combine(Path.GetTempPath(), "validation_test.json");
            File.WriteAllText(tempPath, validJson);

            try
            {
                var cache = new MemoryCacheAdapter();
                var provider = new DataProvider(cache, tempPath);
                var project = provider.LoadProject();

                Assert.NotEmpty(project.Name);
                Assert.NotEmpty(project.Milestones);
                Assert.All(project.WorkItems, w =>
                {
                    Assert.NotEmpty(w.Title);
                    Assert.InRange(w.CompletionPercentage, 0, 100);
                });
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }
    }
}