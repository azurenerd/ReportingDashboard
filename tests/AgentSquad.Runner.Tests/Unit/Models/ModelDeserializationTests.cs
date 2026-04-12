using System.Text.Json;
using AgentSquad.Runner.Models;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class ModelDeserializationTests
{
    private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = false };

    [Fact]
    public void DashboardConfig_Deserializes_WithAllRequiredFields()
    {
        var json = """
        {
          "projectName": "Test Project",
          "description": "Test Description",
          "quarters": [
            {
              "month": "March",
              "year": 2026,
              "shipped": ["Item 1"],
              "inProgress": ["Item 2"],
              "carryover": ["Item 3"],
              "blockers": ["Item 4"]
            }
          ],
          "milestones": [
            {
              "id": "m1",
              "label": "Milestone 1",
              "date": "2026-03-15",
              "type": "checkpoint"
            }
          ]
        }
        """;

        var config = JsonSerializer.Deserialize<DashboardConfig>(json, _options);

        config.Should().NotBeNull();
        config!.ProjectName.Should().Be("Test Project");
        config.Description.Should().Be("Test Description");
        config.Quarters.Should().HaveCount(1);
        config.Milestones.Should().HaveCount(1);
    }

    [Fact]
    public void Quarter_Deserializes_WithAllStatusArrays()
    {
        var json = """
        {
          "month": "April",
          "year": 2026,
          "shipped": ["Item A", "Item B"],
          "inProgress": ["Item C"],
          "carryover": [],
          "blockers": ["Item D"]
        }
        """;

        var quarter = JsonSerializer.Deserialize<Quarter>(json, _options);

        quarter.Should().NotBeNull();
        quarter!.Month.Should().Be("April");
        quarter.Year.Should().Be(2026);
        quarter.Shipped.Should().HaveCount(2);
        quarter.InProgress.Should().HaveCount(1);
        quarter.Carryover.Should().HaveCount(0);
        quarter.Blockers.Should().HaveCount(1);
    }

    [Fact]
    public void Milestone_Deserializes_WithIso8601Date()
    {
        var json = """
        {
          "id": "m-poc",
          "label": "PoC Milestone",
          "date": "2026-03-26",
          "type": "poc"
        }
        """;

        var milestone = JsonSerializer.Deserialize<Milestone>(json, _options);

        milestone.Should().NotBeNull();
        milestone!.Id.Should().Be("m-poc");
        milestone.Label.Should().Be("PoC Milestone");
        milestone.Date.Should().Be("2026-03-26");
        milestone.Type.Should().Be("poc");
    }

    [Fact]
    public void MonthInfo_InitializesWithDefaultValues()
    {
        var monthInfo = new MonthInfo
        {
            Name = "March",
            Year = 2026,
            StartDate = new DateTime(2026, 3, 1),
            EndDate = new DateTime(2026, 3, 31),
            GridColumnIndex = 0,
            IsCurrentMonth = false
        };

        monthInfo.Name.Should().Be("March");
        monthInfo.Year.Should().Be(2026);
        monthInfo.GridColumnIndex.Should().Be(0);
        monthInfo.IsCurrentMonth.Should().BeFalse();
    }

    [Fact]
    public void MilestoneShapeInfo_InitializesWithDiamondShape()
    {
        var shape = new MilestoneShapeInfo
        {
            Type = "poc",
            Shape = "diamond",
            Color = "#F4B400",
            Size = 12
        };

        shape.Type.Should().Be("poc");
        shape.Shape.Should().Be("diamond");
        shape.Color.Should().Be("#F4B400");
        shape.Size.Should().Be(12);
    }

    [Fact]
    public void DashboardConfig_WithEmptyArrays_Deserializes()
    {
        var json = """
        {
          "projectName": "Project",
          "description": "Description",
          "quarters": [],
          "milestones": []
        }
        """;

        var config = JsonSerializer.Deserialize<DashboardConfig>(json, _options);

        config.Should().NotBeNull();
        config!.Quarters.Should().BeEmpty();
        config.Milestones.Should().BeEmpty();
    }

    [Fact]
    public void Quarter_WithNullArrays_DeserializesToEmptyLists()
    {
        var json = """
        {
          "month": "May",
          "year": 2026,
          "shipped": null,
          "inProgress": null,
          "carryover": null,
          "blockers": null
        }
        """;

        var quarter = JsonSerializer.Deserialize<Quarter>(json, _options);

        quarter.Should().NotBeNull();
        quarter!.Shipped.Should().BeEmpty();
        quarter.InProgress.Should().BeEmpty();
        quarter.Carryover.Should().BeEmpty();
        quarter.Blockers.Should().BeEmpty();
    }

    [Fact]
    public void InvalidJson_ThrowsJsonException()
    {
        var invalidJson = "{ invalid json }";

        var action = () => JsonSerializer.Deserialize<DashboardConfig>(invalidJson, _options);

        action.Should().Throw<JsonException>();
    }

    [Fact]
    public void DashboardConfig_WithMissingProjectName_DeserializesToEmptyString()
    {
        var json = """
        {
          "description": "Desc",
          "quarters": [],
          "milestones": []
        }
        """;

        var config = JsonSerializer.Deserialize<DashboardConfig>(json, _options);

        config.Should().NotBeNull();
        config!.ProjectName.Should().Be(string.Empty);
    }

    [Fact]
    public void Milestone_AllTypesDeserialize()
    {
        var types = new[] { "poc", "release", "checkpoint" };

        foreach (var type in types)
        {
            var json = $$$"""
            {{
              "id": "m1",
              "label": "Test",
              "date": "2026-03-15",
              "type": "{{{type}}}"
            }}
            """;

            var milestone = JsonSerializer.Deserialize<Milestone>(json, _options);

            milestone.Should().NotBeNull();
            milestone!.Type.Should().Be(type);
        }
    }
}