using System.Text.Json;
using Xunit;
using ReportingDashboard.Models;

namespace ReportingDashboard.Tests;

public class DataModelDeserializationTests
{
    [Fact]
    public void Deserialize_ValidMinimal_AllRequiredFieldsPopulated()
    {
        var json = File.ReadAllText(Path.Combine("TestData", "valid-minimal.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Equal("Minimal Test Project", data.Title);
        Assert.Equal("Test Subtitle", data.Subtitle);
        Assert.Single(data.Months);
        Assert.Equal(0, data.CurrentMonthIndex);
        Assert.Single(data.Milestones);
        Assert.Equal(4, data.Categories.Count);
    }

    [Fact]
    public void Deserialize_ValidFull_ComplexDataPreserved()
    {
        var json = File.ReadAllText(Path.Combine("TestData", "valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Equal("Project Phoenix Release Roadmap", data.Title);
        Assert.Equal(6, data.Months.Count);
        Assert.Equal(3, data.CurrentMonthIndex);
        Assert.Equal(3, data.Milestones.Count);
        Assert.Equal(4, data.Categories.Count);
    }

    [Fact]
    public void Deserialize_ValidFull_MilestonesHaveMarkers()
    {
        var json = File.ReadAllText(Path.Combine("TestData", "valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        var m1 = data.Milestones.First(m => m.Id == "M1");
        Assert.Equal("#0078D4", m1.Color);
        Assert.True(m1.Markers.Count >= 2);
        Assert.Contains(m1.Markers, m => m.Type == "checkpoint");
    }

    [Fact]
    public void Deserialize_ValidFull_HeatmapCategoriesHaveItems()
    {
        var json = File.ReadAllText(Path.Combine("TestData", "valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        var shipped = data.Categories.First(c => c.Key == "shipped");
        Assert.True(shipped.Items.ContainsKey("Jan"));
        Assert.NotEmpty(shipped.Items["Jan"]);
    }

    [Fact]
    public void Deserialize_HeatmapCategory_ItemsDictionaryPreserved()
    {
        var json = """
        {
            "name": "Shipped",
            "key": "shipped",
            "items": {
                "Jan": ["Item A", "Item B"],
                "Feb": ["Item C"],
                "Mar": []
            }
        }
        """;

        var category = JsonSerializer.Deserialize<HeatmapCategory>(json);

        Assert.NotNull(category);
        Assert.Equal("shipped", category.Key);
        Assert.Equal("Shipped", category.Name);
        Assert.Equal(3, category.Items.Count);
        Assert.Equal(2, category.Items["Jan"].Count);
        Assert.Equal("Item A", category.Items["Jan"][0]);
        Assert.Equal("Item B", category.Items["Jan"][1]);
        Assert.Single(category.Items["Feb"]);
        Assert.Empty(category.Items["Mar"]);
    }

    [Fact]
    public void Deserialize_HeatmapCategory_EmptyItemsDictionary()
    {
        var json = """
        {
            "name": "Blockers",
            "key": "blockers",
            "items": {}
        }
        """;

        var category = JsonSerializer.Deserialize<HeatmapCategory>(json);

        Assert.NotNull(category);
        Assert.Equal("blockers", category.Key);
        Assert.Empty(category.Items);
    }

    [Fact]
    public void Deserialize_MilestoneMarker_AllTypesValid()
    {
        var types = new[] { "checkpoint", "poc", "production", "smallCheckpoint" };
        foreach (var type in types)
        {
            var json = $$"""
            {
                "date": "2026-03-01",
                "type": "{{type}}",
                "label": "Test"
            }
            """;

            var marker = JsonSerializer.Deserialize<MilestoneMarker>(json);
            Assert.NotNull(marker);
            Assert.Equal(type, marker.Type);
        }
    }

    [Fact]
    public void Deserialize_DashboardData_DatesParsedCorrectly()
    {
        var json = File.ReadAllText(Path.Combine("TestData", "valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Equal(new DateTime(2026, 4, 14), data.CurrentDate);
        Assert.Equal(new DateTime(2026, 1, 1), data.TimelineStart);
        Assert.Equal(new DateTime(2026, 6, 30), data.TimelineEnd);
    }
}