using System.IO;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Web.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardCssAssetTests
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
        File.Exists(full).Should().BeTrue($"Expected asset file at {full}");
        return File.ReadAllText(full);
    }

    private static string StripCssComments(string css) =>
        Regex.Replace(css, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

    [Fact]
    public void DashboardCss_DoesNotContainRenamedAprSelectors()
    {
        var css = StripCssComments(
            ReadAsset("src/ReportingDashboard.Web/Components/Pages/Dashboard.razor.css"));

        css.Should().NotContain(".apr-hdr");
        css.Should().NotContain(".apr ");
        css.Should().NotContain(".apr{");
        css.Should().NotContain(".apr.");
        css.Should().NotContain(".apr,");
    }

    [Fact]
    public void DashboardCss_ContainsCurrentRenameAndGridTemplate()
    {
        var css = ReadAsset("src/ReportingDashboard.Web/Components/Pages/Dashboard.razor.css");

        css.Should().Contain(".current");
        css.Should().Contain("current-hdr");
        css.Should().Contain("grid-template-columns: 160px repeat(4, 1fr)");
        css.Should().Contain("grid-template-rows: 36px repeat(4, 1fr)");
    }

    [Fact]
    public void DashboardCss_ContainsCategoryColorPalette()
    {
        var css = ReadAsset("src/ReportingDashboard.Web/Components/Pages/Dashboard.razor.css");

        foreach (var color in new[] { "#34A853", "#0078D4", "#F4B400", "#EA4335", "#FFF0D0", "#C07700", "#1B7A28", "#1565C0", "#B45309", "#991B1B" })
        {
            css.Should().Contain(color, $"palette must include {color}");
        }
    }

    [Fact]
    public void DashboardCss_ContainsPseudoElementDotRule()
    {
        var css = ReadAsset("src/ReportingDashboard.Web/Components/Pages/Dashboard.razor.css");

        css.Should().Contain(".it::before");
        css.Should().Contain("border-radius: 50%");
        css.Should().Contain("width: 6px");
        css.Should().Contain("height: 6px");
        css.Should().Contain("top: 7px");
    }

    [Fact]
    public void AppCss_ContainsGlobalResetAnd1920x1080BodyRule()
    {
        var css = ReadAsset("src/ReportingDashboard.Web/wwwroot/app.css");

        css.Should().Contain("margin:").And.Contain("padding:").And.Contain("box-sizing:");
        css.Should().Contain("1920px");
        css.Should().Contain("1080px");
        css.Should().Contain("overflow:").And.Contain("hidden");
        css.Should().Contain("Segoe UI");
    }
}