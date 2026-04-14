using System.Text.Json;
using Xunit;
using ReportingDashboard.Models;
using ReportingDashboard.Services;

namespace ReportingDashboard.Tests;

public class DataValidationTests
{
    [Fact]
    public void Validate_ValidMinimalData_DoesNotThrow()
    {
        var json = File.ReadAllText(Path.Combine("TestData", "valid-minimal.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;

        var ex = Record.Exception(() => DashboardDataService.ValidateData(data, "test.json"));
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_ValidFullData_DoesNotThrow()
    {
        var json = File.ReadAllText(Path.Combine("TestData", "valid-full.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;

        var ex = Record.Exception(() => DashboardDataService.ValidateData(data, "test.json"));
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_MissingTitle_ThrowsWithMessage()
    {
        var json = File.ReadAllText(Path.Combine("TestData", "invalid-missing-title.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;

        var ex = Assert.Throws<DashboardDataException>(
            () => DashboardDataService.ValidateData(data, "test.json"));
        Assert.Contains("title", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_BadMarkerType_ThrowsWithMessage()
    {
        var json = File.ReadAllText(Path.Combine("TestData", "invalid-bad-milestone.json"));
        var data = JsonSerializer.Deserialize<DashboardData>(json)!;

        var ex = Assert.Throws<DashboardDataException>(
            () => DashboardDataService.ValidateData(data, "test.json"));
        Assert.Contains("Marker type", ex.Message);
    }

    [Fact]
    public void Validate_EmptyMonths_Throws()
    {
        var data = CreateValidData();
        data.Months = new List<string>();

        Assert.Throws<DashboardDataException>(
            () => DashboardDataService.ValidateData(data, "test.json"));
    }

    [Fact]
    public void Validate_TooManyMonths_Throws()
    {
        var data = CreateValidData();
        data.Months = Enumerable.Range(1, 13).Select(i => $"M{i}").ToList();

        Assert.Throws<DashboardDataException>(
            () => DashboardDataService.ValidateData(data, "test.json"));
    }

    [Fact]
    public void Validate_CurrentMonthIndexOutOfRange_Throws()
    {
        var data = CreateValidData();
        data.CurrentMonthIndex = 10;

        Assert.Throws<DashboardDataException>(
            () => DashboardDataService.ValidateData(data, "test.json"));
    }

    [Fact]
    public void Validate_NegativeCurrentMonthIndex_Throws()
    {
        var data = CreateValidData();
        data.CurrentMonthIndex = -1;

        Assert.Throws<DashboardDataException>(
            () => DashboardDataService.ValidateData(data, "test.json"));
    }

    [Fact]
    public void Validate_TimelineStartAfterEnd_Throws()
    {
        var data = CreateValidData();
        data.TimelineStart = new DateTime(2026, 12, 1);
        data.TimelineEnd = new DateTime(2026, 1, 1);

        Assert.Throws<DashboardDataException>(
            () => DashboardDataService.ValidateData(data, "test.json"));
    }

    [Fact]
    public void Validate_NoMilestones_Throws()
    {
        var data = CreateValidData();
        data.Milestones = new List<Milestone>();

        Assert.Throws<DashboardDataException>(
            () => DashboardDataService.ValidateData(data, "test.json"));
    }

    [Fact]
    public void Validate_TooManyMilestones_Throws()
    {
        var data = CreateValidData();
        data.Milestones = Enumerable.Range(1, 6).Select(i => new Milestone
        {
            Id = $"M{i}", Label = $"M{i}", Description = "Test", Color = "#000",
            Markers = new List<MilestoneMarker>()
        }).ToList();

        Assert.Throws<DashboardDataException>(
            () => DashboardDataService.ValidateData(data, "test.json"));
    }

    [Fact]
    public void Validate_WrongCategoryCount_Throws()
    {
        var data = CreateValidData();
        data.Categories = data.Categories.Take(2).ToList();

        Assert.Throws<DashboardDataException>(
            () => DashboardDataService.ValidateData(data, "test.json"));
    }

    [Fact]
    public void Validate_InvalidCategoryKey_Throws()
    {
        var data = CreateValidData();
        data.Categories[0].Key = "invalid_key";

        var ex = Assert.Throws<DashboardDataException>(
            () => DashboardDataService.ValidateData(data, "test.json"));
        Assert.Contains("invalid_key", ex.Message);
    }

    private static DashboardData CreateValidData() => new()
    {
        Title = "Test Project",
        Subtitle = "Test",
        BacklogUrl = "https://example.com",
        CurrentDate = new DateTime(2026, 4, 1),
        Months = new List<string> { "Jan", "Feb", "Mar", "Apr" },
        CurrentMonthIndex = 3,
        TimelineStart = new DateTime(2026, 1, 1),
        TimelineEnd = new DateTime(2026, 6, 30),
        Milestones = new List<Milestone>
        {
            new()
            {
                Id = "M1", Label = "M1", Description = "Test", Color = "#0078D4",
                Markers = new List<MilestoneMarker>
                {
                    new() { Date = new DateTime(2026, 3, 1), Type = "checkpoint", Label = "Mar 1" }
                }
            }
        },
        Categories = new List<HeatmapCategory>
        {
            new() { Name = "Shipped", Key = "shipped", Items = new() },
            new() { Name = "In Progress", Key = "inProgress", Items = new() },
            new() { Name = "Carryover", Key = "carryover", Items = new() },
            new() { Name = "Blockers", Key = "blockers", Items = new() }
        }
    };
}