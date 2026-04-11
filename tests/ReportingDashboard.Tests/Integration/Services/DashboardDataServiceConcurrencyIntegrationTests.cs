using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Services;

/// <summary>
/// Integration tests verifying DashboardDataService singleton behavior
/// and lifecycle within a DI container, including state sharing
/// and service resolution patterns.
/// </summary>
[Trait("Category", "Integration")]
public class DashboardDataServiceConcurrencyIntegrationTests : IDisposable
{
    private readonly string _tempDir;

    public DashboardDataServiceConcurrencyIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardConc_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private static string GetValidJson(string title = "Test") => $$"""
    {
        "title": "{{title}}",
        "subtitle": "Sub",
        "backlogLink": "https://test.com",
        "currentMonth": "Jan",
        "months": ["Jan"],
        "timeline": {
            "startDate": "2026-01-01",
            "endDate": "2026-06-30",
            "nowDate": "2026-04-10",
            "tracks": [{ "name": "T", "label": "L", "milestones": [] }]
        },
        "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
    }
    """;

    [Fact]
    public async Task Singleton_MultipleScopes_SeesSameData()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<DashboardDataService>();

        var provider = services.BuildServiceProvider();

        var service = provider.GetRequiredService<DashboardDataService>();
        var path = Path.Combine(_tempDir, "data.json");
        await File.WriteAllTextAsync(path, GetValidJson("Shared Data"));
        await service.LoadAsync(path);

        // Create multiple scopes and verify they all see the same data
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var s1 = scope1.ServiceProvider.GetRequiredService<DashboardDataService>();
        var s2 = scope2.ServiceProvider.GetRequiredService<DashboardDataService>();

        s1.Should().BeSameAs(s2);
        s1.Data!.Title.Should().Be("Shared Data");
        s2.Data!.Title.Should().Be("Shared Data");
    }

    [Fact]
    public async Task LoadAsync_MultipleSequentialCalls_LastOneWins()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<DashboardDataService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<DashboardDataService>();

        // Load first file
        var path1 = Path.Combine(_tempDir, "data1.json");
        await File.WriteAllTextAsync(path1, GetValidJson("First Load"));
        await service.LoadAsync(path1);
        service.Data!.Title.Should().Be("First Load");

        // Load second file
        var path2 = Path.Combine(_tempDir, "data2.json");
        await File.WriteAllTextAsync(path2, GetValidJson("Second Load"));
        await service.LoadAsync(path2);
        service.Data!.Title.Should().Be("Second Load");
    }

    [Fact]
    public async Task LoadAsync_SuccessThenFailure_ErrorStateCleansPreviousData()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<DashboardDataService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<DashboardDataService>();

        // Load valid data
        var path = Path.Combine(_tempDir, "ok.json");
        await File.WriteAllTextAsync(path, GetValidJson("Good Data"));
        await service.LoadAsync(path);
        service.IsError.Should().BeFalse();

        // Load from nonexistent path
        await service.LoadAsync(Path.Combine(_tempDir, "gone.json"));
        service.IsError.Should().BeTrue();
        service.Data.Should().BeNull("error should clear previous Data");
        service.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task LoadAsync_FailureThenSuccess_SuccessStateRestored()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<DashboardDataService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<DashboardDataService>();

        // Load from nonexistent → error
        await service.LoadAsync(Path.Combine(_tempDir, "missing.json"));
        service.IsError.Should().BeTrue();

        // Load valid → success
        var path = Path.Combine(_tempDir, "valid.json");
        await File.WriteAllTextAsync(path, GetValidJson("Recovered"));
        await service.LoadAsync(path);
        service.IsError.Should().BeFalse();
        service.ErrorMessage.Should().BeNull();
        service.Data!.Title.Should().Be("Recovered");
    }

    [Fact]
    public void DashboardDataService_InitialState_IsClean()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<DashboardDataService>();
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<DashboardDataService>();

        service.Data.Should().BeNull();
        service.IsError.Should().BeFalse();
        service.ErrorMessage.Should().BeNull();
    }
}