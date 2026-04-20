using FluentAssertions;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests
{
    [Fact]
    public void Current_AfterConstruction_IsNotNullAndHasEmptyModel()
    {
        using var svc = new DashboardDataService();

        svc.Current.Should().NotBeNull();
        svc.Current.Model.Should().NotBeNull();
        svc.Current.Error.Should().BeNull();
    }

    [Fact]
    public void Current_AfterConstruction_LoadedAtUtcIsRecent()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        using var svc = new DashboardDataService();
        var after = DateTime.UtcNow.AddSeconds(1);

        svc.Current.LoadedAtUtc.Should().BeOnOrAfter(before);
        svc.Current.LoadedAtUtc.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Reload_RaisesOnChangedExactlyOnce()
    {
        using var svc = new DashboardDataService();
        int count = 0;
        svc.OnChanged += () => count++;

        svc.Reload();

        count.Should().Be(1);
    }

    [Fact]
    public void Reload_UpdatesLoadedAtUtc()
    {
        using var svc = new DashboardDataService();
        var original = svc.Current.LoadedAtUtc;
        Thread.Sleep(5);

        svc.Reload();

        svc.Current.LoadedAtUtc.Should().BeOnOrAfter(original);
        svc.Current.Error.Should().BeNull();
    }

    [Fact]
    public void ParseError_Record_PropertiesRoundTrip()
    {
        var err = new ParseError(42, 7, "msg", "$.path");

        err.Line.Should().Be(42);
        err.Column.Should().Be(7);
        err.Message.Should().Be("msg");
        err.JsonPath.Should().Be("$.path");
    }
}