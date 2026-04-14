using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
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
    public async Task DataService_LoadAsync_WithMissingFile_SetsIsError()
    {
        var service = new DashboardDataService(NullLogger<DashboardDataService>.Instance);

        await service.LoadAsync(Path.Combine(Path.GetTempPath(), "nonexistent_data.json"));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("not found");
        service.Data.Should().BeNull();
    }

    [Fact]
    public async Task DataService_LoadAsync_WithMalformedJson_SetsIsError()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"RD_malformed_{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(tempPath, "{ not valid json }}}");
            var service = new DashboardDataService(NullLogger<DashboardDataService>.Instance);

            await service.LoadAsync(tempPath);

            service.IsError.Should().BeTrue();
            service.ErrorMessage.Should().Contain("Failed to parse");
            service.Data.Should().BeNull();
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void LoadAsync_SingletonBehavior_SameInstanceAcrossResolves()
    {
        using var scope1 = _fixture.Factory.Services.CreateScope();
        using var scope2 = _fixture.Factory.Services.CreateScope();

        var svc1 = scope1.ServiceProvider.GetRequiredService<DashboardDataService>();
        var svc2 = scope2.ServiceProvider.GetRequiredService<DashboardDataService>();

        svc1.Should().BeSameAs(svc2);
    }
}