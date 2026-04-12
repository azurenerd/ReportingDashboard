#nullable enable

using System.Text.Json;
using AgentSquad.Runner.Models;
using Xunit;

namespace AgentSquad.Runner.Tests;

public class DataModelTests
{
    [Fact]
    public void DashboardConfigDeserializesFromValidJson()
    {
        var json = @"{
            ""projectName"": ""Test Project"",
            ""description"": ""Test Description"",
            ""quarters"": [],
            ""milestones"": []
        }";

        var config = JsonSerializer.Deserialize<DashboardConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(config);
        Assert.Equal("Test Project", config.ProjectName);
        Assert.Equal("Test Description", config.Description);
        Assert.Empty(config.Quarters);
        Assert.Empty(config.Milestones);
    }

    [Fact]
    public void QuarterDeserializesWithAllStatusArrays()
    {
        var json = @"{
            ""month"": ""April"",
            ""year"": 2026,
            ""shipped"": [""Item 1""],
            ""inProgress"": [""Item 2""],
            ""carryover"": [""Item 3""],
            ""blockers"": [""Item 4""]
        }";

        var quarter = JsonSerializer.Deserialize<Quarter>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(quarter);
        Assert.Equal("April", quarter.Month);
        Assert.Equal(2026, quarter.Year);
        Assert.Single(quarter.Shipped);
        Assert.Single(quarter.InProgress);
        Assert.Single(quarter.Carryover);
        Assert.Single(quarter.Blockers);
    }

    [Fact]
    public void MilestoneDeserializesWithAllFields()
    {
        var json = @"{
            ""id"": ""m1"",
            ""label"": ""PoC Milestone"",
            ""date"": ""2026-03-26"",
            ""type"": ""poc""
        }";

        var milestone = JsonSerializer.Deserialize<Milestone>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(milestone);
        Assert.Equal("m1", milestone.Id);
        Assert.Equal("PoC Milestone", milestone.Label);
        Assert.Equal("2026-03-26", milestone.Date);
        Assert.Equal("poc", milestone.Type);
    }

    [Fact]
    public void ModelsHaveDefaultEmptyCollections()
    {
        var config = new DashboardConfig();
        var quarter = new Quarter();

        Assert.NotNull(config.Quarters);
        Assert.Empty(config.Quarters);
        Assert.NotNull(config.Milestones);
        Assert.Empty(config.Milestones);

        Assert.NotNull(quarter.Shipped);
        Assert.Empty(quarter.Shipped);
        Assert.NotNull(quarter.InProgress);
        Assert.Empty(quarter.InProgress);
        Assert.NotNull(quarter.Carryover);
        Assert.Empty(quarter.Carryover);
        Assert.NotNull(quarter.Blockers);
        Assert.Empty(quarter.Blockers);
    }

    [Fact]
    public void JsonPropertyNameAttributesUsesCamelCase()
    {
        var json = @"{
            ""projectName"": ""Test"",
            ""description"": ""Desc"",
            ""inProgress"": [],
            ""quarters"": [],
            ""milestones"": []
        }";

        var config = JsonSerializer.Deserialize<DashboardConfig>(json);
        Assert.NotNull(config);
        Assert.Equal("Test", config.ProjectName);
    }
}