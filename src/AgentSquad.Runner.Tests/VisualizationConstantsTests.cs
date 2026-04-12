#nullable enable

using AgentSquad.Runner.Config;
using Xunit;

namespace AgentSquad.Runner.Tests;

public class VisualizationConstantsTests
{
    [Fact]
    public void AllColorConstantsAreValidHexFormat()
    {
        var colorFields = typeof(VisualizationConstants).GetFields()
            .Where(f => f.Name.StartsWith("Color") && f.FieldType == typeof(string))
            .ToList();

        Assert.NotEmpty(colorFields);

        foreach (var field in colorFields)
        {
            var value = (string?)field.GetValue(null);
            Assert.NotNull(value);
            Assert.Matches(@"^#[0-9A-Fa-f]{6}$", value!);
        }
    }

    [Fact]
    public void AllCssClassNamesAreNonEmpty()
    {
        var cssFields = typeof(VisualizationConstants).GetFields()
            .Where(f => f.Name.StartsWith("CssClass") && f.FieldType == typeof(string))
            .ToList();

        Assert.NotEmpty(cssFields);

        foreach (var field in cssFields)
        {
            var value = (string?)field.GetValue(null);
            Assert.NotNull(value);
            Assert.NotEmpty(value);
        }
    }

    [Theory]
    [InlineData(VisualizationConstants.StatusShipped)]
    [InlineData(VisualizationConstants.StatusInProgress)]
    [InlineData(VisualizationConstants.StatusCarryover)]
    [InlineData(VisualizationConstants.StatusBlockers)]
    public void GetStatusDotColorReturnsValidColorForEachStatus(string status)
    {
        var color = VisualizationConstants.GetStatusDotColor(status);
        Assert.NotNull(color);
        Assert.Matches(@"^#[0-9A-Fa-f]{6}$", color);
    }

    [Fact]
    public void GetCellCssClassReturnsCorrectFormat()
    {
        var cellClass = VisualizationConstants.GetCellCssClass(VisualizationConstants.StatusShipped, false);
        Assert.Equal(VisualizationConstants.CssClassShippedCell, cellClass);

        var cellClassCurrent = VisualizationConstants.GetCellCssClass(VisualizationConstants.StatusShipped, true);
        Assert.Contains(VisualizationConstants.CssClassCurrentMonth, cellClassCurrent);
    }

    [Fact]
    public void AllSvgDimensionConstantsArePositive()
    {
        var dimensionFields = typeof(VisualizationConstants).GetFields()
            .Where(f => (f.Name.Contains("Radius") || f.Name.Contains("StrokeWidth") || f.Name.Contains("Width") || f.Name.Contains("Height"))
                        && (f.FieldType == typeof(int) || f.FieldType == typeof(double)))
            .ToList();

        foreach (var field in dimensionFields)
        {
            var value = field.GetValue(null);
            if (field.FieldType == typeof(int))
            {
                Assert.True((int)value! > 0, $"Field {field.Name} should be positive");
            }
            else if (field.FieldType == typeof(double))
            {
                Assert.True((double)value! > 0, $"Field {field.Name} should be positive");
            }
        }
    }
}