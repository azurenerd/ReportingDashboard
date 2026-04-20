using ReportingDashboard.Web.Models;

namespace ReportingDashboard.Web.Tests.Unit;

public class ProjectPlaceholderTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Placeholder_HasExpectedTitle()
    {
        Project.Placeholder.Title.Should().Be("(data.json error)");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Placeholder_HasExpectedSubtitle()
    {
        Project.Placeholder.Subtitle.Should().Be("see error banner above");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Placeholder_HasDefaultBacklogLinkText()
    {
        Project.Placeholder.BacklogLinkText.Should().Be("\u2192 ADO Backlog");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Placeholder_BacklogUrlIsNullByDefault()
    {
        Project.Placeholder.BacklogUrl.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Placeholder_IsSingletonReference()
    {
        var a = Project.Placeholder;
        var b = Project.Placeholder;
        ReferenceEquals(a, b).Should().BeTrue();
    }
}