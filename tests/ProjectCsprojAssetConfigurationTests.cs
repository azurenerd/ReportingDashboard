using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Xunit;

namespace AgentSquad.Tests
{
    public class ProjectCsprojAssetConfigurationTests
    {
        private readonly string _csprojPath;
        private readonly XDocument _csprojDocument;

        public ProjectCsprojAssetConfigurationTests()
        {
            _csprojPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "AgentSquad.Runner.csproj"
            );
            _csprojDocument = XDocument.Load(_csprojPath);
        }

        [Fact]
        public void CsprojFileExists()
        {
            Assert.True(File.Exists(_csprojPath), "AgentSquad.Runner.csproj must exist");
        }

        [Theory]
        [InlineData("wwwroot/css/site.css")]
        [InlineData("wwwroot/index.html")]
        [InlineData("wwwroot/favicon.ico")]
        public void CsprojIncludesWwwrootContent(string contentPath)
        {
            var hasContent = CsprojContainsContent(contentPath);
            Assert.True(
                hasContent,
                $"csproj should include {contentPath} for deployment"
            );
        }

        [Fact]
        public void WwwrootCssHasCopyToOutputDirectory()
        {
            var cssContent = FindCsprojContent("wwwroot/css/site.css");
            Assert.NotNull(cssContent);
            
            var copyAttribute = cssContent?.Attribute("CopyToOutputDirectory");
            Assert.NotNull(copyAttribute);
            Assert.Equal("PreserveNewest", copyAttribute?.Value);
        }

        [Fact]
        public void WwwrootIndexHtmlHasCopyToOutputDirectory()
        {
            var indexContent = FindCsprojContent("wwwroot/index.html");
            Assert.NotNull(indexContent);
            
            var copyAttribute = indexContent?.Attribute("CopyToOutputDirectory");
            Assert.NotNull(copyAttribute);
            Assert.Equal("PreserveNewest", copyAttribute?.Value);
        }

        [Fact]
        public void WwwrootFaviconHasCopyToOutputDirectory()
        {
            var faviconContent = FindCsprojContent("wwwroot/favicon.ico");
            Assert.NotNull(faviconContent);
            
            var copyAttribute = faviconContent?.Attribute("CopyToOutputDirectory");
            Assert.NotNull(copyAttribute);
            Assert.Equal("PreserveNewest", copyAttribute?.Value);
        }

        private bool CsprojContainsContent(string contentPath)
        {
            return FindCsprojContent(contentPath) != null;
        }

        private XElement FindCsprojContent(string contentPath)
        {
            var xmlns = XNamespace.Get("http://schemas.microsoft.com/developer/msbuild/2003");
            var elements = _csprojDocument.Descendants(xmlns + "Content");
            
            foreach (var element in elements)
            {
                var include = element.Attribute("Include");
                if (include?.Value == contentPath || include?.Value.Replace("\\", "/") == contentPath)
                {
                    return element;
                }
            }

            return null;
        }
    }
}