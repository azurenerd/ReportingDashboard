using System.Text.Json;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests;

public class DataModelDeserializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static string GetTestDataPath(string filename) =>
        Path.Combine("TestData", filename);

    [Fact]
    public void Deserialize_ValidMinimal_ReturnsPopulatedModel()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-minimal.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        Assert.Equal("Minimal Project", data.Title);
        Assert.Equal("Org · Workstream · Jan 2026", data.Subtitle);
        Assert.Equal("https://dev.azure.com/org/project", data.BacklogUrl);
        Assert.Single(data.Months);
        Assert.Equal("Jan", data.Months[0]);
        Assert.Equal(0, data.CurrentMonthIndex);
        Assert.Single(data.Milestones);
        Assert.Equal(4, data.Categories.Count);
    }

    [Fact]
    public void Deserialize_ValidFull_ReturnsAllMilestonesAndCategories()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        Assert.Equal("Project Phoenix Release Roadmap", data.Title);
        Assert.Equal(6, data.Months.Count);
        Assert.Equal(3, data.CurrentMonthIndex);
        Assert.Equal(3, data.Milestones.Count);
        Assert.Equal(4, data.Categories.Count);
    }

    [Fact]
    public void Deserialize_ValidFull_MilestoneMarkersHaveCorrectTypes()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        var m1 = data.Milestones.First(m => m.Id == "M1");
        Assert.Equal(4, m1.Markers.Count);
        Assert.Equal("checkpoint", m1.Markers[0].Type);
        Assert.Equal("poc", m1.Markers[2].Type);
        Assert.Equal("production", m1.Markers[3].Type);
    }

    [Fact]
    public void Deserialize_ValidFull_CategoryItemsAreDictionaries()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        var shipped = data.Categories.First(c => c.Key == "shipped");
        Assert.True(shipped.Items.ContainsKey("Jan"));
        Assert.Equal(2, shipped.Items["Jan"].Count);
        Assert.Contains("Auth service MVP", shipped.Items["Jan"]);
    }

    [Fact]
    public void Deserialize_ValidFull_TimelineStartBeforeEnd()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        Assert.True(data.TimelineStart < data.TimelineEnd);
    }

    [Fact]
    public void Deserialize_ValidFull_CurrentDateWithinRange()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        Assert.True(data.CurrentDate >= data.TimelineStart);
        Assert.True(data.CurrentDate <= data.TimelineEnd);
    }

    [Fact]
    public void Deserialize_MissingTitle_DefaultsToEmpty()
    {
        var json = File.ReadAllText(GetTestDataPath("invalid-missing-title.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        Assert.Equal(string.Empty, data.Title);
    }

    [Fact]
    public void Deserialize_HeaderFields_TitleSubtitleBacklogUrl()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        Assert.False(string.IsNullOrWhiteSpace(data.Title));
        Assert.False(string.IsNullOrWhiteSpace(data.Subtitle));
        Assert.False(string.IsNullOrWhiteSpace(data.BacklogUrl));
        Assert.StartsWith("https://", data.BacklogUrl);
    }

    [Fact]
    public void Deserialize_MilestoneColors_AreValidHex()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        foreach (var milestone in data.Milestones)
        {
            Assert.Matches(@"^#[0-9A-Fa-f]{6}$", milestone.Color);
        }
    }

    [Fact]
    public void Deserialize_EmptyItemsList_DeserializesToEmptyList()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        var shipped = data.Categories.First(c => c.Key == "shipped");
        Assert.NotNull(shipped.Items["Apr"]);
        Assert.Empty(shipped.Items["Apr"]);
    }
}