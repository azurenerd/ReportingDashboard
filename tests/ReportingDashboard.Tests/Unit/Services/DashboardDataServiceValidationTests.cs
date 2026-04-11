using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

/// <summary>
/// Comprehensive tests for every validation branch in DashboardDataService.Validate().
/// Complements DashboardDataServiceTests and DashboardDataServiceEdgeCaseTests
/// by targeting specific validation error paths not covered there.
/// </summary>
[Trait("Category", "Unit")]
public class DashboardDataServiceValidationTests : IDisposable
{
    private readonly Mock<ILogger<DashboardDataService>> _mockLogger;
    private readonly string _tempDir;

    public DashboardDataServiceValidationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardVal_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _mockLogger = new Mock<ILogger<DashboardDataService>>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private async Task<DashboardDataService> LoadJsonAsync(string json)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, json);
        var service = new DashboardDataService(_mockLogger.Object);
        await service.LoadAsync(path);
        return service;
    }

    private static string MinimalValid(
        string title = "Test",
        string subtitle = "Sub",
        string backlogLink = "https://test.com",
        string currentMonth = "Jan",
        string months = "\"Jan\"",
        string startDate = "2026-01-01",
        string endDate = "2026-06-30",
        string nowDate = "2026-04-10",
        string tracks = """[{ "name": "T", "label": "L", "milestones": [] }]""",
        string heatmap = """{ "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }""") =>
        $$"""
        {
            "title": "{{title}}",
            "subtitle": "{{subtitle}}",
            "backlogLink": "{{backlogLink}}",
            "currentMonth": "{{currentMonth}}",
            "months": [{{months}}],
            "timeline": {
                "startDate": "{{startDate}}",
                "endDate": "{{endDate}}",
                "nowDate": "{{nowDate}}",
                "tracks": {{tracks}}
            },
            "heatmap": {{heatmap}}
        }
        """;

    // --- Title validation ---

    [Fact]
    public async Task LoadAsync_EmptyTitle_SetsValidationError()
    {
        var service = await LoadJsonAsync(MinimalValid(title: ""));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("title");
        service.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task LoadAsync_WhitespaceOnlyTitle_SetsValidationError()
    {
        var service = await LoadJsonAsync(MinimalValid(title: "   "));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("title");
    }

    // --- Subtitle validation ---

    [Fact]
    public async Task LoadAsync_EmptySubtitle_SetsValidationError()
    {
        var service = await LoadJsonAsync(MinimalValid(subtitle: ""));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("subtitle");
        service.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task LoadAsync_WhitespaceOnlySubtitle_SetsValidationError()
    {
        var service = await LoadJsonAsync(MinimalValid(subtitle: "  "));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("subtitle");
    }

    // --- BacklogLink validation ---

    [Fact]
    public async Task LoadAsync_EmptyBacklogLink_SetsValidationError()
    {
        var service = await LoadJsonAsync(MinimalValid(backlogLink: ""));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("backlogLink");
        service.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task LoadAsync_WhitespaceOnlyBacklogLink_SetsValidationError()
    {
        var service = await LoadJsonAsync(MinimalValid(backlogLink: "   "));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("backlogLink");
    }

    // --- Months validation ---

    [Fact]
    public async Task LoadAsync_EmptyMonthsArray_SetsValidationError()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "Sub",
            "backlogLink": "https://test.com",
            "currentMonth": "Jan",
            "months": [],
            "timeline": {
                "startDate": "2026-01-01",
                "endDate": "2026-06-30",
                "nowDate": "2026-04-10",
                "tracks": [{ "name": "T", "label": "L", "milestones": [] }]
            },
            "heatmap": { "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": {} }
        }
        """;
        var service = await LoadJsonAsync(json);

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("months");
        service.ErrorMessage.Should().Contain("required");
    }

    // --- CurrentMonth validation ---

    [Fact]
    public async Task LoadAsync_EmptyCurrentMonth_SetsValidationError()
    {
        var service = await LoadJsonAsync(MinimalValid(currentMonth: ""));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("currentMonth");
        service.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task LoadAsync_CurrentMonthNotInMonthsArray_SetsValidationError()
    {
        var service = await LoadJsonAsync(MinimalValid(currentMonth: "Dec"));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("currentMonth");
        service.ErrorMessage.Should().Contain("must exist in months");
    }

    [Fact]
    public async Task LoadAsync_CurrentMonthCaseInsensitiveMatch_Succeeds()
    {
        var service = await LoadJsonAsync(MinimalValid(currentMonth: "jan"));

        service.IsError.Should().BeFalse();
        service.Data.Should().NotBeNull();
    }

    // --- Timeline startDate validation ---

    [Fact]
    public async Task LoadAsync_EmptyStartDate_SetsValidationError()
    {
        var service = await LoadJsonAsync(MinimalValid(startDate: ""));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("startDate");
        service.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task LoadAsync_InvalidStartDate_SetsValidationError()
    {
        var service = await LoadJsonAsync(MinimalValid(startDate: "not-a-date"));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("startDate");
        service.ErrorMessage.Should().Contain("not a valid date");
    }

    // --- Timeline endDate validation ---

    [Fact]
    public async Task LoadAsync_EmptyEndDate_SetsValidationError()
    {
        var service = await LoadJsonAsync(MinimalValid(endDate: ""));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("endDate");
        service.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task LoadAsync_InvalidEndDate_SetsValidationError()
    {
        var service = await LoadJsonAsync(MinimalValid(endDate: "xyz"));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("endDate");
        service.ErrorMessage.Should().Contain("not a valid date");
    }

    [Fact]
    public async Task LoadAsync_EndDateBeforeStartDate_SetsValidationError()
    {
        var service = await LoadJsonAsync(MinimalValid(startDate: "2026-06-01", endDate: "2026-01-01"));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("endDate");
        service.ErrorMessage.Should().Contain("after");
    }

    [Fact]
    public async Task LoadAsync_EndDateEqualToStartDate_SetsValidationError()
    {
        var service = await LoadJsonAsync(MinimalValid(startDate: "2026-03-01", endDate: "2026-03-01"));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("endDate");
        service.ErrorMessage.Should().Contain("after");
    }

    // --- Timeline nowDate validation ---

    [Fact]
    public async Task LoadAsync_EmptyNowDate_SetsValidationError()
    {
        var service = await LoadJsonAsync(MinimalValid(nowDate: ""));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("nowDate");
        service.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task LoadAsync_InvalidNowDate_SetsValidationError()
    {
        var service = await LoadJsonAsync(MinimalValid(nowDate: "invalid"));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("nowDate");
        service.ErrorMessage.Should().Contain("not a valid date");
    }

    // --- Timeline tracks validation ---

    [Fact]
    public async Task LoadAsync_EmptyTracksArray_SetsValidationError()
    {
        var service = await LoadJsonAsync(MinimalValid(tracks: "[]"));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("tracks");
        service.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task LoadAsync_TrackMissingName_SetsValidationError()
    {
        var tracks = """[{ "name": "", "label": "L", "milestones": [] }]""";
        var service = await LoadJsonAsync(MinimalValid(tracks: tracks));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("tracks[0].name");
        service.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task LoadAsync_TrackMissingLabel_SetsValidationError()
    {
        var tracks = """[{ "name": "Track", "label": "", "milestones": [] }]""";
        var service = await LoadJsonAsync(MinimalValid(tracks: tracks));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("tracks[0].label");
        service.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task LoadAsync_SecondTrackMissingName_ReportsCorrectIndex()
    {
        var tracks = """
        [
            { "name": "Valid", "label": "L1", "milestones": [] },
            { "name": "", "label": "L2", "milestones": [] }
        ]
        """;
        var service = await LoadJsonAsync(MinimalValid(tracks: tracks));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("tracks[1].name");
    }

    // --- Milestone validation ---

    [Fact]
    public async Task LoadAsync_MilestoneMissingDate_SetsValidationError()
    {
        var tracks = """[{ "name": "T", "label": "L", "milestones": [{ "date": "", "type": "poc", "label": "X" }] }]""";
        var service = await LoadJsonAsync(MinimalValid(tracks: tracks));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("milestones[0].date");
        service.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task LoadAsync_MilestoneInvalidDate_SetsValidationError()
    {
        var tracks = """[{ "name": "T", "label": "L", "milestones": [{ "date": "not-valid", "type": "poc", "label": "X" }] }]""";
        var service = await LoadJsonAsync(MinimalValid(tracks: tracks));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("milestones[0].date");
        service.ErrorMessage.Should().Contain("not a valid date");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("milestone")]
    [InlineData("release")]
    [InlineData("")]
    public async Task LoadAsync_MilestoneInvalidType_SetsValidationError(string invalidType)
    {
        var tracks = $$"""[{ "name": "T", "label": "L", "milestones": [{ "date": "2026-03-01", "type": "{{invalidType}}", "label": "X" }] }]""";
        var service = await LoadJsonAsync(MinimalValid(tracks: tracks));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("type");
    }

    [Theory]
    [InlineData("checkpoint")]
    [InlineData("poc")]
    [InlineData("production")]
    public async Task LoadAsync_MilestoneValidType_Succeeds(string validType)
    {
        var tracks = $$"""[{ "name": "T", "label": "L", "milestones": [{ "date": "2026-03-01", "type": "{{validType}}", "label": "X" }] }]""";
        var service = await LoadJsonAsync(MinimalValid(tracks: tracks));

        service.IsError.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_SecondMilestoneInvalid_ReportsCorrectIndex()
    {
        var tracks = """
        [{
            "name": "T", "label": "L",
            "milestones": [
                { "date": "2026-01-01", "type": "poc", "label": "OK" },
                { "date": "bad-date", "type": "poc", "label": "Bad" }
            ]
        }]
        """;
        var service = await LoadJsonAsync(MinimalValid(tracks: tracks));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("milestones[1].date");
    }

    // --- Heatmap key validation ---

    [Fact]
    public async Task LoadAsync_HeatmapShippedKeyNotInMonths_SetsValidationError()
    {
        var heatmap = """{ "shipped": { "dec": ["Item"] }, "inProgress": {}, "carryover": {}, "blockers": {} }""";
        var service = await LoadJsonAsync(MinimalValid(heatmap: heatmap));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("shipped");
        service.ErrorMessage.Should().Contain("dec");
    }

    [Fact]
    public async Task LoadAsync_HeatmapInProgressKeyNotInMonths_SetsValidationError()
    {
        var heatmap = """{ "shipped": {}, "inProgress": { "may": ["Item"] }, "carryover": {}, "blockers": {} }""";
        var service = await LoadJsonAsync(MinimalValid(heatmap: heatmap));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("inProgress");
        service.ErrorMessage.Should().Contain("may");
    }

    [Fact]
    public async Task LoadAsync_HeatmapCarryoverKeyNotInMonths_SetsValidationError()
    {
        var heatmap = """{ "shipped": {}, "inProgress": {}, "carryover": { "jun": ["Item"] }, "blockers": {} }""";
        var service = await LoadJsonAsync(MinimalValid(heatmap: heatmap));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("carryover");
        service.ErrorMessage.Should().Contain("jun");
    }

    [Fact]
    public async Task LoadAsync_HeatmapBlockersKeyNotInMonths_SetsValidationError()
    {
        var heatmap = """{ "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": { "sep": ["Blocker"] } }""";
        var service = await LoadJsonAsync(MinimalValid(heatmap: heatmap));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("blockers");
        service.ErrorMessage.Should().Contain("sep");
    }

    [Fact]
    public async Task LoadAsync_HeatmapKeyMatchesMonth_Succeeds()
    {
        var heatmap = """{ "shipped": { "jan": ["Done"] }, "inProgress": {}, "carryover": {}, "blockers": {} }""";
        var service = await LoadJsonAsync(MinimalValid(heatmap: heatmap));

        service.IsError.Should().BeFalse();
    }

    // --- Successful validation with various valid configurations ---

    [Fact]
    public async Task LoadAsync_MinimalValidJson_Succeeds()
    {
        var service = await LoadJsonAsync(MinimalValid());

        service.IsError.Should().BeFalse();
        service.Data.Should().NotBeNull();
        service.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_MultipleMonthsWithMatchingHeatmapKeys_Succeeds()
    {
        var heatmap = """
        {
            "shipped": { "jan": ["A"], "feb": ["B"] },
            "inProgress": { "jan": ["C"] },
            "carryover": { "feb": ["D"] },
            "blockers": {}
        }
        """;
        var service = await LoadJsonAsync(MinimalValid(
            months: "\"Jan\", \"Feb\"",
            heatmap: heatmap));

        service.IsError.Should().BeFalse();
    }

    // --- Error state management ---

    [Fact]
    public async Task LoadAsync_WhenValidationFails_DataIsNull()
    {
        var service = await LoadJsonAsync(MinimalValid(title: ""));

        service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_WhenValidationFails_ErrorMessageContainsValidationPrefix()
    {
        var service = await LoadJsonAsync(MinimalValid(subtitle: ""));

        service.ErrorMessage.Should().StartWith("data.json validation:");
    }

    [Fact]
    public async Task LoadAsync_NullDeserialization_SetsError()
    {
        // "null" as JSON deserializes to null DashboardData
        var service = await LoadJsonAsync("null");

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("deserialization returned null");
    }

    [Fact]
    public async Task LoadAsync_JsonArray_SetsParseError()
    {
        var service = await LoadJsonAsync("[]");

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("parse");
    }

    // --- Logger verification ---

    [Fact]
    public async Task LoadAsync_ValidationError_LogsError()
    {
        await LoadJsonAsync(MinimalValid(title: ""));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Dashboard data error")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_SuccessfulLoad_LogsInformation()
    {
        await LoadJsonAsync(MinimalValid());

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully loaded")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_LogsError()
    {
        var service = new DashboardDataService(_mockLogger.Object);
        await service.LoadAsync(Path.Combine(_tempDir, "does_not_exist.json"));

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}