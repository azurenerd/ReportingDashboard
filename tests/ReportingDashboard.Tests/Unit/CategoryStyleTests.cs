using FluentAssertions;
using ReportingDashboard.Models;
using Xunit;

namespace ReportingDashboard.Tests.Unit;

[Trait("Category", "Unit")]
public class CategoryStyleTests
{
    [Fact]
    public void GetCategoryStyle_Shipped_ReturnsCorrectColors()
    {
        var style = CategoryStyle.GetCategoryStyle("shipped");

        style.HeaderText.Should().Be("#1B7A28");
        style.HeaderBg.Should().Be("#E8F5E9");
        style.CellBg.Should().Be("#F0FBF0");
        style.CellAccent.Should().Be("#D8F2DA");
        style.Bullet.Should().Be("#34A853");
        style.DisplayName.Should().Be("Shipped");
    }

    [Fact]
    public void GetCategoryStyle_InProgress_ReturnsCorrectColors()
    {
        var style = CategoryStyle.GetCategoryStyle("in-progress");

        style.HeaderText.Should().Be("#1565C0");
        style.HeaderBg.Should().Be("#E3F2FD");
        style.CellBg.Should().Be("#EEF4FE");
        style.CellAccent.Should().Be("#DAE8FB");
        style.Bullet.Should().Be("#0078D4");
        style.DisplayName.Should().Be("In Progress");
    }

    [Fact]
    public void GetCategoryStyle_Carryover_ReturnsCorrectColors()
    {
        var style = CategoryStyle.GetCategoryStyle("carryover");

        style.HeaderText.Should().Be("#B45309");
        style.HeaderBg.Should().Be("#FFF8E1");
        style.CellBg.Should().Be("#FFFDE7");
        style.CellAccent.Should().Be("#FFF0B0");
        style.Bullet.Should().Be("#F4B400");
        style.DisplayName.Should().Be("Carryover");
    }

    [Fact]
    public void GetCategoryStyle_Blockers_ReturnsCorrectColors()
    {
        var style = CategoryStyle.GetCategoryStyle("blockers");

        style.HeaderText.Should().Be("#991B1B");
        style.HeaderBg.Should().Be("#FEF2F2");
        style.CellBg.Should().Be("#FFF5F5");
        style.CellAccent.Should().Be("#FFE4E4");
        style.Bullet.Should().Be("#EA4335");
        style.DisplayName.Should().Be("Blockers");
    }

    [Fact]
    public void GetCategoryStyle_UnknownCategory_ReturnsFallbackWithCategoryAsDisplayName()
    {
        var style = CategoryStyle.GetCategoryStyle("custom-status");

        style.HeaderText.Should().Be("#666");
        style.HeaderBg.Should().Be("#F5F5F5");
        style.CellBg.Should().Be("#FAFAFA");
        style.CellAccent.Should().Be("#F0F0F0");
        style.Bullet.Should().Be("#999");
        style.DisplayName.Should().Be("custom-status");
    }
}