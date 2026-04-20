using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using ReportingDashboard.Web.Models;
using Xunit;

namespace ReportingDashboard.Web.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataJsonTests
{
    private static JsonSerializerOptions Options => new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private static DashboardData Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "data.json");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<DashboardData>(json, Options)!;
    }

    [Fact]
    public void SampleDataJson_Deserializes_ToNonNullDashboardData()
    {
        var data = Load();

        data.Should().NotBeNull();
        data.Project.Should().NotBeNull();
        data.Timeline.Should().NotBeNull();
        data.Heatmap.Should().NotBeNull();
    }

    [Fact]
    public void SampleDataJson_Timeline_HasThreeLanesWithAtLeastOneMilestoneEach()
    {
        var data = Load();

        data.Timeline.Lanes.Should().HaveCount(3);
        data.Timeline.Lanes.Should().OnlyContain(l => l.Milestones.Count >= 1);
    }

    [Fact]
    public void SampleDataJson_Heatmap_HasFourRowsEachWithFourCells()
    {
        var data = Load();

        data.Heatmap.Rows.Should().HaveCount(4);
        data.Heatmap.Rows.Should().OnlyContain(r => r.Cells.Count == 4);
        data.Heatmap.Months.Should().HaveCount(4);
    }

    [Fact]
    public void SampleDataJson_Project_HasNonNullBacklogUrlAndExpectedTitle()
    {
        var data = Load();

        data.Project.BacklogUrl.Should().NotBeNullOrWhiteSpace();
        data.Project.Title.Should().Be("Privacy Automation Release Roadmap");
    }

    [Fact]
    public void SampleDataJson_Timeline_StartAndEnd_AreExpectedDateOnlyValues()
    {
        var data = Load();

        data.Timeline.Start.Should().Be(new DateOnly(2026, 1, 1));
        data.Timeline.End.Should().Be(new DateOnly(2026, 6, 30));
    }
}