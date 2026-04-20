using System.Globalization;
using System.Threading;
using Bunit;
using FluentAssertions;
using ReportingDashboard.Web.Models;
using ReportingDashboard.Web.Services;
using Xunit;
using TC = Bunit.TestContext;

namespace ReportingDashboard.Web.Tests.Unit;

[Trait("Category", "Unit")]
public class TimelineSvgComponentTests
{
    private static Type TimelineSvgType =>
        typeof(ReportingDashboard.Web.Models.DashboardData).Assembly
            .GetType("ReportingDashboard.Web.Components.Pages.Partials.TimelineSvg", throwOnError: true)!;

    private static IRenderedComponent<Microsoft.AspNetCore.Components.DynamicComponent> RenderWithModel(
        TC ctx, object model)
    {
        return ctx.RenderComponent<Microsoft.AspNetCore.Components.DynamicComponent>(p => p
            .Add(x => x.Type, TimelineSvgType)
            .Add(x => x.Parameters, new Dictionary<string, object?> { ["Model"] = model }));
    }

    [Fact]
    public void Renders_empty_shell_with_svg_filter_and_defs()
    {
        using var ctx = new TC();
        var cut = RenderWithModel(ctx, TimelineViewModel.Empty);

        var markup = cut.Markup;
        markup.Should().Contain("class=\"tl-area\"");
        markup.Should().Contain("width=\"1560\"");
        markup.Should().Contain("height=\"185\"");
        markup.Should().Contain("id=\"sh\"");
        markup.Should().Contain("feDropShadow");
    }

    [Fact]
    public void Empty_model_does_not_emit_now_line()
    {
        using var ctx = new TC();
        var cut = RenderWithModel(ctx, TimelineViewModel.Empty);

        var markup = cut.Markup;
        markup.Should().NotContain("stroke-dasharray=\"5,3\"");
        markup.Should().NotContain(">NOW<");
    }

    [Fact]
    public void No_interactive_blazor_artifacts_in_output()
    {
        using var ctx = new TC();
        var cut = RenderWithModel(ctx, TimelineViewModel.Empty);

        var markup = cut.Markup;
        markup.Should().NotContain("@onclick");
        markup.Should().NotContain("<script");
        markup.Should().NotContain("blazor.server.js");
        markup.Should().NotContain("_framework/blazor.");
    }

    [Fact]
    public void Svg_canvas_has_expected_dimensions_and_inline_style()
    {
        using var ctx = new TC();
        var cut = RenderWithModel(ctx, TimelineViewModel.Empty);

        var svg = cut.Find("svg");
        svg.GetAttribute("width").Should().Be("1560");
        svg.GetAttribute("height").Should().Be("185");
        (svg.GetAttribute("style") ?? string.Empty).Should().Contain("overflow:visible");
    }

    [Fact]
    public void Renders_without_exception_under_non_invariant_culture()
    {
        var prev = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            using var ctx = new TC();
            var cut = RenderWithModel(ctx, TimelineViewModel.Empty);

            cut.Markup.Should().Contain("class=\"tl-area\"");
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = prev;
        }
    }
}