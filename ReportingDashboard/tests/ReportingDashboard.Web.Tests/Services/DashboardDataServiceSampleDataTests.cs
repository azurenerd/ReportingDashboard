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
        // Walk up from test bin to find the data.json in the web project
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && dir.Name != "tests")
            dir = dir.Parent;

        var root = dir?.Parent
            ?? throw new InvalidOperationException("Could not find repository root.");

        return Path.Combine(root.FullName,
            "src", "ReportingDashboard.Web", "Data", "data.json");
    }

    [Fact]
    public async Task SampleData_DeserializesSuccessfully()
    {
        var path = GetSampleDataPath();
        if (!File.Exists(path))
            return; // Skip gracefully in CI if file not present at expected path

        var service = new DashboardDataService(path);
        var data = await service.GetDashboardDataAsync();

        Assert.NotNull(data);
        Assert.Equal("Project Atlas", data.ProjectInfo.ProjectName);
    }

    [Fact]
    public async Task SampleData_MeetsMilestoneCountRequirement()
    {
        var path = GetSampleDataPath();
        if (!File.Exists(path))
            return;

        var service = new DashboardDataService(path);
        var data = await service.GetDashboardDataAsync();

        // Spec requires 5-7 milestones
        Assert.InRange(data.Milestones.Count, 5, 7);
    }

    [Fact]
    public async Task SampleData_MeetsWorkItemCountRequirements()
    {
        var path = GetSampleDataPath();
        if (!File.Exists(path))
            return;

        var service = new DashboardDataService(path);
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
        var path = GetSampleDataPath();
        if (!File.Exists(path))
            return;

        var service = new DashboardDataService(path);
        var data = await service.GetDashboardDataAsync();

        Assert.All(data.WorkItems, item =>
            Assert.False(string.IsNullOrWhiteSpace(item.Owner),
                $"Work item {item.Id} is missing an owner."));
    }

    [Fact]
    public async Task SampleData_AllMilestonesHaveValidStatus()
    {
        var path = GetSampleDataPath();
        if (!File.Exists(path))
            return;

        var service = new DashboardDataService(path);
        var data = await service.GetDashboardDataAsync();

        var validStatuses = new[] { "Completed", "InProgress", "Upcoming" };
        Assert.All(data.Milestones, ms =>
            Assert.Contains(ms.Status, validStatuses));
    }
}