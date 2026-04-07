using System.Text.Json;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Tests.Models;

public class MilestoneTests
{
    [Fact]
    public void Milestone_Deserialize_WithValidJson_ReturnsCorrectObject()
    {
        // Arrange
        var json = """
        {
            "name": "Phase 1 Launch",
            "targetDate": "2024-03-31T00:00:00Z",
            "status": "Completed",
            "description": "Initial project phase"
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var milestone = JsonSerializer.Deserialize<Milestone>(json, options);

        // Assert
        Assert.NotNull(milestone);
        Assert.Equal("Phase 1 Launch", milestone.Name);
        Assert.Equal(new DateTime(2024, 3, 31, 0, 0, 0, DateTimeKind.Utc), milestone.TargetDate);
        Assert.Equal(MilestoneStatus.Completed, milestone.Status);
        Assert.Equal("Initial project phase", milestone.Description);
    }

    [Fact]
    public void Milestone_Deserialize_WithCamelCaseJson_MapsPropertiesCorrectly()
    {
        // Arrange
        var json = """
        {
            "name": "Beta Release",
            "targetDate": "2024-06-30T00:00:00Z",
            "status": "InProgress",
            "description": "Beta testing phase"
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var milestone = JsonSerializer.Deserialize<Milestone>(json, options);

        // Assert
        Assert.Equal(MilestoneStatus.InProgress, milestone.Status);
    }

    [Fact]
    public void Milestone_Deserialize_WithMissingDescription_ReturnsNullDescription()
    {
        // Arrange
        var json = """
        {
            "name": "GA Release",
            "targetDate": "2024-12-31T00:00:00Z",
            "status": "Future"
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var milestone = JsonSerializer.Deserialize<Milestone>(json, options);

        // Assert
        Assert.Null(milestone.Description);
    }

    [Theory]
    [InlineData("Completed")]
    [InlineData("InProgress")]
    [InlineData("AtRisk")]
    [InlineData("Future")]
    public void Milestone_DeserializeAllStatuses_CorrectlyMapsEnumValues(string statusString)
    {
        // Arrange
        var json = $"""
        {{
            "name": "Test Milestone",
            "targetDate": "2024-06-30T00:00:00Z",
            "status": "{statusString}"
        }}
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var milestone = JsonSerializer.Deserialize<Milestone>(json, options);

        // Assert
        Assert.NotNull(milestone);
        var expectedStatus = Enum.Parse<MilestoneStatus>(statusString);
        Assert.Equal(expectedStatus, milestone.Status);
    }
}