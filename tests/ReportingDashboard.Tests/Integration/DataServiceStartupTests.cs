using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Verifies DashboardDataService state after full application startup,
/// and static file serving of data.json.
/// </summary>
public class DataServiceStartupTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _fixture;

    public DataServiceStartupTests(WebAppFixture fixture)
    {
        _fixture = fixture;
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
    public void DataService_LoadedData_HasExpectedTrackCount()
    {
        var svc = _fixture.Factory.Services.GetRequiredService<DashboardDataService>();

        svc.Data.Should().NotBeNull();
        svc.Data!.Timeline.Tracks.Should().HaveCount(3);
        svc.Data.Timeline.Tracks[0].Name.Should().Be("M1");
        svc.Data.Timeline.Tracks[1].Name.Should().Be("M2");
        svc.Data.Timeline.Tracks[2].Name.Should().Be("M3");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DataJson_ServedAsStaticFile_Returns200()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync("/data.json");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        var data = JsonSerializer.Deserialize<DashboardData>(content);
        data.Should().NotBeNull();
        data!.Title.Should().Be("Privacy Automation Release Roadmap");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task DataJson_ContentType_IsApplicationJson()
    {
        var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync("/data.json");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
    }
}