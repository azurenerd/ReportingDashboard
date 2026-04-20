using System;

namespace ReportingDashboard.Web.Tests.Unit;

public class LayoutAndValidatorStubTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void HeatmapLayoutEngine_Build_WithNullHeatmap_ReturnsEmptyViewModel()
    {
        var result = HeatmapLayoutEngine.Build(null!, new DateOnly(2026, 1, 1));

        result.Should().NotBeNull();
        result.Should().BeSameAs(HeatmapViewModel.Empty);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void DashboardDataValidator_Validate_WithNullData_ReturnsEmptyList()
    {
        var errors = DashboardDataValidator.Validate(null!);

        errors.Should().NotBeNull().And.BeEmpty();
    }
}