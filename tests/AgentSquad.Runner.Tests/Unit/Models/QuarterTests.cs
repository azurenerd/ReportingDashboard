using System.Text.Json;
using AgentSquad.Runner.Models;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class QuarterTests
{
    [Fact]
    public void Quarter_HasDefaultEmptyLists()
    {
        var quarter = new Quarter();

        quarter.Shipped.Should().NotBeNull().And.BeEmpty();
        quarter.InProgress.Should().NotBeNull().And.BeEmpty();
        quarter.Carryover.Should().NotBeNull().And.BeEmpty();
        quarter.Blockers.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Quarter_CanSetProperties()
    {
        var quarter = new Quarter
        {
            Month = "March",
            Year = 2026
        };

        quarter.Month.Should().Be("March");
        quarter.Year.Should().Be(2026);
    }

    [Fact]
    public void Quarter_CanAddShippedItems()
    {
        var quarter = new Quarter();
        quarter.Shipped.Add("Feature A");
        quarter.Shipped.Add("Feature B");

        quarter.Shipped.Should().HaveCount(2);
        quarter.Shipped.Should().Contain("Feature A");
    }

    [Fact]
    public void Quarter_CanAddInProgressItems()
    {
        var quarter = new Quarter();
        quarter.InProgress.Add("Feature C");

        quarter.InProgress.Should().HaveCount(1);
    }

    [Fact]
    public void Quarter_CanAddCarryoverItems()
    {
        var quarter = new Quarter();
        quarter.Carryover.Add("Feature D");

        quarter.Carryover.Should().HaveCount(1);
    }

    [Fact]
    public void Quarter_CanAddBlockerItems()
    {
        var quarter = new Quarter();
        quarter.Blockers.Add("Blocker 1");

        quarter.Blockers.Should().HaveCount(1);
    }

    [Fact]
    public void Quarter_DeserializesFromJson()
    {
        var json = """
        {
          "month": "March",
          "year": 2026,
          "shipped": ["Item 1", "Item 2"],
          "inProgress": ["Item 3"],
          "carryover": [],
          "blockers": ["Blocker 1"]
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var quarter = JsonSerializer.Deserialize<Quarter>(json, options);

        quarter.Should().NotBeNull();
        quarter!.Month.Should().Be("March");
        quarter.Year.Should().Be(2026);
        quarter.Shipped.Should().HaveCount(2);
        quarter.InProgress.Should().HaveCount(1);
        quarter.Blockers.Should().HaveCount(1);
    }
}