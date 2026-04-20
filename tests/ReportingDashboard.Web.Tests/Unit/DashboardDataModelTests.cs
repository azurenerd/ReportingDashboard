using System.Text.Json;
using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Tests.Unit;

public class DashboardDataModelTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Heatmap_MaxItemsPerCell_DefaultIs4()
    {
        var h = new Heatmap
        {
            Months = new[] { "Jan" },
            Rows = Array.Empty<HeatmapRow>()
        };

        h.MaxItemsPerCell.Should().Be(4);
        h.CurrentMonthIndex.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Project_BacklogLinkText_DefaultsToAdoArrow()
    {
        var p = new Project { Title = "T", Subtitle = "S" };
        p.BacklogLinkText.Should().Be("\u2192 ADO Backlog");
        p.BacklogUrl.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DashboardData_Theme_IsOptionalAndNullByDefault()
    {
        var data = new DashboardData
        {
            Project = new Project { Title = "T", Subtitle = "S" },
            Timeline = new Timeline
            {
                Start = new DateOnly(2026, 1, 1),
                End = new DateOnly(2026, 12, 31),
                Lanes = Array.Empty<TimelineLane>()
            },
            Heatmap = new Heatmap
            {
                Months = Array.Empty<string>(),
                Rows = Array.Empty<HeatmapRow>()
            }
        };

        data.Theme.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RoundTrip_PreservesMilestoneDateTypeAndLabel()
    {
        var original = new Milestone
        {
            Date = new DateOnly(2026, 7, 15),
            Type = MilestoneType.Prod,
            Label = "Prod Cut",
            CaptionPosition = CaptionPosition.Above
        };

        var json = JsonSerializer.Serialize(original, JsonOptions.Default);
        var parsed = JsonSerializer.Deserialize<Milestone>(json, JsonOptions.Default);

        parsed.Should().NotBeNull();
        parsed!.Date.Should().Be(original.Date);
        parsed.Type.Should().Be(MilestoneType.Prod);
        parsed.Label.Should().Be("Prod Cut");
        parsed.CaptionPosition.Should().Be(CaptionPosition.Above);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Serialize_MilestoneType_EmitsCamelCaseString()
    {
        var m = new Milestone
        {
            Date = new DateOnly(2026, 1, 2),
            Type = MilestoneType.Checkpoint,
            Label = "cp"
        };

        var json = JsonSerializer.Serialize(m, JsonOptions.Default);

        json.Should().Contain("\"type\":\"checkpoint\"");
        json.Should().Contain("\"date\":\"2026-01-02\"");
    }
}