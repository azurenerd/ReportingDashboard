using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class DataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _wwwrootDir;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public DataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "DataServiceTests_" + Guid.NewGuid().ToString("N"));
        _wwwrootDir = Path.Combine(_tempDir, "wwwroot");
        Directory.CreateDirectory(_wwwrootDir);

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_wwwrootDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }

    private static string CreateValidJson(
        string title = "Test Dashboard",
        string? nowDateOverride = null,
        int schemaVersion = 1)
    {
        var data = new
        {
            schemaVersion,
            title,
            subtitle = "Test Subtitle",
            backlogUrl = "https://example.com",
            nowDateOverride,
            timeline = new
            {
                startDate = "2026-01-01",
                endDate = "2026-07-31",
                workstreams = new[]
                {
                    new
                    {
                        id = "ws1",
                        name = "Workstream 1",
                        color = "#0078D4",
                        milestones = new[]
                        {
                            new { label = "M1", date = "2026-03-01", type = "poc", labelPosition = (string?)null }
                        }
                    }
                }
            },
            heatmap = new
            {
                monthColumns = new[] { "Jan", "Feb", "Mar" },
                categories = new[]
                {
                    new
                    {
                        name = "Shipped",
                        emoji = "✅",
                        cssClass = "ship",
                        months = new[]
                        {
                            new { month = "Jan", items = new[] { "Item A" } },
                            new { month = "Feb", items = Array.Empty<string>() },
                            new { month = "Mar", items = new[] { "Item B", "Item C" } }
                        }
                    }
                }
            }
        };
        return JsonSerializer.Serialize(data);
    }

    private void WriteDataJson(string content)
    {
        File.WriteAllText(Path.Combine(_wwwrootDir, "data.json"), content);
    }

    // ─── Valid JSON loading ───

    [Fact]
    public void ValidJson_LoadsCorrectly()
    {
        WriteDataJson(CreateValidJson("My Project"));

        using var service = new DataService(_envMock.Object);

        service.GetData().Should().NotBeNull();
        service.GetData()!.Title.Should().Be("My Project");
        service.GetData()!.SchemaVersion.Should().Be(1);
        service.GetData()!.Timeline.Workstreams.Should().HaveCount(1);
        service.GetError().Should().BeNull();
    }

    [Fact]
    public void ValidJson_DeserializesAllModelFields()
    {
        WriteDataJson(CreateValidJson("Full Model Test"));

        using var service = new DataService(_envMock.Object);
        var data = service.GetData()!;

        data.Title.Should().Be("Full Model Test");
        data.Subtitle.Should().Be("Test Subtitle");
        data.BacklogUrl.Should().Be("https://example.com");
        data.Timeline.StartDate.Should().Be("2026-01-01");
        data.Timeline.EndDate.Should().Be("2026-07-31");
        data.Timeline.Workstreams[0].Id.Should().Be("ws1");
        data.Timeline.Workstreams[0].Name.Should().Be("Workstream 1");
        data.Timeline.Workstreams[0].Color.Should().Be("#0078D4");
        data.Timeline.Workstreams[0].Milestones[0].Label.Should().Be("M1");
        data.Timeline.Workstreams[0].Milestones[0].Date.Should().Be("2026-03-01");
        data.Timeline.Workstreams[0].Milestones[0].Type.Should().Be("poc");
        data.Timeline.Workstreams[0].Milestones[0].LabelPosition.Should().BeNull();
        data.Heatmap.MonthColumns.Should().BeEquivalentTo(new[] { "Jan", "Feb", "Mar" });
        data.Heatmap.Categories[0].Name.Should().Be("Shipped");
        data.Heatmap.Categories[0].Emoji.Should().Be("✅");
        data.Heatmap.Categories[0].CssClass.Should().Be("ship");
        data.Heatmap.Categories[0].Months.Should().HaveCount(3);
        data.Heatmap.Categories[0].Months[0].Items.Should().BeEquivalentTo(new[] { "Item A" });
        data.Heatmap.Categories[0].Months[1].Items.Should().BeEmpty();
    }

    // ─── Missing file ───

    [Fact]
    public void MissingFile_ReturnsError()
    {
        using var service = new DataService(_envMock.Object);

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("No data.json found");
    }

    [Fact]
    public void MissingFile_ErrorContainsGuidance()
    {
        using var service = new DataService(_envMock.Object);

        service.GetError().Should().Contain("wwwroot");
    }

    // ─── Malformed JSON ───

    [Fact]
    public void MalformedJson_ReturnsError()
    {
        WriteDataJson("{ this is not valid json!!!");

        using var service = new DataService(_envMock.Object);

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("invalid JSON");
    }

    [Fact]
    public void EmptyFile_ReturnsError()
    {
        WriteDataJson("");

        using var service = new DataService(_envMock.Object);

        service.GetData().Should().BeNull();
        service.GetError().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void NullJsonRoot_ReturnsError()
    {
        WriteDataJson("null");

        using var service = new DataService(_envMock.Object);

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("invalid JSON");
    }

    // ─── Schema version validation ───

    [Fact]
    public void SchemaVersionMismatch_ReturnsError()
    {
        WriteDataJson(CreateValidJson(schemaVersion: 99));

        using var service = new DataService(_envMock.Object);

        service.GetData().Should().BeNull();
        service.GetError().Should().Contain("expected 1");
        service.GetError().Should().Contain("99");
    }

    [Fact]
    public void SchemaVersionZero_ReturnsError()
    {
        WriteDataJson(CreateValidJson(schemaVersion: 0));

        using var service = new DataService(_envMock.Object);

        service.GetError().Should().Contain("expected 1");
    }

    // ─── Effective date (nowDateOverride) ───

    [Fact]
    public void GetEffectiveDate_WithOverride_ReturnsParsedDate()
    {
        WriteDataJson(CreateValidJson(nowDateOverride: "2026-02-15"));

        using var service = new DataService(_envMock.Object);

        service.GetEffectiveDate().Should().Be(new DateOnly(2026, 2, 15));
    }

    [Fact]
    public void GetEffectiveDate_WithoutOverride_ReturnsToday()
    {
        WriteDataJson(CreateValidJson(nowDateOverride: null));

        using var service = new DataService(_envMock.Object);

        service.GetEffectiveDate().Should().Be(DateOnly.FromDateTime(DateTime.Today));
    }

    [Fact]
    public void GetEffectiveDate_WithInvalidOverride_FallsBackToToday()
    {
        var json = CreateValidJson().Replace("\"nowDateOverride\":null", "\"nowDateOverride\":\"not-a-date\"");
        WriteDataJson(json);

        using var service = new DataService(_envMock.Object);

        service.GetEffectiveDate().Should().Be(DateOnly.FromDateTime(DateTime.Today));
    }

    [Fact]
    public void GetEffectiveDate_WithEmptyOverride_FallsBackToToday()
    {
        var json = CreateValidJson().Replace("\"nowDateOverride\":null", "\"nowDateOverride\":\"\"");
        WriteDataJson(json);

        using var service = new DataService(_envMock.Object);

        service.GetEffectiveDate().Should().Be(DateOnly.FromDateTime(DateTime.Today));
    }

    // ─── Thread safety: concurrent reads do not throw ───

    [Fact]
    public void ConcurrentReads_DoNotThrow()
    {
        WriteDataJson(CreateValidJson());

        using var service = new DataService(_envMock.Object);

        var tasks = Enumerable.Range(0, 100).Select(i => Task.Run(() =>
        {
            service.GetData();
            service.GetError();
            service.GetEffectiveDate();
        }));

        Task.WaitAll(tasks.ToArray());
    }

    // ─── File change triggers reload ───

    [Fact]
    public async Task FileChange_TriggersReload()
    {
        WriteDataJson(CreateValidJson("Original Title"));

        using var service = new DataService(_envMock.Object);
        service.GetData()!.Title.Should().Be("Original Title");

        var tcs = new TaskCompletionSource<bool>();
        service.OnDataChanged += () => tcs.TrySetResult(true);

        // Modify the file
        WriteDataJson(CreateValidJson("Updated Title"));

        // Wait for debounced reload (500ms debounce + margin)
        var completed = await Task.WhenAny(tcs.Task, Task.Delay(3000));
        completed.Should().Be(tcs.Task, "OnDataChanged should fire after file change");

        service.GetData()!.Title.Should().Be("Updated Title");
        service.GetError().Should().BeNull();
    }

    [Fact]
    public async Task FileChange_MalformedJson_PreservesLastGoodData()
    {
        WriteDataJson(CreateValidJson("Good Data"));

        using var service = new DataService(_envMock.Object);
        service.GetData()!.Title.Should().Be("Good Data");

        var tcs = new TaskCompletionSource<bool>();
        service.OnDataChanged += () => tcs.TrySetResult(true);

        // Write malformed JSON
        WriteDataJson("{ broken json!!!");

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(3000));
        completed.Should().Be(tcs.Task, "OnDataChanged should fire even on error");

        // Last-known-good data preserved
        service.GetData()!.Title.Should().Be("Good Data");
        service.GetError().Should().Contain("invalid JSON");
    }

    [Fact]
    public async Task FileChange_FixedJson_ClearsError()
    {
        WriteDataJson(CreateValidJson("Good Data"));

        using var service = new DataService(_envMock.Object);

        // First: break it
        var tcs1 = new TaskCompletionSource<bool>();
        service.OnDataChanged += () => tcs1.TrySetResult(true);
        WriteDataJson("{ broken }");
        await Task.WhenAny(tcs1.Task, Task.Delay(3000));
        service.GetError().Should().NotBeNull();

        // Second: fix it
        var tcs2 = new TaskCompletionSource<bool>();
        service.OnDataChanged += () => tcs2.TrySetResult(true);
        WriteDataJson(CreateValidJson("Fixed Data"));
        await Task.WhenAny(tcs2.Task, Task.Delay(3000));

        service.GetData()!.Title.Should().Be("Fixed Data");
        service.GetError().Should().BeNull();
    }

    // ─── Missing required fields ───

    [Fact]
    public void MissingRequiredField_ReturnsError()
    {
        // JSON with missing required 'title' field
        var json = """{"schemaVersion":1,"subtitle":"x","backlogUrl":"x","timeline":{"startDate":"2026-01-01","endDate":"2026-07-01","workstreams":[]},"heatmap":{"monthColumns":[],"categories":[]}}""";
        WriteDataJson(json);

        using var service = new DataService(_envMock.Object);

        // System.Text.Json treats missing 'required' properties as a deserialization error
        // The service should catch and report it
        service.GetError().Should().NotBeNullOrWhiteSpace();
    }
}