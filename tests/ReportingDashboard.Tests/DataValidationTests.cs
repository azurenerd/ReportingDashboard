using ReportingDashboard.Services;
using ReportingDashboard.Tests.Helpers;
using Xunit;

namespace ReportingDashboard.Tests;

public class DataValidationTests : IDisposable
{
    private readonly string _tempDir;

    public DataValidationTests()
    {
        _tempDir = Path.Combine(
            Path.GetTempPath(),
            "RD_ValTests_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }
        catch { }
    }

    private DashboardDataService CreateServiceWithFile(string json)
    {
        File.WriteAllText(Path.Combine(_tempDir, "data.json"), json);
        var env = new TestWebHostEnvironment { ContentRootPath = _tempDir };
        var logger = new TestLogger<DashboardDataService>();
        return new DashboardDataService(env, logger);
    }

    [Fact]
    public void Validate_ValidData_Succeeds()
    {
        var service = CreateServiceWithFile(TestDataHelper.GenerateValidJson());
        var exception = Record.Exception(() => service.Initialize());
        Assert.Null(exception);
        service.Dispose();
    }

    [Fact]
    public void Validate_MissingTitle_ThrowsDashboardDataException()
    {
        var json = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "TestData", "invalid-missing-title.json"));
        var service = CreateServiceWithFile(json);

