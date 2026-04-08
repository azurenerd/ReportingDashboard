using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AgentSquad.Runner.Data;
using AgentSquad.Runner.Services;
using Xunit;

namespace AgentSquad.Runner.Tests.Integration
{
    public class DashboardIntegrationTests
    {
        [Fact]
        public async Task LoadAndDisplay_WithCompleteProject_SuccessfullyLoads()
        {
            var testDataPath = Path.Combine(Path.GetTempPath(), "complete_project.json");
            var testData = new
            {
                projectName = "Q2 Mobile App Release",
                projectStartDate = "2024-01-01",
                projectEndDate = "2024-06-30",
                milestones = new[]
                {
                    new { name = "Design Phase", targetDate = "2024-02-15", status = "Completed", completionPercentage = 100 },
                    new { name = "Development", targetDate = "2024-04-30", status = "InProgress", completionPercentage = 60 },
                    new { name = "Testing", targetDate = "2024-06-15", status = "Pending", completionPercentage = 0 }
                },
                tasks = new[]
                {
                    new { name = "API Development", status = "Shipped", owner = "Team A" },
                    new { name = "Frontend Development", status = "InProgress", owner = "Team B" },
                    new { name = "Database Migration", status = "CarriedOver", owner = "Team C" }
                }
            };

            File.WriteAllText(testDataPath, JsonSerializer.Serialize(testData));

            try
            {
                var service = new ProjectDataService();
                var result = await service.LoadProjectDataAsync(testDataPath);

                Assert.NotNull(result);
                Assert.Equal("Q2 Mobile App Release", result.ProjectName);
                Assert.Equal(3, result.Milestones.Count);
                Assert.Equal(3, result.Tasks.Count);

                var summary = service.GetTaskStatusSummary(result.Tasks);
                Assert.Equal(1, summary.ShippedCount);
                Assert.Equal(1, summary.InProgressCount);
                Assert.Equal(1, summary.CarriedOverCount);

                var completion = service.CalculateCompletionPercentage(result.Tasks);
                Assert.Equal(33, completion);
            }
            finally
            {
                if (File.Exists(testDataPath))
                    File.Delete(testDataPath);
            }
        }

        [Fact]
        public async Task LoadAndDisplay_WithAllTasksShipped_CompletionIs100()
        {
            var testDataPath = Path.Combine(Path.GetTempPath(), "shipped_project.json");
            var testData = new
            {
                projectName = "Completed Project",
                projectStartDate = "2024-01-01",
                projectEndDate = "2024-03-31",
                milestones = new object[] { },
                tasks = new[]
                {
                    new { name = "Task 1", status = "Shipped", owner = "Alice" },
                    new { name = "Task 2", status = "Shipped", owner = "Bob" },
                    new { name = "Task 3", status = "Shipped", owner = "Charlie" }
                }
            };

            File.WriteAllText(testDataPath, JsonSerializer.Serialize(testData));

            try
            {
                var service = new ProjectDataService();
                var result = await service.LoadProjectDataAsync(testDataPath);
                var completion = service.CalculateCompletionPercentage(result.Tasks);

                Assert.Equal(100, completion);
            }
            finally
            {
                if (File.Exists(testDataPath))
                    File.Delete(testDataPath);
            }
        }

        [Fact]
        public async Task RefreshAfterEdit_UpdatesDisplay()
        {
            var testDataPath = Path.Combine(Path.GetTempPath(), "refresh_test.json");
            var initialData = new
            {
                projectName = "Test Project",
                projectStartDate = "2024-01-01",
                projectEndDate = "2024-12-31",
                milestones = new object[] { },
                tasks = new[]
                {
                    new { name = "Task 1", status = "InProgress", owner = "Alice" }
                }
            };

            File.WriteAllText(testDataPath, JsonSerializer.Serialize(initialData));

            try
            {
                var service = new ProjectDataService();
                var result1 = await service.LoadProjectDataAsync(testDataPath);
                var summary1 = service.GetTaskStatusSummary(result1.Tasks);
                Assert.Equal(0, summary1.ShippedCount);
                Assert.Equal(1, summary1.InProgressCount);

                var updatedData = new
                {
                    projectName = "Test Project",
                    projectStartDate = "2024-01-01",
                    projectEndDate = "2024-12-31",
                    milestones = new object[] { },
                    tasks = new[]
                    {
                        new { name = "Task 1", status = "Shipped", owner = "Alice" }
                    }
                };

                File.WriteAllText(testDataPath, JsonSerializer.Serialize(updatedData));

                var result2 = await service.LoadProjectDataAsync(testDataPath);
                var summary2 = service.GetTaskStatusSummary(result2.Tasks);
                Assert.Equal(1, summary2.ShippedCount);
                Assert.Equal(0, summary2.InProgressCount);
            }
            finally
            {
                if (File.Exists(testDataPath))
                    File.Delete(testDataPath);
            }
        }

        [Fact]
        public async Task Dashboard_RespondsToDataChanges()
        {
            var testDataPath = Path.Combine(Path.GetTempPath(), "responsive_test.json");
            var data = new
            {
                projectName = "Responsive Test",
                projectStartDate = "2024-01-01",
                projectEndDate = "2024-12-31",
                milestones = new object[] { },
                tasks = new object[] { }
            };

            File.WriteAllText(testDataPath, JsonSerializer.Serialize(data));

            try
            {
                var service = new ProjectDataService();
                var result = await service.LoadProjectDataAsync(testDataPath);

                Assert.NotNull(result);
                Assert.Equal("Responsive Test", result.ProjectName);
            }
            finally
            {
                if (File.Exists(testDataPath))
                    File.Delete(testDataPath);
            }
        }
    }
}