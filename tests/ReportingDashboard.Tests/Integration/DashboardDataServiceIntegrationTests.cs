using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Services;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class DashboardDataServiceIntegrationTests : IDisposable
{
    private readonly WebAppFactory _factory;

    public DashboardDataServiceIntegrationTests()
    {
        _factory = new WebAppFactory();
    }

    public void Dispose() => _factory.Dispose();

    private DashboardDataService ResolveService()
    {
        // Resolve the service from the real DI container
        var scope = _factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<DashboardDataService>();
    }

    [Fact]
    public void Service_IsRegisteredAsSingleton_InDIContainer()
    {
        var service1 = _factory.Services.GetRequiredService<DashboardDataService>();
        var service2 = _factory.Services.GetRequiredService<DashboardDataService>();

        service1.Should().BeSameAs(service2);
    }

    [Fact]
    public void Service_ResolvesFromDI_WithWebHostEnvironment()
    {
        var service = _factory.Services.GetService<DashboardDataService>();
        service.Should().NotBeNull();
    }

    [Fact]
    public void Load_WithValidFile_ThroughDI_ReturnsData()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString());
        var service = ResolveService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data.Should().NotBeNull();
        data!.Project.Title.Should().Be("Test Project");
    }

    [Fact]
    public void Load_WithMissingFile_ThroughDI_ReturnsFileNotFoundError()
    {
        _factory.DeleteDataJson();
        var service = ResolveService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("data.json not found at:");
    }

    [Fact]
    public void Load_WithMalformedJson_ThroughDI_ReturnsParseError()
    {
        _factory.WriteDataJson("{ broken json }");
        var service = ResolveService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("Invalid JSON in data.json:");
    }

    [Fact]
    public void Load_ReReadsFileOnEachCall_ThroughDI()
    {
        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString(title: "Version 1"));
        var service = ResolveService();

        var (data1, _) = service.Load();
        data1!.Project.Title.Should().Be("Version 1");

        _factory.WriteDataJson(TestDataHelper.BuildValidJsonString(title: "Version 2"));

        var (data2, _) = service.Load();
        data2!.Project.Title.Should().Be("Version 2");
    }

    [Fact]
    public void IWebHostEnvironment_IsAvailable_InDI()
    {
        var env = _factory.Services.GetService<IWebHostEnvironment>();
        env.Should().NotBeNull();
        env!.ContentRootPath.Should().NotBeNullOrEmpty();
    }
}