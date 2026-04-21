using System.Text.Json;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Web.Services;

namespace ReportingDashboard.Web.Tests;

public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _wwwrootDir;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"dash_test_{Guid.NewGuid():N}");
        _wwwrootDir = Path.Combine(_tempDir, "wwwroot");
        Directory.CreateDirectory(_wwwrootDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private DashboardDataService CreateService()
    {
        var env = new TestWebHostEnvironment(_wwwrootDir);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var logger = NullLogger<DashboardDataService>.Instance;
        return new DashboardDataService(env, cache, logger);
    }

    private void WriteDataJson(string json)
    {
        File.WriteAllText(Path.Combine(_wwwrootDir, "data.json"), json);
    }

    [Fact]
    public void MissingFile_Returns_NotFound_Error()
    {
        using var svc = CreateService();
        var result = svc.GetCurrent();

        result.Error.Should().NotBeNull();
        result.Error!.Kind.Should().Be("NotFound");
        result.Data.Should().BeNull();
    }

    [Fact]
    public void MalformedJson_Returns_ParseError()
    {
        WriteDataJson("{ invalid json }}}");
        using var svc = CreateService();
        var result = svc.GetCurrent();

        result.Error.Should().NotBeNull();
        result.Error!.Kind.Should().Be("ParseError");
        result.Data.Should().BeNull();
    }

    [Fact]
    public void ValidJson_Returns_Data()
    {
        WriteDataJson("""
        {
          "project": { "title": "Test", "subtitle": "Sub" },
          "timeline": {
            "start": "2026-01-01", "end": "2026-06-30",
            "lanes": [{ "id": "M1", "label": "L", "color": "#000000", "milestones": [] }]
          },
          "heatmap": {
            "months": ["Jan","Feb","Mar","Apr"],
            "maxItemsPerCell": 4,
            "rows": [
              { "category": "shipped", "cells": [[], [], [], []] },
              { "category": "inProgress", "cells": [[], [], [], []] },
              { "category": "carryover", "cells": [[], [], [], []] },
              { "category": "blockers", "cells": [[], [], [], []] }
            ]
          }
        }
        """);

        using var svc = CreateService();
        var result = svc.GetCurrent();

        result.Data.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Data!.Project.Title.Should().Be("Test");
    }

    private class TestWebHostEnvironment : Microsoft.AspNetCore.Hosting.IWebHostEnvironment
    {
        public TestWebHostEnvironment(string webRootPath)
        {
            WebRootPath = webRootPath;
            ContentRootPath = Path.GetDirectoryName(webRootPath)!;
        }

        public string WebRootPath { get; set; }
        public Microsoft.Extensions.FileProviders.IFileProvider WebRootFileProvider { get; set; } = null!;
        public string ApplicationName { get; set; } = "Test";
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Test";
    }
}