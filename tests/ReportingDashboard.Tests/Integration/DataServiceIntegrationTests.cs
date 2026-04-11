using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

public class DataServiceIntegrationTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _fixture;

    public DataServiceIntegrationTests(WebAppFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DataService_LoadDataAsync_DoesNotThrow()
    {
        using var factory = new WebApplicationFactory<Program>();
        var service = factory.Services.GetRequiredService<DashboardDataService>();

        var data = await service.LoadDataAsync();

        // With default content root (project dir), data.json may or may not exist.
        // The key assertion is that it does not throw.
        data.Should().NotBeNull();
    }

    [Fact]
    public async Task DataService_LoadDataAsync_WithMissingFile_ReturnsErrorData()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                var tempDir = Path.Combine(Path.GetTempPath(), $"RD_DSIntTest_{Guid.NewGuid():N}");
                Directory.CreateDirectory(tempDir);
                builder.UseContentRoot(tempDir);
                builder.UseSetting("Dashboard:DataFilePath", "nonexistent/data.json");
            });

        var service = factory.Services.GetRequiredService<DashboardDataService>();
        var data = await service.LoadDataAsync();

        data.ErrorMessage.Should().NotBeNullOrEmpty();
        data.ErrorMessage.Should().Contain("not found");
    }

    /// <summary>
    /// Verifies singleton registration: resolving from two different scopes yields the same instance.
    /// Note: synchronous — no async work needed, avoids CS1998.
    /// </summary>
    [Fact]
    public void LoadAsync_SingletonBehavior_SameInstanceAcrossResolves()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var scope1 = factory.Services.CreateScope();
        using var scope2 = factory.Services.CreateScope();

        var svc1 = scope1.ServiceProvider.GetRequiredService<DashboardDataService>();
        var svc2 = scope2.ServiceProvider.GetRequiredService<DashboardDataService>();

        svc1.Should().BeSameAs(svc2);
    }
}