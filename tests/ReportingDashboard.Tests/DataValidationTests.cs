using Xunit;
using ReportingDashboard.Services;

namespace ReportingDashboard.Tests;

public class DataValidationTests : IDisposable
{
    private readonly string _tempDir;

    public DataValidationTests()
    {
        _tempDir = TestHelper.CreateTempDir();
    }

    [Fact]
    public void MissingTitle_ThrowsDashboardDataException()
    {
        var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "invalid-missing-title.json"));
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), json);

        var service = TestHelper.CreateService(_tempDir);
        var ex = Assert.Throws<DashboardDataException>(() => service.GetData());
        Assert.Contains("title", ex.Message, StringComparison.OrdinalIgnoreCase);
        service.Dispose();
    }

    [Fact]
    public void InvalidMarkerType_ThrowsDashboardDataException()
    {
        var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "invalid-bad-milestone.json"));
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), json);

        var service = TestHelper.CreateService(_tempDir);
        var ex = Assert.Throws<DashboardDataException>(() => service.GetData());
        Assert.Contains("invalid_type", ex.Message);
        service.Dispose();
    }

    [Fact]
    public void EmptyMonths_ThrowsDashboardDataException()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "S",
            "backlogUrl": "https://x.com",
            "currentDate": "2026-01-01",
            "months": [],
            "currentMonthIndex": 0,
            "timelineStart": "2026-01-01",
            "timelineEnd": "2026-01-31",
            "milestones": [{"id":"M1","label":"M1","description":"D","color":"#000","markers":[]}],
            "categories": [
                {"name":"Shipped","key":"shipped","items":{}},
                {"name":"In Progress","key":"inProgress","items":{}},
                {"name":"Carryover","key":"carryover","items":{}},
                {"name":"Blockers","key":"blockers","items":{}}
            ]
        }
        """;
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), json);

        var service = TestHelper.CreateService(_tempDir);
        var ex = Assert.Throws<DashboardDataException>(() => service.GetData());
        Assert.Contains("months", ex.Message, StringComparison.OrdinalIgnoreCase);
        service.Dispose();
    }

    [Fact]
    public void InvalidCurrentMonthIndex_ThrowsDashboardDataException()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "S",
            "backlogUrl": "https://x.com",
            "currentDate": "2026-01-01",
            "months": ["Jan"],
            "currentMonthIndex": 5,
            "timelineStart": "2026-01-01",
            "timelineEnd": "2026-01-31",
            "milestones": [{"id":"M1","label":"M1","description":"D","color":"#000","markers":[]}],
            "categories": [
                {"name":"Shipped","key":"shipped","items":{}},
                {"name":"In Progress","key":"inProgress","items":{}},
                {"name":"Carryover","key":"carryover","items":{}},
                {"name":"Blockers","key":"blockers","items":{}}
            ]
        }
        """;
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), json);

        var service = TestHelper.CreateService(_tempDir);
        var ex = Assert.Throws<DashboardDataException>(() => service.GetData());
        Assert.Contains("currentMonthIndex", ex.Message, StringComparison.OrdinalIgnoreCase);
        service.Dispose();
    }

    [Fact]
    public void TimelineStartAfterEnd_ThrowsDashboardDataException()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "S",
            "backlogUrl": "https://x.com",
            "currentDate": "2026-01-01",
            "months": ["Jan"],
            "currentMonthIndex": 0,
            "timelineStart": "2026-06-01",
            "timelineEnd": "2026-01-01",
            "milestones": [{"id":"M1","label":"M1","description":"D","color":"#000","markers":[]}],
            "categories": [
                {"name":"Shipped","key":"shipped","items":{}},
                {"name":"In Progress","key":"inProgress","items":{}},
                {"name":"Carryover","key":"carryover","items":{}},
                {"name":"Blockers","key":"blockers","items":{}}
            ]
        }
        """;
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), json);

        var service = TestHelper.CreateService(_tempDir);
        var ex = Assert.Throws<DashboardDataException>(() => service.GetData());
        Assert.Contains("timelineStart", ex.Message, StringComparison.OrdinalIgnoreCase);
        service.Dispose();
    }

    [Fact]
    public void TooManyMilestones_ThrowsDashboardDataException()
    {
        var milestoneList = new List<string>();
        for (int i = 1; i <= 6; i++)
        {
            milestoneList.Add("{\"id\":\"M" + i + "\",\"label\":\"M" + i + "\",\"description\":\"D\",\"color\":\"#000\",\"markers\":[]}");
        }
        var milestones = string.Join(",", milestoneList);
        var json = @"
        {
            ""title"": ""Test"",
            ""subtitle"": ""S"",
            ""backlogUrl"": ""https://x.com"",
            ""currentDate"": ""2026-01-01"",
            ""months"": [""Jan""],
            ""currentMonthIndex"": 0,
            ""timelineStart"": ""2026-01-01"",
            ""timelineEnd"": ""2026-01-31"",
            ""milestones"": [" + milestones + @"],
            ""categories"": [
                {""name"":""Shipped"",""key"":""shipped"",""items"":{}},
                {""name"":""In Progress"",""key"":""inProgress"",""items"":{}},
                {""name"":""Carryover"",""key"":""carryover"",""items"":{}},
                {""name"":""Blockers"",""key"":""blockers"",""items"":{}}
            ]
        }";
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), json);

        var service = TestHelper.CreateService(_tempDir);
        var ex = Assert.Throws<DashboardDataException>(() => service.GetData());
        Assert.Contains("milestones", ex.Message, StringComparison.OrdinalIgnoreCase);
        service.Dispose();
    }

    [Fact]
    public void WrongCategoryCount_ThrowsDashboardDataException()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "S",
            "backlogUrl": "https://x.com",
            "currentDate": "2026-01-01",
            "months": ["Jan"],
            "currentMonthIndex": 0,
            "timelineStart": "2026-01-01",
            "timelineEnd": "2026-01-31",
            "milestones": [{"id":"M1","label":"M1","description":"D","color":"#000","markers":[]}],
            "categories": [
                {"name":"Shipped","key":"shipped","items":{}}
            ]
        }
        """;
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), json);

        var service = TestHelper.CreateService(_tempDir);
        var ex = Assert.Throws<DashboardDataException>(() => service.GetData());
        Assert.Contains("categories", ex.Message, StringComparison.OrdinalIgnoreCase);
        service.Dispose();
    }

    [Fact]
    public void InvalidCategoryKey_ThrowsDashboardDataException()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "S",
            "backlogUrl": "https://x.com",
            "currentDate": "2026-01-01",
            "months": ["Jan"],
            "currentMonthIndex": 0,
            "timelineStart": "2026-01-01",
            "timelineEnd": "2026-01-31",
            "milestones": [{"id":"M1","label":"M1","description":"D","color":"#000","markers":[]}],
            "categories": [
                {"name":"Shipped","key":"shipped","items":{}},
                {"name":"In Progress","key":"inProgress","items":{}},
                {"name":"Carryover","key":"carryover","items":{}},
                {"name":"Bad","key":"invalidKey","items":{}}
            ]
        }
        """;
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), json);

        var service = TestHelper.CreateService(_tempDir);
        var ex = Assert.Throws<DashboardDataException>(() => service.GetData());
        Assert.Contains("invalidKey", ex.Message);
        service.Dispose();
    }

    [Fact]
    public void InvalidHexColor_ThrowsDashboardDataException()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "S",
            "backlogUrl": "https://x.com",
            "currentDate": "2026-01-01",
            "months": ["Jan"],
            "currentMonthIndex": 0,
            "timelineStart": "2026-01-01",
            "timelineEnd": "2026-01-31",
            "milestones": [{"id":"M1","label":"M1","description":"D","color":"not-a-color","markers":[]}],
            "categories": [
                {"name":"Shipped","key":"shipped","items":{}},
                {"name":"In Progress","key":"inProgress","items":{}},
                {"name":"Carryover","key":"carryover","items":{}},
                {"name":"Blockers","key":"blockers","items":{}}
            ]
        }
        """;
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), json);

        var service = TestHelper.CreateService(_tempDir);
        var ex = Assert.Throws<DashboardDataException>(() => service.GetData());
        Assert.Contains("invalid color", ex.Message, StringComparison.OrdinalIgnoreCase);
        service.Dispose();
    }

    [Fact]
    public void ValidData_PassesValidation()
    {
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), TestHelper.MinimalValidJson);

        var service = TestHelper.CreateService(_tempDir);
        var data = service.GetData();

        Assert.NotNull(data);
        Assert.Equal("Test Dashboard", data.Title);
        service.Dispose();
    }

    [Fact]
    public void MissingDataFile_ThrowsDashboardDataException()
    {
        // No data.json in temp dir
        var service = TestHelper.CreateService(_tempDir);
        var ex = Assert.Throws<DashboardDataException>(() => service.Initialize());
        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
        service.Dispose();
    }

    [Fact]
    public void MalformedJson_ThrowsDashboardDataException()
    {
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), "{ this is not valid json }");

        var service = TestHelper.CreateService(_tempDir);
        var ex = Assert.Throws<DashboardDataException>(() => service.GetData());
        Assert.Contains("parse", ex.Message, StringComparison.OrdinalIgnoreCase);
        service.Dispose();
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, true); } catch { }
    }
}