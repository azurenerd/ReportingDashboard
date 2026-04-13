using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests.Services;

[Trait("Category", "Unit")]
public class DashboardDataServiceSampleDataTests
{
    /// <summary>
    /// Resolves the path to the actual Data/data.json in the source project.
    /// Walks up from the test assembly's output directory to find the repository root,
    /// then navigates to the known location of data.json.
    /// </summary>
    private static string GetSampleDataPath()
    {
        // Start from the test assembly output directory (e.g., bin/Debug/net8.0)
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        // Walk up to find the repository structure.
        // We look for the "src" folder which sits alongside "tests" in the repo root.
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "ReportingDashboard.Web", "Data", "data.json");
            if (File.Exists(candidate))
                return candidate;

            dir = dir.Parent;
        }

        // If we can't find via src, try the ReportingDashboard subfolder pattern
        dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "ReportingDashboard", "src", "ReportingDashboard.Web", "Data", "data.json");
            if (File.Exists(candidate))
                return candidate;

            dir = dir.Parent;
        }

        return string.Empty;
    }

    [Fact]
    public void SampleDataFile_Exists()
    {
        var path = GetSampleDataPath();

        // Feedback #6: Never silently skip — fail with a clear message if data.json is not found
        Assert.True(
            !string.IsNullOrEmpty(path) && File.Exists(path),
            "Could not locate Data/data.json in the source project. " +
            "Ensure the file exists at src/ReportingDashboard.Web/Data/data.json relative to the repository root.");
    }

    [Fact]
    public async Task SampleData_DeserializesSuccessfully()
    {
        var path = GetSampleDataPath();
        Assert.True(
            !string.IsNullOrEmpty(path) && File.Exists(path),
            "Could not locate Data/data.json — cannot run deserialization test. " +
            "Ensure the file exists at src/ReportingDashboard.Web/Data/data.json.");

        var service = new DashboardDataService(path);
        var data = await service.GetDashboardDataAsync();

        Assert.NotNull(data);
        Assert.NotNull(data.Project);
        Assert.NotNull(data.Milestones);
        Assert.NotNull(data.WorkItems);
    }

    [Fact]
    public async Task SampleData_MeetsAcceptanceCriteriaCounts()
    {
        var path = GetSampleDataPath();
        Assert.True(
            !string.IsNullOrEmpty(path) && File.Exists(path),
            "Could not locate Data/data.json — cannot validate acceptance criteria counts. " +
            "Ensure the file exists at src/ReportingDashboard.Web/Data/data.json.");

        var service = new DashboardDataService(path);
        var data = await service.GetDashboardDataAsync();

        // Acceptance criteria: 5-7 milestones
        Assert.InRange(data.Milestones.Count, 5, 7);

        // Acceptance criteria: 4-5 shipped, 3-4 in-progress, 2-3 carried-over
        var shipped = data.WorkItems.Count(w => w.Category == "Shipped");
        var inProgress = data.WorkItems.Count(w => w.Category == "InProgress");
        var carriedOver = data.WorkItems.Count(w => w.Category == "CarriedOver");

        Assert.InRange(shipped, 4, 5);
        Assert.InRange(inProgress, 3, 4);
        Assert.InRange(carriedOver, 2, 3);
    }

    [Fact]
    public async Task SampleData_ProjectInfoIsPopulated()
    {
        var path = GetSampleDataPath();
        Assert.True(
            !string.IsNullOrEmpty(path) && File.Exists(path),
            "Could not locate Data/data.json — cannot validate project info. " +
            "Ensure the file exists at src/ReportingDashboard.Web/Data/data.json.");

        var service = new DashboardDataService(path);
        var data = await service.GetDashboardDataAsync();

        Assert.False(string.IsNullOrWhiteSpace(data.Project.ProjectName),
            "ProjectName should not be empty");
        Assert.False(string.IsNullOrWhiteSpace(data.Project.OverallStatus),
            "OverallStatus should not be empty");
        Assert.False(string.IsNullOrWhiteSpace(data.Project.ReportingPeriod),
            "ReportingPeriod should not be empty");
        Assert.False(string.IsNullOrWhiteSpace(data.Project.Summary),
            "Summary should not be empty");
    }
}