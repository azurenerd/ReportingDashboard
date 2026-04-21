using System;
using System.Collections.Generic;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ReportingDashboard.Web.Components.Pages;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Web.Tests;

public class DashboardRenderTests
{
    private sealed class StubService : IDashboardDataService
    {
        private readonly DashboardLoadResult _result;
        public StubService(DashboardLoadResult r) { _result = r; }
        public DashboardLoadResult GetCurrent() => _result;
        public event EventHandler? DataChanged { add { } remove { } }
    }

    private static DashboardData SampleData()
    {
        return new DashboardData
        {
            Project = new Project { Title = "Sample Project", Subtitle = "Org \u2022 WS \u2022 Apr 2026", BacklogUrl = "https://example.com/backlog" },
            Timeline = new Timeline
            {
                Start = new DateOnly(2026, 1, 1),
                End = new DateOnly(2026, 6, 30),
                Lanes = new List<TimelineLane>
                {
                    new() { Id = "M1", Label = "Lane One", Color = "#0078D4", Milestones = new List<Milestone>
                    {
                        new() { Date = new DateOnly(2026, 3, 26), Type = MilestoneType.Poc, Label = "Mar 26 PoC" }
                    }},
                    new() { Id = "M2", Label = "Lane Two", Color = "#00897B", Milestones = new List<Milestone>() },
                    new() { Id = "M3", Label = "Lane Three", Color = "#546E7A", Milestones = new List<Milestone>() }
                }
            },
            Heatmap = new Heatmap
            {
                Months = new List<string> { "Jan", "Feb", "Mar", "Apr" },
                CurrentMonthIndex = 3,
                MaxItemsPerCell = 4,
                Rows = new List<HeatmapRow>
                {
                    new() { Category = HeatmapCategory.Shipped,    Cells = new List<IReadOnlyList<string>> { new List<string>{"A"}, new List<string>(), new List<string>(), new List<string>{"B"} } },
                    new() { Category = HeatmapCategory.InProgress, Cells = new List<IReadOnlyList<string>> { new List<string>(), new List<string>(), new List<string>{"X"}, new List<string>{"Y"} } },
                    new() { Category = HeatmapCategory.Carryover,  Cells = new List<IReadOnlyList<string>> { new List<string>(), new List<string>(), new List<string>(), new List<string>{"L"} } },
                    new() { Category = HeatmapCategory.Blockers,   Cells = new List<IReadOnlyList<string>> { new List<string>(), new List<string>(), new List<string>(), new List<string>{"V"} } }
                }
            }
        };
    }

    [Fact]
    public void Dashboard_Renders_HappyPath_WithoutException()
    {
        using var ctx = new TestContext();
        var data = SampleData();
        var result = new DashboardLoadResult(data, null, DateTimeOffset.UtcNow);
        ctx.Services.AddSingleton<IDashboardDataService>(new StubService(result));

        var cut = ctx.Render<Dashboard>(parameters => { });

        var markup = cut.Markup;
        markup.Should().Contain("Sample Project");
        markup.Should().Contain("<svg");
        markup.Should().Contain("hm-grid");
        markup.Should().NotContain("error-banner");
    }

    [Fact]
    public void Dashboard_Renders_ErrorBanner_OnNotFound()
    {
        using var ctx = new TestContext();
        var err = new DashboardLoadError(
            FilePath: @"C:\app\wwwroot\data.json",
            Message: "File not found",
            Line: null,
            Column: null,
            Kind: "NotFound");
        var result = new DashboardLoadResult(null, err, DateTimeOffset.UtcNow);
        ctx.Services.AddSingleton<IDashboardDataService>(new StubService(result));

        var cut = ctx.Render<Dashboard>(parameters => { });

        var markup = cut.Markup;
        markup.Should().Contain("error-banner");
        markup.Should().Contain("data.json not found");
        markup.Should().Contain(@"C:\app\wwwroot\data.json");
        markup.Should().Contain("(data.json error)");
    }

    [Fact]
    public void Dashboard_Renders_ErrorBanner_OnParseError_WithLineColumn()
    {
        using var ctx = new TestContext();
        var err = new DashboardLoadError(
            FilePath: "wwwroot/data.json",
            Message: "Unexpected end of JSON input",
            Line: 42,
            Column: 3,
            Kind: "ParseError");
        var result = new DashboardLoadResult(null, err, DateTimeOffset.UtcNow);
        ctx.Services.AddSingleton<IDashboardDataService>(new StubService(result));

        var cut = ctx.Render<Dashboard>(parameters => { });

        var markup = cut.Markup;
        markup.Should().Contain("Failed to load data.json");
        markup.Should().Contain("line 42, column 3");
    }

    [Fact]
    public void Dashboard_Renders_ErrorBanner_OnValidationError()
    {
        using var ctx = new TestContext();
        var err = new DashboardLoadError(
            FilePath: "wwwroot/data.json",
            Message: "timeline.end must be after timeline.start",
            Line: null,
            Column: null,
            Kind: "ValidationError");
        var result = new DashboardLoadResult(null, err, DateTimeOffset.UtcNow);
        ctx.Services.AddSingleton<IDashboardDataService>(new StubService(result));

        var cut = ctx.Render<Dashboard>(parameters => { });

        var markup = cut.Markup;
        markup.Should().Contain("validation failed");
        markup.Should().Contain("timeline.end must be after timeline.start");
    }
}