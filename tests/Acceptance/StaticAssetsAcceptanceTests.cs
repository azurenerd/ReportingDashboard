using Xunit;
using System;
using System.IO;
using System.Linq;

namespace AgentSquad.Tests.Acceptance
{
    public class StaticAssetsAcceptanceTests
    {
        private readonly string _projectRoot = AppDomain.CurrentDomain.BaseDirectory;
        private readonly string _wwwrootPath;

        public StaticAssetsAcceptanceTests()
        {
            _wwwrootPath = Path.Combine(_projectRoot, "wwwroot");
        }

        [Fact]
        public void DashboardCSSExists_And_IsResponsive()
        {
            var cssPath = Path.Combine(_wwwrootPath, "css", "dashboard.css");
            Assert.True(File.Exists(cssPath), "dashboard.css should exist in wwwroot/css/");
            
            var content = File.ReadAllText(cssPath);
            Assert.NotEmpty(content);
            Assert.Contains("@media", content, "Should contain media queries for responsive design");
        }

        [Fact]
        public void PrintCSSExists_With_PrintMediaQueries()
        {
            var printCssPath = Path.Combine(_wwwrootPath, "css", "print.css");
            Assert.True(File.Exists(printCssPath), "print.css should exist");
            
            var content = File.ReadAllText(printCssPath);
            Assert.Contains("@media print", content, "Should contain print media queries");
        }

        [Fact]
        public void BaseCSSExists_With_ColorPalette()
        {
            var baseCssPath = Path.Combine(_wwwrootPath, "css", "base.css");
            Assert.True(File.Exists(baseCssPath), "base.css should exist");
            
            var content = File.ReadAllText(baseCssPath);
            Assert.NotEmpty(content);
            Assert.Contains("27AE60", content, "Should define executive green color");
            Assert.Contains("E74C3C", content, "Should define warning red color");
            Assert.Contains("3498DB", content, "Should define info blue color");
        }

        [Fact]
        public void DashboardJSExists_And_ContainsInitFunction()
        {
            var dashboardJsPath = Path.Combine(_wwwrootPath, "js", "dashboard.js");
            Assert.True(File.Exists(dashboardJsPath), "dashboard.js should exist");
            
            var content = File.ReadAllText(dashboardJsPath);
            Assert.Contains("init:", content, "Should contain init function");
            Assert.Contains("initializeTimeline", content, "Should contain timeline initialization");
        }

        [Fact]
        public void PrintHandlerJSExists_And_ContainsPrintOptimization()
        {
            var printHandlerPath = Path.Combine(_wwwrootPath, "js", "print-handler.js");
            Assert.True(File.Exists(printHandlerPath), "print-handler.js should exist");
            
            var content = File.ReadAllText(printHandlerPath);
            Assert.Contains("onPrintStart", content, "Should contain print start handler");
            Assert.Contains("hideScrollbars", content, "Should contain scrollbar hiding logic");
        }

        [Fact]
        public void WwwrootDirectoryStructureIsCorrect()
        {
            Assert.True(Directory.Exists(Path.Combine(_wwwrootPath, "css")), "css directory should exist");
            Assert.True(Directory.Exists(Path.Combine(_wwwrootPath, "js")), "js directory should exist");
            Assert.True(Directory.Exists(Path.Combine(_wwwrootPath, "lib")), "lib directory should exist");
            Assert.True(Directory.Exists(Path.Combine(_wwwrootPath, "images")), "images directory should exist");
        }

        [Fact]
        public void ResponsiveBreakpointsAreDefined()
        {
            var dashboardCssPath = Path.Combine(_wwwrootPath, "css", "dashboard.css");
            var content = File.ReadAllText(dashboardCssPath);
            
            Assert.Contains("1024px", content, "Should define 1024px breakpoint for tablet");
            Assert.Contains("1280px", content, "Should define 1280px breakpoint for desktop");
            Assert.Contains("1920px", content, "Should define 1920px breakpoint for presentation");
        }

        [Fact]
        public void PrintOptimizedCSSMinimizesClipping()
        {
            var printCssPath = Path.Combine(_wwwrootPath, "css", "print.css");
            var content = File.ReadAllText(printCssPath);
            
            Assert.Contains("1280x720", content, "Should optimize for 1280x720 resolution");
            Assert.Contains("1920x1080", content, "Should optimize for 1920x1080 resolution");
            Assert.Contains("overflow", content, "Should manage overflow for print");
        }

        [Fact]
        public void ChartJSLibraryIncluded()
        {
            var indexHtmlPath = Path.Combine(_wwwrootPath, "index.html");
            if (File.Exists(indexHtmlPath))
            {
                var content = File.ReadAllText(indexHtmlPath);
                Assert.Contains("chart", content.ToLower(), "Should reference Chart.js library");
            }
        }

        [Fact]
        public void CSSGridLayoutClassesDefined()
        {
            var dashboardCssPath = Path.Combine(_wwwrootPath, "css", "dashboard.css");
            var content = File.ReadAllText(dashboardCssPath);
            
            Assert.Contains("metrics-grid", content, "Should define metrics-grid class");
            Assert.Contains("timeline-container", content, "Should define timeline-container class");
            Assert.Contains("work-item-column", content, "Should define work-item-column class");
        }

        [Fact]
        public void NoHttp404ForStaticAssets()
        {
            var cssFiles = Directory.GetFiles(Path.Combine(_wwwrootPath, "css"), "*.css");
            Assert.NotEmpty(cssFiles);
            
            var jsFiles = Directory.GetFiles(Path.Combine(_wwwrootPath, "js"), "*.js");
            Assert.NotEmpty(jsFiles);
        }

        [Fact]
        public void FontFamilyStackDefined()
        {
            var baseCssPath = Path.Combine(_wwwrootPath, "css", "base.css");
            var content = File.ReadAllText(baseCssPath);
            
            Assert.Contains("font-family", content, "Should define font-family stack");
            Assert.Contains("Segoe UI", content, "Should use Segoe UI in font stack");
        }

        [Fact]
        public void PrintMediaQueriesRemoveNavigation()
        {
            var printCssPath = Path.Combine(_wwwrootPath, "css", "print.css");
            var content = File.ReadAllText(printCssPath);
            
            Assert.Contains("display: none", content, "Should hide elements during print");
        }
    }
}