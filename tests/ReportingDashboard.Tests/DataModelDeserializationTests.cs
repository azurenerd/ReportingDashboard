using System.Text.Json;
using Xunit;
using ReportingDashboard.Models;

namespace ReportingDashboard.Tests;

public class DataModelDeserializationTests
{
    private readonly string _testDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");

    [Fact]
    public void ValidMinimal_DeserializesSuccessfully()
    {
        var json = File.ReadAllText(Path.Combine(_testDataDir, "valid-minimal.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Equal("Minimal Test", data.Title);
        Assert.Equal("Test Subtitle", data.Subtitle);
        Assert.Single(data.Months);
        Assert.Equal(0, data.CurrentMonthIndex);
        Assert.Single(data.Milestones);
        Assert.Equal(4, data.Categories.Count);
    }

    [Fact]
    public void ValidFull_DeserializesAllFields()
    {
        var json = File.ReadAllText(Path.Combine(_testDataDir, "valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Equal("Full Test Dashboard", data.Title);
        Assert.Equal("https://dev.azure.com/test/project/_backlogs", data.BacklogUrl);
        Assert.Equal(new DateTime(2026, 4, 14), data.CurrentDate);
        Assert.Equal(6, data.Months.Count);
        Assert.Equal(3, data.CurrentMonthIndex);
        Assert.Equal(2, data.Milestones.Count);
        Assert.Equal(4, data.Categories.Count);
    }

    [Fact]
    public void ValidFull_MilestoneMarkersDeserialized()
    {
        var json = File.ReadAllText(Path.Combine(_testDataDir, "valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;

        var m1 = data.Milestones[0];
        Assert.Equal("M1", m1.Id);
        Assert.Equal("#0078D4", m1.Color);
        Assert.Equal(3, m1.Markers.Count);
        Assert.Equal("checkpoint", m1.Markers[0].Type);
        Assert.Equal("poc", m1.Markers[1].Type);
        Assert.Equal("production", m1.Markers[2].Type);
    }

    [Fact]
    public void ValidFull_CategoryItemsDeserialized()
    {
        var json = File.ReadAllText(Path.Combine(_testDataDir, "valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;

        var shipped = data.Categories.First(c => c.Key == "shipped");
        Assert.True(shipped.Items.ContainsKey("Jan"));
        Assert.Single(shipped.Items["Jan"]);
        Assert.Equal("Item A shipped", shipped.Items["Jan"][0]);
    }

    [Fact]
    public void EmptyCollections_InitializedProperly()
    {
        var json = """{"title":"T","subtitle":"S","backlogUrl":"https://x.com","currentDate":"2026-01-01","months":["Jan"],"currentMonthIndex":0,"timelineStart":"2026-01-01","timelineEnd":"2026-01-31","milestones":[],"categories":[]}""";
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Empty(data!.Milestones);
        Assert.Empty(data.Categories);
    }

    [Fact]
    public void DateFields_ParsedCorrectly()
    {
        var json = File.ReadAllText(Path.Combine(_testDataDir, "valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;

        Assert.Equal(new DateTime(2026, 1, 1), data.TimelineStart);
        Assert.Equal(new DateTime(2026, 6, 30), data.TimelineEnd);
        Assert.Equal(new DateTime(2026, 2, 15), data.Milestones[0].Markers[0].Date);
    }
}