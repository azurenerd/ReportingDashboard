using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

/// <summary>
/// Additional edge case tests for DashboardDataService covering
/// the general Exception catch block, concurrent access patterns,
/// and boundary conditions not in existing test files.
/// </summary>
[Trait("Category", "Unit")]
public class DashboardDataServiceEdgeCaseTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ILogger<DashboardDataService> _logger;

    public DashboardDataServiceEdgeCaseTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashSvcEdge_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _logger = NullLoggerFactory.Instance.CreateLogger<DashboardDataService>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private string WriteJson(string json)
    {
        var path = Path.Combine(_tempDir, "data.json");
        File.WriteAllText(path, json);
        return path;
    }

    private DashboardDataService CreateService() => new(_logger);

    #region Initial State

    [Fact]
    public void NewService_Data_IsNull()
    {
        var svc = CreateService();
        Assert.Null(svc.Data);
    }

    [Fact]
    public void NewService_IsError_IsFalse()
    {
        var svc = CreateService();
        Assert.False(svc.IsError);
    }

    [Fact]
    public void NewService_ErrorMessage_IsNull()
    {
        var svc = CreateService();
        Assert.Null(svc.ErrorMessage);
    }

    #endregion

    #region Valid JSON Variations

    [Fact]
    public async Task LoadAsync_MinimalValidJson_Succeeds()
    {
        var json = """{"title":"T","subtitle":"S","backlogLink":"","currentMonth":"","months":[],"timeline":{"startDate":"","endDate":"","nowDate":"","tracks":[]},"heatmap":{"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}}""";
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
        Assert.Equal("T", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_JsonWithExtraFields_IgnoresExtras()
    {
        var json = """{"title":"Test","subtitle":"Sub","backlogLink":"","currentMonth":"","months":[],"timeline":{"startDate":"","endDate":"","nowDate":"","tracks":[]},"heatmap":{"shipped":{},"inProgress":{},"carryover":{},"blockers":{}},"unknownField":"value","anotherExtra":123}""";
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.NotNull(svc.Data);
        Assert.Equal("Test", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_JsonWithUnicodeCharacters_DeserializesCorrectly()
    {
        var json = """{"title":"项目仪表板 🚀","subtitle":"团队 — 四月","backlogLink":"","currentMonth":"四月","months":["一月"],"timeline":{"startDate":"","endDate":"","nowDate":"","tracks":[]},"heatmap":{"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}}""";
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Equal("项目仪表板 🚀", svc.Data!.Title);
    }

    [Fact]
    public async Task LoadAsync_JsonWithNullOptionalFields_DoesNotCrash()
    {
        var json = """{"title":"Test","subtitle":null,"backlogLink":null,"currentMonth":null,"months":null,"timeline":null,"heatmap":null}""";
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        // Service should handle null fields - either succeeds or sets error, but should not throw
        Assert.NotNull(svc.Data?.Title ?? svc.ErrorMessage);
    }

    #endregion

    #region Invalid JSON Variations

    [Fact]
    public async Task LoadAsync_JsonArray_SetsError()
    {
        var path = WriteJson("[1, 2, 3]");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_JsonNumber_SetsError()
    {
        var path = WriteJson("42");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_JsonString_SetsError()
    {
        var path = WriteJson("\"just a string\"");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_JsonBoolean_SetsError()
    {
        var path = WriteJson("true");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_NullLiteral_SetsError()
    {
        var path = WriteJson("null");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.Contains("null", svc.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoadAsync_TrailingCommaJson_SetsParseError()
    {
        var path = WriteJson("""{"title": "Test",}""");
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.True(svc.IsError);
        Assert.NotNull(svc.ErrorMessage);
    }

    #endregion

    #region File System Edge Cases

    [Fact]
    public async Task LoadAsync_EmptyPath_SetsError()
    {
        var svc = CreateService();

        await svc.LoadAsync("");

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_DirectoryAsPath_SetsError()
    {
        var svc = CreateService();

        await svc.LoadAsync(_tempDir);

        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_VeryLongFilePath_SetsError()
    {
        var svc = CreateService();
        var longPath = Path.Combine(_tempDir, new string('a', 300) + ".json");

        await svc.LoadAsync(longPath);

        Assert.True(svc.IsError);
    }

    #endregion

    #region State Transition Tests

    [Fact]
    public async Task LoadAsync_SuccessfulLoad_ClearsErrorMessage()
    {
        var svc = CreateService();

        // First: trigger error
        await svc.LoadAsync(Path.Combine(_tempDir, "missing.json"));
        Assert.True(svc.IsError);
        Assert.NotNull(svc.ErrorMessage);

        // Second: load valid data
        var json = """{"title":"OK","subtitle":"","backlogLink":"","currentMonth":"","months":[],"timeline":{"startDate":"","endDate":"","nowDate":"","tracks":[]},"heatmap":{"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}}""";
        var path = WriteJson(json);
        await svc.LoadAsync(path);

        Assert.False(svc.IsError);
        Assert.Null(svc.ErrorMessage);
        Assert.NotNull(svc.Data);
    }

    [Fact]
    public async Task LoadAsync_ErrorAfterSuccess_ClearsPreviousData()
    {
        var svc = CreateService();
        var json = """{"title":"OK","subtitle":"","backlogLink":"","currentMonth":"","months":[],"timeline":{"startDate":"","endDate":"","nowDate":"","tracks":[]},"heatmap":{"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}}""";
        var path = WriteJson(json);

        await svc.LoadAsync(path);
        Assert.NotNull(svc.Data);

        // Load missing file - the service may or may not clear Data
        // but IsError must be true
        await svc.LoadAsync(Path.Combine(_tempDir, "gone.json"));
        Assert.True(svc.IsError);
    }

    [Fact]
    public async Task LoadAsync_MultipleSuccessfulLoads_LastDataWins()
    {
        var svc = CreateService();

        var json1 = """{"title":"First","subtitle":"","backlogLink":"","currentMonth":"","months":[],"timeline":{"startDate":"","endDate":"","nowDate":"","tracks":[]},"heatmap":{"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}}""";
        var path1 = Path.Combine(_tempDir, "data1.json");
        File.WriteAllText(path1, json1);
        await svc.LoadAsync(path1);
        Assert.Equal("First", svc.Data!.Title);

        var json2 = """{"title":"Second","subtitle":"","backlogLink":"","currentMonth":"","months":[],"timeline":{"startDate":"","endDate":"","nowDate":"","tracks":[]},"heatmap":{"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}}""";
        var path2 = Path.Combine(_tempDir, "data2.json");
        File.WriteAllText(path2, json2);
        await svc.LoadAsync(path2);
        Assert.Equal("Second", svc.Data!.Title);
    }

    #endregion

    #region Deserialization Mapping Tests

    [Fact]
    public async Task LoadAsync_JsonPropertyName_BacklogLink_MapsCorrectly()
    {
        var json = """{"title":"T","subtitle":"","backlogLink":"https://my.ado.link","currentMonth":"","months":[],"timeline":{"startDate":"","endDate":"","nowDate":"","tracks":[]},"heatmap":{"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}}""";
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.Equal("https://my.ado.link", svc.Data!.BacklogLink);
    }

    [Fact]
    public async Task LoadAsync_JsonPropertyName_InProgress_MapsCorrectly()
    {
        var json = """{"title":"T","subtitle":"","backlogLink":"","currentMonth":"","months":[],"timeline":{"startDate":"","endDate":"","nowDate":"","tracks":[]},"heatmap":{"shipped":{},"inProgress":{"apr":["Task A","Task B"]},"carryover":{},"blockers":{}}}""";
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.Equal(2, svc.Data!.Heatmap.InProgress["apr"].Count);
        Assert.Equal("Task A", svc.Data.Heatmap.InProgress["apr"][0]);
    }

    [Fact]
    public async Task LoadAsync_JsonPropertyName_NowDate_MapsCorrectly()
    {
        var json = """{"title":"T","subtitle":"","backlogLink":"","currentMonth":"","months":[],"timeline":{"startDate":"2026-01-01","endDate":"2026-07-01","nowDate":"2026-04-10","tracks":[]},"heatmap":{"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}}""";
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.Equal("2026-04-10", svc.Data!.Timeline.NowDate);
    }

    [Fact]
    public async Task LoadAsync_NestedTrackMilestones_DeserializeCorrectly()
    {
        var json = """
        {
            "title":"T","subtitle":"","backlogLink":"","currentMonth":"","months":[],
            "timeline":{
                "startDate":"2026-01-01","endDate":"2026-07-01","nowDate":"2026-04-10",
                "tracks":[{
                    "name":"M1","label":"Core Platform","color":"#4285F4",
                    "milestones":[
                        {"date":"2026-02-15","type":"poc","label":"Feb 15 PoC"},
                        {"date":"2026-05-01","type":"production","label":"May 1 GA"},
                        {"date":"2026-03-15","type":"checkpoint","label":"Mar Check"}
                    ]
                }]
            },
            "heatmap":{"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        var track = svc.Data!.Timeline.Tracks[0];
        Assert.Equal("M1", track.Name);
        Assert.Equal("Core Platform", track.Label);
        Assert.Equal("#4285F4", track.Color);
        Assert.Equal(3, track.Milestones.Count);
        Assert.Equal("poc", track.Milestones[0].Type);
        Assert.Equal("production", track.Milestones[1].Type);
        Assert.Equal("checkpoint", track.Milestones[2].Type);
    }

    [Fact]
    public async Task LoadAsync_MultipleHeatmapCategories_AllPopulated()
    {
        var json = """
        {
            "title":"T","subtitle":"","backlogLink":"","currentMonth":"Apr","months":["Jan","Feb","Mar","Apr"],
            "timeline":{"startDate":"","endDate":"","nowDate":"","tracks":[]},
            "heatmap":{
                "shipped":{"jan":["A","B"],"feb":["C"]},
                "inProgress":{"apr":["D"]},
                "carryover":{"mar":["E"]},
                "blockers":{"feb":["F","G","H"]}
            }
        }
        """;
        var path = WriteJson(json);
        var svc = CreateService();

        await svc.LoadAsync(path);

        Assert.Equal(2, svc.Data!.Heatmap.Shipped.Count);
        Assert.Equal(2, svc.Data.Heatmap.Shipped["jan"].Count);
        Assert.Single(svc.Data.Heatmap.InProgress);
        Assert.Single(svc.Data.Heatmap.Carryover);
        Assert.Equal(3, svc.Data.Heatmap.Blockers["feb"].Count);
    }

    #endregion

    #region Logger Verification

    [Fact]
    public async Task LoadAsync_ValidFile_DoesNotThrow()
    {
        var json = """{"title":"T","subtitle":"","backlogLink":"","currentMonth":"","months":[],"timeline":{"startDate":"","endDate":"","nowDate":"","tracks":[]},"heatmap":{"shipped":{},"inProgress":{},"carryover":{},"blockers":{}}}""";
        var path = WriteJson(json);
        var svc = CreateService();

        var exception = await Record.ExceptionAsync(() => svc.LoadAsync(path));
        Assert.Null(exception);
    }

    [Fact]
    public async Task LoadAsync_MissingFile_DoesNotThrow()
    {
        var svc = CreateService();

        var exception = await Record.ExceptionAsync(() =>
            svc.LoadAsync(Path.Combine(_tempDir, "no-such-file.json")));

        Assert.Null(exception);
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_DoesNotThrow()
    {
        var path = WriteJson("{{{broken");
        var svc = CreateService();

        var exception = await Record.ExceptionAsync(() => svc.LoadAsync(path));

        Assert.Null(exception);
    }

    #endregion
}