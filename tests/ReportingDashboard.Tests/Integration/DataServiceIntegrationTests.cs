using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests verifying DashboardDataService DI registration,
/// data availability after application startup, and expected content.
/// </summary>
public class DataServiceIntegrationTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _fixture;

    public DataServiceIntegrationTests(WebAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ServiceRegistration_ResolvesAsSingleton()
    {
        using var scope1 = _fixture.Factory.Services.CreateScope();
        using var scope2 = _fixture.Factory.Services.CreateScope();

        var svc1 = scope1.ServiceProvider.GetRequiredService<DashboardDataService>();
        var svc2 = scope2.ServiceProvider.GetRequiredService<DashboardDataService>();

        svc1.Should().NotBeNull();
        svc2.Should().NotBeNull();
        svc1.Should().BeSameAs(svc2, "DashboardDataService must be registered as singleton");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void DataAvailableAfterStartup_IsNotError()
    {
        var svc = _fixture.Factory.Services.GetRequiredService<DashboardDataService>();

        svc.IsError.Should().BeFalse("data.json should load successfully at startup");
        svc.Data.Should().NotBeNull("Data should be populated after LoadAsync runs at startup");
        svc.ErrorMessage.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void DataContainsExpectedTitle()
    {
        var svc = _fixture.Factory.Services.GetRequiredService<DashboardDataService>();

        svc.Data.Should().NotBeNull();
        svc.Data!.Title.Should().Be("Privacy Automation Release Roadmap");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void DataContainsExpectedTracks()
    {
        var svc = _fixture.Factory.Services.GetRequiredService<DashboardDataService>();

        svc.Data.Should().NotBeNull();
        svc.Data!.Timeline.Tracks.Should().HaveCount(3);
        svc.Data.Timeline.Tracks[0].Name.Should().Be("M1");
        svc.Data.Timeline.Tracks[0].Color.Should().Be("#0078D4");
        svc.Data.Timeline.Tracks[1].Name.Should().Be("M2");
        svc.Data.Timeline.Tracks[1].Color.Should().Be("#00897B");
        svc.Data.Timeline.Tracks[2].Name.Should().Be("M3");
        svc.Data.Timeline.Tracks[2].Color.Should().Be("#546E7A");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void DataContainsExpectedMonths()
    {
        var svc = _fixture.Factory.Services.GetRequiredService<DashboardDataService>();

        svc.Data.Should().NotBeNull();
        svc.Data!.Months.Should().BeEquivalentTo(new[] { "Jan", "Feb", "Mar", "Apr" });
        svc.Data.CurrentMonth.Should().Be("Apr");
    }
}