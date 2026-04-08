using Xunit;
using AgentSquad.Runner.Models;
using AgentSquad.Runner.Services;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AgentSquad.Runner.Tests.Acceptance
{
    public class DashboardAcceptanceTests
    {
        private string _testDataPath;
        private string _tempDir;

        public DashboardAcceptanceTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
            _testDataPath = Path.Combine(_tempDir, "data.json");
        }

        private void CreateWwwrootDataFile(string content)
        {
            File.WriteAllText(_testDataPath, content);
        }

        [Fact]
        public async Task Dashboard_LoadsDataFromWwwroot()
        {
            var jsonContent = """
                {
                    "milestones": [
                        {
                            "name": "v1.0",
                            "status": "InProgress",
                            "items": [
                                {"title": "Login", "status": "Shipped"},
                                {"title": "API", "status": "InProgress"}
                            ]
                        }
                    ]
                }
                """;
            CreateWwwrootDataFile(jsonContent);

            var cache = new MemoryCacheAdapter();
            var provider = new DataProvider(_testDataPath, cache);
            var project = await provider.LoadProjectDataAsync();

            Assert.NotNull(project);
            Assert.Single(project.Milestones);
        }

        [Fact]
        public async Task Dashboard_GroupsWorkItemsByStatus_AsPerRequirement()
        {
            var jsonContent = """
                {
                    "milestones": [
                        {
                            "name": "v1.0",
                            "status": "InProgress",
                            "items": [
                                {"title": "Feature 1", "status": "Shipped"},
                                {"title": "Feature 2", "status": "InProgress"},
                                {"title": "Feature 3", "status": "CarriedOver"}
                            ]
                        }
                    ]
                }
                """;
            CreateWwwrootDataFile(jsonContent);

            var cache = new MemoryCacheAdapter();
            var provider = new DataProvider(_testDataPath, cache);
            var project = await provider.LoadProjectDataAsync();

            var shipped = project.Milestones[0].Items.FindAll(i => i.Status == WorkItemStatus.Shipped);
            var inProgress = project.Milestones[0].Items.FindAll(i => i.Status == WorkItemStatus.InProgress);
            var carriedOver = project.Milestones[0].Items.FindAll(i => i.Status == WorkItemStatus.CarriedOver);

            Assert.Single(shipped);
            Assert.Single(inProgress);
            Assert.Single(carriedOver);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
    }
}