using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class WebAppIntegrationTests
{
    [Fact]
    public void DashboardDataService_CanBeConstructed_WithWebHostEnvironment()
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

        var service = new DashboardDataService(mockEnv.Object);

        service.Should().NotBeNull();
    }

    [Fact]
    public void DashboardDataService_LoadDashboard_ReturnsErrorWhenNoDataFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"IntTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);

            var service = new DashboardDataService(mockEnv.Object);
            var (data, error) = service.LoadDashboard();

            data.Should().BeNull();
            error.Should().NotBeNullOrEmpty();
            error.Should().Contain("data.json not found");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ServiceCollection_CanRegisterDashboardDataServiceAsSingleton()
    {
        var services = new ServiceCollection();
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

        services.AddSingleton<IWebHostEnvironment>(mockEnv.Object);
        services.AddSingleton<DashboardDataService>();

        using var provider = services.BuildServiceProvider();
        var service1 = provider.GetService<DashboardDataService>();
        var service2 = provider.GetService<DashboardDataService>();

        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        service1.Should().BeSameAs(service2);
    }

    [Fact]
    public void ServiceCollection_ResolvesTransientScopes_WithSameSingleton()
    {
        var services = new ServiceCollection();
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

        services.AddSingleton<IWebHostEnvironment>(mockEnv.Object);
        services.AddSingleton<DashboardDataService>();

        using var provider = services.BuildServiceProvider();
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var svc1 = scope1.ServiceProvider.GetRequiredService<DashboardDataService>();
        var svc2 = scope2.ServiceProvider.GetRequiredService<DashboardDataService>();

        svc1.Should().BeSameAs(svc2);
    }

    [Fact]
    public void DashboardDataService_LoadDashboard_ValidJson_ReturnsData()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"IntTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var json = """
            {
                "title": "Integration Test",
                "subtitle": "Test Suite",
                "milestones": [],
                "heatmap": { "months": ["Jan"], "currentMonthIndex": 0, "rows": [] }
            }
            """;
            File.WriteAllText(Path.Combine(tempDir, "data.json"), json);

            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);

            var service = new DashboardDataService(mockEnv.Object);
            var (data, error) = service.LoadDashboard();

            error.Should().BeNull();
            data.Should().NotBeNull();
            data!.Title.Should().Be("Integration Test");
            data.Heatmap.Months.Should().HaveCount(1);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}