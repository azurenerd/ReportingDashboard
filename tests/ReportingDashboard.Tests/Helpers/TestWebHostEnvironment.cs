using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace ReportingDashboard.Tests.Helpers;

internal class TestWebHostEnvironment : IWebHostEnvironment
{
    public string ContentRootPath { get; set; } = string.Empty;
    public string WebRootPath { get; set; } = string.Empty;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public string EnvironmentName { get; set; } = "Testing";
    public string ApplicationName { get; set; } = "ReportingDashboard";
}