using Bunit;
using Xunit;
using ReportingDashboard.Components;

namespace ReportingDashboard.Tests.Unit.Components;

[Trait("Category", "Unit")]
public class ErrorPanelInlineStyleTests : TestContext
{
    [Fact]
    public void ErrorPanel_HasErrorPanelCssClass()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "test error"));

        Assert.Contains("error-panel", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_HasErrorContentCssClass()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "test error"));

        Assert.Contains("error-content", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_IndicatorHasRedColor()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "test error"));

        Assert.Contains("#EA4335", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_IndicatorHas48pxFontSize()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "test error"));

        Assert.Contains("48px", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_TitleHas20pxFontSize()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "test error"));

        Assert.Contains("20px", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_TitleHasBoldFontWeight()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "test error"));

        Assert.Contains("700", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_MessageHasMonospaceFont()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "test error"));

        Assert.Contains("monospace", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_MessageHas14pxFontSize()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "test error"));

        Assert.Contains("14px", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_HelpTextHas12pxFontSize()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "test error"));

        Assert.Contains("12px", cut.Markup);
    }

    [Fact]
    public void ErrorPanel_HelpTextHasGrayColor()
    {
        var cut = RenderComponent<ErrorPanel>(p =>
            p.Add(x => x.ErrorMessage, "test error"));

        Assert.Contains("#888", cut.Markup);
    }
}