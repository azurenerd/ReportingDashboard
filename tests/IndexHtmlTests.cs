using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Xunit;

namespace AgentSquad.Tests
{
    public class IndexHtmlTests
    {
        private readonly string _indexHtmlPath;
        private readonly string _indexContent;

        public IndexHtmlTests()
        {
            _indexHtmlPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");
            _indexContent = File.ReadAllText(_indexHtmlPath);
        }

        [Fact]
        public void IndexHtmlContainsDoctype()
        {
            Assert.Contains("<!DOCTYPE html>", _indexContent, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void IndexHtmlHasCharsetUtf8()
        {
            Assert.Contains("charset=utf-8", _indexContent, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void IndexHtmlHasBaseHrefTag()
        {
            Assert.Contains("<base href=\"/\"", _indexContent, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void IndexHtmlHasViewportMetaTag()
        {
            Assert.Contains("name=\"viewport\"", _indexContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("width=device-width", _indexContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("initial-scale=1", _indexContent, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void IndexHtmlLinksBootstrap5Css()
        {
            Assert.Contains("bootstrap", _indexContent, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(".css", _indexContent);
        }

        [Fact]
        public void IndexHtmlLinksChartJs()
        {
            Assert.Contains("chart.js", _indexContent, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void IndexHtmlLinksSiteCss()
        {
            Assert.Contains("css/site.css", _indexContent);
        }

        [Fact]
        public void IndexHtmlIncludesBlazorServerJs()
        {
            Assert.Contains("_framework/blazor.server.js", _indexContent);
        }

        [Fact]
        public void IndexHtmlHasFaviconReference()
        {
            Assert.Contains("favicon.ico", _indexContent, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void IndexHtmlHasRootAppElement()
        {
            Assert.Contains("<div id=\"app\"", _indexContent) || Assert.Contains("<app ", _indexContent);
        }

        [Fact]
        public void IndexHtmlHasLoadingMessage()
        {
            Assert.Contains("Loading", _indexContent) || Assert.Contains("loading", _indexContent);
        }

        [Theory]
        [InlineData("<html")]
        [InlineData("</html>")]
        [InlineData("<head")]
        [InlineData("</head>")]
        [InlineData("<body")]
        [InlineData("</body>")]
        public void IndexHtmlHasRequiredElements(string element)
        {
            Assert.Contains(element, _indexContent, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void IndexHtmlHasProperLanguageAttribute()
        {
            Assert.Contains("lang=", _indexContent, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void IndexHtmlNoMixedContentWarnings()
        {
            var lowerContent = _indexContent.ToLower();
            Assert.DoesNotContain("http://", lowerContent.Substring(lowerContent.IndexOf("<head") ?? 0));
            Assert.True(
                lowerContent.Contains("https://") || !lowerContent.Contains("://cdn"),
                "CDN links should use HTTPS to avoid mixed content warnings"
            );
        }

        [Fact]
        public void IndexHtmlValidatesAsXhtml()
        {
            try
            {
                var doc = XDocument.Parse("<root>" + _indexContent + "</root>");
                Assert.NotNull(doc);
            }
            catch (XmlException ex)
            {
                Assert.True(false, $"index.html has XML validation errors: {ex.Message}");
            }
        }
    }
}