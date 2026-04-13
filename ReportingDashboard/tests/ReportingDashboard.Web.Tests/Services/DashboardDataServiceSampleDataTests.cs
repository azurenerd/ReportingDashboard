using Microsoft.AspNetCore.Hosting;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests.Services;

/// <summary>
/// Validates that the actual shipped data.json file deserializes correctly
/// and meets the specification requirements for sample data volume.
/// </summary>
public class DashboardDataServiceSampleDataTests
{
    private static string GetSampleDataPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && dir.Name != "tests")
            dir = dir.Parent;

        var root = dir?.Parent
            ?? throw new InvalidOperationException("Could not find repository root.");

        return root.FullName;
    }

    private static DashboardDataService CreateServiceForSampleData()
    {
        var rootPath = GetSampleDataPath();
        var dataDir = Path.Combine(rootPath, "src", "ReportingDashboard.Web");
        var dataFile = Path.Combine(dataDir, "Data", "data.json");

        Assert.True(File.Exists(dataFile),
            $"Sample data.json not found at expected path: {dataFile}. " +
            "Ensure the file exists in the web project's Data directory.");

        var env = new TestWebHostEnvironment(dataDir);
        return new DashboardDataService(env);
    }

    [Fact]
    public async Task SampleData_DeserializesSuccessfully()
    {
        var service = CreateServiceForSampleData();
        var data = await service.GetDashboardDataAsync();

        Assert.NotNull(data);
        Assert.Equal("Project Atlas", data.Project.ProjectName);
    }

    [Fact]
    public async Task SampleData_MeetsMilestoneCountRequirement()
    {
        var service = CreateServiceForSampleData();
        var data = await service.GetDashboardDataAsync();

        // Spec requires 5-7 milestones
        Assert.InRange(data.Milestones.Count, 5, 7);
    }

    [Fact]
    public async Task SampleData_MeetsWorkItemCountRequirements()
    {
        var service = CreateServiceForSampleData();
        var data = await service.GetDashboardDataAsync();

        var shipped = data.WorkItems.Count(w => w.Category == "Shipped");
        var inProgress = data.WorkItems.Count(w => w.Category == "InProgress");
        var carriedOver = data.WorkItems.Count(w => w.Category == "CarriedOver");

        // Spec: 4-5 shipped, 3-4 in-progress, 2-3 carried-over
        Assert.InRange(shipped, 4, 5);
        Assert.InRange(inProgress, 3, 4);
        Assert.InRange(carriedOver, 2, 3);
    }

    [Fact]
    public async Task SampleData_AllWorkItemsHaveOwners()
    {
        var service = CreateServiceForSampleData();
        var data = await service.GetDashboardDataAsync();

        Assert.All(data.WorkItems, item =>
            Assert.False(string.IsNullOrWhiteSpace(item.Owner),
                $"Work item {item.Id} is missing an owner."));
    }

    [Fact]
    public async Task SampleData_AllMilestonesHaveValidStatus()
    {
        var service = CreateServiceForSampleData();
        var data = await service.GetDashboardDataAsync();

        var validStatuses = new[] { "Completed", "InProgress", "Upcoming" };
        Assert.All(data.Milestones, ms =>
            Assert.Contains(ms.Status, validStatuses));
    }

    private class TestWebHostEnvironment : IWebHostEnvironment
    {
        public TestWebHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
        }

        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = null!;
        public string ApplicationName { get; set; } = "TestApp";
        public IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Development";
    }
}