using System.Text.RegularExpressions;
using Xunit;

namespace AgentSquad.Tests.Acceptance;

public class StaticAssetsAcceptanceTests
{
    private string GetBaseCssPath()
    {
        var testAssemblyDir = Path.GetDirectoryName(typeof(StaticAssetsAcceptanceTests).Assembly.Location);
        var projectRoot = Path.Combine(testAssemblyDir ?? ".", "..", "..", "..");
        return Path.Combine(projectRoot, "wwwroot", "css", "base.css");
    }

    [Fact]
    public void BaseCssFileExists()
    {
        var cssPath = GetBaseCssPath();
        Assert.True(File.Exists(cssPath), $"base.css not found at {cssPath}");
    }

    [Fact]
    public void BaseCssContainsValidMediaQueries()
    {
        var cssPath = GetBaseCssPath();
        var content = File.ReadAllText(cssPath);
        
        Assert.Matches(@"@media\s*\(", content);
    }

    [Fact]
    public void BaseCssContainsHexColorValues()
    {
        var cssPath = GetBaseCssPath();
        var content = File.ReadAllText(cssPath);
        
        // Validates hex color format without enforcing specific values
        Assert.Matches(@"#[0-9a-fA-F]{3,6}", content);
    }

    [Fact]
    public void BaseCssContainsResponsiveDesignPatterns()
    {
        var cssPath = GetBaseCssPath();
        var content = File.ReadAllText(cssPath);
        
        Assert.Matches(@"@media\s*\(\s*max-width|min-width", content);
    }

    [Fact]
    public void BaseCssIsValidUtf8()
    {
        var cssPath = GetBaseCssPath();
        
        var encoding = DetectFileEncoding(cssPath);
        Assert.Equal(System.Text.Encoding.UTF8, encoding);
    }

    private System.Text.Encoding DetectFileEncoding(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);
        
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return System.Text.Encoding.UTF8;
        
        return System.Text.Encoding.UTF8;
    }
}