using System.IO;
using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Web.Tests.Unit;

[Trait("Category", "Unit")]
public class AppCssStrictContentTests
{
    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "src")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? Directory.GetCurrentDirectory();
    }

    private static string ReadAsset(string relativePath)
    {
        var root = FindRepoRoot();
        var full = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        return File.ReadAllText(full);
    }

    [Fact]
    public void AppCss_ContainsFlexColumnRootLayout()
    {
        var css = ReadAsset("src/ReportingDashboard.Web/wwwroot/app.css");
        css.Should().Contain("display: flex");
        css.Should().Contain("flex-direction: column");
    }

    [Fact]
    public void AppCss_DoesNotContainComponentLevelRules()
    {
        var css = ReadAsset("src/ReportingDashboard.Web/wwwroot/app.css");

        // Component styles must live in Dashboard.razor.css, not app.css
        css.Should().NotContain(".hdr");
        css.Should().NotContain(".hm-grid");
        css.Should().NotContain(".tl-area");
        css.Should().NotContain(".error-banner");
        css.Should().NotContain(".ship-cell");
    }

    [Fact]
    public void AppCss_ContainsBackgroundAndTextColor()
    {
        var css = ReadAsset("src/ReportingDashboard.Web/wwwroot/app.css");
        css.Should().Contain("#FFFFFF");
        css.Should().Contain("#111");
    }
}