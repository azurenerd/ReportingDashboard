using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

/// <summary>
/// Verifies that all model properties have correct [JsonPropertyName] attributes
/// matching the expected JSON schema keys (camelCase).
/// </summary>
[Trait("Category", "Unit")]
public class ModelPropertyAttributeTests
{
    // --- DashboardData attribute verification ---

    [Theory]
    [InlineData(nameof(DashboardData.Title), "title")]
    [InlineData(nameof(DashboardData.Subtitle), "subtitle")]
    [InlineData(nameof(DashboardData.BacklogLink), "backlogLink")]
    [InlineData(nameof(DashboardData.CurrentMonth), "currentMonth")]
    [InlineData(nameof(DashboardData.Months), "months")]
    [InlineData(nameof(DashboardData.Timeline), "timeline")]
    [InlineData(nameof(DashboardData.Heatmap), "heatmap")]
    public void DashboardData_Property_HasCorrectJsonPropertyName(string propertyName, string expectedJsonName)
    {
        var attr = typeof(DashboardData)
            .GetProperty(propertyName)!
            .GetCustomAttribute<JsonPropertyNameAttribute>();

        attr.Should().NotBeNull($"{propertyName} should have [JsonPropertyName]");
        attr!.Name.Should().Be(expectedJsonName);
    }

    [Fact]
    public void DashboardData_AllPublicProperties_HaveJsonPropertyNameAttribute()
    {
        var properties = typeof(DashboardData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            prop.GetCustomAttribute<JsonPropertyNameAttribute>()
                .Should().NotBeNull($"DashboardData.{prop.Name} must have [JsonPropertyName]");
        }
    }

    // --- TimelineData attribute verification ---

    [Theory]
    [InlineData(nameof(TimelineData.StartDate), "startDate")]
    [InlineData(nameof(TimelineData.EndDate), "endDate")]
    [InlineData(nameof(TimelineData.NowDate), "nowDate")]
    [InlineData(nameof(TimelineData.Tracks), "tracks")]
    public void TimelineData_Property_HasCorrectJsonPropertyName(string propertyName, string expectedJsonName)
    {
        var attr = typeof(TimelineData)
            .GetProperty(propertyName)!
            .GetCustomAttribute<JsonPropertyNameAttribute>();

        attr.Should().NotBeNull();
        attr!.Name.Should().Be(expectedJsonName);
    }

    [Fact]
    public void TimelineData_AllPublicProperties_HaveJsonPropertyNameAttribute()
    {
        var properties = typeof(TimelineData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            prop.GetCustomAttribute<JsonPropertyNameAttribute>()
                .Should().NotBeNull($"TimelineData.{prop.Name} must have [JsonPropertyName]");
        }
    }

    // --- TimelineTrack attribute verification ---

    [Theory]
    [InlineData(nameof(TimelineTrack.Name), "name")]
    [InlineData(nameof(TimelineTrack.Label), "label")]
    [InlineData(nameof(TimelineTrack.Color), "color")]
    [InlineData(nameof(TimelineTrack.Milestones), "milestones")]
    public void TimelineTrack_Property_HasCorrectJsonPropertyName(string propertyName, string expectedJsonName)
    {
        var attr = typeof(TimelineTrack)
            .GetProperty(propertyName)!
            .GetCustomAttribute<JsonPropertyNameAttribute>();

        attr.Should().NotBeNull();
        attr!.Name.Should().Be(expectedJsonName);
    }

    // --- Milestone attribute verification ---

    [Theory]
    [InlineData(nameof(Milestone.Date), "date")]
    [InlineData(nameof(Milestone.Type), "type")]
    [InlineData(nameof(Milestone.Label), "label")]
    public void Milestone_Property_HasCorrectJsonPropertyName(string propertyName, string expectedJsonName)
    {
        var attr = typeof(Milestone)
            .GetProperty(propertyName)!
            .GetCustomAttribute<JsonPropertyNameAttribute>();

        attr.Should().NotBeNull();
        attr!.Name.Should().Be(expectedJsonName);
    }

    // --- HeatmapData attribute verification ---

    [Theory]
    [InlineData(nameof(HeatmapData.Shipped), "shipped")]
    [InlineData(nameof(HeatmapData.InProgress), "inProgress")]
    [InlineData(nameof(HeatmapData.Carryover), "carryover")]
    [InlineData(nameof(HeatmapData.Blockers), "blockers")]
    public void HeatmapData_Property_HasCorrectJsonPropertyName(string propertyName, string expectedJsonName)
    {
        var attr = typeof(HeatmapData)
            .GetProperty(propertyName)!
            .GetCustomAttribute<JsonPropertyNameAttribute>();

        attr.Should().NotBeNull();
        attr!.Name.Should().Be(expectedJsonName);
    }

    [Fact]
    public void HeatmapData_AllPublicProperties_HaveJsonPropertyNameAttribute()
    {
        var properties = typeof(HeatmapData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            prop.GetCustomAttribute<JsonPropertyNameAttribute>()
                .Should().NotBeNull($"HeatmapData.{prop.Name} must have [JsonPropertyName]");
        }
    }

    // --- Serialization roundtrip with camelCase keys ---

    [Fact]
    public void DashboardData_Serialization_ProducesCamelCaseKeys()
    {
        var data = new DashboardData
        {
            Title = "Test",
            BacklogLink = "https://link",
            CurrentMonth = "Jan"
        };

        var json = JsonSerializer.Serialize(data);

        json.Should().Contain("\"title\":");
        json.Should().Contain("\"backlogLink\":");
        json.Should().Contain("\"currentMonth\":");
        json.Should().NotContain("\"Title\":");
        json.Should().NotContain("\"BacklogLink\":");
        json.Should().NotContain("\"CurrentMonth\":");
    }

    [Fact]
    public void HeatmapData_Serialization_ProducesCamelCaseKeys()
    {
        var data = new HeatmapData
        {
            InProgress = new Dictionary<string, List<string>> { ["jan"] = new() { "X" } }
        };

        var json = JsonSerializer.Serialize(data);

        json.Should().Contain("\"inProgress\":");
        json.Should().NotContain("\"InProgress\":");
    }

    [Fact]
    public void TimelineData_Serialization_ProducesCamelCaseKeys()
    {
        var data = new TimelineData
        {
            StartDate = "2026-01-01",
            EndDate = "2026-06-30",
            NowDate = "2026-04-10"
        };

        var json = JsonSerializer.Serialize(data);

        json.Should().Contain("\"startDate\":");
        json.Should().Contain("\"endDate\":");
        json.Should().Contain("\"nowDate\":");
    }
}