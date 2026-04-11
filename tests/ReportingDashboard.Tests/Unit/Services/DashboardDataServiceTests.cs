using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Services;

[Trait("Category", "Unit")]
public class DashboardDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _dataDir;
    private readonly string _dataJsonPath;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public DashboardDataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DashboardTests_{Guid.NewGuid():N}");
        _dataDir = Path.Combine(_tempDir, "Data");
        Directory.CreateDirectory(_dataDir);
        _dataJsonPath = Path.Combine(_dataDir, "data.json");

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.ContentRootPath).Returns(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private DashboardDataService CreateService() => new(_envMock.Object);

    private void WriteJson(object data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_dataJsonPath, json);
    }

    private void WriteRawJson(string json)
    {
        File.WriteAllText(_dataJsonPath, json);
    }

    private static object BuildValidData(
        string? title = "Test Project",
        string? subtitle = "Test Subtitle",
        string? backlogUrl = "https://example.com",
        string? currentMonth = "Apr",
        List<string>? timelineMonths = null,
        double nowPosition = 0.5,
        List<object>? tracks = null,
        List<string>? heatmapMonths = null,
        List<object>? categories = null)
    {
        return new
        {
            project = new
            {
                title,
                subtitle,
                backlogUrl,
                currentMonth
            },
            timeline = new
            {
                months = timelineMonths ?? new List<string> { "Jan", "Feb", "Mar" },
                nowPosition
            },
            tracks = tracks ?? new List<object>
            {
                new
                {
                    id = "m1",
                    label = "Track 1",
                    color = "#FF0000",
                    milestones = new List<object>
                    {
                        new { date = "2024-03-15", type = "checkpoint", position = 0.4, label = "CP1" }
                    }
                }
            },
            heatmap = new
            {
                months = heatmapMonths ?? new List<string> { "Jan", "Feb", "Mar" },
                categories = categories ?? new List<object>
                {
                    new
                    {
                        name = "Shipped",
                        cssClass = "ship",
                        emoji = "✅",
                        items = new Dictionary<string, List<string>>
                        {
                            { "Jan", new List<string> { "Item A" } }
                        }
                    }
                }
            }
        };
    }

    // ---- File Not Found ----

    [Fact]
    public void Load_FileNotFound_ReturnsError()
    {
        if (File.Exists(_dataJsonPath)) File.Delete(_dataJsonPath);
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("data.json not found at:");
        error.Should().Contain(_dataJsonPath);
    }

    // ---- Invalid JSON ----

    [Fact]
    public void Load_MalformedJson_ReturnsParseError()
    {
        WriteRawJson("{ \"bad json }");
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("Invalid JSON in data.json:");
    }

    [Fact]
    public void Load_EmptyFile_ReturnsError()
    {
        WriteRawJson("");
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Load_NullJsonRoot_ReturnsError()
    {
        WriteRawJson("null");
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("deserialized to null");
    }

    // ---- Valid Data ----

    [Fact]
    public void Load_ValidData_ReturnsDataWithNoError()
    {
        WriteJson(BuildValidData());
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data.Should().NotBeNull();
        data!.Project.Title.Should().Be("Test Project");
        data.Project.Subtitle.Should().Be("Test Subtitle");
        data.Timeline.NowPosition.Should().Be(0.5);
        data.Tracks.Should().HaveCount(1);
        data.Heatmap.Categories.Should().HaveCount(1);
    }

    [Fact]
    public void Load_ReReadsFileEachCall_ReflectsChanges()
    {
        WriteJson(BuildValidData(title: "First Title"));
        var service = CreateService();

        var (data1, _) = service.Load();
        data1!.Project.Title.Should().Be("First Title");

        WriteJson(BuildValidData(title: "Second Title"));

        var (data2, _) = service.Load();
        data2!.Project.Title.Should().Be("Second Title");
    }

    // ---- Project Validation ----

    [Fact]
    public void Load_MissingProjectTitle_ReturnsValidationError()
    {
        WriteJson(BuildValidData(title: ""));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Be("Missing required field: project.title");
    }

    [Fact]
    public void Load_WhitespaceProjectTitle_ReturnsValidationError()
    {
        WriteJson(BuildValidData(title: "   "));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Be("Missing required field: project.title");
    }

    [Fact]
    public void Load_MissingProjectSubtitle_ReturnsValidationError()
    {
        WriteJson(BuildValidData(subtitle: ""));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Be("Missing required field: project.subtitle");
    }

    [Fact]
    public void Load_MissingCurrentMonth_ReturnsValidationError()
    {
        WriteJson(BuildValidData(currentMonth: ""));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Be("Missing required field: project.currentMonth");
    }

    // ---- Timeline Validation ----

    [Fact]
    public void Load_EmptyTimelineMonths_ReturnsValidationError()
    {
        WriteJson(BuildValidData(timelineMonths: new List<string>()));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("timeline.months");
        error.Should().Contain("at least 1 element");
    }

    [Fact]
    public void Load_NowPositionBelowZero_ReturnsValidationError()
    {
        WriteJson(BuildValidData(nowPosition: -0.1));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("timeline.nowPosition must be between 0.0 and 1.0");
    }

    [Fact]
    public void Load_NowPositionAboveOne_ReturnsValidationError()
    {
        WriteJson(BuildValidData(nowPosition: 1.1));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("timeline.nowPosition must be between 0.0 and 1.0");
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void Load_NowPositionAtBoundary_Succeeds(double nowPosition)
    {
        WriteJson(BuildValidData(nowPosition: nowPosition));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data.Should().NotBeNull();
        data!.Timeline.NowPosition.Should().Be(nowPosition);
    }

    // ---- Tracks Validation ----

    [Fact]
    public void Load_TrackMissingId_ReturnsValidationError()
    {
        var tracks = new List<object>
        {
            new { id = "", label = "Track 1", color = "#FF0000", milestones = new List<object>() }
        };
        WriteJson(BuildValidData(tracks: tracks));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Be("Missing required field: tracks[0].id");
    }

    [Fact]
    public void Load_TrackMissingLabel_ReturnsValidationError()
    {
        var tracks = new List<object>
        {
            new { id = "m1", label = "", color = "#FF0000", milestones = new List<object>() }
        };
        WriteJson(BuildValidData(tracks: tracks));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Be("Missing required field: tracks[0].label");
    }

    [Fact]
    public void Load_TrackMissingColor_ReturnsValidationError()
    {
        var tracks = new List<object>
        {
            new { id = "m1", label = "Track 1", color = "", milestones = new List<object>() }
        };
        WriteJson(BuildValidData(tracks: tracks));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Be("Missing required field: tracks[0].color");
    }

    [Fact]
    public void Load_EmptyTracksArray_Succeeds()
    {
        WriteJson(BuildValidData(tracks: new List<object>()));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data.Should().NotBeNull();
        data!.Tracks.Should().BeEmpty();
    }

    // ---- Milestone Validation ----

    [Fact]
    public void Load_MilestoneInvalidType_ReturnsValidationError()
    {
        var tracks = new List<object>
        {
            new
            {
                id = "m1", label = "Track 1", color = "#FF0000",
                milestones = new List<object>
                {
                    new { date = "2024-03-15", type = "invalid", position = 0.5 }
                }
            }
        };
        WriteJson(BuildValidData(tracks: tracks));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("Invalid milestone type: 'invalid'");
        error.Should().Contain("tracks[0].milestones[0]");
    }

    [Theory]
    [InlineData("checkpoint")]
    [InlineData("poc")]
    [InlineData("production")]
    public void Load_MilestoneValidType_Succeeds(string type)
    {
        var tracks = new List<object>
        {
            new
            {
                id = "m1", label = "Track 1", color = "#FF0000",
                milestones = new List<object>
                {
                    new { date = "2024-03-15", type, position = 0.5 }
                }
            }
        };
        WriteJson(BuildValidData(tracks: tracks));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data.Should().NotBeNull();
    }

    [Fact]
    public void Load_MilestonePositionBelowZero_ReturnsValidationError()
    {
        var tracks = new List<object>
        {
            new
            {
                id = "m1", label = "Track 1", color = "#FF0000",
                milestones = new List<object>
                {
                    new { date = "2024-03-15", type = "checkpoint", position = -0.01 }
                }
            }
        };
        WriteJson(BuildValidData(tracks: tracks));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("milestones[0].position must be between 0.0 and 1.0");
    }

    [Fact]
    public void Load_MilestonePositionAboveOne_ReturnsValidationError()
    {
        var tracks = new List<object>
        {
            new
            {
                id = "m1", label = "Track 1", color = "#FF0000",
                milestones = new List<object>
                {
                    new { date = "2024-03-15", type = "checkpoint", position = 1.01 }
                }
            }
        };
        WriteJson(BuildValidData(tracks: tracks));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("milestones[0].position must be between 0.0 and 1.0");
    }

    [Fact]
    public void Load_MilestoneMissingDate_ReturnsValidationError()
    {
        var tracks = new List<object>
        {
            new
            {
                id = "m1", label = "Track 1", color = "#FF0000",
                milestones = new List<object>
                {
                    new { date = "", type = "checkpoint", position = 0.5 }
                }
            }
        };
        WriteJson(BuildValidData(tracks: tracks));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("tracks[0].milestones[0].date");
    }

    [Fact]
    public void Load_MilestoneMissingType_ReturnsValidationError()
    {
        var tracks = new List<object>
        {
            new
            {
                id = "m1", label = "Track 1", color = "#FF0000",
                milestones = new List<object>
                {
                    new { date = "2024-03-15", type = "", position = 0.5 }
                }
            }
        };
        WriteJson(BuildValidData(tracks: tracks));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("tracks[0].milestones[0].type");
    }

    [Fact]
    public void Load_MilestoneOptionalLabelNull_Succeeds()
    {
        var tracks = new List<object>
        {
            new
            {
                id = "m1", label = "Track 1", color = "#FF0000",
                milestones = new List<object>
                {
                    new { date = "2024-03-15", type = "checkpoint", position = 0.5 }
                }
            }
        };
        WriteJson(BuildValidData(tracks: tracks));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data!.Tracks[0].Milestones[0].Label.Should().BeNull();
    }

    // ---- Heatmap Validation ----

    [Fact]
    public void Load_HeatmapEmptyMonths_ReturnsValidationError()
    {
        WriteJson(BuildValidData(heatmapMonths: new List<string>()));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("heatmap.months");
    }

    [Fact]
    public void Load_CategoryMissingName_ReturnsValidationError()
    {
        var categories = new List<object>
        {
            new
            {
                name = "",
                cssClass = "ship",
                emoji = "✅",
                items = new Dictionary<string, List<string>>()
            }
        };
        WriteJson(BuildValidData(categories: categories));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Be("Missing required field: heatmap.categories[0].name");
    }

    [Fact]
    public void Load_CategoryMissingCssClass_ReturnsValidationError()
    {
        var categories = new List<object>
        {
            new
            {
                name = "Shipped",
                cssClass = "",
                emoji = "✅",
                items = new Dictionary<string, List<string>>()
            }
        };
        WriteJson(BuildValidData(categories: categories));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Be("Missing required field: heatmap.categories[0].cssClass");
    }

    [Fact]
    public void Load_CategoryInvalidCssClass_ReturnsValidationError()
    {
        var categories = new List<object>
        {
            new
            {
                name = "Shipped",
                cssClass = "invalid",
                emoji = "✅",
                items = new Dictionary<string, List<string>>()
            }
        };
        WriteJson(BuildValidData(categories: categories));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("Invalid cssClass: 'invalid'");
        error.Should().Contain("heatmap.categories[0]");
    }

    [Theory]
    [InlineData("ship")]
    [InlineData("prog")]
    [InlineData("carry")]
    [InlineData("block")]
    public void Load_CategoryValidCssClass_Succeeds(string cssClass)
    {
        var categories = new List<object>
        {
            new
            {
                name = "Test",
                cssClass,
                emoji = "✅",
                items = new Dictionary<string, List<string>>()
            }
        };
        WriteJson(BuildValidData(categories: categories));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data.Should().NotBeNull();
    }

    [Fact]
    public void Load_CategoryMissingEmoji_ReturnsValidationError()
    {
        var categories = new List<object>
        {
            new
            {
                name = "Shipped",
                cssClass = "ship",
                emoji = "",
                items = new Dictionary<string, List<string>>()
            }
        };
        WriteJson(BuildValidData(categories: categories));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Be("Missing required field: heatmap.categories[0].emoji");
    }

    // ---- Multiple Tracks / Categories Indexing ----

    [Fact]
    public void Load_SecondTrackInvalid_ReportsCorrectIndex()
    {
        var tracks = new List<object>
        {
            new { id = "m1", label = "Track 1", color = "#FF0000", milestones = new List<object>() },
            new { id = "", label = "Track 2", color = "#00FF00", milestones = new List<object>() }
        };
        WriteJson(BuildValidData(tracks: tracks));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Be("Missing required field: tracks[1].id");
    }

    [Fact]
    public void Load_SecondCategoryInvalid_ReportsCorrectIndex()
    {
        var categories = new List<object>
        {
            new
            {
                name = "Shipped", cssClass = "ship", emoji = "✅",
                items = new Dictionary<string, List<string>>()
            },
            new
            {
                name = "", cssClass = "prog", emoji = "🔄",
                items = new Dictionary<string, List<string>>()
            }
        };
        WriteJson(BuildValidData(categories: categories));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Be("Missing required field: heatmap.categories[1].name");
    }

    [Fact]
    public void Load_SecondMilestoneInvalid_ReportsCorrectIndex()
    {
        var tracks = new List<object>
        {
            new
            {
                id = "m1", label = "Track 1", color = "#FF0000",
                milestones = new List<object>
                {
                    new { date = "2024-03-15", type = "checkpoint", position = 0.4 },
                    new { date = "2024-04-15", type = "badtype", position = 0.6 }
                }
            }
        };
        WriteJson(BuildValidData(tracks: tracks));
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().Contain("tracks[0].milestones[1]");
    }

    // ---- Data Integrity / Deserialization ----

    [Fact]
    public void Load_ValidData_DeserializesAllFieldsCorrectly()
    {
        var tracks = new List<object>
        {
            new
            {
                id = "m1", label = "Track Alpha", color = "#123ABC",
                milestones = new List<object>
                {
                    new { date = "2024-06-01", type = "poc", position = 0.75, label = "PoC Ready" }
                }
            }
        };
        var categories = new List<object>
        {
            new
            {
                name = "Blockers", cssClass = "block", emoji = "🚫",
                items = new Dictionary<string, List<string>>
                {
                    { "Feb", new List<string> { "Bug X", "Bug Y" } },
                    { "Mar", new List<string> { "Dep Z" } }
                }
            }
        };
        WriteJson(BuildValidData(
            title: "Alpha Project",
            subtitle: "Org > Team > Apr 2024",
            backlogUrl: "https://dev.azure.com/backlog",
            currentMonth: "Apr",
            timelineMonths: new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
            nowPosition: 0.65,
            tracks: tracks,
            heatmapMonths: new List<string> { "Jan", "Feb", "Mar", "Apr" },
            categories: categories
        ));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data.Should().NotBeNull();

        data!.Project.Title.Should().Be("Alpha Project");
        data.Project.Subtitle.Should().Be("Org > Team > Apr 2024");
        data.Project.BacklogUrl.Should().Be("https://dev.azure.com/backlog");
        data.Project.CurrentMonth.Should().Be("Apr");

        data.Timeline.Months.Should().HaveCount(6);
        data.Timeline.NowPosition.Should().Be(0.65);

        data.Tracks.Should().HaveCount(1);
        data.Tracks[0].Id.Should().Be("m1");
        data.Tracks[0].Label.Should().Be("Track Alpha");
        data.Tracks[0].Color.Should().Be("#123ABC");
        data.Tracks[0].Milestones.Should().HaveCount(1);
        data.Tracks[0].Milestones[0].Date.Should().Be("2024-06-01");
        data.Tracks[0].Milestones[0].Type.Should().Be("poc");
        data.Tracks[0].Milestones[0].Position.Should().Be(0.75);
        data.Tracks[0].Milestones[0].Label.Should().Be("PoC Ready");

        data.Heatmap.Months.Should().HaveCount(4);
        data.Heatmap.Categories.Should().HaveCount(1);
        data.Heatmap.Categories[0].Name.Should().Be("Blockers");
        data.Heatmap.Categories[0].CssClass.Should().Be("block");
        data.Heatmap.Categories[0].Items.Should().ContainKey("Feb");
        data.Heatmap.Categories[0].Items["Feb"].Should().BeEquivalentTo(new[] { "Bug X", "Bug Y" });
        data.Heatmap.Categories[0].Items["Mar"].Should().BeEquivalentTo(new[] { "Dep Z" });
    }

    [Fact]
    public void Load_MultipleTracks_AllDeserialized()
    {
        var tracks = new List<object>
        {
            new { id = "m1", label = "Track 1", color = "#FF0000", milestones = new List<object>() },
            new { id = "m2", label = "Track 2", color = "#00FF00", milestones = new List<object>() },
            new { id = "m3", label = "Track 3", color = "#0000FF", milestones = new List<object>() }
        };
        WriteJson(BuildValidData(tracks: tracks));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data!.Tracks.Should().HaveCount(3);
        data.Tracks[2].Id.Should().Be("m3");
    }

    [Fact]
    public void Load_MultipleCategories_AllDeserialized()
    {
        var categories = new List<object>
        {
            new { name = "Shipped", cssClass = "ship", emoji = "✅", items = new Dictionary<string, List<string>>() },
            new { name = "In Progress", cssClass = "prog", emoji = "🔄", items = new Dictionary<string, List<string>>() },
            new { name = "Carryover", cssClass = "carry", emoji = "📦", items = new Dictionary<string, List<string>>() },
            new { name = "Blockers", cssClass = "block", emoji = "🚫", items = new Dictionary<string, List<string>>() }
        };
        WriteJson(BuildValidData(categories: categories));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data!.Heatmap.Categories.Should().HaveCount(4);
    }

    // ---- Never Throws ----

    [Fact]
    public void Load_NeverThrows_OnMissingFile()
    {
        if (File.Exists(_dataJsonPath)) File.Delete(_dataJsonPath);
        var service = CreateService();

        var act = () => service.Load();

        act.Should().NotThrow();
    }

    [Fact]
    public void Load_NeverThrows_OnMalformedJson()
    {
        WriteRawJson("{{{");
        var service = CreateService();

        var act = () => service.Load();

        act.Should().NotThrow();
    }

    [Fact]
    public void Load_NeverThrows_OnInvalidData()
    {
        WriteJson(new { project = new { title = "" } });
        var service = CreateService();

        var act = () => service.Load();

        act.Should().NotThrow();
    }

    // ---- Edge Cases ----

    [Fact]
    public void Load_EmptyJsonObject_ReturnsValidationError()
    {
        WriteRawJson("{}");
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Load_ValidJsonArray_ReturnsError()
    {
        WriteRawJson("[1,2,3]");
        var service = CreateService();

        var (data, error) = service.Load();

        data.Should().BeNull();
        error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Load_TrackWithNullMilestones_Succeeds()
    {
        // When milestones is present but null/omitted, validation should still pass
        // because the code checks `if (track.Milestones is not null)`
        WriteRawJson(JsonSerializer.Serialize(new
        {
            project = new { title = "T", subtitle = "S", backlogUrl = "", currentMonth = "Jan" },
            timeline = new { months = new[] { "Jan" }, nowPosition = 0.5 },
            tracks = new[]
            {
                new { id = "m1", label = "Track", color = "#000" }
                // milestones omitted
            },
            heatmap = new
            {
                months = new[] { "Jan" },
                categories = new[]
                {
                    new { name = "X", cssClass = "ship", emoji = "✅", items = new Dictionary<string, List<string>>() }
                }
            }
        }));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data.Should().NotBeNull();
    }

    [Fact]
    public void Load_CategoryWithEmptyItems_Succeeds()
    {
        var categories = new List<object>
        {
            new
            {
                name = "Shipped", cssClass = "ship", emoji = "✅",
                items = new Dictionary<string, List<string>>()
            }
        };
        WriteJson(BuildValidData(categories: categories));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
        data!.Heatmap.Categories[0].Items.Should().BeEmpty();
    }

    [Fact]
    public void Load_MilestonePositionExactlyZero_Succeeds()
    {
        var tracks = new List<object>
        {
            new
            {
                id = "m1", label = "Track 1", color = "#FF0000",
                milestones = new List<object>
                {
                    new { date = "2024-01-01", type = "checkpoint", position = 0.0 }
                }
            }
        };
        WriteJson(BuildValidData(tracks: tracks));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
    }

    [Fact]
    public void Load_MilestonePositionExactlyOne_Succeeds()
    {
        var tracks = new List<object>
        {
            new
            {
                id = "m1", label = "Track 1", color = "#FF0000",
                milestones = new List<object>
                {
                    new { date = "2024-12-31", type = "production", position = 1.0 }
                }
            }
        };
        WriteJson(BuildValidData(tracks: tracks));
        var service = CreateService();

        var (data, error) = service.Load();

        error.Should().BeNull();
    }

    // ---- Constructor / Path ----

    [Fact]
    public void Constructor_SetsPathFromEnvironment()
    {
        var service = CreateService();

        // Verify by checking Load returns file-not-found with expected path
        if (File.Exists(_dataJsonPath)) File.Delete(_dataJsonPath);
        var (_, error) = service.Load();

        error.Should().Contain(Path.Combine(_tempDir, "Data", "data.json"));
    }
}