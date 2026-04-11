using System.Reflection;
using System.Text.Json.Serialization;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Models;

[Trait("Category", "Unit")]
public class ModelPropertyAttributeTests
{
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

        attr.Should().NotBeNull();
        attr!.Name.Should().Be(expectedJsonName);
    }

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

    [Theory]
    [InlineData(nameof(TimelineTrack.Id), "id")]
    [InlineData(nameof(TimelineTrack.Name), "name")]
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

    [Theory]
    [InlineData(nameof(MilestoneMarker.Date), "date")]
    [InlineData(nameof(MilestoneMarker.Label), "label")]
    [InlineData(nameof(MilestoneMarker.Type), "type")]
    public void MilestoneMarker_Property_HasCorrectJsonPropertyName(string propertyName, string expectedJsonName)
    {
        var attr = typeof(MilestoneMarker)
            .GetProperty(propertyName)!
            .GetCustomAttribute<JsonPropertyNameAttribute>();

        attr.Should().NotBeNull();
        attr!.Name.Should().Be(expectedJsonName);
    }

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
}