        var ex = Assert.Throws<DashboardDataException>(() => service.Initialize());
        Assert.Contains("'title' is required", ex.Message);
        service.Dispose();
    }

    [Fact]
    public void Validate_InvalidMilestoneColor_ThrowsDashboardDataException()
    {
        var json = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "TestData", "invalid-bad-milestone.json"));
        var service = CreateServiceWithFile(json);

        var ex = Assert.Throws<DashboardDataException>(() => service.Initialize());
        Assert.Contains("invalid color", ex.Message);
        service.Dispose();
    }

    [Fact]
    public void Validate_MissingDataJson_LogsWarningButDoesNotThrow()
    {
        // No data.json written to temp dir
        var env = new TestWebHostEnvironment { ContentRootPath = _tempDir };
        var logger = new TestLogger<DashboardDataService>();
        var service = new DashboardDataService(env, logger);

        var exception = Record.Exception(() => service.Initialize());
        Assert.Null(exception);
        Assert.True(logger.HasLogLevel(Microsoft.Extensions.Logging.LogLevel.Warning));
        service.Dispose();
    }

    [Fact]
    public void GetData_MissingDataJson_ThrowsDashboardDataException()
    {
        var env = new TestWebHostEnvironment { ContentRootPath = _tempDir };
        var logger = new TestLogger<DashboardDataService>();
        var service = new DashboardDataService(env, logger);
        service.Initialize();

        var ex = Assert.Throws<DashboardDataException>(() => service.GetData());
        Assert.Contains("not found", ex.Message);
        service.Dispose();
    }

    [Fact]
    public void GetData_InvalidProjectName_ThrowsDashboardDataException()
    {
        var service = CreateServiceWithFile(TestDataHelper.GenerateValidJson());
        service.Initialize();

        var ex = Assert.Throws<DashboardDataException>(() => service.GetData("../../../etc"));
        Assert.Contains("Invalid project name", ex.Message);
        service.Dispose();
    }

    [Fact]
    public void GetData_NonexistentProject_ThrowsDashboardDataException()
    {
        var service = CreateServiceWithFile(TestDataHelper.GenerateValidJson());
        service.Initialize();

        var ex = Assert.Throws<DashboardDataException>(() => service.GetData("nonexistent"));
        Assert.Contains("not found", ex.Message);
        service.Dispose();
    }

    [Fact]
    public void GetData_ValidProject_ReturnsProjectData()
    {
        var service = CreateServiceWithFile(TestDataHelper.GenerateValidJson());
        File.WriteAllText(
            Path.Combine(_tempDir, "data.phoenix.json"),
            TestDataHelper.GenerateValidJson("Phoenix Project"));
        service.Initialize();

        var data = service.GetData("phoenix");
        Assert.Equal("Phoenix Project", data.Title);
        service.Dispose();
    }

    [Fact]
    public void GetData_CachesResult_ReturnsSameInstance()
    {
        var service = CreateServiceWithFile(TestDataHelper.GenerateValidJson());
        service.Initialize();

        var data1 = service.GetData();
        var data2 = service.GetData();
        Assert.Same(data1, data2);
        service.Dispose();
    }

    [Fact]
    public void Validate_EmptyMonths_ThrowsDashboardDataException()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "",
            "backlogUrl": "",
            "currentDate": "2026-01-01",
            "months": [],
            "currentMonthIndex": 0,
            "timelineStart": "2026-01-01",
            "timelineEnd": "2026-06-30",
            "milestones": [{"id":"M1","label":"M1","description":"T","color":"#000000","markers":[]}],
            "categories": [
                {"name":"S","key":"shipped","items":{}},
                {"name":"I","key":"inProgress","items":{}},
                {"name":"C","key":"carryover","items":{}},
                {"name":"B","key":"blockers","items":{}}
            ]
        }
        """;
        var service = CreateServiceWithFile(json);
        var ex = Assert.Throws<DashboardDataException>(() => service.Initialize());
        Assert.Contains("'months' must contain 1-12 entries", ex.Message);
        service.Dispose();
    }

    [Fact]
    public void Validate_TimelineStartAfterEnd_ThrowsDashboardDataException()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "",
            "backlogUrl": "",
            "currentDate": "2026-01-01",
            "months": ["Jan"],
            "currentMonthIndex": 0,
            "timelineStart": "2026-12-01",
            "timelineEnd": "2026-01-01",
            "milestones": [{"id":"M1","label":"M1","description":"T","color":"#000000","markers":[]}],
            "categories": [
                {"name":"S","key":"shipped","items":{}},
                {"name":"I","key":"inProgress","items":{}},
                {"name":"C","key":"carryover","items":{}},
                {"name":"B","key":"blockers","items":{}}
            ]
        }
        """;
        var service = CreateServiceWithFile(json);
        var ex = Assert.Throws<DashboardDataException>(() => service.Initialize());
        Assert.Contains("'timelineStart' must be before 'timelineEnd'", ex.Message);
        service.Dispose();
    }

    [Fact]
    public void Validate_InvalidMarkerType_ThrowsDashboardDataException()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "",
            "backlogUrl": "",
            "currentDate": "2026-01-01",
            "months": ["Jan"],
            "currentMonthIndex": 0,
            "timelineStart": "2026-01-01",
            "timelineEnd": "2026-06-30",
            "milestones": [{"id":"M1","label":"M1","description":"T","color":"#0078D4","markers":[
                {"date":"2026-02-01","type":"invalid_type","label":"Test"}
            ]}],
            "categories": [
                {"name":"S","key":"shipped","items":{}},
                {"name":"I","key":"inProgress","items":{}},
                {"name":"C","key":"carryover","items":{}},
                {"name":"B","key":"blockers","items":{}}
            ]
        }
        """;
        var service = CreateServiceWithFile(json);
        var ex = Assert.Throws<DashboardDataException>(() => service.Initialize());
        Assert.Contains("Marker type 'invalid_type' is not recognized", ex.Message);
        service.Dispose();
    }

    [Fact]
    public void Validate_WrongCategoryCount_ThrowsDashboardDataException()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "",
            "backlogUrl": "",
            "currentDate": "2026-01-01",
            "months": ["Jan"],
            "currentMonthIndex": 0,
            "timelineStart": "2026-01-01",
            "timelineEnd": "2026-06-30",
            "milestones": [{"id":"M1","label":"M1","description":"T","color":"#0078D4","markers":[]}],
            "categories": [
                {"name":"S","key":"shipped","items":{}}
            ]
        }
        """;
        var service = CreateServiceWithFile(json);
        var ex = Assert.Throws<DashboardDataException>(() => service.Initialize());
        Assert.Contains("'categories' must contain exactly 4 entries", ex.Message);
        service.Dispose();
    }

    [Fact]
    public void Validate_InvalidCategoryKey_ThrowsDashboardDataException()
    {
        var json = """
        {
            "title": "Test",
            "subtitle": "",
            "backlogUrl": "",
            "currentDate": "2026-01-01",
            "months": ["Jan"],
            "currentMonthIndex": 0,
            "timelineStart": "2026-01-01",
            "timelineEnd": "2026-06-30",
            "milestones": [{"id":"M1","label":"M1","description":"T","color":"#0078D4","markers":[]}],
            "categories": [
                {"name":"S","key":"shipped","items":{}},
                {"name":"I","key":"badKey","items":{}},
                {"name":"C","key":"carryover","items":{}},
                {"name":"B","key":"blockers","items":{}}
            ]
        }
        """;
        var service = CreateServiceWithFile(json);
        var ex = Assert.Throws<DashboardDataException>(() => service.Initialize());
        Assert.Contains("Category key 'badKey' is not recognized", ex.Message);
        service.Dispose();
    }
}