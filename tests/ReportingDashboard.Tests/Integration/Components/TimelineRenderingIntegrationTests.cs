using Bunit;
using FluentAssertions;
using ReportingDashboard.Components;
using Xunit;

namespace ReportingDashboard.Tests.Integration.Components;

/// <summary>
/// Timeline.razor is currently a placeholder with no [Parameter] properties.
/// These tests verify the placeholder renders without errors.
/// Full rendering tests will be added when the component is implemented.
/// </summary>
[Trait("Category", "Integration")]
public class TimelineRenderingIntegrationTests : TestContext
{
    [Fact]
    public void Timeline_Placeholder_RendersWithoutException()
    {
        var cut = RenderComponent<Timeline>();

        cut.Should().NotBeNull();
    }

    [Fact]
    public void Timeline_Placeholder_ProducesEmptyMarkup()
    {
        var cut = RenderComponent<Timeline>();

        cut.Markup.Trim().Should().BeEmpty();
    }

    [Fact]
    public void Timeline_Placeholder_CanBeRenderedMultipleTimes()
    {
        var cut1 = RenderComponent<Timeline>();
        var cut2 = RenderComponent<Timeline>();

        cut1.Should().NotBeNull();
        cut2.Should().NotBeNull();
    }
}