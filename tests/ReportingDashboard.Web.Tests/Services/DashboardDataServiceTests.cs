using System;
using System.IO;
using System.Text;
using System.Threading;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;

namespace ReportingDashboard.Web.Tests.Services;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests
{
    private const string ValidDataJson = """
    {
      "project": {
        "title": "My Project",
        "subtitle": "Org - Workstream - Jan 2026",
        "backlogUrl": "https://example.com/backlog"
      },
      "timeline": {
        "start": "2025-01-01",
        "end": "2026-12-31",
        "lanes": []
      },
      "heatmap": {
        "months": [],
        "rows": []
      }
    }
    """;

    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    private sealed class TempDir : IDisposable
    {
        public string Path { get; }
        public TempDir()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "rd-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }
        public string FilePath => System.IO.Path.Combine(Path, "data.json");
        public void Dispose()
        {
            try { if (Directory.Exists(Path)) Directory.Delete(Path, true); }
            catch { /* best effort */ }
        }
    }

    private static DashboardDataService CreateService(string filePath)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        return new DashboardDataService(filePath, cache, NullLogger<DashboardDataService>.Instance);
    }

    [Fact]
    public void GetCurrent_HappyPath_ReturnsData()
    {
        using var temp = new TempDir();
        File.WriteAllText(temp.FilePath, ValidDataJson, Utf8NoBom);

        using var svc = CreateService(temp.FilePath);
        var result = svc.GetCurrent();

        result.Should().NotBeNull();
        result.Error.Should().BeNull();
        result.Data.Should().NotBeNull();
        result.LoadedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void GetCurrent_MissingFile_ReturnsNotFoundError()
    {
        using var temp = new TempDir();

        using var svc = CreateService(temp.FilePath);
        var result = svc.GetCurrent();

        result.Data.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Kind.Should().Be(DashboardLoadErrorKind.NotFound);
        result.Error.FilePath.Should().EndWith("data.json");
        result.Error.Message.Should().Contain("not found");
    }

    [Fact]
    public void GetCurrent_MalformedJson_ReturnsParseError()
    {
        using var temp = new TempDir();
        File.WriteAllText(temp.FilePath, "{ not valid json", Utf8NoBom);

        using var svc = CreateService(temp.FilePath);
        var result = svc.GetCurrent();

        result.Data.Should().BeNull();
        result.Error.Should().NotBeNull();
        result.Error!.Kind.Should().Be(DashboardLoadErrorKind.ParseError);
        result.Error.Message.Should().NotBeNullOrEmpty();
        result.Error.Line.Should().NotBeNull();
        result.Error.Line!.Value.Should().BeGreaterThan(0);
        result.Error.Column.Should().NotBeNull();
        result.Error.Column!.Value.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        using var temp = new TempDir();
        File.WriteAllText(temp.FilePath, ValidDataJson, Utf8NoBom);

        var svc = CreateService(temp.FilePath);
        svc.Dispose();
        Action second = () => svc.Dispose();
        second.Should().NotThrow();
    }

    [Fact]
    public void GetCurrent_NeverThrows_WhenDirectoryMissing()
    {
        var nonexistent = Path.Combine(
            Path.GetTempPath(),
            "rd-missing-" + Guid.NewGuid().ToString("N"),
            "data.json");

        DashboardDataService? svc = null;
        Action construct = () => svc = CreateService(nonexistent);
        construct.Should().NotThrow();

        try
        {
            var result = svc!.GetCurrent();
            result.Should().NotBeNull();
            result.Data.Should().BeNull();
            result.Error.Should().NotBeNull();
            result.Error!.Kind.Should().Be(DashboardLoadErrorKind.NotFound);
        }
        finally
        {
            svc?.Dispose();
        }
    }
}