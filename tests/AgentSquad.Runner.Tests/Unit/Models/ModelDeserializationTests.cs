using System.Text.Json;
using AgentSquad.Runner.Models;
using FluentAssertions;
using Xunit;

namespace AgentSquad.Runner.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class ModelDeserializationTests
{
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public void ValidDashboardConfigJson_DeserializesWithoutErrors()
    {
        var json = """
        {
            "projectName": "Test Project",
            "description": "Test Description",
            "quarters": [],
            "milestones": []
        }
        """;

        var result = JsonSerializer.Deserialize<DashboardConfig>(json, _jsonOptions);

        result.Should().NotBeNull();
        result!.ProjectName.Should().Be("Test Project");
        result.Description.Should().Be("Test Description");
        result.Quarters.Should().BeEmpty();
        result.Milestones.Should().BeEmpty();
    }

    [Fact]
    public void ValidQuarterJson_DeserializesWithAllStatusArrays()
    {
        var json = """
        {
            "month": "March",
            "year": 2026,
            "shipped": ["Item 1"],
            "inProgress": ["Item 2"],
            "carryover": ["Item 3"],
            "blockers": ["Item 4"]
        }
        """;

        var result = JsonSerializer.Deserialize<Quarter>(json, _jsonOptions);

        result.Should().NotBeNull();
        result!.Month.Should().Be("March");
        result.Year.Should().Be(2026);
        result.Shipped.Should().ContainSingle(s => s == "Item 1");
        result.InProgress.Should().ContainSingle(s => s == "Item 2");
        result.Carryover.Should().ContainSingle(s => s == "Item 3");
        result.Blockers.Should().ContainSingle(s => s == "Item 4");
    }

    [Fact]
    public void ValidMilestoneJson_DeserializesWithAllFields()
    {
        var json = """
        {
            "id": "m1",
            "label": "First Milestone",
            "date": "2026-03-15",
            "type": "checkpoint"
        }
        """;

        var result = JsonSerializer.Deserialize<Milestone>(json, _jsonOptions);

        result.Should().NotBeNull();
        result!.Id.Should().Be("m1");
        result.Label.Should().Be("First Milestone");
        result.Date.Should().Be("2026-03-15");
        result.Type.Should().Be("checkpoint");
    }

    [Fact]
    public void MissingOptionalFields_DeserializeToEmptyLists()
    {
        var json = """
        {
            "month": "April",
            "year": 2026
        }
        """;

        var result = JsonSerializer.Deserialize<Quarter>(json, _jsonOptions);

        result.Should().NotBeNull();
        result!.Shipped.Should().BeEmpty();
        result.InProgress.Should().BeEmpty();
        result.Carryover.Should().BeEmpty();
        result.Blockers.Should().BeEmpty();
    }

    [Fact]
    public void InvalidJsonFormat_ThrowsJsonException()
    {
        var invalidJson = "{ invalid json }";

        var action = () => JsonSerializer.Deserialize<DashboardConfig>(invalidJson, _jsonOptions);

        action.Should().Throw<JsonException>();
    }

    [Fact]
    public void DashboardConfigWithCamelCaseProperties_DeserializesCorrectly()
    {
        var json = """
        {
            "projectName": "MyProject",
            "description": "MyDescription",
            "quarters": [],
            "milestones": []
        }
        """;

        var result = JsonSerializer.Deserialize<DashboardConfig>(json, _jsonOptions);

        result.Should().NotBeNull();
        result!.ProjectName.Should().Be("MyProject");
        result.Description.Should().Be("MyDescription");
    }

    [Fact]
    public void QuarterWithInProgressProperty_DeserializesCorrectly()
    {
        var json = """
        {
            "month": "May",
            "year": 2026,
            "inProgress": ["Task A", "Task B"]
        }
        """;

        var result = JsonSerializer.Deserialize<Quarter>(json, _jsonOptions);

        result.Should().NotBeNull();
        result!.InProgress.Should().HaveCount(2);
        result.InProgress.Should().Contain("Task A");
    }

    [Fact]
    public void MilestoneTypesAcceptValidStrings()
    {
        var types = new[] { "poc", "release", "checkpoint" };

        foreach (var type in types)
        {
            var json = $$"""
            {
                "id": "m1",
                "label": "Test",
                "date": "2026-03-15",
                "type": "{{type}}"
            }
            """;

            var result = JsonSerializer.Deserialize<Milestone>(json, _jsonOptions);
            result!.Type.Should().Be(type);
        }
    }

    [Fact]
    public void DashboardConfigWithManyQuarters_DeserializesList()
    {
        var json = """
        {
            "projectName": "Project",
            "description": "Desc",
            "quarters": [
                { "month": "March", "year": 2026 },
                { "month": "April", "year": 2026 },
                { "month": "May", "year": 2026 },
                { "month": "June", "year": 2026 }
            ],
            "milestones": []
        }
        """;

        var result = JsonSerializer.Deserialize<DashboardConfig>(json, _jsonOptions);

        result!.Quarters.Should().HaveCount(4);
        result.Quarters.Should().AllSatisfy(q => q.Year.Should().Be(2026));
    }

    [Fact]
    public void MilestoneJsonWithVariousDateFormats_DeserializesAsString()
    {
        var json = """
        {
            "id": "m1",
            "label": "Milestone",
            "date": "2026-03-15T14:30:00Z",
            "type": "release"
        }
        """;

        var result = JsonSerializer.Deserialize<Milestone>(json, _jsonOptions);

        result!.Date.Should().Be("2026-03-15T14:30:00Z");
    }

    [Fact]
    public void EmptyStringValuesPreservedInModels()
    {
        var json = """
        {
            "projectName": "",
            "description": "",
            "quarters": [],
            "milestones": []
        }
        """;

        var result = JsonSerializer.Deserialize<DashboardConfig>(json, _jsonOptions);

        result!.ProjectName.Should().Be("");
        result.Description.Should().Be("");
    }

    [Fact]
    public void NestedQuartersDeserializeIndependently()
    {
        var json = """
        {
            "projectName": "Project",
            "description": "Desc",
            "quarters": [
                {
                    "month": "March",
                    "year": 2026,
                    "shipped": ["A"],
                    "inProgress": ["B"],
                    "carryover": [],
                    "blockers": []
                }
            ],
            "milestones": []
        }
        """;

        var result = JsonSerializer.Deserialize<DashboardConfig>(json, _jsonOptions);

        result!.Quarters[0].Shipped.Should().Contain("A");
        result.Quarters[0].InProgress.Should().Contain("B");
    }
}