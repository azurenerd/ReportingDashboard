using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Services;
using ReportingDashboard.Tests.Integration.Helpers;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Integration tests verifying the DI container is wired correctly,
/// services are properly registered, and singleton lifecycle behaves as expected.
/// </summary>
[Trait("Category", "Integration")]
public class DIContainerIntegrationTests : IDisposable
{
    private readonly string _tempWebRoot;

    public DIContainerIntegrationTests()
    {
        _tempWebRoot = Path.Combine(Path.GetTempPath(), $"DICont_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempWebRoot);
        Directory.CreateDirectory(Path.Combine(_tempWebRoot, "css"));
        File.WriteAllText(Path.Combine(_tempWebRoot, "css", "dashboard.css"), "body{}");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempWebRoot))
            Directory.Delete(_tempWebRoot, recursive: true);
    }

    private WebApplicationFactory<ReportingDashboard.Components.App> CreateFactory(string? dataJson = null)
    {
        if (dataJson is not null)
            File.WriteAllText(Path.Combine(_tempWebRoot, "data.json"), dataJson);

        return new WebApplicationFactory<ReportingDashboard.Components.App>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.UseSetting("WebRootPath", _tempWebRoot);
            });
    }

    [Fact]
    public void DashboardDataService_IsRegistered()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateMinimalValidJsonString());
        using var scope = factory.Services.CreateScope();

        var svc = scope.ServiceProvider.GetService<DashboardDataService>();

        Assert.NotNull(svc);
    }

    [Fact]
    public void DashboardDataService_IsRegisteredAsSingleton()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateMinimalValidJsonString());

        DashboardDataService svc1, svc2;
        using (var scope1 = factory.Services.CreateScope())
        {
            svc1 = scope1.ServiceProvider.GetRequiredService<DashboardDataService>();
        }
        using (var scope2 = factory.Services.CreateScope())
        {
            svc2 = scope2.ServiceProvider.GetRequiredService<DashboardDataService>();
        }

        Assert.Same(svc1, svc2);
    }

    [Fact]
    public void DashboardDataService_DataLoadedAtStartup()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateValidDataJsonString());
        using var scope = factory.Services.CreateScope();

        var svc = scope.ServiceProvider.GetRequiredService<DashboardDataService>();

        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
        Assert.Equal("Integration Test Dashboard", svc.Data!.Title);
    }

    [Fact]
    public void DashboardDataService_ErrorState_WhenDataMissing()
    {
        // Don't write data.json - factory will try to load from non-existent path
        using var factory = CreateFactory();
        using var scope = factory.Services.CreateScope();

        var svc = scope.ServiceProvider.GetRequiredService<DashboardDataService>();

        Assert.True(svc.IsError);
        Assert.NotNull(svc.ErrorMessage);
        Assert.Contains("not found", svc.ErrorMessage!);
    }

    [Fact]
    public void DashboardDataService_ErrorState_WhenDataMalformed()
    {
        using var factory = CreateFactory(dataJson: "{ broken json {{{}");
        using var scope = factory.Services.CreateScope();

        var svc = scope.ServiceProvider.GetRequiredService<DashboardDataService>();

        Assert.True(svc.IsError);
        Assert.NotNull(svc.ErrorMessage);
        Assert.Contains("parse", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DashboardDataService_LoadedData_HasCorrectTimeline()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateValidDataJsonString());
        using var scope = factory.Services.CreateScope();

        var svc = scope.ServiceProvider.GetRequiredService<DashboardDataService>();

        Assert.NotNull(svc.Data?.Timeline);
        Assert.Equal(2, svc.Data!.Timeline.Tracks.Count);
        Assert.Equal("M1", svc.Data.Timeline.Tracks[0].Name);
        Assert.Equal("2026-04-10", svc.Data.Timeline.NowDate);
    }

    [Fact]
    public void DashboardDataService_LoadedData_HasCorrectHeatmap()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateValidDataJsonString());
        using var scope = factory.Services.CreateScope();

        var svc = scope.ServiceProvider.GetRequiredService<DashboardDataService>();

        Assert.NotNull(svc.Data?.Heatmap);
        Assert.True(svc.Data!.Heatmap.Shipped.Count > 0);
        Assert.True(svc.Data.Heatmap.InProgress.Count > 0);
    }

    [Fact]
    public void DashboardDataService_LoadedData_HasCorrectMonths()
    {
        using var factory = CreateFactory(dataJson: TestDataHelper.CreateValidDataJsonString());
        using var scope = factory.Services.CreateScope();

        var svc = scope.ServiceProvider.GetRequiredService<DashboardDataService>();

        Assert.Equal(4, svc.Data!.Months.Count);
        Assert.Equal("January", svc.Data.Months[0]);
        Assert.Equal("April", svc.Data.Months[3]);
    }
}