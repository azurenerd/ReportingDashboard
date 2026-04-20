using System.IO;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Web.Tests.Unit;

[Trait("Category", "Unit")]
public class DashboardRazorSourceTests
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
    public void DashboardRazor_HasPageRouteAndNoRenderMode()
    {
        var razor = ReadAsset("src/ReportingDashboard.Web/Components/Pages/Dashboard.razor");

        razor.Should().Contain("@page \"/\"");

        // The razor file contains a comment mentioning "@rendermode" as documentation.
        // Assert there is no actual @rendermode directive (line starting with @rendermode)
        // and no render-mode= attribute.
        Regex.IsMatch(razor, @"^\s*@rendermode\b", RegexOptions.Multiline)
            .Should().BeFalse("Dashboard.razor must not have an @rendermode directive");
        razor.Should().NotContain("render-mode=");
    }
}