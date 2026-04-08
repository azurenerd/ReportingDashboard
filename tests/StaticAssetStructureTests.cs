using System;
using System.IO;
using Xunit;

namespace AgentSquad.Tests
{
    public class StaticAssetStructureTests
    {
        private readonly string _wwwrootPath;

        public StaticAssetStructureTests()
        {
            _wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        [Fact]
        public void WwwrootDirectoryExists()
        {
            Assert.True(Directory.Exists(_wwwrootPath), "wwwroot directory must exist");
        }

        [Theory]
        [InlineData("css")]
        [InlineData("js")]
        [InlineData("images")]
        public void RequiredSubdirectoriesExist(string subdirectory)
        {
            var path = Path.Combine(_wwwrootPath, subdirectory);
            Assert.True(Directory.Exists(path), $"wwwroot/{subdirectory} subdirectory must exist");
        }

        [Fact]
        public void IndexHtmlExists()
        {
            var indexPath = Path.Combine(_wwwrootPath, "index.html");
            Assert.True(File.Exists(indexPath), "wwwroot/index.html must exist");
        }

        [Fact]
        public void FaviconExists()
        {
            var faviconPath = Path.Combine(_wwwrootPath, "favicon.ico");
            Assert.True(File.Exists(faviconPath), "wwwroot/favicon.ico must exist");
        }

        [Fact]
        public void DashboardCssExists()
        {
            var cssPath = Path.Combine(_wwwrootPath, "css", "site.css");
            Assert.True(File.Exists(cssPath), "wwwroot/css/site.css must exist");
        }

        [Fact]
        public void IndexHtmlCopiedToBuildOutput()
        {
            var buildOutputPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "bin", "Debug", "net8.0", "wwwroot", "index.html"
            );
            Assert.True(File.Exists(buildOutputPath), "index.html must be copied to bin output directory");
        }

        [Fact]
        public void SiteCssCopiedToBuildOutput()
        {
            var buildOutputPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "bin", "Debug", "net8.0", "wwwroot", "css", "site.css"
            );
            Assert.True(File.Exists(buildOutputPath), "site.css must be copied to bin output directory");
        }
    }
}