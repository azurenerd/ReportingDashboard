using Bunit;
using FluentAssertions;
using ReportingDashboard.Web.Components.Pages.Partials;
using ReportingDashboard.Web.Models;
using Xunit;

namespace ReportingDashboard.Web.Tests.Components;

public class DashboardHeaderTests : TestContext
{
    private static Project MakeProject(string? backlogUrl = "https://dev.azure.com/contoso/privacy/_backlogs/backlog/") => new()
    {
        Title = "Privacy Automation Release Roadmap",
        Subtitle = "Trusted Platform \u2022 Privacy Automation Workstream \u2022 April 2026",
        BacklogUrl = backlogUrl
    };

    [Fact]
    public void Renders_Title_Subtitle_And_NowLabel()
    {
        var cut = RenderComponent<DashboardHeader>(p => p
            .Add(h => h.Project, MakeProject())
            .Add(h => h.NowLabel, "Now (Apr 2026)"));

        var h1 = cut.Find(".hdr h1");
        h1.TextContent.Should().Contain("Privacy Automation Release Roadmap");

        cut.Find(".sub").TextContent.Should().Contain("Privacy Automation Workstream");
        cut.Markup.Should().Contain("Now (Apr 2026)");
    }

    [Fact]
    public void Renders_Backlog_Link_As_Anchor_When_Url_Is_Http()
    {
        var cut = RenderComponent<DashboardHeader>(p => p
            .Add(h => h.Project, MakeProject("http://example.com/backlog"))
            .Add(h => h.NowLabel, "Now (Apr 2026)"));

        var anchor = cut.Find("a.backlog-link");
        anchor.GetAttribute("href").Should().Be("http://example.com/backlog");
    }

    [Fact]
    public void Renders_Backlog_Link_As_Anchor_When_Url_Is_Https()
    {
        var cut = RenderComponent<DashboardHeader>(p => p
            .Add(h => h.Project, MakeProject("https://dev.azure.com/x/_backlogs/"))
            .Add(h => h.NowLabel, "Now (Apr 2026)"));

        cut.FindAll("a.backlog-link").Should().HaveCount(1);
    }

    [Fact]
    public void Renders_Backlog_As_Plain_Text_When_Url_Is_Null()
    {
        var cut = RenderComponent<DashboardHeader>(p => p
            .Add(h => h.Project, MakeProject(null))
            .Add(h => h.NowLabel, "Now (Apr 2026)"));

        cut.FindAll("a.backlog-link").Should().BeEmpty();
        cut.FindAll("span.backlog-link").Should().HaveCount(1);
    }

    [Fact]
    public void Renders_Backlog_As_Plain_Text_When_Url_Is_Empty_Or_Whitespace()
    {
        var cut = RenderComponent<DashboardHeader>(p => p
            .Add(h => h.Project, MakeProject("   "))
            .Add(h => h.NowLabel, "Now (Apr 2026)"));

        cut.FindAll("a.backlog-link").Should().BeEmpty();
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://example.com/file")]
    [InlineData("file:///etc/passwd")]
    [InlineData("not a url")]
    [InlineData("/relative/path")]
    public void Renders_Backlog_As_Plain_Text_When_Url_Is_Not_Http_Or_Https(string badUrl)
    {
        var cut = RenderComponent<DashboardHeader>(p => p
            .Add(h => h.Project, MakeProject(badUrl))
            .Add(h => h.NowLabel, "Now (Apr 2026)"));

        cut.FindAll("a.backlog-link").Should().BeEmpty();
        cut.FindAll("span.backlog-link").Should().HaveCount(1);
    }

    [Fact]
    public void Renders_All_Four_Legend_Items()
    {
        var cut = RenderComponent<DashboardHeader>(p => p
            .Add(h => h.Project, MakeProject())
            .Add(h => h.NowLabel, "Now (Apr 2026)"));

        var items = cut.FindAll(".hdr-legend .legend-item");
        items.Count.Should().Be(4);

        cut.Markup.Should().Contain("PoC Milestone");
        cut.Markup.Should().Contain("Production Release");
        cut.Markup.Should().Contain("Checkpoint");
        cut.Markup.Should().Contain("Now (Apr 2026)");
    }

    [Fact]
    public void Renders_Legend_Shape_Classes()
    {
        var cut = RenderComponent<DashboardHeader>(p => p
            .Add(h => h.Project, MakeProject())
            .Add(h => h.NowLabel, "Now (Apr 2026)"));

        cut.FindAll(".legend-diamond--poc").Should().HaveCount(1);
        cut.FindAll(".legend-diamond--prod").Should().HaveCount(1);
        cut.FindAll(".legend-circle").Should().HaveCount(1);
        cut.FindAll(".legend-bar").Should().HaveCount(1);
    }

    [Fact]
    public void Renders_Hdr_Container_Class()
    {
        var cut = RenderComponent<DashboardHeader>(p => p
            .Add(h => h.Project, MakeProject())
            .Add(h => h.NowLabel, "Now (Apr 2026)"));

        cut.FindAll("header.hdr").Should().HaveCount(1);
    }
}