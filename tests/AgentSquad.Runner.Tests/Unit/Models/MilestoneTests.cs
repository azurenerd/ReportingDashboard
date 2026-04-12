using System.Text.Json;
using AgentSquad.Runner.Models;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class MilestoneTests
{
    [Fact]
    public void Milestone_HasDefaultEmptyId()
    {
        var milestone = new Milestone();
        milestone.Id.Should().Be(string.Empty);
    }

    [Fact]
    public void Milestone_HasDefaultEmptyLabel()
    {
        var milestone = new Milestone();
        milestone.Label.Should().Be(string.Empty);
    }

    [Fact]
    public void Milestone_HasDefaultEmptyDate()
    {
        var milestone = new Milestone();
        milestone.Date.Should().Be(string.Empty);
    }

    [Fact]
    public void Milestone_HasDefaultEmptyType()
    {
        var milestone = new Milestone();
        milestone.Type.Should().Be(string.Empty);
    }

    [Fact]
    public void Milestone_CanBeDeserializedFromJson()
    {
        var json = """
        {
          "id": "m1",
          "label": "PoC Release",
          "date": "2026-03-15",
          "type": "poc"
        }
        """;

        var milestone = JsonSerializer.Deserialize<Milestone>(json);

        milestone.Should().NotBeNull();
        milestone!.Id.Should().Be("m1");
        milestone.Label.Should().Be("PoC Release");
        milestone.Date.Should().Be("2026-03-15");
        milestone.Type.Should().Be("poc");
    }

    [Fact]
    public void Milestone_DeserializationUsesJsonPropertyNames()
    {
        var json = """
        {
          "id": "m2",
          "label": "Production Release",
          "date": "2026-04-30",
          "type": "release"
        }
        """;

        var milestone = JsonSerializer.Deserialize<Milestone>(json);

        milestone!.Id.Should().Be("m2");
        milestone.Type.Should().Be("release");
    }

    [Fact]
    public void Milestone_SupportsAllValidTypes()
    {
        var validTypes = new[] { "poc", "release", "checkpoint" };

        foreach (var type in validTypes)
        {
            var milestone = new Milestone { Id = "test", Label = "Test", Date = "2026-03-15", Type = type };
            milestone.Type.Should().Be(type);
        }
    }

    [Fact]
    public void Milestone_SupportsValidIso8601Dates()
    {
        var validDates = new[] { "2026-01-01", "2026-12-31", "2025-06-15", "2027-03-20" };

        foreach (var date in validDates)
        {
            var milestone = new Milestone { Id = "test", Label = "Test", Date = date, Type = "release" };
            milestone.Date.Should().Be(date);
            DateTime.TryParse(date, out _).Should().BeTrue();
        }
    }

    [Fact]
    public void Milestone_CanDeserializeMultipleMilestones()
    {
        var json = """
        [
          {
            "id": "m1",
            "label": "PoC",
            "date": "2026-03-15",
            "type": "poc"
          },
          {
            "id": "m2",
            "label": "Prod",
            "date": "2026-04-30",
            "type": "release"
          },
          {
            "id": "m3",
            "label": "Checkpoint",
            "date": "2026-02-01",
            "type": "checkpoint"
          }
        ]
        """;

        var milestones = JsonSerializer.Deserialize<List<Milestone>>(json);

        milestones.Should().HaveCount(3);
        milestones![0].Type.Should().Be("poc");
        milestones[1].Type.Should().Be("release");
        milestones[2].Type.Should().Be("checkpoint");
    }

    [Fact]
    public void Milestone_CanSetPropertiesProgrammatically()
    {
        var milestone = new Milestone
        {
            Id = "custom-1",
            Label = "Custom Milestone",
            Date = "2026-05-15",
            Type = "checkpoint"
        };

        milestone.Id.Should().Be("custom-1");
        milestone.Label.Should().Be("Custom Milestone");
        milestone.Date.Should().Be("2026-05-15");
        milestone.Type.Should().Be("checkpoint");
    }

    [Fact]
    public void Milestone_PreservesDateFormat()
    {
        var originalDate = "2026-03-26";
        var milestone = new Milestone { Id = "m1", Label = "Test", Date = originalDate, Type = "release" };

        milestone.Date.Should().Be(originalDate);
    }

    [Fact]
    public void Milestone_HandlesCaseInsensitiveTypes()
    {
        var types = new[] { "POC", "Release", "CHECKPOINT", "Poc" };

        foreach (var type in types)
        {
            var milestone = new Milestone { Id = "test", Label = "Test", Date = "2026-03-15", Type = type };
            milestone.Type.Should().Be(type);
        }
    }
}