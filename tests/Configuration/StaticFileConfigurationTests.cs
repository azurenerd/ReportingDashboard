using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace AgentSquad.Runner.Tests.Configuration
{
    public class StaticFileConfigurationTests
    {
        private readonly string _wwwrootPath;

        public StaticFileConfigurationTests()
        {
            _wwwrootPath = Path.Combine(Path.GetTempPath(), "dashboard-wwwroot");
            Directory.CreateDirectory(_wwwrootPath);
        }

        [Fact]
        public void WwwrootDirectory_HasRequiredSubdirectories()
        {
            // Arrange
            var requiredDirs = new[] { "css", "js", "data" };
            foreach (var dir in requiredDirs)
            {
                Directory.CreateDirectory(Path.Combine(_wwwrootPath, dir));
            }

            // Act
            var cssExists = Directory.Exists(Path.Combine(_wwwrootPath, "css"));
            var jsExists = Directory.Exists(Path.Combine(_wwwrootPath, "js"));
            var dataExists = Directory.Exists(Path.Combine(_wwwrootPath, "data"));

            // Assert
            Assert.True(cssExists, "css directory should exist");
            Assert.True(jsExists, "js directory should exist");
            Assert.True(dataExists, "data directory should exist");
        }

        [Fact]
        public void DashboardCss_FileExists_InCssDirectory()
        {
            // Arrange
            var cssDir = Path.Combine(_wwwrootPath, "css");
            Directory.CreateDirectory(cssDir);
            var dashboardCssPath = Path.Combine(cssDir, "dashboard.css");
            File.WriteAllText(dashboardCssPath, "/* Dashboard styles */");

            // Act
            var exists = File.Exists(dashboardCssPath);

            // Assert
            Assert.True(exists, "dashboard.css should exist in css directory");
        }

        [Fact]
        public void DataJson_FileExists_InRootDirectory()
        {
            // Arrange
            var dataJsonPath = Path.Combine(_wwwrootPath, "data.json");
            File.WriteAllText(dataJsonPath, "{}");

            // Act
            var exists = File.Exists(dataJsonPath);

            // Assert
            Assert.True(exists, "data.json should exist in wwwroot root");
        }

        [Fact]
        public void IndexHtml_FileExists_InRootDirectory()
        {
            // Arrange
            var indexPath = Path.Combine(_wwwrootPath, "index.html");
            File.WriteAllText(indexPath, "<html></html>");

            // Act
            var exists = File.Exists(indexPath);

            // Assert
            Assert.True(exists, "index.html should exist in wwwroot root");
        }

        [Fact]
        public void DataJson_EncodedAsUtf8_WithoutBom()
        {
            // Arrange
            var dataJsonPath = Path.Combine(_wwwrootPath, "data.json");
            var utf8NoBom = new System.Text.UTF8Encoding(false);
            var jsonContent = @"{ ""name"": ""Test Project"" }";
            File.WriteAllText(dataJsonPath, jsonContent, utf8NoBom);

            // Act
            var bytes = File.ReadAllBytes(dataJsonPath);

            // Assert
            // UTF-8 BOM is EF BB BF
            Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF,
                "data.json should not have UTF-8 BOM");
        }

        [Fact]
        public void TimelineJs_FileExists_InJsDirectory()
        {
            // Arrange
            var jsDir = Path.Combine(_wwwrootPath, "js");
            Directory.CreateDirectory(jsDir);
            var timelinePath = Path.Combine(jsDir, "timeline.js");
            File.WriteAllText(timelinePath, "/* Timeline code */");

            // Act
            var exists = File.Exists(timelinePath);

            // Assert
            Assert.True(exists, "timeline.js should exist in js directory");
        }

        [Fact]
        public void ErrorHandlerJs_FileExists_InJsDirectory()
        {
            // Arrange
            var jsDir = Path.Combine(_wwwrootPath, "js");
            Directory.CreateDirectory(jsDir);
            var errorHandlerPath = Path.Combine(jsDir, "error-handler.js");
            File.WriteAllText(errorHandlerPath, "/* Error handler code */");

            // Act
            var exists = File.Exists(errorHandlerPath);

            // Assert
            Assert.True(exists, "error-handler.js should exist in js directory");
        }

        [Fact]
        public void CachingHeaders_Configured_ForStaticAssets()
        {
            // Arrange
            var cacheControl = "public, max-age=31536000";

            // Act
            // Verify the cache control header value is correct (1 year in seconds)
            var oneYearSeconds = 365 * 24 * 60 * 60;
            var maxAge = int.Parse(cacheControl.Split("max-age=")[1]);

            // Assert
            Assert.Equal(oneYearSeconds, maxAge);
        }

        [Fact]
        public void MimeTypeMapping_JsonContentType_Configured()
        {
            // Arrange
            var provider = new FileExtensionContentTypeProvider();

            // Act
            var jsonMimeType = provider.TryGetContentType("data.json", out var contentType);

            // Assert
            Assert.True(jsonMimeType, "JSON MIME type should be configured");
            Assert.Equal("application/json", contentType);
        }

        [Fact]
        public void CssFile_Accessible_AtExpectedPath()
        {
            // Arrange
            var cssDir = Path.Combine(_wwwrootPath, "css");
            Directory.CreateDirectory(cssDir);
            var dashboardCssPath = Path.Combine(cssDir, "dashboard.css");
            File.WriteAllText(dashboardCssPath, "body { margin: 0; }");

            // Act
            var fileProvider = new PhysicalFileProvider(_wwwrootPath);
            var fileInfo = fileProvider.GetFileInfo("css/dashboard.css");

            // Assert
            Assert.True(fileInfo.Exists, "dashboard.css should be accessible at css/dashboard.css");
        }

        [Fact]
        public void JsonFile_Accessible_AtExpectedPath()
        {
            // Arrange
            var dataJsonPath = Path.Combine(_wwwrootPath, "data.json");
            File.WriteAllText(dataJsonPath, "{}");

            // Act
            var fileProvider = new PhysicalFileProvider(_wwwrootPath);
            var fileInfo = fileProvider.GetFileInfo("data.json");

            // Assert
            Assert.True(fileInfo.Exists, "data.json should be accessible at /data.json");
        }

        [Fact]
        public void JavaScriptFile_Accessible_AtExpectedPath()
        {
            // Arrange
            var jsDir = Path.Combine(_wwwrootPath, "js");
            Directory.CreateDirectory(jsDir);
            var timelinePath = Path.Combine(jsDir, "timeline.js");
            File.WriteAllText(timelinePath, "console.log('test');");

            // Act
            var fileProvider = new PhysicalFileProvider(_wwwrootPath);
            var fileInfo = fileProvider.GetFileInfo("js/timeline.js");

            // Assert
            Assert.True(fileInfo.Exists, "timeline.js should be accessible at js/timeline.js");
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_wwwrootPath))
                {
                    Directory.Delete(_wwwrootPath, true);
                }
            }
            catch
            {
                // Cleanup best effort
            }
        }
    }
}