using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataModelTests
{
    [Fact]
    public void DashboardData_DefaultValues_PreventNullReferences()
    {
        var data = new DashboardData();

        data.Title.Should().BeEmpty();
        data.Subtitle.Should().BeEmpty();
        data.BacklogUrl.Should().BeEmpty();
        data.CurrentDate.Should().BeEmpty();
        data.Months.Should().NotBeNull().And.BeEmpty();
        data.Tracks.Should().NotBeNull().And.BeEmpty();
        data.StatusRows.Should().NotBeNull();
    }

    [Fact]
    public void StatusRowsModel_DefaultValues_PreventNullReferences()
    {
        var statusRows = new StatusRowsModel();

        statusRows.Shipped.Should().NotBeNull().And.BeEmpty();
        statusRows.InProgress.Should().NotBeNull().And.BeEmpty();
        statusRows.Carryover.Should().NotBeNull().And.BeEmpty();
        statusRows.Blockers.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void TrackModel_DefaultValues_PreventNullReferences()
    {
        var track = new TrackModel();

        track.Id.Should().BeEmpty();
        track.Label.Should().BeEmpty();
        track.Color.Should().BeEmpty();
        track.Milestones.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void MilestoneModel_DefaultValues_PreventNullReferences()
    {
        var milestone = new MilestoneModel();

        milestone.Date.Should().BeEmpty();
        milestone.Type.Should().BeEmpty();
        milestone.Label.Should().BeEmpty();
    }
}