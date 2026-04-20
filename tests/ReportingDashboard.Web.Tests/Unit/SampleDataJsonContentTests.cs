using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using ReportingDashboard.Web.Models;
using Xunit;

namespace ReportingDashboard.Web.Tests.Unit;

[Trait("Category", "Unit")]
public class SampleDataJsonContentTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static string LocateSampleFile()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "ReportingDashboard.Web", "wwwroot", "data.json");
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }
        throw new FileNotFoundException("Could not locate src/ReportingDashboard.Web/wwwroot/data.json by walking up from the test base directory.");
    }

    private static DashboardData LoadSample()
    {
        var path = LocateSampleFile();
        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<DashboardData>(json, Options);
        data.Should().NotBeNull("sample data.json must deserialize cleanly with project JSON options");
        return data!;
    }

    [Fact]
    public void SampleFile_DeserializesAndMatchesExactProjectAndTimelineBounds()
    {
        var data = LoadSample();

        data.Project.Title.Should().Be("Privacy Automation Release Roadmap");
        data.Project.Subtitle.Should().Be("Trusted Platform \u2022 Privacy Automation Workstream \u2022 April 2026");
        data.Project.BacklogUrl.Should().NotBeNullOrEmpty();
        data.Project.BacklogUrl!.Should().StartWith("https://dev.azure.com/");

        data.Timeline.Start.Should().Be(new DateOnly(2026, 1, 1));
        data.Timeline.End.Should().Be(new DateOnly(2026, 6, 30));
    }

    [Fact]
    public void SampleFile_HasExactlyThreeLanes_WithSpecifiedIdsAndColors()
    {
        var data = LoadSample();

        data.Timeline.Lanes.Should().HaveCount(3);

        var lanes = data.Timeline.Lanes;
        lanes[0].Id.Should().Be("M1");
        lanes[0].Color.Should().Be("#0078D4");
        lanes[0].Label.Should().NotBeNullOrWhiteSpace();

        lanes[1].Id.Should().Be("M2");
        lanes[1].Color.Should().Be("#00897B");
        lanes[1].Label.Should().NotBeNullOrWhiteSpace();

        lanes[2].Id.Should().Be("M3");
        lanes[2].Color.Should().Be("#546E7A");
        lanes[2].Label.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void SampleFile_MilestoneCoverage_AllTypesPresent_AndDatesWithinTimelineBounds()
    {
        var data = LoadSample();

        var milestones = data.Timeline.Lanes.SelectMany(l => l.Milestones).ToList();
        milestones.Should().HaveCountGreaterThanOrEqualTo(6);

        var types = milestones.Select(m => m.Type).Distinct().ToList();
        types.Should().Contain(MilestoneType.Poc);
        types.Should().Contain(MilestoneType.Prod);
        types.Should().Contain(MilestoneType.Checkpoint);

        milestones.Should().OnlyContain(m =>
            m.Date >= data.Timeline.Start && m.Date <= data.Timeline.End);

        milestones.Should().OnlyContain(m => !string.IsNullOrWhiteSpace(m.Label));
    }

    [Fact]
    public void SampleFile_Heatmap_HasExpectedShapeAndCategories()
    {
        var data = LoadSample();

        data.Heatmap.Months.Should().Equal("Jan", "Feb", "Mar", "Apr");
        data.Heatmap.CurrentMonthIndex.Should().BeNull();
        data.Heatmap.MaxItemsPerCell.Should().Be(4);

        data.Heatmap.Rows.Should().HaveCount(4);
        data.Heatmap.Rows.Select(r => r.Category).Should().Equal(
            HeatmapCategory.Shipped,
            HeatmapCategory.InProgress,
            HeatmapCategory.Carryover,
            HeatmapCategory.Blockers);

        foreach (var row in data.Heatmap.Rows)
        {
            row.Cells.Should().HaveCount(4, "each row must have one cell-array per month");
        }
    }

    [Fact]
    public void SampleFile_Heatmap_ContainsAtLeastOneEmptyCell_AndAtLeastOneOverflowCell()
    {
        var data = LoadSample();

        var allCells = data.Heatmap.Rows.SelectMany(r => r.Cells).ToList();

        allCells.Should().Contain(c => c.Count == 0,
            "at least one cell must be empty to exercise the '-' placeholder rendering");

        allCells.Should().Contain(c => c.Count >= 7,
            "at least one cell must contain >= 7 items to exercise the '+K more' overflow rendering given maxItemsPerCell=4");

        allCells.SelectMany(c => c).Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s));
        allCells.SelectMany(c => c).Should().OnlyContain(s => !s.Contains('<') && !s.Contains('>'));

        var fileInfo = new FileInfo(LocateSampleFile());
        fileInfo.Length.Should().BeLessThan(10 * 1024, "sample data.json must remain under 10 KB");
    }
}