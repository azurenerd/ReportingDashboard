using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Services;

[Trait("Category", "Integration")]
public class DashboardDataServiceDIIntegrationTests
{
    [Fact]
    public void DashboardDataService_CanBeResolvedFromDI()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<DashboardDataService>();

        var provider = services.BuildServiceProvider();

        var service = provider.GetService<DashboardDataService>();

        service.Should().NotBeNull();
    }

    [Fact]
    public void DashboardDataService_RegisteredAsSingleton_ReturnsSameInstance()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<DashboardDataService>();

        var provider = services.BuildServiceProvider();

        var first = provider.GetRequiredService<DashboardDataService>();
        var second = provider.GetRequiredService<DashboardDataService>();

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void DashboardDataService_ReceivesLogger_FromDI()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<DashboardDataService>();

        var provider = services.BuildServiceProvider();

        var action = () => provider.GetRequiredService<DashboardDataService>();

        action.Should().NotThrow();
    }

    [Fact]
    public void DashboardDataService_InitialState_IsNotError()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<DashboardDataService>();

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<DashboardDataService>();

        service.IsError.Should().BeFalse();
        service.Data.Should().BeNull();
        service.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task DashboardDataService_LoadAsync_WorksAfterDIResolution()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<DashboardDataService>();

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<DashboardDataService>();

        var tempDir = Path.Combine(Path.GetTempPath(), $"DI_Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var json = """
            {
                "title": "DI Test",
                "subtitle": "Sub",
                "backlogLink": "https://test.com",
                "currentMonth": "Apr",
                "months": ["Jan"],
                "timeline": {
                    "startDate": "2026-01-01",
                    "endDate": "2026-06-30",
                    "nowDate": "2026-04-10",
                    "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
                }
            }
            """;
            var filePath = Path.Combine(tempDir, "data.json");
            await File.WriteAllTextAsync(filePath, json);

            await service.LoadAsync(filePath);

            service.IsError.Should().BeFalse();
            service.Data.Should().NotBeNull();
            service.Data!.Title.Should().Be("DI Test");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task DashboardDataService_SharedState_AcrossMultipleConsumers()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<DashboardDataService>();

        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<DashboardDataService>();

        var tempDir = Path.Combine(Path.GetTempPath(), $"SharedState_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var json = """
            {
                "title": "Shared State Test",
                "subtitle": "Sub",
                "backlogLink": "https://test.com",
                "currentMonth": "Apr",
                "months": ["Jan"],
                "timeline": {
                    "startDate": "2026-01-01",
                    "endDate": "2026-06-30",
                    "nowDate": "2026-04-10",
                    "tracks": [{ "name": "M1", "label": "T", "color": "#000", "milestones": [] }]
                }
            }
            """;
            var filePath = Path.Combine(tempDir, "data.json");
            await File.WriteAllTextAsync(filePath, json);

            await service.LoadAsync(filePath);

            // Second consumer resolves the same singleton and sees loaded data
            var consumer2 = provider.GetRequiredService<DashboardDataService>();
            consumer2.Data.Should().NotBeNull();
            consumer2.Data!.Title.Should().Be("Shared State Test");
            consumer2.IsError.Should().BeFalse();
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}