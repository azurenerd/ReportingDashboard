using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Web.Components.Pages;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using ReportingDashboard.Web.ViewModels;
using Xunit;

namespace ReportingDashboard.Web.Tests;

public class SmokeTests
{
    private static DashboardDataService CreateService(out string tempDir)
    {
        tempDir = Path.Combine(Path.GetTempPath(), "rd-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(tempDir, "wwwroot"));

        var sourceJson = Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..",
            "src", "ReportingDashboard.Web", "wwwroot", "data.json");

        var destJson = Path.Combine(tempDir, "wwwroot", "data.json");
        if (File.Exists(sourceJson))
        {
            File.Copy(sourceJson, destJson, overwrite: true);
        }
        else
        {
            File.WriteAllText(destJson, "{}");
        }

        var env = new FakeWebHostEnvironment
        {
            ContentRootPath = tempDir,
            WebRootPath = Path.Combine(tempDir, "wwwroot"),
            EnvironmentName = "Development",
            ApplicationName = "ReportingDashboard.Web",
            ContentRootFileProvider = new PhysicalFileProvider(tempDir),
            WebRootFileProvider = new PhysicalFileProvider(Path.Combine(tempDir, "wwwroot"))
        };

        return new DashboardDataService(
            NullLogger<DashboardDataService>.Instance,
            env,
            new MemoryCache(new MemoryCacheOptions()));
    }

    [Fact]
    public void DashboardDataService_GetCurrent_ReturnsNonNullResult()
    {
        var service = CreateService(out _);

        var result = service.GetCurrent();

        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public void TimelineLayoutEngine_Build_ReturnsEmpty_ForStub()
    {
        var timeline = new Timeline
        {
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31),
            Lanes = Array.Empty<TimelineLane>()
        };

        var vm = TimelineLayoutEngine.Build(
            timeline,
            new DateOnly(2026, 4, 1),
            TimelineLayoutEngine.SvgWidth,
            TimelineLayoutEngine.SvgHeight);

        vm.Should().BeSameAs(TimelineViewModel.Empty);
    }

    [Fact]
    public void HeatmapLayoutEngine_Build_ReturnsEmpty_ForStub()
    {
        var heatmap = new Heatmap
        {
            Rows = Array.Empty<HeatmapRow>()
        };

        var vm = HeatmapLayoutEngine.Build(heatmap, new DateOnly(2026, 4, 1), 4);

        vm.Should().BeSameAs(HeatmapViewModel.Empty);
    }

    [Fact]
    public void DashboardDataValidator_Validate_ReturnsEmpty_ForStub()
    {
        var data = new DashboardData
        {
            Project = Project.Placeholder,
            Timeline = new Timeline
            {
                StartDate = new DateOnly(2026, 1, 1),
                EndDate = new DateOnly(2026, 12, 31),
                Lanes = Array.Empty<TimelineLane>()
            },
            Heatmap = new Heatmap { Rows = Array.Empty<HeatmapRow>() },
            Theme = new Theme()
        };

        var errors = DashboardDataValidator.Validate(data);

        errors.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Dashboard_Renders_WithoutException_AndHasNoBlazorJs()
    {
        using var ctx = new TestContext();

        var service = CreateService(out _);
        ctx.Services.AddSingleton<IDashboardDataService>(service);

        var cut = ctx.RenderComponent<Dashboard>();
        var markup = cut.Markup;

        markup.Should().Contain("placeholder-hdr");
        markup.Should().Contain("placeholder-timeline");
        markup.Should().Contain("placeholder-heatmap");

        markup.Should().NotContain("_framework/blazor.server.js");
        markup.Should().NotContain("_framework/blazor.web.js");
    }

    [Fact]
    public void DataJson_Deserializes_IntoDashboardData()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..",
            "src", "ReportingDashboard.Web", "wwwroot", "data.json");

        if (!File.Exists(path))
        {
            return;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters = { new JsonStringEnumConverter() }
        };

        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<DashboardData>(json, options);

        data.Should().NotBeNull();
        data!.Project.Should().NotBeNull();
        data.Timeline.Should().NotBeNull();
        data.Timeline.Lanes.Should().HaveCount(3);
        data.Heatmap.Should().NotBeNull();
        data.Heatmap.Rows.Should().HaveCount(4);
    }

    private sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ApplicationName { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
    }
}