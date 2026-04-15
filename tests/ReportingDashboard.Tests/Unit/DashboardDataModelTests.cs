using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataModelTests
{
    [Fact]
    public void DashboardData_DefaultValues_AreCorrect()
    {
        var data = new DashboardData();

        data.Title.Should().Be("");
        data.Subtitle.Should().Be("");
        data.BacklogUrl.Should().BeNull();
        data.NowDate.Should().BeNull();
        data.Timeline.Should().NotBeNull();
        data.Heatmap.Should().NotBeNull();
    }

    [Fact]
    public void Track_DefaultColor_IsBlue()
    {
        var track = new Track();

        track.Id.Should().Be("");
        track.Label.Should().Be("");
        track.Color.Should().Be("#0078D4");
        track.Milestones.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Milestone_DefaultType_IsCheckpoint()
    {
        var milestone = new Milestone();

        milestone.Date.Should().Be("");
        milestone.Type.Should().Be("checkpoint");
        milestone.Label.Should().Be("");
    }

    [Fact]
    public void HeatmapCategory_Items_DefaultsToEmptyDictionary()
    {
        var category = new HeatmapCategory();

        category.Name.Should().Be("");
        category.CssClass.Should().Be("");
        category.Items.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void TimelineConfig_Tracks_DefaultsToEmptyList()
    {
        var config = new TimelineConfig();

        config.StartMonth.Should().Be("");
        config.EndMonth.Should().Be("");
        config.Tracks.Should().NotBeNull().And.BeEmpty();
    }
}