using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

/// <summary>
/// Validates the actual wwwroot/data.json sample file against Project Atlas acceptance criteria.
/// These tests deserialize the real shipped data.json and assert specific field values,
/// covering gaps not addressed by the existing DashboardDataModelTests (which use synthetic JSON).
/// </summary>
[Trait("Category", "Unit")]
public class SampleDataJsonTests
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    private static DashboardData LoadSampleData()
    {
        // Walk up from bin/Debug/net8.0 to find the src project wwwroot
        var dir = AppContext.BaseDirectory;
        string? jsonPath = null;

        // Try relative path from test output to src project
        var candidate = Path.GetFullPath(Path.Combine(dir, "..", "..", "..", "..", "..", "src", "ReportingDashboard", "wwwroot", "data.json"));
        if (File.Exists(candidate))
            jsonPath = candidate;

        // Fallback: walk up looking for src folder
        if (jsonPath == null)
        {
            var current = new DirectoryInfo(dir);
            while (current != null)
            {
                var test = Path.Combine(current.FullName, "src", "ReportingDashboard", "wwwroot", "data.json");
                if (File.Exists(test))
                {
                    jsonPath = test;
                    break;
                }
                current = current.Parent;
            }
        }

        jsonPath.Should().NotBeNull("data.json must exist in src/ReportingDashboard/wwwroot/");
        var json = File.ReadAllText(jsonPath!);
        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);
        data.Should().NotBeNull("data.json must deserialize to DashboardData without errors");
        return data!;
    }

    [Fact]
    public void SampleData_HeaderFields_MatchAcceptanceCriteria()
    {
        var data = LoadSampleData();

        data.SchemaVersion.Should().Be(1);
        data.Title.Should().Be("Project Atlas Release Roadmap");
        // Validate subtitle structure without being sensitive to dash vs en-dash encoding
        data.Subtitle.Should().StartWith("Platform Engineering");
        data.Subtitle.Should().Contain("Atlas Workstream");
        data.Subtitle.Should().Contain("April 2026");
        data.BacklogUrl.Should().Be("https://dev.azure.com/contoso/Atlas/_backlogs");
        data.NowDateOverride.Should().BeNull();
    }

    [Fact]
    public void SampleData_TimelineWorkstreams_ThreeWithCorrectIdsAndColors()
    {
        var data = LoadSampleData();

        data.Timeline.StartDate.Should().Be("2026-01-01");
        data.Timeline.EndDate.Should().Be("2026-07-01");
        data.Timeline.Workstreams.Should().HaveCount(3);

        var ws = data.Timeline.Workstreams;
        ws[0].Id.Should().Be("M1");
        ws[0].Name.Should().Be("Chatbot & MS Role");
        ws[0].Color.Should().Be("#0078D4");

        ws[1].Id.Should().Be("M2");
        ws[1].Name.Should().Be("PDS & Data Inventory");
        ws[1].Color.Should().Be("#00897B");

        ws[2].Id.Should().Be("M3");
        ws[2].Name.Should().Be("Auto Review DFD");
        ws[2].Color.Should().Be("#546E7A");
    }

    [Fact]
    public void SampleData_AllFourMilestoneTypes_PresentAcrossWorkstreams()
    {
        var data = LoadSampleData();

        var allMilestones = data.Timeline.Workstreams.SelectMany(w => w.Milestones).ToList();
        allMilestones.Should().HaveCount(15, "3 workstreams x 5 milestones each");

        var types = allMilestones.Select(m => m.Type).Distinct().ToList();
        types.Should().Contain("start");
        types.Should().Contain("checkpoint");
        types.Should().Contain("poc");
        types.Should().Contain("production");

        // All dates within timeline range
        var rangeStart = DateOnly.Parse("2026-01-01");
        var rangeEnd = DateOnly.Parse("2026-07-01");
        foreach (var m in allMilestones)
        {
            var date = DateOnly.Parse(m.Date);
            date.Should().BeOnOrAfter(rangeStart);
            date.Should().BeOnOrBefore(rangeEnd);
        }

        // At least one explicit labelPosition per workstream
        foreach (var ws in data.Timeline.Workstreams)
        {
            ws.Milestones.Should().Contain(m => m.LabelPosition != null,
                $"workstream {ws.Id} should have at least one explicit labelPosition");
        }
    }

    [Fact]
    public void SampleData_HeatmapStructure_FourCategoriesFourMonths()
    {
        var data = LoadSampleData();

        data.Heatmap.MonthColumns.Should().Equal("Jan", "Feb", "Mar", "Apr");
        data.Heatmap.Categories.Should().HaveCount(4);

        var cats = data.Heatmap.Categories;
        cats[0].Name.Should().Be("Shipped");
        cats[0].CssClass.Should().Be("ship");
        cats[1].Name.Should().Be("In Progress");
        cats[1].CssClass.Should().Be("prog");
        cats[2].Name.Should().Be("Carryover");
        cats[2].CssClass.Should().Be("carry");
        cats[3].Name.Should().Be("Blockers");
        cats[3].CssClass.Should().Be("block");

        // Each category has exactly 4 months
        foreach (var cat in cats)
        {
            cat.Months.Should().HaveCount(4, $"category '{cat.Name}' should have 4 month entries");
        }
    }

    [Fact]
    public void SampleData_EmptyCellsAndMultiItemCells_ExistForRenderingPaths()
    {
        var data = LoadSampleData();

        var allMonthItems = data.Heatmap.Categories.SelectMany(c => c.Months).ToList();

        // At least 2 empty cells (Jan/Carryover, Jan/Blockers, Mar/Blockers)
        var emptyCells = allMonthItems.Where(m => m.Items.Length == 0).ToList();
        emptyCells.Should().HaveCountGreaterOrEqualTo(2, "need empty cells for gray-dash rendering");

        // At least 1 cell with 4+ items (Apr/In Progress has 4)
        var multiItemCells = allMonthItems.Where(m => m.Items.Length >= 4).ToList();
        multiItemCells.Should().HaveCountGreaterOrEqualTo(1, "need a cell with 4+ items for multi-line rendering");
    }
}