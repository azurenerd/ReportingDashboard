using System.Text.Json;
using ReportingDashboard.Models;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests;

public class DataValidationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static string GetTestDataPath(string filename) =>
        Path.Combine("TestData", filename);

    private static DashboardData LoadTestData(string filename)
    {
        var json = File.ReadAllText(GetTestDataPath(filename));
        return JsonSerializer.Deserialize<DashboardData>(json, JsonOptions)!;
    }

    [Fact]
    public void Validate_ValidMinimal_NoException()
    {
        var data = LoadTestData("valid-minimal.json");
        var ex = Record.Exception(() => DashboardDataService.Validate(data, "test.json"));
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_ValidFull_NoException()
    {
        var data = LoadTestData("valid-full.json");
        var ex = Record.Exception(() => DashboardDataService.Validate(data, "test.json"));
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_MissingTitle_ThrowsWithMessage()
    {
        var data = LoadTestData("invalid-missing-title.json");
        var ex = Assert.Throws<DashboardDataException>(
            () => DashboardDataService.Validate(data, "test.json"));
        Assert.Contains("'title' is required", ex.Message);
    }

    [Fact]
    public void Validate_EmptyTitle_ThrowsWithMessage()
    {
        var data = LoadTestData("valid-full.json");
        data.Title = "";
        var ex = Assert.Throws<DashboardDataException>(
            () => DashboardDataService.Validate(data, "test.json"));
        Assert.Contains("'title' is required", ex.Message);
    }

    [Fact]
    public void Validate_EmptyMonths_ThrowsWithMessage()
    {
        var data = LoadTestData("valid-full.json");
        data.Months = new List<string>();
        var ex = Assert.Throws<DashboardDataException>(
            () => DashboardDataService.Validate(data, "test.json"));
        Assert.Contains("'months' must contain 1-12 entries", ex.Message);
    }

    [Fact]
    public void Validate_TooManyMonths_ThrowsWithMessage()
    {
        var data = LoadTestData("valid-full.json");
        data.Months = Enumerable.Range(1, 13).Select(i => $"M{i}").ToList();
        var ex = Assert.Throws<DashboardDataException>(
            () => DashboardDataService.Validate(data, "test.json"));
        Assert.Contains("'months' must contain 1-12 entries", ex.Message);
    }

    [Fact]
    public void Validate_CurrentMonthIndexOutOfRange_ThrowsWithMessage()
    {
        var data = LoadTestData("valid-full.json");
        data.CurrentMonthIndex = 99;
        var ex = Assert.Throws<DashboardDataException>(
            () => DashboardDataService.Validate(data, "test.json"));
        Assert.Contains("'currentMonthIndex'", ex.Message);
        Assert.Contains("out of range", ex.Message);
    }

    [Fact]
    public void Validate_NegativeCurrentMonthIndex_ThrowsWithMessage()
    {
        var data = LoadTestData("valid-full.json");
        data.CurrentMonthIndex = -1;
        var ex = Assert.Throws<DashboardDataException>(
            () => DashboardDataService.Validate(data, "test.json"));
        Assert.Contains("'currentMonthIndex'", ex.Message);
    }

    [Fact]
    public void Validate_TimelineStartAfterEnd_ThrowsWithMessage()
    {
        var data = LoadTestData("valid-full.json");
        data.TimelineStart = data.TimelineEnd.AddDays(1);
        var ex = Assert.Throws<DashboardDataException>(
            () => DashboardDataService.Validate(data, "test.json"));
        Assert.Contains("'timelineStart' must be before 'timelineEnd'", ex.Message);
    }

    [Fact]
    public void Validate_NoMilestones_ThrowsWithMessage()
    {
        var data = LoadTestData("valid-full.json");
        data.Milestones = new List<Milestone>();
        var ex = Assert.Throws<DashboardDataException>(
            () => DashboardDataService.Validate(data, "test.json"));
        Assert.Contains("'milestones' must contain 1-5 entries", ex.Message);
    }

    [Fact]
    public void Validate_TooManyMilestones_ThrowsWithMessage()
    {
        var data = LoadTestData("valid-full.json");
        data.Milestones = Enumerable.Range(1, 6).Select(i => new Milestone
        {
            Id = $"M{i}", Label = $"M{i}", Color = "#000000", Markers = new()
        }).ToList();
        var ex = Assert.Throws<DashboardDataException>(
            () => DashboardDataService.Validate(data, "test.json"));
        Assert.Contains("'milestones' must contain 1-5 entries", ex.Message);
    }

    [Fact]
    public void Validate_InvalidMilestoneColor_ThrowsWithMessage()
    {
        var data = LoadTestData("valid-full.json");
        data.Milestones[0].Color = "not-a-color";
        var ex = Assert.Throws<DashboardDataException>(
            () => DashboardDataService.Validate(data, "test.json"));
        Assert.Contains("invalid color", ex.Message);
        Assert.Contains(data.Milestones[0].Id, ex.Message);
    }

    [Fact]
    public void Validate_InvalidMarkerType_ThrowsWithMessage()
    {
        var data = LoadTestData("invalid-bad-milestone.json");
        var ex = Assert.Throws<DashboardDataException>(
            () => DashboardDataService.Validate(data, "test.json"));
        Assert.Contains("Marker type", ex.Message);
        Assert.Contains("not recognized", ex.Message);
    }

    [Fact]
    public void Validate_WrongCategoryCount_ThrowsWithMessage()
    {
        var data = LoadTestData("valid-full.json");
        data.Categories = data.Categories.Take(2).ToList();
        var ex = Assert.Throws<DashboardDataException>(
            () => DashboardDataService.Validate(data, "test.json"));
        Assert.Contains("'categories' must contain exactly 4 entries", ex.Message);
    }

    [Fact]
    public void Validate_InvalidCategoryKey_ThrowsWithMessage()
    {
        var data = LoadTestData("valid-full.json");
        data.Categories[0].Key = "invalid_key";
        var ex = Assert.Throws<DashboardDataException>(
            () => DashboardDataService.Validate(data, "test.json"));
        Assert.Contains("Category key 'invalid_key' is not recognized", ex.Message);
    }

    [Fact]
    public void Validate_HeaderFieldsPresent_ForDashboardHeader()
    {
        var data = LoadTestData("valid-full.json");
        DashboardDataService.Validate(data, "test.json");

        Assert.Equal("Project Phoenix Release Roadmap", data.Title);
        Assert.Contains("·", data.Subtitle);
        Assert.StartsWith("https://", data.BacklogUrl);
    }

    [Fact]
    public void Validate_ThreeLetterMonthAbbreviations()
    {
        var data = LoadTestData("valid-full.json");
        DashboardDataService.Validate(data, "test.json");

        foreach (var month in data.Months)
        {
            Assert.Equal(3, month.Length);
        }
    }
}