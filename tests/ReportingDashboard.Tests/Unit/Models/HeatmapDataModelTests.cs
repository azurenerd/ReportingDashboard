using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class HeatmapDataModelTests
{
    [Fact]
    public void DefaultConstructor_InitializesEmptyDictionaries()
    {
        var data = new HeatmapData();

        data.Shipped.Should().NotBeNull().And.BeEmpty();
        data.InProgress.Should().NotBeNull().And.BeEmpty();
        data.Carryover.Should().NotBeNull().And.BeEmpty();
        data.Blockers.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Deserialize_WithAllCategories()
    {
        var json = """
        {
            "shipped": {
                "jan": ["Item A", "Item B"],
                "feb": ["Item C"]
            },
            "inProgress": {
                "mar": ["Item D"]
            },
            "carryover": {
                "apr": ["Item E"]
            },
            "blockers": {
                "apr": ["Blocker 1"]
            }
        }
        """;

        var data = JsonSerializer.Deserialize<HeatmapData>(json);

        data.Should().NotBeNull();
        data!.Shipped.Should().ContainKey("jan");
        data.Shipped["jan"].Should().HaveCount(2);
        data.InProgress.Should().ContainKey("mar");
        data.Carryover.Should().ContainKey("apr");
        data.Blockers.Should().ContainKey("apr");
    }

    [Fact]
    public void Deserialize_EmptyJson_UsesDefaults()
    {
        var json = "{}";

        var data = JsonSerializer.Deserialize<HeatmapData>(json);

        data.Should().NotBeNull();
        data!.Shipped.Should().NotBeNull();
        data.InProgress.Should().NotBeNull();
        data.Carryover.Should().NotBeNull();
        data.Blockers.Should().NotBeNull();
    }

    [Fact]
    public void Deserialize_MissingMonthKey_DoesNotContainKey()
    {
        var json = """
        {
            "shipped": {
                "jan": ["Item A"]
            }
        }
        """;

        var data = JsonSerializer.Deserialize<HeatmapData>(json);

        data.Should().NotBeNull();
        data!.Shipped.Should().ContainKey("jan");
        data.Shipped.Should().NotContainKey("feb");
        data.Shipped.Should().NotContainKey("may");
    }

    [Fact]
    public void Deserialize_EmptyItemsList_ReturnsEmptyList()
    {
        var json = """
        {
            "shipped": {
                "jan": []
            }
        }
        """;

        var data = JsonSerializer.Deserialize<HeatmapData>(json);

        data.Should().NotBeNull();
        data!.Shipped["jan"].Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Collections_CanBeIteratedWithoutNullReference()
    {
        var data = new HeatmapData();

        var action = () =>
        {
            foreach (var _ in data.Shipped) { }
            foreach (var _ in data.InProgress) { }
            foreach (var _ in data.Carryover) { }
            foreach (var _ in data.Blockers) { }
        };

        action.Should().NotThrow();
    }

    [Fact]
    public void Serialize_UsesCorrectJsonPropertyNames()
    {
        var data = new HeatmapData();
        data.Shipped["jan"] = new List<string> { "Test" };

        var json = JsonSerializer.Serialize(data);

        json.Should().Contain("\"shipped\"");
        json.Should().Contain("\"inProgress\"");
        json.Should().Contain("\"carryover\"");
        json.Should().Contain("\"blockers\"");
    }

    [Fact]
    public void Deserialize_MultipleItemsPerMonth()
    {
        var json = """
        {
            "shipped": {
                "jan": ["Item 1", "Item 2", "Item 3", "Item 4"]
            }
        }
        """;

        var data = JsonSerializer.Deserialize<HeatmapData>(json);

        data!.Shipped["jan"].Should().HaveCount(4);
        data.Shipped["jan"].Should().ContainInOrder("Item 1", "Item 2", "Item 3", "Item 4");
    }

    [Fact]
    public void RoundTrip_PreservesAllData()
    {
        var original = new HeatmapData();
        original.Shipped["jan"] = new List<string> { "A", "B" };
        original.InProgress["feb"] = new List<string> { "C" };
        original.Carryover["mar"] = new List<string> { "D" };
        original.Blockers["apr"] = new List<string> { "E" };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<HeatmapData>(json);

        deserialized!.Shipped["jan"].Should().BeEquivalentTo(original.Shipped["jan"]);
        deserialized.InProgress["feb"].Should().BeEquivalentTo(original.InProgress["feb"]);
        deserialized.Carryover["mar"].Should().BeEquivalentTo(original.Carryover["mar"]);
        deserialized.Blockers["apr"].Should().BeEquivalentTo(original.Blockers["apr"]);
    }
}