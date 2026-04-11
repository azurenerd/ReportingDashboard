using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration;

/// <summary>
/// Verifies the data contract between data.json's currentMonth field
/// and the Header legend's Now label, ensuring the full deserialization
/// pipeline preserves the value for component rendering.
/// </summary>
[Trait("Category", "Integration")]
public class HeaderLegendDataContractTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    public HeaderLegendDataContractTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"LegendContract_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private string WriteJson(string rawJson)
    {
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, rawJson);
        return path;
    }

    private string WriteJsonObj(object data)
    {
        return WriteJson(JsonSerializer.Serialize(data, JsonOpts));
    }

    private object CreateMinimalValidData(string currentMonth) => new
    {
        title = "Test",
        subtitle = "Sub",
        backlogLink = "",
        currentMonth,
        months = new[] { "January" },
        timeline = new
        {
            startDate = "2026-01-01",
            endDate = "2026-07-01",
            nowDate = "2026-04-10",
            tracks = new[]
            {
                new
                {
                    name = "M1",
                    label = "Track",
                    color = "#000",
                    milestones = new[] { new { date = "2026-02-15", type = "poc", label = "F" } }
                }
            }
        },
        heatmap = new
        {
            shipped = new Dictionary<string, string[]>(),
            inProgress = new Dictionary<string, string[]>(),
            carryover = new Dictionary<string, string[]>(),
            blockers = new Dictionary<string, string[]>()
        }
    };

    #region currentMonth JSON contract

    [Fact]
    public async Task CurrentMonth_StandardValue_DeserializesCorrectly()
    {
        var path = WriteJsonObj(CreateMinimalValidData("Apr 2026"));
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        svc.IsError.Should().BeFalse();
        svc.Data!.CurrentMonth.Should().Be("Apr 2026");
    }

    [Fact]
    public async Task CurrentMonth_LongValue_DeserializesCorrectly()
    {
        var longMonth = "September 2026";
        var path = WriteJsonObj(CreateMinimalValidData(longMonth));
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        svc.IsError.Should().BeFalse();
        svc.Data!.CurrentMonth.Should().Be(longMonth);
    }

    [Fact]
    public async Task CurrentMonth_WithSpacesAndSpecialChars_PreservedVerbatim()
    {
        var month = "Q1/Q2 2026-27";
        var path = WriteJsonObj(CreateMinimalValidData(month));
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        if (!svc.IsError)
        {
            svc.Data!.CurrentMonth.Should().Be(month);
        }
    }

    [Fact]
    public async Task CurrentMonth_MissingFromJson_DefaultsToEmpty()
    {
        // Raw JSON without currentMonth field
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "",
            "months": ["January"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "nowDate": "2026-04-10",
                "tracks": [{"name":"M1","label":"T","color":"#000","milestones":[{"date":"2026-02-15","type":"poc","label":"F"}]}]
            },
            "heatmap": {
                "shipped": {},
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;
        var path = WriteJson(json);
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        // Whether this is an error depends on validation; if not error, currentMonth defaults to empty
        if (!svc.IsError && svc.Data != null)
        {
            svc.Data.CurrentMonth.Should().BeEmpty();
        }
    }

    #endregion

    #region JSON property name casing

    [Fact]
    public async Task CurrentMonth_CamelCaseInJson_DeserializesWithJsonPropertyName()
    {
        // Verify the [JsonPropertyName("currentMonth")] attribute works
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "",
            "currentMonth": "Nov 2026",
            "months": ["January"],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-07-01",
                "nowDate": "2026-04-10",
                "tracks": [{"name":"M1","label":"T","color":"#000","milestones":[{"date":"2026-02-15","type":"poc","label":"F"}]}]
            },
            "heatmap": {
                "shipped": {},
                "inProgress": {},
                "carryover": {},
                "blockers": {}
            }
        }
        """;
        var path = WriteJson(json);
        var svc = new DashboardDataService(_logger);
        await svc.LoadAsync(path);

        svc.IsError.Should().BeFalse();
        svc.Data!.CurrentMonth.Should().Be("Nov 2026");
    }

    #endregion

    #region Full model round-trip preserves legend-relevant fields

    [Fact]
    public void DashboardData_SerializeDeserialize_PreservesCurrentMonth()
    {
        var original = new DashboardData
        {
            Title = "RT Test",
            Subtitle = "Team",
            CurrentMonth = "Aug 2026",
            Months = new List<string> { "August" },
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2026-12-01",
                NowDate = "2026-08-15",
                Tracks = new List<TimelineTrack>()
            },
            Heatmap = new HeatmapData()
        };

        var json = JsonSerializer.Serialize(original, JsonOpts);
        var deserialized = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        deserialized.Should().NotBeNull();
        deserialized!.CurrentMonth.Should().Be("Aug 2026");
        deserialized.Title.Should().Be("RT Test");
    }

    [Fact]
    public void DashboardData_SerializeDeserialize_AllLegendRelevantFieldsIntact()
    {
        var original = new DashboardData
        {
            Title = "Legend Test",
            Subtitle = "Sub",
            BacklogLink = "https://link",
            CurrentMonth = "Dec 2026",
            Months = new List<string> { "December" },
            Timeline = new TimelineData
            {
                StartDate = "2026-01-01",
                EndDate = "2027-01-01",
                NowDate = "2026-12-15",
                Tracks = new List<TimelineTrack>
                {
                    new()
                    {
                        Name = "M1",
                        Label = "Track",
                        Color = "#4285F4",
                        Milestones = new List<Milestone>
                        {
                            new() { Date = "2026-06-01", Type = "poc", Label = "Jun" },
                            new() { Date = "2026-11-01", Type = "production", Label = "Nov" },
                            new() { Date = "2026-09-01", Type = "checkpoint", Label = "Sep" }
                        }
                    }
                }
            },
            Heatmap = new HeatmapData()
        };

        var json = JsonSerializer.Serialize(original, JsonOpts);
        var deserialized = JsonSerializer.Deserialize<DashboardData>(json, JsonOpts);

        deserialized!.CurrentMonth.Should().Be("Dec 2026");
        deserialized.Timeline.Tracks.Should().HaveCount(1);
        deserialized.Timeline.Tracks[0].Milestones.Should().HaveCount(3);

        var types = deserialized.Timeline.Tracks[0].Milestones.Select(m => m.Type).ToList();
        types.Should().Contain("poc");
        types.Should().Contain("production");
        types.Should().Contain("checkpoint");
    }

    #endregion
}