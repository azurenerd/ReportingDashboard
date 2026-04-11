using Bunit;
using FluentAssertions;
using ReportingDashboard.Components;
using Xunit;

namespace ReportingDashboard.Tests.Unit.Components;

/// <summary>
/// Timeline.razor is currently a placeholder with no [Parameter] properties.
/// These tests verify the placeholder renders without errors.
/// Full parameter-driven tests will be added when the component is implemented.
/// </summary>
[Trait("Category", "Unit")]
public class TimelineComponentTests : TestContext
{
    [Fact]
    public void Timeline_Placeholder_RendersWithoutException()
    {
        var cut = RenderComponent<Timeline>();

        // Placeholder component should render without throwing
        cut.Should().NotBeNull();
    }

    [Fact]
    public void Timeline_Placeholder_RendersEmptyOrMinimalMarkup()
    {
        var cut = RenderComponent<Timeline>();

        // Placeholder has no visible output
        cut.Markup.Trim().Should().BeEmpty();
    }
}