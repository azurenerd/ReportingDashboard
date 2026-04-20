using System.IO;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace ReportingDashboard.Web.Tests.Unit;

[Trait("Category", "Unit")]
public class LayoutCssTests
{
    private static string FindRepoFile(string relativePath)
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }
        throw new FileNotFoundException($"Could not locate {relativePath} walking up from test output directory.");
    }

    private static string ReadAppCss() =>
        File.ReadAllText(FindRepoFile("src/ReportingDashboard.Web/wwwroot/app.css"));

    private static string ReadDashboardCss() =>
        File.ReadAllText(FindRepoFile("src/ReportingDashboard.Web/Components/Pages/Dashboard.razor.css"));

    private static string StripCssComments(string css) =>
        Regex.Replace(css, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

    [Fact]
    public void AppCss_ContainsResetAndBodySizingAndFontStack()
    {
        var css = ReadAppCss();
        css.Should().Contain("*{margin:0");
        css.Should().Contain("box-sizing:border-box");
        css.Should().Contain("width:1920px");
        css.Should().Contain("height:1080px");
        css.Should().Contain("overflow:hidden");
        css.Should().Contain("'Segoe UI'");
        css.Should().Contain("'Helvetica Neue'");
        css.Should().Contain("flex-direction:column");
    }

    [Theory]
    [InlineData(".hdr")]
    [InlineData(".sub")]
    [InlineData(".tl-area")]
    [InlineData(".tl-labels")]
    [InlineData(".tl-svg-box")]
    [InlineData(".hm-wrap")]
    [InlineData(".hm-title")]
    [InlineData(".hm-grid")]
    [InlineData(".hm-corner")]
    [InlineData(".hm-col-hdr")]
    [InlineData(".hm-row-hdr")]
    [InlineData(".hm-cell")]
    [InlineData(".it")]
    [InlineData(".ship-hdr")]
    [InlineData(".ship-cell")]
    [InlineData(".prog-hdr")]
    [InlineData(".prog-cell")]
    [InlineData(".carry-hdr")]
    [InlineData(".carry-cell")]
    [InlineData(".block-hdr")]
    [InlineData(".block-cell")]
    [InlineData(".current")]
    [InlineData(".current-hdr")]
    [InlineData(".error-banner")]
    public void DashboardCss_DefinesRequiredClass(string selector)
    {
        var css = ReadDashboardCss();
        css.Should().Contain(selector, $"selector {selector} must be defined in Dashboard.razor.css");
    }

    [Fact]
    public void DashboardCss_NoAprClassRemains()
    {
        var css = StripCssComments(ReadDashboardCss());
        Regex.IsMatch(css, @"\.apr\b")
            .Should().BeFalse(".apr must have been renamed to .current (outside of comments)");
        Regex.IsMatch(css, @"\.apr-hdr\b")
            .Should().BeFalse(".apr-hdr must have been renamed to .current-hdr (outside of comments)");
    }

    [Fact]
    public void DashboardCss_GridTemplateAndTimelineHeightAreExact()
    {
        var css = ReadDashboardCss();
        css.Should().Contain("grid-template-columns:160px repeat(4,1fr)");
        css.Should().Contain("grid-template-rows:36px repeat(4,1fr)");
        css.Should().Contain("height:196px");
    }

    [Theory]
    [InlineData("#0078D4")]
    [InlineData("#34A853")]
    [InlineData("#F4B400")]
    [InlineData("#EA4335")]
    [InlineData("#FFF0D0")]
    [InlineData("#C07700")]
    [InlineData("#E8F5E9")]
    [InlineData("#FEF2F2")]
    [InlineData("#AAA")]
    public void DashboardCss_CanonicalHexColorsPresent(string hex)
    {
        var css = ReadDashboardCss();
        css.Should().Contain(hex);
    }
}