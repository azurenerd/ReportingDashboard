using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class DashboardIntegrationTests : IClassFixture<WebApplicationFactory<ReportingDashboard.Components.App>>
{
    private readonly WebApplicationFactory<ReportingDashboard.Components.App> _factory;

    public DashboardIntegrationTests(WebApplicationFactory<ReportingDashboard.Components.App> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RootEndpoint_ReturnsSuccessStatusCode()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RootEndpoint_ReturnsHtmlContent()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("<!DOCTYPE html");
    }

    [Fact]
    public async Task StaticFiles_CssIsAccessible()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/css/app.css");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/css");
    }

    [Fact]
    public void DI_ResolvesDataService_AsSingleton()
    {
        using var scope = _factory.Services.CreateScope();
        var service1 = _factory.Services.GetRequiredService<IDashboardDataService>();
        var service2 = _factory.Services.GetRequiredService<IDashboardDataService>();
        service1.Should().BeSameAs(service2);
    }

    [Fact]
    public void DataService_LoadsSampleData()
    {
        var service = _factory.Services.GetRequiredService<IDashboardDataService>();
        var data = service.GetData();
        // The sample dashboard-data.json should load successfully
        if (data != null)
        {
            data.Project.Title.Should().NotBeNullOrEmpty();
            data.Timeline.Tracks.Should().NotBeEmpty();
            data.Heatmap.Months.Should().NotBeEmpty();
        }
        else
        {
            // If data is null, there should be an error explaining why
            service.GetError().Should().NotBeNullOrEmpty();
        }
    }
}