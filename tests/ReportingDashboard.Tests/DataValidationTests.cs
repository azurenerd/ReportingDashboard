using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using ReportingDashboard.Models;
using ReportingDashboard.Services;

namespace ReportingDashboard.Tests;

public class DataValidationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static string GetTestDataPath(string filename)
    {
        return Path.Combine(AppContext.BaseDirectory, "TestData", filename);
    }

    private static DashboardDataService CreateService(string contentRoot)
    {
        var env = new TestWebHostEnvironment(contentRoot);
        var logger = NullLogger<DashboardDataService>.Instance;
        return new DashboardDataService(env, logger);
    }

    [Fact]
    public void GetData_ValidMinimal_Succeeds()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"rd-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDir);
        try
        {
            File.Copy(GetTestDataPath("valid-minimal.json"), Path.Combine(testDir, "data.json"));
            using var service = CreateService(testDir);
            var data = service.GetData();
            Assert.Equal("Minimal Project", data.Title);
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [Fact]
    public void GetData_ValidFull_Succeeds()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"rd-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDir);
        try
        {
            File.Copy(GetTestDataPath("valid-full.json"), Path.Combine(testDir, "data.json"));
            using var service = CreateService(testDir);
            var data = service.GetData();
            Assert.Equal("Project Phoenix Release Roadmap", data.Title);
            Assert.Equal(3, data.Milestones.Count);
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [Fact]
    public void GetData_MissingTitle_ThrowsDashboardDataException()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"rd-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDir);
        try
        {
            File.Copy(GetTestDataPath("invalid-missing-title.json"), Path.Combine(testDir, "data.json"));
            using var service = CreateService(testDir);
            var ex = Assert.Throws<DashboardDataException>(() => service.GetData());
            Assert.Contains("title", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [Fact]
    public void GetData_BadMilestone_ThrowsDashboardDataException()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"rd-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDir);
        try
        {
            File.Copy(GetTestDataPath("invalid-bad-milestone.json"), Path.Combine(testDir, "data.json"));
            using var service = CreateService(testDir);
            var ex = Assert.Throws<DashboardDataException>(() => service.GetData());
            Assert.Contains("color", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [Fact]
    public void GetData_FileNotFound_ThrowsDashboardDataException()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"rd-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDir);
        try
        {
            using var service = CreateService(testDir);
            var ex = Assert.Throws<DashboardDataException>(() => service.GetData());
            Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [Fact]
    public void GetData_InvalidProjectName_ThrowsDashboardDataException()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"rd-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDir);
        try
        {
            using var service = CreateService(testDir);
            var ex = Assert.Throws<DashboardDataException>(() => service.GetData("../../etc/passwd"));
            Assert.Contains("Invalid project name", ex.Message);
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [Fact]
    public void GetData_ProjectNotFound_ThrowsDashboardDataException()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"rd-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDir);
        try
        {
            using var service = CreateService(testDir);
            var ex = Assert.Throws<DashboardDataException>(() => service.GetData("nonexistent"));
            Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("nonexistent", ex.Message);
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [Fact]
    public void GetData_MultiProject_LoadsCorrectFile()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"rd-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDir);
        try
        {
            File.Copy(GetTestDataPath("valid-full.json"), Path.Combine(testDir, "data.phoenix.json"));
            using var service = CreateService(testDir);
            var data = service.GetData("phoenix");
            Assert.Equal("Project Phoenix Release Roadmap", data.Title);
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [Fact]
    public void GetData_MultiProject_FallsBackToProjectsSubfolder()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"rd-test-{Guid.NewGuid():N}");
        var projectsDir = Path.Combine(testDir, "projects");
        Directory.CreateDirectory(projectsDir);
        try
        {
            File.Copy(GetTestDataPath("valid-minimal.json"), Path.Combine(projectsDir, "atlas.json"));
            using var service = CreateService(testDir);
            var data = service.GetData("atlas");
            Assert.Equal("Minimal Project", data.Title);
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    [Fact]
    public void GetData_CachesResults()
    {
        var testDir = Path.Combine(Path.GetTempPath(), $"rd-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(testDir);
        try
        {
            File.Copy(GetTestDataPath("valid-minimal.json"), Path.Combine(testDir, "data.json"));
            using var service = CreateService(testDir);
            var data1 = service.GetData();
            var data2 = service.GetData();
            Assert.Same(data1, data2);
        }
        finally
        {
            Directory.Delete(testDir, true);
        }
    }

    private class TestWebHostEnvironment : IWebHostEnvironment
    {
        public TestWebHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
            WebRootPath = Path.Combine(contentRootPath, "wwwroot");
            EnvironmentName = "Testing";
            ApplicationName = "ReportingDashboard.Tests";
            ContentRootFileProvider = new NullFileProvider();
            WebRootFileProvider = new NullFileProvider();
        }

        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
    }
}