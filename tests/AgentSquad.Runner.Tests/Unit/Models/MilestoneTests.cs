using System.Text.Json;
using AgentSquad.Runner.Models;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class MilestoneTests
{
    [Fact]
    public void Milestone_CanSetProperties()
    {
        var milestone = new Milestone
        {
            Id = "m1",
            Label = "POC Release",
            Date = "2026-03-15",
            Type = "poc"
        };

        milestone.Id.Should().Be("m1");
        milestone.Label.Should().Be("POC Release");
        milestone.Date.Should().Be("2026-03-15");
        milestone.Type.Should().Be("poc");
    }

    [Fact]
    public void Milestone_DateIsIso8601Format()
    {
        var milestone = new Milestone { Date = "2026-03-15" };

        milestone.Date.Should().MatchRegex(@"^\d{4}-\d{2}-\d{2}$");
    }

    [Fact]
    public void Milestone_TypeCanBepoc()
    {
        var milestone = new Milestone { Type = "poc" };

        milestone.Type.Should().Be("poc");
    }

    [Fact]
    public void Milestone_TypeCanBerelease()
    {
        var milestone = new Milestone { Type = "release" };

        milestone.Type.Should().Be("release");
    }

    [Fact]
    public void Milestone_TypeCanBecheckpoint()
    {
        var milestone = new Milestone { Type = "checkpoint" };

        milestone.Type.Should().Be("checkpoint");
    }

    [Fact]
    public void Milestone_DeserializesFromJson()
    {
        var json = """
        {
          "id": "m1",
          "label": "POC Release",
          "date": "2026-03-15",
          "type": "poc"
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var milestone = JsonSerializer.Deserialize<Milestone>(json, options);

        milestone.Should().NotBeNull();
        milestone!.Id.Should().Be("m1");
        milestone.Label.Should().Be("POC Release");
    }
}