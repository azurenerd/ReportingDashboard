using System.Text.Json;
using AgentSquad.Runner.Models;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class DashboardConfigTests
{
    [Fact]
    public void DashboardConfig_HasDefaultEmptyProjectName()
    {
        var config = new DashboardConfig();
        config.ProjectName.Should().Be(string.Empty);
    }

    [Fact]
    public void DashboardConfig_HasDefaultEmptyDescription()
    {
        var config = new DashboardConfig();
        config.Description.Should().Be(string.Empty);
    }

    [Fact]
    public void DashboardConfig_HasDefaultEmptyQuarters()
    {
        var config = new DashboardConfig();
        config.Quarters.Should().BeEmpty();
        config.Quarters.Should().NotBeNull();
    }

    [Fact]
    public void DashboardConfig_HasDefaultEmptyMilestones()
    {
        var config = new DashboardConfig();
        config.Milestones.Should().BeEmpty();
        config.Milestones.Should().NotBeNull();
    }

    [Fact]
    public void DashboardConfig_CanBeDeserializedFromJson()
    {
        var json = """
        {
          "projectName": "Test Project",
          "description": "Test Description",
          "quarters": [],
          "milestones": []
        }
        """;

        var config = JsonSerializer.Deserialize<DashboardConfig>(json);

        config.Should().NotBeNull();
        config!.ProjectName.Should().Be("Test Project");
        config.Description.Should().Be("Test Description");
    }

    [Fact]
    public void DashboardConfig_DeserializationUsesJsonPropertyNames()
    {
        var json = """
        {
          "projectName": "My Project",
          "description": "My Description",
          "quarters": [
            {
              "month": "March",
              "year": 2026,
              "shipped": [],
              "inProgress": [],
              "carryover": [],
              "blockers": []
            }
          ],
          "milestones": []
        }
        """;

        var config = JsonSerializer.Deserialize<DashboardConfig>(json);

        config.Should().NotBeNull();
        config!.Quarters.Should().HaveCount(1);
        config.Quarters[0].Month.Should().Be("March");
    }

    [Fact]
    public void DashboardConfig_CanDeserializeWithComplexData()
    {
        var json = """
        {
          "projectName": "Privacy Automation",
          "description": "Trusted Platform - April 2026",
          "quarters": [
            {
              "month": "March",
              "year": 2026,
              "shipped": ["Item A", "Item B"],
              "inProgress": ["Item C"],
              "carryover": ["Item D"],
              "blockers": []
            },
            {
              "month": "April",
              "year": 2026,
              "shipped": [],
              "inProgress": ["Item E", "Item F"],
              "carryover": [],
              "blockers": ["Item G"]
            }
          ],
          "milestones": [
            {
              "id": "m1",
              "label": "PoC Release",
              "date": "2026-03-15",
              "type": "poc"
            }
          ]
        }
        """;

        var config = JsonSerializer.Deserialize<DashboardConfig>(json);

        config.Should().NotBeNull();
        config!.Quarters.Should().HaveCount(2);
        config.Milestones.Should().HaveCount(1);
        config.Quarters[0].Shipped.Should().HaveCount(2);
        config.Quarters[1].InProgress.Should().HaveCount(2);
    }

    [Fact]
    public void DashboardConfig_HandlesNullQuartersArray()
    {
        var json = """
        {
          "projectName": "Test",
          "description": "Test",
          "quarters": null,
          "milestones": []
        }
        """;

        var config = JsonSerializer.Deserialize<DashboardConfig>(json);

        config.Should().NotBeNull();
        config!.Quarters.Should().BeNull();
    }

    [Fact]
    public void DashboardConfig_CanSetPropertiesProgrammatically()
    {
        var config = new DashboardConfig
        {
            ProjectName = "New Project",
            Description = "New Description"
        };

        config.ProjectName.Should().Be("New Project");
        config.Description.Should().Be("New Description");
    }

    [Fact]
    public void DashboardConfig_CanAddQuartersToCollection()
    {
        var config = new DashboardConfig();
        var quarter = new Quarter { Month = "March", Year = 2026 };

        config.Quarters.Add(quarter);

        config.Quarters.Should().HaveCount(1);
        config.Quarters[0].Month.Should().Be("March");
    }

    [Fact]
    public void DashboardConfig_CanAddMilestonesToCollection()
    {
        var config = new DashboardConfig();
        var milestone = new Milestone { Id = "m1", Label = "Release", Date = "2026-03-15", Type = "release" };

        config.Milestones.Add(milestone);

        config.Milestones.Should().HaveCount(1);
        config.Milestones[0].Label.Should().Be("Release");
    }
}