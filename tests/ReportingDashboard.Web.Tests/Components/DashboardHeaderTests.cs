using Bunit;
using FluentAssertions;
using ReportingDashboard.Web.Models;
using Xunit;

namespace ReportingDashboard.Web.Tests.Components;

[Trait("Category", "Unit")]
public class DashboardHeaderTests : IDisposable
{
    private readonly Bunit.TestContext _ctx = new();

    public void Dispose() => _ctx.Dispose();

    private static Project MakeProject(
        string? backlogUrl = "https://dev.azure.com/x",
        string title = "Privacy Roadmap",
        string subtitle = "Trusted Platform - Privacy - April 2026") => new()
    {
        Title = title,
        Subtitle = subtitle,
        BacklogUrl = backlogUrl,
        BacklogLinkText = "ADO Backlog"
    };

    [Fact]
    public void Renders_Title_Subtitle_And_Legend()
    {
        var cut = _ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Partials.DashboardHeader>(p => p
            .Add(x => x.Project, MakeProject())
            .Add(x => x.NowLabel, "Now (Apr 2026)"));

        cut.Find("header.hdr").Should().NotBeNull();
        cut.Find("h1").TextContent.Should().Contain("Privacy Roadmap");
        cut.Find(".sub").TextContent.Should().Be("Trusted Platform - Privacy - April 2026");
        cut.FindAll(".legend-item").Count.Should().Be(4);
    }

    [Fact]
    public void Renders_Active_Backlog_Link_For_Https_Url()
    {
        var cut = _ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Partials.DashboardHeader>(p => p
            .Add(x => x.Project, MakeProject("https://dev.azure.com/x"))
            .Add(x => x.NowLabel, "Now (Apr 2026)"));

        var anchor = cut.Find("a.backlog-link");
        anchor.GetAttribute("href").Should().Be("https://dev.azure.com/x");
        anchor.TextContent.Trim().Should().Be("ADO Backlog");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ftp://x/y")]
    [InlineData("javascript:alert(1)")]
    [InlineData("file:///c:/x")]
    [InlineData("not-a-url")]
    public void Renders_Disabled_Span_When_Url_Is_Unsafe_Or_Missing(string? url)
    {
        var cut = _ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Partials.DashboardHeader>(p => p
            .Add(x => x.Project, MakeProject(url))
            .Add(x => x.NowLabel, "Now (Apr 2026)"));

        cut.FindAll("a.backlog-link").Should().BeEmpty();
        cut.FindAll("span.backlog-link--disabled").Count.Should().Be(1);
    }

    [Fact]
    public void Renders_NowLabel_Verbatim_In_Last_Legend_Item()
    {
        var cut = _ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Partials.DashboardHeader>(p => p
            .Add(x => x.Project, MakeProject())
            .Add(x => x.NowLabel, "CUSTOM_LABEL_XYZ"));

        var items = cut.FindAll(".legend-item");
        items[items.Count - 1].TextContent.Should().Contain("CUSTOM_LABEL_XYZ");
    }

    [Fact]
    public void Html_Encodes_Hostile_Title()
    {
        var project = MakeProject(title: "<script>alert(1)</script>");

        var cut = _ctx.RenderComponent<ReportingDashboard.Web.Components.Pages.Partials.DashboardHeader>(p => p
            .Add(x => x.Project, project)
            .Add(x => x.NowLabel, "Now (Apr 2026)"));

        cut.Markup.Should().NotContain("<script>alert(1)</script>");
        cut.Markup.Should().Contain("&lt;script&gt;");
    }
}