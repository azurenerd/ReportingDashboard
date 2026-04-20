using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Tests;

public sealed class SmokeTests
{
    [Fact]
    public void DashboardState_Empty_IsNonNullAndSelfConsistent()
    {
        var state = DashboardState.Empty;

        state.Should().NotBeNull();
        state.Model.Should().NotBeNull();
        state.Model.Timeline.Should().NotBeNull();
        state.Model.Heatmap.Should().NotBeNull();
        state.Model.Heatmap.Months.Should().HaveCount(4);
        state.Error.Should().BeNull();
    }

    [Fact]
    public void HeatmapModel_Empty_HasAllFourCategoriesWithFourMonthCells()
    {
        var heatmap = HeatmapModel.Empty;

        foreach (var category in Enum.GetValues<HeatmapCategory>())
        {
            heatmap.Rows.Should().ContainKey(category);
            heatmap.Rows[category].Should().HaveCount(4);
            foreach (var cell in heatmap.Rows[category])
            {
                cell.Should().NotBeNull();
            }
        }
    }

    [Fact]
    public void Di_Resolves_IDashboardDataService_AsSingleton()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new TestHostEnv());
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddSingleton<IDashboardDataService, DashboardDataService>();

        using var provider = services.BuildServiceProvider();

        var a = provider.GetRequiredService<IDashboardDataService>();
        var b = provider.GetRequiredService<IDashboardDataService>();

        a.Should().NotBeNull();
        a.Should().BeSameAs(b);
        a.Current.Should().NotBeNull();
    }

    private sealed class TestHostEnv : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "ReportingDashboard.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; }
            = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}