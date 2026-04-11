using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Services;

[Trait("Category", "Integration")]
public class DashboardDataServiceDIIntegrationTests : IDisposable
{
    private readonly string _tempDir;

    public DashboardDataServiceDIIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardDI_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void DashboardDataService_RegisteredAsSingleton_ResolvesToSameInstance()
    {
        var services = new ServiceCollection();

        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(_tempDir);
        services.AddSingleton<IWebHostEnvironment>(mockEnv.Object);
        services.AddLogging();
        services.AddSingleton<DashboardDataService>();

        var provider = services.BuildServiceProvider();

        var instance1 = provider.GetRequiredService<DashboardDataService>();
        var instance2 = provider.GetRequiredService<DashboardDataService>();

        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void DashboardDataService_CanBeResolved_FromServiceProvider()
    {
        var services = new ServiceCollection();

        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(_tempDir);
        services.AddSingleton<IWebHostEnvironment>(mockEnv.Object);
        services.AddLogging();
        services.AddSingleton<DashboardDataService>();

        var provider = services.BuildServiceProvider();

        var service = provider.GetService<DashboardDataService>();
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task DashboardDataService_Singleton_StateSharedAcrossResolutions()
    {
        var services = new ServiceCollection();

        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(_tempDir);
        services.AddSingleton<IWebHostEnvironment>(mockEnv.Object);
        services.AddLogging();
        services.AddSingleton<DashboardDataService>();

        var provider = services.BuildServiceProvider();

        // Load data through one resolution
        var service1 = provider.GetRequiredService<DashboardDataService>();
        var dataPath = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(dataPath, """{ "title": "Shared State Test" }""");
        await service1.LoadAsync(dataPath);

        // Verify through another resolution
        var service2 = provider.GetRequiredService<DashboardDataService>();
        service2.Data.Should().NotBeNull();
        service2.Data!.Title.Should().Be("Shared State Test");
        service2.IsError.Should().BeFalse();
    }

    [Fact]
    public void DashboardDataService_InitialState_BeforeLoadAsync()
    {
        var services = new ServiceCollection();

        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.Setup(e => e.WebRootPath).Returns(_tempDir);
        services.AddSingleton<IWebHostEnvironment>(mockEnv.Object);
        services.AddLogging();
        services.AddSingleton<DashboardDataService>();

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<DashboardDataService>();

        service.Data.Should().BeNull();
        service.IsError.Should().BeFalse();
        service.ErrorMessage.Should().BeNull();
    }
}