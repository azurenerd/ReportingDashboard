using System.Text.Json;
using Xunit;
using ReportingDashboard.Models;

namespace ReportingDashboard.Tests;

public class DataModelDeserializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static string GetTestDataPath(string filename)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", filename);
    }

    [Fact]
    public void Deserialize_ValidMinimal_ReturnsCorrectTitle()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-minimal.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        Assert.Equal("Minimal Project", data.Title);
    }

    [Fact]
    public void Deserialize_ValidMinimal_ReturnsCorrectMonths()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-minimal.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        Assert.Equal(3, data.Months.Count);
        Assert.Equal("Jan", data.Months[0]);
        Assert.Equal("Feb", data.Months[1]);
        Assert.Equal("Mar", data.Months[2]);
    }

    [Fact]
    public void Deserialize_ValidMinimal_ReturnsOneMilestone()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-minimal.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        Assert.Single(data.Milestones);
        Assert.Equal("M1", data.Milestones[0].Id);
        Assert.Equal("#0078D4", data.Milestones[0].Color);
    }

    [Fact]
    public void Deserialize_ValidMinimal_ReturnsFourCategories()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-minimal.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        Assert.Equal(4, data.Categories.Count);
        Assert.Equal("shipped", data.Categories[0].Key);
        Assert.Equal("inProgress", data.Categories[1].Key);
        Assert.Equal("carryover", data.Categories[2].Key);
        Assert.Equal("blockers", data.Categories[3].Key);
    }

    [Fact]
    public void Deserialize_ValidFull_ReturnsThreeMilestones()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        Assert.Equal(3, data.Milestones.Count);
    }

    [Fact]
    public void Deserialize_ValidFull_MilestoneMarkersHaveCorrectTypes()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        var m1Markers = data.Milestones[0].Markers;
        Assert.Contains(m1Markers, m => m.Type == "checkpoint");
        Assert.Contains(m1Markers, m => m.Type == "poc");
        Assert.Contains(m1Markers, m => m.Type == "production");
    }

    [Fact]
    public void Deserialize_ValidFull_CategoryItemsDeserializeCorrectly()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        var shipped = data.Categories.First(c => c.Key == "shipped");
        Assert.True(shipped.Items.ContainsKey("Jan"));
        Assert.Equal(2, shipped.Items["Jan"].Count);
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
    public void Deserialize_ValidFull_CurrentMonthIndexWithinRange()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        Assert.InRange(data.CurrentMonthIndex, 0, data.Months.Count - 1);
    }

    [Fact]
    public void Deserialize_ValidFull_BacklogUrlIsPresent()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        Assert.StartsWith("https://", data.BacklogUrl);
    }

    [Fact]
    public void Deserialize_EmptyItems_ReturnsEmptyList()
    {
        var json = File.ReadAllText(GetTestDataPath("valid-minimal.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json, JsonOptions);

        Assert.NotNull(data);
        var shipped = data.Categories.First(c => c.Key == "shipped");
        Assert.True(shipped.Items.ContainsKey("Mar"));
        Assert.Empty(shipped.Items["Mar"]);
    }

    [Fact]
    public void Deserialize_InvalidJson_ThrowsJsonException()
    {
        var invalidJson = "{ this is not valid json }";
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<DashboardData>(invalidJson, JsonOptions));
    }
}