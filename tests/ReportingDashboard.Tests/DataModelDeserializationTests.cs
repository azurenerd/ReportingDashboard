using System.Text.Json;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests;

public class DataModelDeserializationTests
{
    private static readonly string TestDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");

    [Fact]
    public void Deserialize_ValidMinimal_ReturnsPopulatedModel()
    {
        var json = File.ReadAllText(Path.Combine(TestDataDir, "valid-minimal.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Equal("Minimal Project", data.Title);
        Assert.Single(data.Months);
        Assert.Single(data.Milestones);
        Assert.Equal(4, data.Categories.Count);
    }

    [Fact]
    public void Deserialize_ValidFull_ReturnsAllFields()
    {
        var json = File.ReadAllText(Path.Combine(TestDataDir, "valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Equal("Full Project Phoenix", data.Title);
        Assert.Equal(6, data.Months.Count);
        Assert.Equal(3, data.Milestones.Count);
        Assert.Equal(4, data.Categories.Count);

        // Verify milestone markers
        var m1 = data.Milestones[0];
        Assert.Equal("M1", m1.Id);
        Assert.Equal("#0078D4", m1.Color);
        Assert.Equal(3, m1.Markers.Count);

        // Verify category items
        var shipped = data.Categories.First(c => c.Key == "shipped");
        Assert.True(shipped.Items.ContainsKey("Jan"));
        Assert.Equal(2, shipped.Items["Jan"].Count);
    }

    [Fact]
    public void Deserialize_MilestoneMarker_ParsesDateCorrectly()
    {
        var json = """{"date":"2026-03-15","type":"poc","label":"Mar 15 PoC"}""";
        var marker = JsonSerializer.Deserialize<MilestoneMarker>(json);

        Assert.NotNull(marker);
        Assert.Equal(new DateTime(2026, 3, 15), marker.Date);
        Assert.Equal("poc", marker.Type);
        Assert.Equal("Mar 15 PoC", marker.Label);
    }

    [Fact]
    public void Deserialize_HeatmapCategory_ParsesItemsDictionary()
    {
        var json = """
        {
            "name": "Shipped",
            "key": "shipped",
            "items": {
                "Jan": ["Auth v2", "SSO"],
                "Feb": ["Pipeline"]
            }
        }
        """;
        var category = JsonSerializer.Deserialize<HeatmapCategory>(json);

        Assert.NotNull(category);
        Assert.Equal("shipped", category.Key);
        Assert.Equal(2, category.Items.Count);
        Assert.Equal(2, category.Items["Jan"].Count);
        Assert.Single(category.Items["Feb"]);
    }

    [Fact]
    public void Deserialize_EmptyCategories_DefaultsToEmptyCollections()
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
        Assert.Empty(category.Items);
    }

    [Fact]
    public void Deserialize_MissingOptionalFields_UsesDefaults()
    {
        var json = "{}";
        var data = JsonSerializer.Deserialize<DashboardData>(json);

        Assert.NotNull(data);
        Assert.Equal(string.Empty, data.Title);
        Assert.Empty(data.Months);
        Assert.Empty(data.Milestones);
        Assert.Empty(data.Categories);
    }

    [Fact]
    public void Deserialize_InvalidJson_ThrowsJsonException()
    {
        var json = "{invalid json content";
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<DashboardData>(json));
    }

    [Fact]
    public void Deserialize_NullJson_ReturnsNull()
    {
        var json = "null";
        var data = JsonSerializer.Deserialize<DashboardData>(json);
        Assert.Null(data);
    }
}