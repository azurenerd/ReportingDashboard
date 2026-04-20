using FluentAssertions;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Web.Tests;

public class PlaceholderTests
{
    [Fact]
    public void Smoke_True_IsTrue()
    {
        Assert.True(true);
    }

    [Fact]
    public void DashboardLoadResult_CanBeConstructed()
    {
        var err = new DashboardLoadError("wwwroot/data.json", "msg", 1, 1, "NotFound");
        var result = new DashboardLoadResult(null, err, DateTimeOffset.UtcNow);

        result.Data.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Kind.Should().Be("NotFound");
        result.Error.Line.Should().Be(1);
        result.Error.Column.Should().Be(1);
        result.Error.FilePath.Should().Be("wwwroot/data.json");
        result.Error.Message.Should().Be("msg");
    }

    [Fact]
    public void TimelineViewModel_Empty_IsNotNull()
    {
        TimelineViewModel.Empty.Should().NotBeNull();
        TimelineViewModel.Empty.Gridlines.Should().BeEmpty();
        TimelineViewModel.Empty.Lanes.Should().BeEmpty();
        TimelineViewModel.Empty.Now.InRange.Should().BeFalse();
    }

    [Fact]
    public void HeatmapViewModel_Empty_IsNotNull()
    {
        HeatmapViewModel.Empty.Should().NotBeNull();
        HeatmapViewModel.Empty.Months.Should().BeEmpty();
        HeatmapViewModel.Empty.Rows.Should().BeEmpty();
        HeatmapViewModel.Empty.CurrentMonthIndex.Should().Be(-1);
    }

    [Fact]
    public void Stub_DashboardDataService_Returns_NotFound()
    {
        IDashboardDataService svc = new DashboardDataService();
        var result = svc.GetCurrent();

        result.Should().NotBeNull();
        result.Data.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Kind.Should().Be("NotFound");
    }

    [Fact]
    public void Stub_DashboardDataValidator_Returns_Empty()
    {
        var data = new ReportingDashboard.Web.Models.DashboardData
        {
            Project = ReportingDashboard.Web.Models.Project.Placeholder,
            Timeline = new ReportingDashboard.Web.Models.Timeline
            {
                Start = new DateOnly(2026, 1, 1),
                End = new DateOnly(2026, 6, 30),
                Lanes = Array.Empty<ReportingDashboard.Web.Models.TimelineLane>()
            },
            Heatmap = new ReportingDashboard.Web.Models.Heatmap
            {
                Months = new[] { "Jan", "Feb", "Mar", "Apr" },
                Rows = Array.Empty<ReportingDashboard.Web.Models.HeatmapRow>()
            }
        };

        DashboardDataValidator.Validate(data).Should().BeEmpty();
    }
}