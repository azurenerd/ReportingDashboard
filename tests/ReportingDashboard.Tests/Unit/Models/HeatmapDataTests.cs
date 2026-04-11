using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class HeatmapDataTests
{
    [Fact]
    public void HeatmapData_DefaultConstructor_PropertiesAreNotNull()
    {
        var data = new HeatmapData();

        Assert.NotNull(data.Shipped);
        Assert.NotNull(data.InProgress);
        Assert.NotNull(data.Carryover);
        Assert.NotNull(data.Blockers);
    }

    [Fact]
    public void HeatmapData_CanSetShipped()
    {
        var data = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["jan"] = new() { "Feature A" }
            }
        };

        Assert.Single(data.Shipped);
        Assert.Contains("Feature A", data.Shipped["jan"]);
    }

    [Fact]
    public void HeatmapData_CanSetInProgress()
    {
        var data = new HeatmapData
        {
            InProgress = new Dictionary<string, List<string>>
            {
                ["feb"] = new() { "Task 1", "Task 2" }
            }
        };

        Assert.Equal(2, data.InProgress["feb"].Count);
    }

    [Fact]
    public void HeatmapData_CanSetCarryover()
    {
        var data = new HeatmapData
        {
            Carryover = new Dictionary<string, List<string>>
            {
                ["mar"] = new() { "Legacy Item" }
            }
        };

        Assert.Single(data.Carryover["mar"]);
    }

    [Fact]
    public void HeatmapData_CanSetBlockers()
    {
        var data = new HeatmapData
        {
            Blockers = new Dictionary<string, List<string>>
            {
                ["apr"] = new() { "Blocker 1" }
            }
        };

        Assert.Contains("Blocker 1", data.Blockers["apr"]);
    }

    [Fact]
    public void HeatmapData_MultipleMonthKeys_AllAccessible()
    {
        var data = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["jan"] = new() { "A" },
                ["feb"] = new() { "B" },
                ["mar"] = new() { "C" },
                ["apr"] = new() { "D" }
            },
            InProgress = new(),
            Carryover = new(),
            Blockers = new()
        };

        Assert.Equal(4, data.Shipped.Count);
    }

    [Fact]
    public void HeatmapData_EmptyListValue_IsValidEntry()
    {
        var data = new HeatmapData
        {
            Shipped = new Dictionary<string, List<string>>
            {
                ["jan"] = new()
            },
            InProgress = new(),
            Carryover = new(),
            Blockers = new()
        };

        Assert.Empty(data.Shipped["jan"]);
    }
}