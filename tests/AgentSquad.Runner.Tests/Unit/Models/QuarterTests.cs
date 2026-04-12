using System.Text.Json;
using AgentSquad.Runner.Models;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class QuarterTests
{
    [Fact]
    public void Quarter_HasDefaultEmptyMonth()
    {
        var quarter = new Quarter();
        quarter.Month.Should().Be(string.Empty);
    }

    [Fact]
    public void Quarter_HasDefaultZeroYear()
    {
        var quarter = new Quarter();
        quarter.Year.Should().Be(0);
    }

    [Fact]
    public void Quarter_HasDefaultEmptyShipped()
    {
        var quarter = new Quarter();
        quarter.Shipped.Should().BeEmpty();
        quarter.Shipped.Should().NotBeNull();
    }

    [Fact]
    public void Quarter_HasDefaultEmptyInProgress()
    {
        var quarter = new Quarter();
        quarter.InProgress.Should().BeEmpty();
        quarter.InProgress.Should().NotBeNull();
    }

    [Fact]
    public void Quarter_HasDefaultEmptyCarryover()
    {
        var quarter = new Quarter();
        quarter.Carryover.Should().BeEmpty();
        quarter.Carryover.Should().NotBeNull();
    }

    [Fact]
    public void Quarter_HasDefaultEmptyBlockers()
    {
        var quarter = new Quarter();
        quarter.Blockers.Should().BeEmpty();
        quarter.Blockers.Should().NotBeNull();
    }

    [Fact]
    public void Quarter_CanBeDeserializedFromJson()
    {
        var json = """
        {
          "month": "March",
          "year": 2026,
          "shipped": ["Item 1"],
          "inProgress": ["Item 2"],
          "carryover": [],
          "blockers": []
        }
        """;

        var quarter = JsonSerializer.Deserialize<Quarter>(json);

        quarter.Should().NotBeNull();
        quarter!.Month.Should().Be("March");
        quarter.Year.Should().Be(2026);
        quarter.Shipped.Should().HaveCount(1);
    }

    [Fact]
    public void Quarter_DeserializationUsesJsonPropertyNames()
    {
        var json = """
        {
          "month": "April",
          "year": 2026,
          "shipped": [],
          "inProgress": [],
          "carryover": [],
          "blockers": []
        }
        """;

        var quarter = JsonSerializer.Deserialize<Quarter>(json);

        quarter.Should().NotBeNull();
        quarter!.Month.Should().Be("April");
    }

    [Fact]
    public void Quarter_CanDeserializeWithMultipleItems()
    {
        var json = """
        {
          "month": "May",
          "year": 2026,
          "shipped": ["A", "B", "C"],
          "inProgress": ["D", "E"],
          "carryover": ["F"],
          "blockers": ["G", "H"]
        }
        """;

        var quarter = JsonSerializer.Deserialize<Quarter>(json);

        quarter!.Shipped.Should().HaveCount(3);
        quarter.InProgress.Should().HaveCount(2);
        quarter.Carryover.Should().HaveCount(1);
        quarter.Blockers.Should().HaveCount(2);
    }

    [Fact]
    public void Quarter_CanAddItemsToShipped()
    {
        var quarter = new Quarter { Month = "March", Year = 2026 };
        quarter.Shipped.Add("New Item");

        quarter.Shipped.Should().HaveCount(1);
        quarter.Shipped[0].Should().Be("New Item");
    }

    [Fact]
    public void Quarter_CanAddItemsToInProgress()
    {
        var quarter = new Quarter();
        quarter.InProgress.Add("Task 1");
        quarter.InProgress.Add("Task 2");

        quarter.InProgress.Should().HaveCount(2);
    }

    [Fact]
    public void Quarter_HandlesNullArrays()
    {
        var json = """
        {
          "month": "March",
          "year": 2026,
          "shipped": null,
          "inProgress": null,
          "carryover": null,
          "blockers": null
        }
        """;

        var quarter = JsonSerializer.Deserialize<Quarter>(json);

        quarter!.Shipped.Should().BeNull();
        quarter.InProgress.Should().BeNull();
    }

    [Fact]
    public void Quarter_SupportsValidMonthNames()
    {
        var validMonths = new[] { "January", "February", "March", "April", "May", "June", 
                                  "July", "August", "September", "October", "November", "December" };

        foreach (var month in validMonths)
        {
            var quarter = new Quarter { Month = month, Year = 2026 };
            quarter.Month.Should().Be(month);
        }
    }

    [Fact]
    public void Quarter_SupportsYearRange()
    {
        var quarter = new Quarter { Month = "March", Year = 2050 };
        quarter.Year.Should().Be(2050);
    }
}