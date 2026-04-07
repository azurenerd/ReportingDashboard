using System.Text.Json;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Tests.Models;

public class WorkItemTests
{
    [Fact]
    public void WorkItem_Deserialize_WithValidJson_ReturnsCorrectObject()
    {
        // Arrange
        var json = """
        {
            "title": "Implement auth module",
            "description": "Add JWT authentication",
            "status": "InProgress",
            "assignedTo": "John Doe"
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var workItem = JsonSerializer.Deserialize<WorkItem>(json, options);

        // Assert
        Assert.NotNull(workItem);
        Assert.Equal("Implement auth module", workItem.Title);
        Assert.Equal("Add JWT authentication", workItem.Description);
        Assert.Equal(WorkItemStatus.InProgress, workItem.Status);
        Assert.Equal("John Doe", workItem.AssignedTo);
    }

    [Theory]
    [InlineData("Shipped")]
    [InlineData("InProgress")]
    [InlineData("CarriedOver")]
    public void WorkItem_DeserializeAllStatuses_CorrectlyMapsEnumValues(string statusString)
    {
        // Arrange
        var json = $"""
        {{
            "title": "Test Work Item",
            "status": "{statusString}"
        }}
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var workItem = JsonSerializer.Deserialize<WorkItem>(json, options);

        // Assert
        var expectedStatus = Enum.Parse<WorkItemStatus>(statusString);
        Assert.Equal(expectedStatus, workItem.Status);
    }

    [Fact]
    public void WorkItem_Deserialize_WithMissingOptionalFields_ReturnsNullValues()
    {
        // Arrange
        var json = """
        {
            "title": "Minimal Work Item",
            "status": "Shipped"
        }
        """;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var workItem = JsonSerializer.Deserialize<WorkItem>(json, options);

        // Assert
        Assert.Null(workItem.Description);
        Assert.Null(workItem.AssignedTo);
    }
}