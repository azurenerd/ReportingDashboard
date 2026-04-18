using FluentAssertions;
using ReportingDashboard.Data;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataTests
{
    [Fact]
    public void DashboardReport_DefaultValues_AreEmpty()
    {
        var report = new DashboardReport();

        report.Header.Should().NotBeNull();
        report.TimelineTracks.Should().BeEmpty();
        report.Heatmap.Should().NotBeNull();
    }

    [Fact]
    public void HeaderInfo_DefaultValues_AreCorrect()
    {
        var header = new HeaderInfo();

        header.Title.Should().Be("");
        header.Subtitle.Should().Be("");
        header.BacklogLink.Should().Be("#");
        header.ReportDate.Should().Be("");
        header.TimelineMonths.Should().BeEmpty();
    }

    [Fact]
    public void TimelineTrack_DefaultColor_IsGray()
    {
        var track = new TimelineTrack();

        track.Color.Should().Be("#999");
        track.Milestones.Should().BeEmpty();
    }

    [Fact]
    public void Milestone_DefaultType_IsCheckpoint()
    {
        var milestone = new Milestone();

        milestone.Type.Should().Be("checkpoint");
        milestone.LabelPosition.Should().BeNull();
    }

    [Fact]
    public void HeatmapData_DefaultValues_AreEmpty()
    {
        var heatmap = new HeatmapData();

        heatmap.Columns.Should().BeEmpty();
        heatmap.HighlightColumnIndex.Should().Be(0);
        heatmap.Rows.Should().BeEmpty();
    }
}