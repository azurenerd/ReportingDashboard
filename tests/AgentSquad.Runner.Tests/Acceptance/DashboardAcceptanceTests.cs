using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Xunit;
using AgentSquad.Runner.Services;
using AgentSquad.Runner.Models;

namespace AgentSquad.Runner.Tests.Acceptance
{
    public class DashboardAcceptanceTests
    {
        private const string TestDataFile = "wwwroot/test-dashboard.json";

        [Fact]
        public async Task Dashboard_LoadsProjectData_DisplaysProjectName()
        {
            // Arrange
            var testData = @"{""name"": ""Dashboard Test"", ""milestones"": []}";
            System.IO.Directory.CreateDirectory("wwwroot");
            System.IO.File.WriteAllText(TestDataFile, testData);
            
            var cache = new MemoryCache(new MemoryCacheOptions());
            var provider = new DataProvider(cache, TestDataFile);

            // Act
            var project = await provider.LoadProjectDataAsync();

            // Assert
            Assert.Equal("Dashboard Test", project.Name);
            
            System.IO.File.Delete(TestDataFile);
        }

        [Fact]
        public async Task Dashboard_GroupsWorkItemsByStatus_AsPerRequirement()
        {
            // Arrange
            var testData = @"{
                ""name"": ""Test"",
                ""milestones"": [
                    {
                        ""id"": ""M1"",
                        ""title"": ""M1"",
                        ""status"": ""Active"",
                        ""completionPercentage"": 50,
                        ""workItems"": [
                            {""id"": ""W1"", ""title"": ""W1"", ""status"": ""InProgress"", ""completionPercentage"": 50},
                            {""id"": ""W2"", ""title"": ""W2"", ""status"": ""Completed"", ""completionPercentage"": 100}
                        ]
                    }
                ]
            }";
            
            System.IO.Directory.CreateDirectory("wwwroot");
            System.IO.File.WriteAllText(TestDataFile, testData);
            
            var cache = new MemoryCache(new MemoryCacheOptions());
            var provider = new DataProvider(cache, TestDataFile);

            // Act
            var project = await provider.LoadProjectDataAsync();
            var inProgress = project.Milestones[0].WorkItems.FindAll(w => w.Status == WorkItemStatus.InProgress);
            var completed = project.Milestones[0].WorkItems.FindAll(w => w.Status == WorkItemStatus.Completed);

            // Assert
            Assert.Single(inProgress);
            Assert.Single(completed);
            
            System.IO.File.Delete(TestDataFile);
        }
    }
}