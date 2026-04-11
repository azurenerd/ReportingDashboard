using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Services;

/// <summary>
/// Integration tests for DashboardDataService validation logic exercised
/// through real file I/O with various JSON payloads. Covers every validation
/// branch in the Validate() method end-to-end through LoadAsync.
/// </summary>
[Trait("Category", "Integration")]
public class DashboardDataServiceValidationIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly Mock<ILogger<DashboardDataService>> _mockLogger;

    public DashboardDataServiceValidationIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardValInteg_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _mockLogger = new Mock<ILogger<DashboardDataService>>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private async Task<DashboardDataService> LoadFromJsonAsync(string json)
    {
        var path = Path.Combine(_tempDir, $"data_{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(path, json);
        var service = new DashboardDataService(_mockLogger.Object);
        await service.LoadAsync(path);
        return service;
    }

    private static string BuildJson(
        string title = "Test Title",
        string subtitle = "Test Subtitle",
        string backlogLink = "https://test.com",
        string currentMonth = "Jan",
        string months = """["Jan"]""",
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
            "months": {{months}},
            "timeline": {
                "startDate": "{{startDate}}",
                "endDate": "{{endDate}}",
                "nowDate": "{{nowDate}}",
                "tracks": {{tracks}}
            },
            "heatmap": {{heatmap}}
        }
        """;

    // --- Subtitle validation (not covered in existing tests) ---

    [Fact]
    public async Task LoadAsync_EmptySubtitle_FailsValidation()
    {
        var service = await LoadFromJsonAsync(BuildJson(subtitle: ""));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("subtitle");
        service.ErrorMessage.Should().Contain("required");
        service.Data.Should().BeNull();
    }

    [Fact]
    public async Task LoadAsync_WhitespaceSubtitle_FailsValidation()
    {
        var service = await LoadFromJsonAsync(BuildJson(subtitle: "   "));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("subtitle");
    }

    // --- BacklogLink validation (not covered in existing tests) ---

    [Fact]
    public async Task LoadAsync_EmptyBacklogLink_FailsValidation()
    {
        var service = await LoadFromJsonAsync(BuildJson(backlogLink: ""));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("backlogLink");
        service.ErrorMessage.Should().Contain("required");
    }

    // --- CurrentMonth must exist in months array ---

    [Fact]
    public async Task LoadAsync_CurrentMonthNotInMonths_FailsValidation()
    {
        var service = await LoadFromJsonAsync(BuildJson(currentMonth: "Dec"));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("currentMonth must exist in months array");
    }

    [Fact]
    public async Task LoadAsync_CurrentMonthCaseInsensitive_Succeeds()
    {
        var service = await LoadFromJsonAsync(BuildJson(currentMonth: "jan"));

        service.IsError.Should().BeFalse();
        service.Data.Should().NotBeNull();
    }

    // --- Timeline date validation ---

    [Fact]
    public async Task LoadAsync_InvalidStartDate_FailsWithMessage()
    {
        var service = await LoadFromJsonAsync(BuildJson(startDate: "not-a-date"));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("startDate");
        service.ErrorMessage.Should().Contain("not a valid date");
    }

    [Fact]
    public async Task LoadAsync_InvalidEndDate_FailsWithMessage()
    {
        var service = await LoadFromJsonAsync(BuildJson(endDate: "bad"));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("endDate");
        service.ErrorMessage.Should().Contain("not a valid date");
    }

    [Fact]
    public async Task LoadAsync_EndDateBeforeStartDate_FailsValidation()
    {
        var service = await LoadFromJsonAsync(BuildJson(
            startDate: "2026-06-30",
            endDate: "2026-01-01"));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("endDate must be after");
    }

    [Fact]
    public async Task LoadAsync_EndDateEqualsStartDate_FailsValidation()
    {
        var service = await LoadFromJsonAsync(BuildJson(
            startDate: "2026-03-15",
            endDate: "2026-03-15"));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("endDate must be after");
    }

    [Fact]
    public async Task LoadAsync_InvalidNowDate_FailsWithMessage()
    {
        var service = await LoadFromJsonAsync(BuildJson(nowDate: "xyz"));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("nowDate");
        service.ErrorMessage.Should().Contain("not a valid date");
    }

    [Fact]
    public async Task LoadAsync_EmptyNowDate_FailsValidation()
    {
        var service = await LoadFromJsonAsync(BuildJson(nowDate: ""));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("nowDate");
        service.ErrorMessage.Should().Contain("required");
    }

    // --- Track-level validation ---

    [Fact]
    public async Task LoadAsync_TrackMissingName_FailsWithIndex()
    {
        var tracks = """[{ "name": "", "label": "L", "milestones": [] }]""";
        var service = await LoadFromJsonAsync(BuildJson(tracks: tracks));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("tracks[0].name");
    }

    [Fact]
    public async Task LoadAsync_TrackMissingLabel_FailsWithIndex()
    {
        var tracks = """[{ "name": "T", "label": "", "milestones": [] }]""";
        var service = await LoadFromJsonAsync(BuildJson(tracks: tracks));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("tracks[0].label");
    }

    [Fact]
    public async Task LoadAsync_SecondTrackInvalid_ReportsIndex1()
    {
        var tracks = """
        [
            { "name": "Good", "label": "L1", "milestones": [] },
            { "name": "", "label": "L2", "milestones": [] }
        ]
        """;
        var service = await LoadFromJsonAsync(BuildJson(tracks: tracks));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("tracks[1].name");
    }

    // --- Milestone validation ---

    [Fact]
    public async Task LoadAsync_MilestoneMissingDate_FailsWithIndex()
    {
        var tracks = """[{ "name": "T", "label": "L", "milestones": [{ "date": "", "type": "poc", "label": "X" }] }]""";
        var service = await LoadFromJsonAsync(BuildJson(tracks: tracks));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("milestones[0].date");
        service.ErrorMessage.Should().Contain("required");
    }

    [Fact]
    public async Task LoadAsync_MilestoneInvalidDate_FailsWithDetails()
    {
        var tracks = """[{ "name": "T", "label": "L", "milestones": [{ "date": "nope", "type": "poc", "label": "X" }] }]""";
        var service = await LoadFromJsonAsync(BuildJson(tracks: tracks));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("milestones[0].date");
        service.ErrorMessage.Should().Contain("not a valid date");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("release")]
    [InlineData("milestone")]
    [InlineData("")]
    public async Task LoadAsync_MilestoneInvalidType_FailsValidation(string badType)
    {
        var tracks = $$"""[{ "name": "T", "label": "L", "milestones": [{ "date": "2026-03-01", "type": "{{badType}}", "label": "X" }] }]""";
        var service = await LoadFromJsonAsync(BuildJson(tracks: tracks));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("type");
        service.ErrorMessage.Should().Contain("must be one of");
    }

    [Theory]
    [InlineData("checkpoint")]
    [InlineData("poc")]
    [InlineData("production")]
    public async Task LoadAsync_MilestoneValidTypes_PassValidation(string validType)
    {
        var tracks = $$"""[{ "name": "T", "label": "L", "milestones": [{ "date": "2026-03-01", "type": "{{validType}}", "label": "X" }] }]""";
        var service = await LoadFromJsonAsync(BuildJson(tracks: tracks));

        service.IsError.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_SecondMilestoneInvalid_ReportsCorrectIndices()
    {
        var tracks = """
        [{
            "name": "T", "label": "L",
            "milestones": [
                { "date": "2026-01-15", "type": "poc", "label": "OK" },
                { "date": "garbage", "type": "poc", "label": "Bad" }
            ]
        }]
        """;
        var service = await LoadFromJsonAsync(BuildJson(tracks: tracks));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("milestones[1].date");
    }

    // --- Heatmap key validation ---

    [Fact]
    public async Task LoadAsync_HeatmapShippedKeyMismatch_FailsValidation()
    {
        var heatmap = """{ "shipped": { "dec": ["Item"] }, "inProgress": {}, "carryover": {}, "blockers": {} }""";
        var service = await LoadFromJsonAsync(BuildJson(heatmap: heatmap));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("shipped");
        service.ErrorMessage.Should().Contain("dec");
    }

    [Fact]
    public async Task LoadAsync_HeatmapInProgressKeyMismatch_FailsValidation()
    {
        var heatmap = """{ "shipped": {}, "inProgress": { "aug": ["Item"] }, "carryover": {}, "blockers": {} }""";
        var service = await LoadFromJsonAsync(BuildJson(heatmap: heatmap));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("inProgress");
        service.ErrorMessage.Should().Contain("aug");
    }

    [Fact]
    public async Task LoadAsync_HeatmapCarryoverKeyMismatch_FailsValidation()
    {
        var heatmap = """{ "shipped": {}, "inProgress": {}, "carryover": { "nov": ["Item"] }, "blockers": {} }""";
        var service = await LoadFromJsonAsync(BuildJson(heatmap: heatmap));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("carryover");
        service.ErrorMessage.Should().Contain("nov");
    }

    [Fact]
    public async Task LoadAsync_HeatmapBlockersKeyMismatch_FailsValidation()
    {
        var heatmap = """{ "shipped": {}, "inProgress": {}, "carryover": {}, "blockers": { "sep": ["B"] } }""";
        var service = await LoadFromJsonAsync(BuildJson(heatmap: heatmap));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("blockers");
        service.ErrorMessage.Should().Contain("sep");
    }

    [Fact]
    public async Task LoadAsync_HeatmapWithMultipleMonths_MatchingKeys_Succeeds()
    {
        var heatmap = """
        {
            "shipped": { "jan": ["A"], "feb": ["B"], "mar": ["C"] },
            "inProgress": { "jan": ["D"] },
            "carryover": { "mar": ["E"] },
            "blockers": {}
        }
        """;
        var service = await LoadFromJsonAsync(BuildJson(
            months: """["Jan", "Feb", "Mar"]""",
            heatmap: heatmap));

        service.IsError.Should().BeFalse();
        service.Data!.Heatmap.Shipped.Should().HaveCount(3);
    }

    // --- Error message prefixes ---

    [Fact]
    public async Task LoadAsync_FileNotFound_ErrorContainsNotFoundAndPath()
    {
        var service = new DashboardDataService(_mockLogger.Object);
        var fakePath = Path.Combine(_tempDir, "missing_file.json");

        await service.LoadAsync(fakePath);

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("not found");
        service.ErrorMessage.Should().Contain(fakePath);
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_ErrorContainsParsePrefix()
    {
        var service = await LoadFromJsonAsync("{ this is not valid json }");

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().Contain("Failed to parse data.json");
    }

    [Fact]
    public async Task LoadAsync_ValidationError_ErrorContainsValidationPrefix()
    {
        var service = await LoadFromJsonAsync(BuildJson(title: ""));

        service.IsError.Should().BeTrue();
        service.ErrorMessage.Should().StartWith("data.json validation:");
    }

    // --- State transitions ---

    [Fact]
    public async Task LoadAsync_SuccessfulThenError_ClearsDataOnError()
    {
        var service = new DashboardDataService(_mockLogger.Object);

        // First: success
        var validPath = Path.Combine(_tempDir, "valid.json");
        await File.WriteAllTextAsync(validPath, BuildJson());
        await service.LoadAsync(validPath);
        service.IsError.Should().BeFalse();
        service.Data.Should().NotBeNull();

        // Second: error (file not found)
        await service.LoadAsync(Path.Combine(_tempDir, "gone.json"));
        service.IsError.Should().BeTrue();
        service.Data.Should().BeNull("previous data should be cleared on error");
    }

    [Fact]
    public async Task LoadAsync_ErrorThenSuccess_ClearsErrorOnSuccess()
    {
        var service = new DashboardDataService(_mockLogger.Object);

        // First: error
        await service.LoadAsync(Path.Combine(_tempDir, "nope.json"));
        service.IsError.Should().BeTrue();

        // Second: success
        var validPath = Path.Combine(_tempDir, "ok.json");
        await File.WriteAllTextAsync(validPath, BuildJson(title: "Recovered"));
        await service.LoadAsync(validPath);
        service.IsError.Should().BeFalse();
        service.ErrorMessage.Should().BeNull();
        service.Data!.Title.Should().Be("Recovered");
    }
}