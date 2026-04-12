using System.Text.Json;
using AgentSquad.Runner.Models;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class DashboardConfigTests
{
    [Fact]
    public void DashboardConfig_HasDefaultEmptyLists()
    {
        var config = new DashboardConfig();

        config.Quarters.Should().NotBeNull().And.BeEmpty();
        config.Milestones.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void DashboardConfig_CanSetAndGetProperties()
    {
        var config = new DashboardConfig
        {
            ProjectName = "Test Project",
            Description = "Test Description"
        };

        config.ProjectName.Should().Be("Test Project");
        config.Description.Should().Be("Test Description");
    }

    [Fact]
    public void DashboardConfig_CanAddQuarters()
    {
        var config = new DashboardConfig();
        var quarter = new Quarter { Month = "March", Year = 2026 };

        config.Quarters.Add(quarter);

        config.Quarters.Should().HaveCount(1);
        config.Quarters[0].Month.Should().Be("March");
    }

    [Fact]
    public void DashboardConfig_CanAddMilestones()
    {
        var config = new DashboardConfig();
        var milestone = new Milestone { Id = "m1", Label = "Milestone 1", Date = "2026-03-15", Type = "poc" };

        config.Milestones.Add(milestone);

        config.Milestones.Should().HaveCount(1);
        config.Milestones[0].Label.Should().Be("Milestone 1");
    }

    [Fact]
    public void DashboardConfig_DeserializesFromJson()
    {
        var json = """
        {
          "projectName": "My Project",
          "description": "Project Description",
          "quarters": [
            {
              "month": "March",
              "year": 2026,
              "shipped": ["Item 1"],
              "inProgress": ["Item 2"],
              "carryover": [],
              "blockers": []
            }
          ],
          "milestones": [
            {
              "id": "m1",
              "label": "Milestone 1",
              "date": "2026-03-15",
              "type": "poc"
            }
          ]
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var config = JsonSerializer.Deserialize<DashboardConfig>(json, options);

        config.Should().NotBeNull();
        config!.ProjectName.Should().Be("My Project");
        config.Description.Should().Be("Project Description");
        config.Quarters.Should().HaveCount(1);
        config.Milestones.Should().HaveCount(1);
    }
}