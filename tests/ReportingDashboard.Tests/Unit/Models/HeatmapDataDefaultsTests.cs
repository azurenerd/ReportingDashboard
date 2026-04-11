using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class HeatmapDataDefaultsTests
{
    [Fact]
    public void HeatmapData_DefaultConstruction_ShippedIsEmptyDictionary()
    {
        var data = new HeatmapData();
        data.Shipped.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void HeatmapData_DefaultConstruction_InProgressIsEmptyDictionary()
    {
        var data = new HeatmapData();
        data.InProgress.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void HeatmapData_DefaultConstruction_CarryoverIsEmptyDictionary()
    {
        var data = new HeatmapData();
        data.Carryover.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void HeatmapData_DefaultConstruction_BlockersIsEmptyDictionary()
    {
        var data = new HeatmapData();
        data.Blockers.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void HeatmapData_WithPopulatedData_DictionaryAccessWorks()
    {
        var data = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["jan"] = new() { "Item A", "Item B" },
                ["feb"] = new() { "Item C" }
            },
            InProgress = new Dictionary<string, List<string>>
            {
                ["mar"] = new() { "Item D" }
            }
        };

        data.Shipped["jan"].Should().HaveCount(2);
        data.Shipped["feb"].Should().HaveCount(1);
        data.InProgress["mar"].Should().ContainSingle().Which.Should().Be("Item D");
    }

    [Fact]
    public void HeatmapData_MissingKey_DoesNotExistInDictionary()
    {
        var data = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["jan"] = new() { "Item A" }
            }
        };

        data.Shipped.ContainsKey("may").Should().BeFalse();
    }

    [Fact]
    public void HeatmapData_EmptyListForMonth_ReturnsEmptyCollection()
    {
        var data = new HeatmapData
        {
            Blockers = new Dictionary<string, List<string>>
            {
                ["apr"] = new()
            }
        };

        data.Blockers["apr"].Should().BeEmpty();
    }

    [Fact]
    public void HeatmapData_AllFourCategories_CanBeIndependentlyPopulated()
    {
        var data = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>> { ["jan"] = new() { "S1" } },
            InProgress = new Dictionary<string, List<string>> { ["feb"] = new() { "I1" } },
            Carryover = new Dictionary<string, List<string>> { ["mar"] = new() { "C1" } },
            Blockers = new Dictionary<string, List<string>> { ["apr"] = new() { "B1" } }
        };

        data.Shipped.Should().HaveCount(1);
        data.InProgress.Should().HaveCount(1);
        data.Carryover.Should().HaveCount(1);
        data.Blockers.Should().HaveCount(1);
    }
}