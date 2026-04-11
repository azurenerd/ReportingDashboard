using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

[Trait("Category", "Integration")]
public class StartupAndDITests
{
    [Fact]
    public void DashboardDataService_CanBeResolvedFromDI()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddProvider(NullLoggerProvider.Instance));
        services.AddSingleton<DashboardDataService>();

        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<DashboardDataService>();

        svc.Should().NotBeNull();
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

        svc1.Should().BeSameAs(svc2);
    }

    [Fact]
    public void DashboardDataService_InitialState_NoDataNoError()
    {
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddProvider(NullLoggerProvider.Instance));
        services.AddSingleton<DashboardDataService>();

        var provider = services.BuildServiceProvider();
        var svc = provider.GetRequiredService<DashboardDataService>();

        svc.Data.Should().BeNull();
        svc.IsError.Should().BeFalse();
        svc.ErrorMessage.Should().BeNull();
    }
}