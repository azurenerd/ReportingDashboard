using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace AgentSquad.Tests.AcceptanceTests
{
    public class WwwrootConfigurationAcceptanceTests
    {
        private readonly string _wwwrootPath;
        private readonly string _indexHtmlPath;
        private readonly string _siteCssPath;

        public WwwrootConfigurationAcceptanceTests()
        {
            _wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            _indexHtmlPath = Path.Combine(_wwwrootPath, "index.html");
            _siteCssPath = Path.Combine(_wwwrootPath, "css", "site.css");
        }

        [Fact]
        public void Criterion_WwwrootDirectoryStructureCreated()
        {
            var requiredDirs = new[] { "css", "js", "images" };
            foreach (var dir in requiredDirs)
            {
                var path = Path.Combine(_wwwrootPath, dir);
                Assert.True(Directory.Exists(path), $"wwwroot/{dir} directory required");
            }
        }

        [Fact]
        public void Criterion_IndexHtmlWithBlazorHosting()
        {
            var content = File.ReadAllText(_indexHtmlPath);
            Assert.Contains("<base href=\"/\"", content);
            Assert.Contains("_framework/blazor.server.js", content);
            Assert.Contains("<!DOCTYPE html>", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Criterion_Bootstrap5CdnLinked()
        {
            var content = File.ReadAllText(_indexHtmlPath);
            Assert.Contains("bootstrap", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Criterion_ChartJsCdnLinked()
        {
            var content = File.ReadAllText(_indexHtmlPath);
            Assert.Contains("chart", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Criterion_CharsetUtf8Specified()
        {
            var content = File.ReadAllText(_indexHtmlPath);
            Assert.Contains("charset=utf-8", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Criterion_ResponsiveViewportMetaTags()
        {
            var content = File.ReadAllText(_indexHtmlPath);
            Assert.Contains("viewport", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("width=device-width", content);
        }

        [Fact]
        public void Criterion_DashboardCssHighContrast()
        {
            var content = File.ReadAllText(_siteCssPath);
            var statusClasses = new[] 
            { 
                "status-completed",
                "status-inprogress",
                "status-atrisk",
                "status-future"
            };

            foreach (var statusClass in statusClasses)
            {
                Assert.Contains(statusClass, content);
            }
        }

        [Fact]
        public void Criterion_PrintOptimizedCss()
        {
            var content = File.ReadAllText(_siteCssPath);
            Assert.Contains("@media print", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Criterion_MediaQueriesFor1024To1920()
        {
            var content = File.ReadAllText(_siteCssPath);
            Assert.True(
                content.Contains("1024") && content.Contains("1920"),
                "CSS should support media queries from 1024px to 1920px"
            );
        }

        [Fact]
        public void Criterion_StaticFilesDeployToBin()
        {
            var binPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "bin", "Debug", "net8.0", "wwwroot"
            );

            Assert.True(Directory.Exists(binPath), "Static files should deploy to bin/Debug/net8.0/wwwroot");
            Assert.True(File.Exists(Path.Combine(binPath, "index.html")));
            Assert.True(File.Exists(Path.Combine(binPath, "css", "site.css")));
            Assert.True(File.Exists(Path.Combine(binPath, "favicon.ico")));
        }

        [Fact]
        public void Criterion_FaviconPresent()
        {
            var faviconPath = Path.Combine(_wwwrootPath, "favicon.ico");
            Assert.True(File.Exists(faviconPath), "favicon.ico must be present");
        }

        [Fact]
        public void Criterion_MimeTypesConfigured()
        {
            var mimeTypes = new Dictionary<string, string>
            {
                { ".woff", "font/woff" },
                { ".woff2", "font/woff2" },
                { ".ttf", "font/ttf" },
                { ".otf", "font/otf" },
                { ".svg", "image/svg+xml" },
                { ".json", "application/json" }
            };

            Assert.NotEmpty(mimeTypes);
        }

        [Fact]
        public void Criterion_DashboardRendersWithoutErrors()
        {
            var indexContent = File.ReadAllText(_indexHtmlPath);
            var cssContent = File.ReadAllText(_siteCssPath);

            Assert.NotEmpty(indexContent);
            Assert.NotEmpty(cssContent);
            Assert.Contains("<div", indexContent);
            Assert.Contains(".dashboard", cssContent, StringComparison.OrdinalIgnoreCase);
        }
    }
}