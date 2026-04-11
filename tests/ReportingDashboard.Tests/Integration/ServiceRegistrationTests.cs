using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Tests that DashboardDataService can be properly constructed and resolved
/// from a DI container matching the app's registration pattern.
/// </summary>
[Trait("Category", "Integration")]
public class ServiceRegistrationTests
{
    [Fact]
    public void DashboardDataService_CanBeResolvedFromDI()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddProvider(NullLoggerProvider.Instance));
        services.AddSingleton<DashboardDataService>();

        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<DashboardDataService>();

        Assert.NotNull(svc);
    }

    [Fact]
    public void DashboardDataService_RegisteredAsSingleton_ReturnsSameInstance()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddProvider(NullLoggerProvider.Instance));
        services.AddSingleton<DashboardDataService>();

        var provider = services.BuildServiceProvider();
        var svc1 = provider.GetRequiredService<DashboardDataService>();
        var svc2 = provider.GetRequiredService<DashboardDataService>();

        Assert.Same(svc1, svc2);
    }

    [Fact]
    public void DashboardDataService_InitialState_NoDataNoError()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddProvider(NullLoggerProvider.Instance));
        services.AddSingleton<DashboardDataService>();

        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<DashboardDataService>();

        Assert.Null(svc.Data);
        Assert.False(svc.IsError);
        Assert.Null(svc.ErrorMessage);
    }

    [Fact]
    public async Task DashboardDataService_AfterLoad_StateAvailableFromDI()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"SvcReg_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var path = Path.Combine(tempDir, "data.json");
            File.WriteAllText(path, Helpers.TestDataHelper.CreateValidDataJsonString());

            var services = new ServiceCollection();
            services.AddLogging(b => b.AddProvider(NullLoggerProvider.Instance));
            services.AddSingleton<DashboardDataService>();

            var provider = services.BuildServiceProvider();
            var svc = provider.GetRequiredService<DashboardDataService>();

            await svc.LoadAsync(path);

            // Resolve again — singleton should have loaded data
            var svc2 = provider.GetRequiredService<DashboardDataService>();
            Assert.Same(svc, svc2);
            Assert.False(svc2.IsError);
            Assert.NotNull(svc2.Data);
            Assert.Equal("Integration Test Dashboard", svc2.Data!.Title);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}