using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

/// <summary>
/// Tests for DataService.GetCurrentMonthName() covering CurrentMonthOverride
/// and derivation from effective date.
/// </summary>
[Trait("Category", "Unit")]
public class DataServiceCurrentMonthTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _wwwrootDir;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public DataServiceCurrentMonthTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "DSMonthTests_" + Guid.NewGuid().ToString("N"));
        _wwwrootDir = Path.Combine(_tempDir, "wwwroot");
        Directory.CreateDirectory(_wwwrootDir);

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_wwwrootDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private static string CreateJson(string? nowDateOverride = null, string? currentMonthOverride = null)
    {
        var data = new
        {
            schemaVersion = 1,
            title = "T",
            subtitle = "S",
            backlogUrl = "https://example.com",
            nowDateOverride,
            currentMonthOverride,
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-31",
                workstreams = Array.Empty<object>()
            },
            heatmap = new
            {
                monthColumns = Array.Empty<string>(),
                categories = Array.Empty<object>()
            }
        };
        return JsonSerializer.Serialize(data);
    }

    private void WriteDataJson(string content) =>
        File.WriteAllText(Path.Combine(_wwwrootDir, "data.json"), content);

    [Fact]
    public void GetCurrentMonthName_WithCurrentMonthOverride_ReturnsOverrideValue()
    {
        WriteDataJson(CreateJson(currentMonthOverride: "Mar"));
        using var service = new DataService(_envMock.Object);

        service.GetCurrentMonthName().Should().Be("Mar");
    }

    [Fact]
    public void GetCurrentMonthName_WithNowDateOverride_DerivesMonthFromDate()
    {
        WriteDataJson(CreateJson(nowDateOverride: "2026-06-15"));
        using var service = new DataService(_envMock.Object);

        service.GetCurrentMonthName().Should().Be("Jun");
    }

    [Fact]
    public void GetCurrentMonthName_NoOverrides_DerivesFromSystemDate()
    {
        WriteDataJson(CreateJson());
        using var service = new DataService(_envMock.Object);

        var expected = DateOnly.FromDateTime(DateTime.Today).ToString("MMM");
        service.GetCurrentMonthName().Should().Be(expected);
    }

    [Fact]
    public void GetCurrentMonthName_CurrentMonthOverrideTakesPrecedence_OverNowDate()
    {
        WriteDataJson(CreateJson(nowDateOverride: "2026-06-15", currentMonthOverride: "Apr"));
        using var service = new DataService(_envMock.Object);

        service.GetCurrentMonthName().Should().Be("Apr");
    }
}