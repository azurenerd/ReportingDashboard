using System;
using System.IO;
using System.Linq;
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

    [Fact]
    public void RendersDashboard_AgainstSampleFixture_WithoutException()
    {
        var data = LoadSampleData();
        var result = new DashboardLoadResult(data, null, DateTimeOffset.UtcNow);
        Services.AddSingleton<IDashboardDataService>(new MockDashboardDataService(result));

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".error-banner").Should().BeEmpty();
    }

    [Fact]
    public void HdrH1_ContainsProjectTitle()
    {
        var data = LoadSampleData();
        Services.AddSingleton<IDashboardDataService>(
            new MockDashboardDataService(new DashboardLoadResult(data, null, DateTimeOffset.UtcNow)));

        var cut = RenderComponent<Dashboard>();

        var h1 = cut.Find(".hdr h1");
        h1.TextContent.Should().Contain("Sample Fixture Project");
    }

    [Fact]
    public void AllLaneIds_ArePresent_InTimelineLabels()
    {
        var data = LoadSampleData();
        Services.AddSingleton<IDashboardDataService>(
            new MockDashboardDataService(new DashboardLoadResult(data, null, DateTimeOffset.UtcNow)));

        var cut = RenderComponent<Dashboard>();

        var markup = cut.Markup;
        foreach (var lane in data.Timeline.Lanes)
        {
            markup.Should().Contain(lane.Id, $"lane id {lane.Id} must appear somewhere in the rendered dashboard");
        }
        markup.Should().Contain("PDS &amp; Data Inventory").And.Contain("Auto Review DFD");
    }

    [Fact]
    public void FourCategoryRowHeaders_ArePresent()
    {
        var data = LoadSampleData();
        Services.AddSingleton<IDashboardDataService>(
            new MockDashboardDataService(new DashboardLoadResult(data, null, DateTimeOffset.UtcNow)));

        var cut = RenderComponent<Dashboard>();

        var rowHeaders = cut.FindAll(".hm-row-hdr");
        rowHeaders.Should().HaveCount(4);

        var classes = string.Join(" ", rowHeaders.Select(e => e.GetAttribute("class") ?? string.Empty));
        classes.Should().Contain("ship-hdr");
        classes.Should().Contain("prog-hdr");
        classes.Should().Contain("carry-hdr");
        classes.Should().Contain("block-hdr");
    }

    [Fact]
    public void HeatmapGrid_IsRendered_WithFourMonthColumnHeaders()
    {
        var data = LoadSampleData();
        Services.AddSingleton<IDashboardDataService>(
            new MockDashboardDataService(new DashboardLoadResult(data, null, DateTimeOffset.UtcNow)));

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".hm-grid").Should().HaveCount(1);
        cut.FindAll(".hm-col-hdr").Should().HaveCount(4);
    }
}
