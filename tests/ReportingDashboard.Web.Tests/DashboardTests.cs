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

public class DashboardTests : TestContext
{
    private sealed class FakeDashboardDataService : IDashboardDataService
    {
        private readonly Func<DashboardLoadResult> _factory;
        public int CallCount { get; private set; }

        public FakeDashboardDataService(DashboardLoadResult result)
            : this(() => result) { }

        public FakeDashboardDataService(Func<DashboardLoadResult> factory)
        {
            _factory = factory;
        }

        public event EventHandler? DataChanged;

        public DashboardLoadResult GetCurrent()
        {
            CallCount++;
            return _factory();
        }

        // Keep the event referenced to avoid CS0067 in some configurations.
        public void RaiseChanged() => DataChanged?.Invoke(this, EventArgs.Empty);
    }

    private static DashboardData BuildValidData()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = new DateOnly(today.Year, 1, 1);
        var end = new DateOnly(today.Year, 12, 31);

        return new DashboardData
        {
            Project = new Project
            {
                Title = "Test Project",
                Subtitle = "Test Workstream"
            },
            Timeline = new Timeline
            {
                Start = start,
                End = end,
                Lanes = new List<TimelineLane>
                {
                    new TimelineLane
                    {
                        Id = "M1",
                        Label = "Lane 1",
                        Color = "#0078D4",
                        Milestones = new List<Milestone>()
                    }
                }
            },
            Heatmap = new Heatmap
            {
                Months = new List<string> { "Jan", "Feb", "Mar", "Apr" },
                CurrentMonthIndex = null,
                MaxItemsPerCell = 4,
                Rows = new List<HeatmapRow>
                {
                    new HeatmapRow { Category = HeatmapCategory.Shipped,    Cells = Cells() },
                    new HeatmapRow { Category = HeatmapCategory.InProgress, Cells = Cells() },
                    new HeatmapRow { Category = HeatmapCategory.Carryover,  Cells = Cells() },
                    new HeatmapRow { Category = HeatmapCategory.Blockers,   Cells = Cells() }
                }
            }
        };

        static IReadOnlyList<IReadOnlyList<string>> Cells() =>
            new List<IReadOnlyList<string>>
            {
                new List<string>(), new List<string>(),
                new List<string>(), new List<string>()
            };
    }

    private FakeDashboardDataService RegisterFake(DashboardLoadResult result)
    {
        var fake = new FakeDashboardDataService(result);
        Services.AddSingleton<IDashboardDataService>(fake);
        return fake;
    }

    private FakeDashboardDataService RegisterFake(Func<DashboardLoadResult> factory)
    {
        var fake = new FakeDashboardDataService(factory);
        Services.AddSingleton<IDashboardDataService>(fake);
        return fake;
    }

    [Fact]
    public void HappyPath_RendersAllThreeSections_AndNoBanner()
    {
        var data = BuildValidData();
        RegisterFake(new DashboardLoadResult(data, null, DateTimeOffset.UtcNow));

        var cut = RenderComponent<Dashboard>();

        cut.FindAll(".error-banner").Should().BeEmpty();
        cut.FindAll(".hdr").Should().NotBeEmpty();
        cut.FindAll(".tl-area svg").Should().NotBeEmpty();
        cut.FindAll(".hm-grid").Should().NotBeEmpty();
    }

    [Fact]
    public void NotFoundError_RendersBannerWithCorrectHeading()
    {
        var err = new DashboardLoadError("wwwroot/data.json", "File not found", null, null, "NotFound");
        RegisterFake(new DashboardLoadResult(null, err, DateTimeOffset.UtcNow));

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-banner").Should().NotBeNull();
        cut.Find(".error-banner strong").TextContent.Should().Be("data.json not found");
        cut.Find(".error-path").TextContent.Should().Be("wwwroot/data.json");
        cut.FindAll(".hdr").Should().NotBeEmpty();
        cut.FindAll(".tl-area svg").Should().NotBeEmpty();
        cut.FindAll(".hm-grid").Should().NotBeEmpty();
    }

    [Fact]
    public void ParseError_RendersLineAndColumn()
    {
        var err = new DashboardLoadError("wwwroot/data.json", "Unexpected token", 42, 3, "ParseError");
        RegisterFake(new DashboardLoadResult(null, err, DateTimeOffset.UtcNow));

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-banner strong").TextContent.Should().Be("Failed to parse data.json");
        cut.Find(".error-location").TextContent.Should().Contain("line 42").And.Contain("col 3");
    }

    [Fact]
    public void ValidationError_OmitsLocationSpan()
    {
        var err = new DashboardLoadError("wwwroot/data.json", "Invalid color", null, null, "ValidationError");
        RegisterFake(new DashboardLoadResult(null, err, DateTimeOffset.UtcNow));

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-banner strong").TextContent.Should().Be("data.json validation failed");
        cut.FindAll(".error-location").Should().BeEmpty();
    }

    [Fact]
    public void ServiceThrows_DoesNotPropagate_RendersUnexpectedErrorBanner()
    {
        RegisterFake(() => throw new InvalidOperationException("boom"));

        var cut = RenderComponent<Dashboard>();

        cut.Find(".error-banner").Should().NotBeNull();
        cut.Find(".error-banner strong").TextContent.Should().Be("Failed to load data.json");
        cut.Find(".error-message").TextContent.Should().Contain("boom");
    }

    [Fact]
    public void GetCurrent_IsCalledExactlyOnce()
    {
        var data = BuildValidData();
        var fake = RegisterFake(new DashboardLoadResult(data, null, DateTimeOffset.UtcNow));

        RenderComponent<Dashboard>();

        fake.CallCount.Should().Be(1);
    }

    [Fact]
    public void NoBlazorServerScriptInOutput()
    {
        var data = BuildValidData();
        RegisterFake(new DashboardLoadResult(data, null, DateTimeOffset.UtcNow));

        var cut = RenderComponent<Dashboard>();

        cut.Markup.Should().NotContain("blazor.server.js");
    }
}