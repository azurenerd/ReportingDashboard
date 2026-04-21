using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Web.Components.Pages;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Web.Tests;

[Trait("Category", "Unit")]
public class DashboardRenderTests : TestContext
{
    public DashboardRenderTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private sealed class MockDashboardDataService : IDashboardDataService
    {
        private readonly DashboardLoadResult _result;
        public MockDashboardDataService(DashboardLoadResult result) { _result = result; }
        public event EventHandler? DataChanged { add { } remove { } }
        public DashboardLoadResult GetCurrent() => _result;
    }

    private static DashboardData LoadSampleData()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "sample-data.json");
        File.Exists(path).Should().BeTrue($"fixture file must be copied to output: {path}");
        var json = File.ReadAllText(path);
        var opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
        return JsonSerializer.Deserialize<DashboardData>(json, opts)!;
    }

    private void RegisterData(DashboardLoadResult result) =>
        Services.AddSingleton<IDashboardDataService>(new MockDashboardDataService(result));

    [Fact]
    public void Dashboard_Renders_HappyPath_WithoutException()
    {
        var data = LoadSampleData();
        RegisterData(new DashboardLoadResult(data, null, DateTimeOffset.UtcNow));

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".error-banner").Should().BeEmpty();
        cut.Markup.Should().Contain(data.Project.Title);
    }

    [Fact]
    public void Dashboard_Renders_ErrorBanner_OnNotFound()
    {
        var err = new DashboardLoadError("wwwroot/data.json", "missing", null, null, "NotFound");
        RegisterData(new DashboardLoadResult(null, err, DateTimeOffset.UtcNow));

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".error-banner").Should().HaveCount(1);
        cut.Markup.Should().Contain("data.json not found");
    }

    [Fact]
    public void Dashboard_Renders_ErrorBanner_OnValidationError()
    {
        var err = new DashboardLoadError("wwwroot/data.json", "bad color", null, null, "ValidationError");
        RegisterData(new DashboardLoadResult(null, err, DateTimeOffset.UtcNow));

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".error-banner").Should().HaveCount(1);
        cut.Markup.Should().Contain("data.json validation failed");
    }

    [Fact]
    public void Dashboard_Renders_ErrorBanner_OnParseError_WithLineColumn()
    {
        var err = new DashboardLoadError("wwwroot/data.json", "Unexpected token", 42, 3, "ParseError");
        RegisterData(new DashboardLoadResult(null, err, DateTimeOffset.UtcNow));

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".error-banner").Should().HaveCount(1);
        cut.Markup.Should().Contain("Failed to load data.json")
            .And.Contain("line 42")
            .And.Contain("column 3");
    }

    [Fact]
    public void Dashboard_DoesNotContainBlazorServerScript()
    {
        var data = LoadSampleData();
        RegisterData(new DashboardLoadResult(data, null, DateTimeOffset.UtcNow));

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().NotContain("blazor.server.js");
    }
}