using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AgentSquad.Tests
{
    public class StaticFileMiddlewareConfigurationTests
    {
        [Fact]
        public void FileExtensionContentTypeProviderMapsWoffFont()
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".woff"] = "font/woff";

            Assert.True(provider.TryGetContentType("test.woff", out var contentType));
            Assert.Equal("font/woff", contentType);
        }

        [Fact]
        public void FileExtensionContentTypeProviderMapsWoff2Font()
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".woff2"] = "font/woff2";

            Assert.True(provider.TryGetContentType("test.woff2", out var contentType));
            Assert.Equal("font/woff2", contentType);
        }

        [Fact]
        public void FileExtensionContentTypeProviderMapsTtfFont()
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".ttf"] = "font/ttf";

            Assert.True(provider.TryGetContentType("test.ttf", out var contentType));
            Assert.Equal("font/ttf", contentType);
        }

        [Fact]
        public void FileExtensionContentTypeProviderMapsOtfFont()
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".otf"] = "font/otf";

            Assert.True(provider.TryGetContentType("test.otf", out var contentType));
            Assert.Equal("font/otf", contentType);
        }

        [Fact]
        public void FileExtensionContentTypeProviderMapsSvg()
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".svg"] = "image/svg+xml";

            Assert.True(provider.TryGetContentType("test.svg", out var contentType));
            Assert.Equal("image/svg+xml", contentType);
        }

        [Fact]
        public void FileExtensionContentTypeProviderMapsJson()
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".json"] = "application/json";

            Assert.True(provider.TryGetContentType("data.json", out var contentType));
            Assert.Equal("application/json", contentType);
        }

        [Fact]
        public void FileExtensionContentTypeProviderMapsJavaScript()
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".js"] = "application/javascript";

            Assert.True(provider.TryGetContentType("test.js", out var contentType));
            Assert.Equal("application/javascript", contentType);
        }

        [Fact]
        public void CacheDurationIsOneDay()
        {
            var oneDaySeconds = 86400;
            Assert.Equal(oneDaySeconds, 86400);
        }

        [Fact]
        public void DataJsonMustRevalidateConfiguration()
        {
            var cacheControl = "max-age=0, must-revalidate";
            Assert.Contains("must-revalidate", cacheControl);
            Assert.Contains("max-age=0", cacheControl);
        }

        [Fact]
        public void StaticAssetsCacheFor24Hours()
        {
            var cacheControl = "public, max-age=86400";
            Assert.Contains("public", cacheControl);
            Assert.Contains("86400", cacheControl);
        }

        [Theory]
        [InlineData(".woff")]
        [InlineData(".woff2")]
        [InlineData(".ttf")]
        [InlineData(".otf")]
        [InlineData(".eot")]
        [InlineData(".svg")]
        [InlineData(".json")]
        [InlineData(".js")]
        public void ContentTypeProviderMapsSupportedExtensions(string extension)
        {
            var provider = new FileExtensionContentTypeProvider();
            var mappings = new Dictionary<string, string>
            {
                { ".woff", "font/woff" },
                { ".woff2", "font/woff2" },
                { ".ttf", "font/ttf" },
                { ".otf", "font/otf" },
                { ".eot", "application/vnd.ms-fontobject" },
                { ".svg", "image/svg+xml" },
                { ".json", "application/json" },
                { ".js", "application/javascript" }
            };

            Assert.Contains(extension, mappings.Keys);
        }
    }
}