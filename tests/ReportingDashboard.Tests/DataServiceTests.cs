using Microsoft.AspNetCore.Hosting;
using Moq;
using ReportingDashboard.Services;
using Xunit;

namespace ReportingDashboard.Tests;

public class DataServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly string _wwwroot;
    private readonly Mock<IWebHostEnvironment> _envMock;

    public DataServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "DSTests_" + Guid.NewGuid().ToString("N").Substring(0, 8));
        _wwwroot = Path.Combine(_tempRoot, "wwwroot");
        Directory.CreateDirectory(_wwwroot);

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_wwwroot);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempRoot, true); } catch { }
    }

    private void WriteJson(string json)
    {
        File.WriteAllText(Path.Combine(_wwwroot, "data.json"), json);
    }

    private static string ValidJson()
    {
        return @"{
  ""schemaVersion"": 1,
  ""title"": ""Test"",
  ""subtitle"": ""Sub"",
  ""backlogUrl"": ""https://example.com"",
  ""nowDateOverride"": null,
  ""timeline"": {
    ""startDate"": ""2026-01-01"",
    ""endDate"": ""2026-07-01"",
    ""workstreams"": []
  },
  ""heatmap"": {
    ""monthColumns"": [],
    ""categories"": []
  }
}";
    }

    [Fact]
    public void ValidJson_LoadsSuccessfully()
    {
        WriteJson(ValidJson());
        using var svc = new DataService(_envMock.Object);
        Assert.NotNull(svc.GetData());
        Assert.Null(svc.GetError());
    }

    [Fact]
    public void MissingFile_ReturnsError()
    {
        using var svc = new DataService(_envMock.Object);
        Assert.Null(svc.GetData());
        Assert.NotNull(svc.GetError());
        Assert.Contains("No data.json found", svc.GetError()!);
    }

    [Fact]
    public void InvalidJson_ReturnsError()
    {
        WriteJson("not json {{{");
        using var svc = new DataService(_envMock.Object);
        Assert.Null(svc.GetData());
        Assert.Contains("invalid JSON", svc.GetError()!);
    }

    [Fact]
    public void WrongSchemaVersion_ReturnsError()
    {
        WriteJson(ValidJson().Replace("\"schemaVersion\": 1", "\"schemaVersion\": 42"));
        using var svc = new DataService(_envMock.Object);
        Assert.Null(svc.GetData());
        Assert.Contains("expected 1", svc.GetError()!);
    }
}