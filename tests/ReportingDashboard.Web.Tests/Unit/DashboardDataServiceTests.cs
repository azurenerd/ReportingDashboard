using Microsoft.Extensions.DependencyInjection;

namespace ReportingDashboard.Web.Tests.Unit;

public class DashboardDataServiceTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void GetCurrent_ResolvedFromDI_ReturnsNonNullResult()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IDashboardDataService, DashboardDataService>();
        using var provider = services.BuildServiceProvider();

        var svc = provider.GetRequiredService<IDashboardDataService>();
        var result = svc.GetCurrent();

        result.Should().NotBeNull();
    }
}