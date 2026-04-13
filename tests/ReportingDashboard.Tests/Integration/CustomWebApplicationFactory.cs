using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ReportingDashboard.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<ReportingDashboard.Components.App>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }
}